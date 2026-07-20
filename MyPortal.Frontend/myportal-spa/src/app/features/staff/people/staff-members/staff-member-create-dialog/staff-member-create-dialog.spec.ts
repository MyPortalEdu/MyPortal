import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { of, throwError } from 'rxjs';
import { TranslocoService } from '@jsverse/transloco';
import { createSpyObj, type SpyObj } from '@testing/spy';

import { StaffMemberCreateDialog } from './staff-member-create-dialog';
import { StaffMembersDataService } from '../../../../../shared/services/staff-members-data.service';
import { NotificationService } from '../../../../../core/services/notification.service';
import { PersonMatchResponse } from '../../../../../shared/types/person-match';
import { StaffBasicDetailsUpsertRequest } from '../../../../../shared/types/staff-basic-details';

interface CreateModel {
  code: string;
  firstName: string;
  lastName: string;
  gender: string;
}
interface FieldLike {
  valid(): boolean;
  invalid(): boolean;
  submitting(): boolean;
}
interface Api {
  model: { (): CreateModel; update(fn: (m: CreateModel) => CreateModel): void };
  f: () => FieldLike;
  step(): string;
  pickPerson(p: PersonMatchResponse): void;
  save(): Promise<void>;
  attach(): Promise<void>;
}

function match(overrides: Partial<PersonMatchResponse> = {}): PersonMatchResponse {
  return {
    personId: 'person-1',
    title: null,
    firstName: 'Ada',
    middleName: null,
    lastName: 'Lovelace',
    preferredFirstName: null,
    preferredLastName: null,
    dob: null,
    existingStaffMemberId: null,
    isStaffMember: false,
    ...overrides,
  };
}

describe('StaffMemberCreateDialog (applyWhen step-conditional)', () => {
  let fixture: ComponentFixture<StaffMemberCreateDialog>;
  let component: StaffMemberCreateDialog;
  let api: Api;
  let data: SpyObj<StaffMembersDataService>;
  let notify: SpyObj<NotificationService>;

  function configure() {
    data = createSpyObj<StaffMembersDataService>(['searchPeople', 'create', 'createForPerson']);
    notify = createSpyObj<NotificationService>(['success', 'apiError']);
    data.searchPeople.mockReturnValue(of([]));
    data.create.mockReturnValue(of({ id: 'new-staff' }));
    data.createForPerson.mockReturnValue(of({ id: 'attached-staff' }));

    const translocoStub = {
      translate: (k: string) => k,
      getActiveLang: () => 'en',
    } as Partial<TranslocoService> as TranslocoService;

    TestBed.overrideComponent(StaffMemberCreateDialog, { set: { template: '' } });

    TestBed.configureTestingModule({
      providers: [
        { provide: StaffMembersDataService, useValue: data },
        { provide: NotificationService, useValue: notify },
        { provide: TranslocoService, useValue: translocoStub },
      ],
    });

    fixture = TestBed.createComponent(StaffMemberCreateDialog);
    component = fixture.componentInstance;
    api = component as unknown as Api;
    fixture.componentRef.setInput('open', false);
    fixture.componentRef.setInput('open', true);
    fixture.detectChanges();
  }

  it('search step (new person) requires name + gender + code', () => {
    configure();
    expect(api.step()).toBe('search');
    expect(api.f().invalid()).toBe(true);

    api.model.update(m => ({ ...m, code: 'S1', firstName: 'Ada', lastName: 'Lovelace', gender: 'F' }));
    expect(api.f().valid()).toBe(true);
  });

  it('attach step only requires code (applyWhen drops name/gender by step)', () => {
    configure();
    api.pickPerson(match());
    expect(api.step()).toBe('attach');
    // names/gender are empty, but not required in the attach step
    expect(api.f().invalid()).toBe(true);
    api.model.update(m => ({ ...m, code: 'S1' }));
    expect(api.f().valid()).toBe(true);
  });

  it('picking an existing staff member emits openExisting rather than attaching', () => {
    configure();
    const openExisting = vi.fn();
    component.openExisting.subscribe(openExisting);
    api.pickPerson(match({ isStaffMember: true, existingStaffMemberId: 'sm-9' }));
    expect(openExisting).toHaveBeenCalledWith('sm-9');
    expect(api.step()).toBe('search');
  });

  it('save() creates a new person and emits created', async () => {
    configure();
    api.model.update(m => ({ ...m, code: '  S1  ', firstName: '  Ada  ', lastName: 'Lovelace', gender: 'F' }));
    const created = vi.fn();
    component.created.subscribe(created);

    await api.save();

    const payload = data.create.mock.calls.at(-1)![0] as StaffBasicDetailsUpsertRequest;
    expect(payload.firstName).toBe('Ada');
    expect(payload.code).toBe('S1');
    expect(created).toHaveBeenCalledWith('new-staff');
  });

  it('attach() creates for the selected person and emits created', async () => {
    configure();
    api.pickPerson(match({ personId: 'person-7' }));
    api.model.update(m => ({ ...m, code: 'S2' }));
    const created = vi.fn();
    component.created.subscribe(created);

    await api.attach();

    expect(data.createForPerson).toHaveBeenCalledWith({ personId: 'person-7', code: 'S2' });
    expect(created).toHaveBeenCalledWith('attached-staff');
  });

  it('save() surfaces an apiError toast on failure', async () => {
    configure();
    data.create.mockReturnValue(throwError(() => new Error('boom')));
    api.model.update(m => ({ ...m, code: 'S1', firstName: 'Ada', lastName: 'Lovelace', gender: 'F' }));

    await api.save();

    expect(notify.apiError).toHaveBeenCalled();
  });
});

