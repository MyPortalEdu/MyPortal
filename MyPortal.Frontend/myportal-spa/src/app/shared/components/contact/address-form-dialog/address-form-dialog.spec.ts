import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { TranslocoService } from '@jsverse/transloco';
import { createSpyObj, type SpyObj } from '@testing/spy';

import { AddressFormDialog } from './address-form-dialog';
import { PersonAddressDataSource } from '../person-address-data-source';
import { StaffMembersDataService } from '../../../services/staff-members-data.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { ConfirmationDialog } from '../../../../core/services/confirmation.service';
import { AddressMatchResponse, PersonAddressResponse, PersonAddressUpsertRequest } from '../../../types/staff-address';

interface AddressModel {
  typeId: string;
  street: string;
  town: string;
  county: string;
  postcode: string;
  country: string;
  isMain: boolean;
}
interface FieldLike {
  valid(): boolean;
  invalid(): boolean;
  submitting(): boolean;
}
interface Api {
  model: { (): AddressModel; update(fn: (m: AddressModel) => AddressModel): void };
  f: () => FieldLike;
  canSave(): boolean;
  step(): string;
  enterManually(): void;
  pickExisting(m: AddressMatchResponse): void;
  save(): void;
}

function makeMatch(): AddressMatchResponse {
  return {
    addressId: 'addr-1',
    buildingNumber: '10',
    buildingName: null,
    apartment: null,
    street: 'High Street',
    district: null,
    town: 'Townsville',
    county: 'Countyshire',
    postcode: 'AB1 2CD',
    country: 'United Kingdom',
    linkedPersonCount: 2,
  };
}

const ADDRESS_TYPES = [{ id: 'type-home', description: 'Home' }];

describe('AddressFormDialog (applyWhen step-conditional)', () => {
  let fixture: ComponentFixture<AddressFormDialog>;
  let api: Api;
  let data: SpyObj<StaffMembersDataService>;
  let notify: SpyObj<NotificationService>;

  function configure(editTarget: PersonAddressResponse | null = null) {
    data = createSpyObj<StaffMembersDataService>(['searchAddressMatches', 'addAddress', 'updateAddress']);
    notify = createSpyObj<NotificationService>(['success', 'apiError']);
    data.searchAddressMatches.mockReturnValue(of([]));
    data.addAddress.mockReturnValue(of({ id: 'new-addr' }));
    data.updateAddress.mockReturnValue(of({ id: 'addr-1' }));

    const translocoStub = {
      translate: (k: string) => k,
      getActiveLang: () => 'en',
    } as Partial<TranslocoService> as TranslocoService;

    TestBed.overrideComponent(AddressFormDialog, { set: { template: '' } });

    TestBed.configureTestingModule({
      providers: [
        { provide: PersonAddressDataSource, useValue: data },
        { provide: NotificationService, useValue: notify },
        { provide: ConfirmationDialog, useValue: createSpyObj<ConfirmationDialog>(['confirm']) },
        { provide: TranslocoService, useValue: translocoStub },
      ],
    });

    fixture = TestBed.createComponent(AddressFormDialog);
    api = fixture.componentInstance as unknown as Api;
    fixture.componentRef.setInput('open', false);
    fixture.componentRef.setInput('staffMemberId', 'staff-1');
    fixture.componentRef.setInput('addressTypes', ADDRESS_TYPES);
    fixture.componentRef.setInput('editTarget', editTarget);
    fixture.componentRef.setInput('open', true);
    fixture.detectChanges();
  }

  it('form step requires the address fields (invalid until filled)', () => {
    configure();
    api.enterManually();
    expect(api.step()).toBe('form');
    expect(api.f().invalid()).toBe(true);
    expect(api.canSave()).toBe(false);

    api.model.update(m => ({
      ...m,
      street: 'High Street',
      town: 'Townsville',
      county: 'Countyshire',
      postcode: 'AB1 2CD',
      country: 'United Kingdom',
    }));
    expect(api.f().valid()).toBe(true);
    expect(api.canSave()).toBe(true);
  });

  it('link step does NOT require address fields (applyWhen re-evaluates by step)', () => {
    configure();
    api.pickExisting(makeMatch());
    expect(api.step()).toBe('link');
    // address fields are still empty, but they are not required in the link step
    expect(api.f().valid()).toBe(true);
    expect(api.canSave()).toBe(true);
  });

  const flush = () => new Promise<void>(resolve => setTimeout(resolve, 0));

  it('save() in link mode posts an existing-address link payload', async () => {
    configure();
    api.pickExisting(makeMatch());

    api.save();
    await flush();

    const payload = data.addAddress.mock.calls.at(-1)![1] as PersonAddressUpsertRequest;
    expect(payload.existingAddressId).toBe('addr-1');
    expect(payload.typeId).toBe('type-home');
    expect(notify.success).toHaveBeenCalled();
  });

  it('save() in manual add mode posts the trimmed address payload', async () => {
    configure();
    api.enterManually();
    api.model.update(m => ({
      ...m,
      street: '  High Street  ',
      town: 'Townsville',
      county: 'Countyshire',
      postcode: 'AB1 2CD',
      country: 'United Kingdom',
    }));

    api.save();
    await flush();

    const payload = data.addAddress.mock.calls.at(-1)![1] as PersonAddressUpsertRequest;
    expect(payload.existingAddressId).toBeUndefined();
    expect(payload.street).toBe('High Street');
    expect(payload.town).toBe('Townsville');
  });

  it('save() surfaces an apiError toast on failure', async () => {
    configure();
    data.addAddress.mockReturnValue(throwError(() => new Error('boom')));
    api.pickExisting(makeMatch());

    api.save();
    await flush();

    expect(notify.apiError).toHaveBeenCalled();
  });
});
