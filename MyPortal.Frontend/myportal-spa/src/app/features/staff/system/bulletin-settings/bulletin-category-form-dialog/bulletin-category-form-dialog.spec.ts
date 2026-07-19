import { createSpyObj, type SpyObj } from '@testing/spy';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { TranslocoService } from '@jsverse/transloco';

import {
  BulletinCategoryFormDialog,
  CATEGORY_COLOURS,
  CATEGORY_ICONS,
} from './bulletin-category-form-dialog';
import { BulletinsDataService } from '../../../../../shared/services/bulletins-data.service';
import { NotificationService } from '../../../../../core/services/notification.service';
import { ConfirmationDialog } from '../../../../../core/services/confirmation.service';
import { BulletinCategoryResponse } from '../../../../../shared/types/bulletin';

interface FormModel {
  name: string;
  icon: string;
  colour: string;
  displayOrder: number;
  active: boolean;
}
interface FieldLike {
  valid(): boolean;
  invalid(): boolean;
  submitting(): boolean;
  touched(): boolean;
}
interface Internals {
  model: { (): FormModel; set(v: FormModel): void; update(fn: (m: FormModel) => FormModel): void };
  f: (() => FieldLike) & { name: () => FieldLike };
}

function makeCategory(overrides: Partial<BulletinCategoryResponse> = {}): BulletinCategoryResponse {
  return {
    id: 'c1',
    name: 'Existing',
    icon: 'pi pi-bell',
    colourCode: '#dc2626',
    displayOrder: 5,
    active: false,
    isSystem: false,
    version: 3,
    ...overrides,
  };
}

describe('BulletinCategoryFormDialog', () => {
  let fixture: ComponentFixture<BulletinCategoryFormDialog>;
  let component: BulletinCategoryFormDialog;
  let internals: Internals;
  let data: SpyObj<BulletinsDataService>;
  let notify: SpyObj<NotificationService>;
  let confirmDialog: SpyObj<ConfirmationDialog>;

  beforeEach(async () => {
    data = createSpyObj<BulletinsDataService>(['createCategory', 'updateCategory']);
    notify = createSpyObj<NotificationService>(['success', 'apiError']);

    data.createCategory.mockReturnValue(of({ id: 'new-id' }));
    data.updateCategory.mockReturnValue(of(void 0));

    confirmDialog = createSpyObj<ConfirmationDialog>(['confirm', 'danger']);
    confirmDialog.confirm.mockResolvedValue(true);
    confirmDialog.danger.mockResolvedValue(true);

    const translocoStub = {
      translate: (key: string) => key,
      getActiveLang: () => 'en',
    } as Partial<TranslocoService> as TranslocoService;

    TestBed.overrideComponent(BulletinCategoryFormDialog, { set: { template: '' } });

    await TestBed.configureTestingModule({
      imports: [BulletinCategoryFormDialog],
      providers: [
        { provide: BulletinsDataService, useValue: data },
        { provide: NotificationService, useValue: notify },
        { provide: ConfirmationDialog, useValue: confirmDialog },
        { provide: TranslocoService, useValue: translocoStub },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(BulletinCategoryFormDialog);
    component = fixture.componentInstance;
    internals = component as unknown as Internals;
    fixture.componentRef.setInput('visible', false);
    fixture.componentRef.setInput('existing', null);
    fixture.detectChanges();
  });

  function open(existing: BulletinCategoryResponse | null = null) {
    fixture.componentRef.setInput('existing', existing);
    fixture.componentRef.setInput('visible', true);
    fixture.detectChanges();
  }

  it('create mode initialises with defaults (first icon, first colour, displayOrder 100, active=true)', () => {
    open(null);
    expect(component.isEdit()).toBe(false);
    expect(internals.model()).toEqual({
      name: '',
      icon: CATEGORY_ICONS[0],
      colour: CATEGORY_COLOURS[0],
      displayOrder: 100,
      active: true,
    });
  });

  it('edit mode hydrates every field from the existing category', () => {
    open(makeCategory());
    expect(component.isEdit()).toBe(true);
    expect(internals.model()).toEqual({
      name: 'Existing',
      icon: 'pi pi-bell',
      colour: '#dc2626',
      displayOrder: 5,
      active: false,
    });
  });

  it('edit mode falls back to defaults when icon/colour are missing on the existing category', () => {
    open(makeCategory({ icon: '', colourCode: '' }));
    expect(internals.model().icon).toBe(CATEGORY_ICONS[0]);
    expect(internals.model().colour).toBe(CATEGORY_COLOURS[0]);
  });

  it('form is invalid when the name is empty or whitespace', () => {
    open(null);
    expect(internals.f().invalid()).toBe(true);
    internals.model.update(m => ({ ...m, name: '  ' }));
    expect(internals.f().invalid()).toBe(true);
    internals.model.update(m => ({ ...m, name: 'Notices' }));
    expect(internals.f().valid()).toBe(true);
  });

  it('save() in create mode posts the trimmed payload, toasts, and emits saved', async () => {
    open(null);
    internals.model.set({
      name: '  Notices  ',
      icon: 'pi pi-megaphone',
      colour: '#4f46e5',
      displayOrder: 3,
      active: true,
    });

    const saved = vi.fn();
    component.saved.subscribe(saved);

    await component.save();

    expect(data.createCategory).toHaveBeenCalledWith({
      name: 'Notices',
      icon: 'pi pi-megaphone',
      colourCode: '#4f46e5',
      displayOrder: 3,
      active: true,
      expectedVersion: 0,
    });
    expect(notify.success).toHaveBeenCalled();
    expect(saved).toHaveBeenCalled();
    expect(internals.f().submitting()).toBe(false);
  });

  it('save() in edit mode calls updateCategory with expectedVersion', async () => {
    open(makeCategory({ id: 'c1', version: 9 }));
    internals.model.update(m => ({ ...m, name: 'Renamed' }));

    await component.save();

    expect(data.updateCategory).toHaveBeenCalled();
    const [id, payload] = data.updateCategory.mock.calls.at(-1)!;
    expect(id).toBe('c1');
    expect(payload.expectedVersion).toBe(9);
    expect(payload.name).toBe('Renamed');
    expect(data.createCategory).not.toHaveBeenCalled();
  });

  it('save() on an invalid form skips the API and marks the name touched to reveal errors', async () => {
    open(null);
    await component.save();
    expect(data.createCategory).not.toHaveBeenCalled();
    expect(internals.f.name().touched()).toBe(true);
  });

  it('save() shows an apiError toast and clears submitting on failure', async () => {
    data.createCategory.mockReturnValue(throwError(() => new Error('boom')));
    open(null);
    internals.model.update(m => ({ ...m, name: 'X' }));

    await component.save();

    expect(notify.apiError).toHaveBeenCalled();
    expect(internals.f().submitting()).toBe(false);
  });

  it('onCancel/onHide emit closed', () => {
    open(null);
    const closed = vi.fn();
    component.closed.subscribe(closed);
    component.onCancel();
    component.onHide();
    expect(closed).toHaveBeenCalledTimes(2);
  });
});