interface CodeField {
  errors(): ReadonlyArray<{ kind: string }>;
  pending(): boolean;
}
interface AsyncApi {
  model: { (): CreateModel; update(fn: (m: CreateModel) => CreateModel): void };
  f: (() => { valid(): boolean }) & { code: () => CodeField };
}

describe('StaffMemberCreateDialog code availability (validateHttp)', () => {
  function setup() {
    const dataSpy = createSpyObj<StaffMembersDataService>(['searchPeople', 'create', 'createForPerson']);
    dataSpy.searchPeople.mockReturnValue(of([]));

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: StaffMembersDataService, useValue: dataSpy },
        { provide: NotificationService, useValue: createSpyObj<NotificationService>(['success', 'apiError']) },
        { provide: TranslocoService, useValue: { translate: (k: string) => k, getActiveLang: () => 'en' } },
      ],
    });
    TestBed.overrideComponent(StaffMemberCreateDialog, { set: { template: '' } });

    const fixture = TestBed.createComponent(StaffMemberCreateDialog);
    fixture.componentRef.setInput('open', false);
    fixture.componentRef.setInput('open', true);
    fixture.detectChanges();
    return {
      fixture,
      api: fixture.componentInstance as unknown as AsyncApi,
      http: TestBed.inject(HttpTestingController),
    };
  }

  async function fillValidCode(fixture: ComponentFixture<StaffMemberCreateDialog>, api: AsyncApi, code: string) {
    api.model.update(m => ({ ...m, code, firstName: 'A', lastName: 'B', gender: 'F' }));
    fixture.detectChanges();
    await vi.advanceTimersByTimeAsync(500);
    fixture.detectChanges();
  }

  it('calls the availability endpoint and flags a taken code', async () => {
    const { fixture, api, http } = setup();
    vi.useFakeTimers();
    try {
      await fillValidCode(fixture, api, 'ABC123');

      const req = http.expectOne(r => r.url.includes('/api/v1/staffmembers/code-available'));
      expect(req.request.urlWithParams).toContain('code=ABC123');
      req.flush({ available: false });
      await vi.advanceTimersByTimeAsync(1);
      fixture.detectChanges();

      expect(api.f.code().errors().some(e => e.kind === 'taken')).toBe(true);
      http.verify();
    } finally {
      vi.useRealTimers();
    }
  });

  it('leaves an available code with no taken error', async () => {
    const { fixture, api, http } = setup();
    vi.useFakeTimers();
    try {
      await fillValidCode(fixture, api, 'FREE99');

      http.expectOne(r => r.url.includes('/code-available')).flush({ available: true });
      await vi.advanceTimersByTimeAsync(1);
      fixture.detectChanges();

      expect(api.f.code().errors().some(e => e.kind === 'taken')).toBe(false);
      http.verify();
    } finally {
      vi.useRealTimers();
    }
  });
});
