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
  let data: jasmine.SpyObj<BulletinsDataService>;
  let notify: jasmine.SpyObj<NotificationService>;
  let confirmDialog: jasmine.SpyObj<ConfirmationDialog>;

  beforeEach(async () => {
    data = jasmine.createSpyObj<BulletinsDataService>('BulletinsDataService',
      ['createCategory', 'updateCategory']);
    notify = jasmine.createSpyObj<NotificationService>('NotificationService', ['success', 'apiError']);

    data.createCategory.and.returnValue(of({ id: 'new-id' }));
    data.updateCategory.and.returnValue(of(void 0));

    confirmDialog = jasmine.createSpyObj<ConfirmationDialog>('ConfirmationDialog', ['confirm', 'danger']);
    confirmDialog.confirm.and.resolveTo(true);
    confirmDialog.danger.and.resolveTo(true);

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
    fixture.componentRef.setInput('visible', false);
    fixture.componentRef.setInput('existing', null);
    fixture.detectChanges();
  });

  function open(existing: BulletinCategoryResponse | null = null) {
    fixture.componentRef.setInput('existing', existing);
    fixture.componentRef.setInput('visible', true);
    fixture.detectChanges();
  }

  // ─── reset / open lifecycle ─────────────────────────────────────────────

  it('create mode initialises with defaults (first icon, first colour, displayOrder 100, active=true)', () => {
    open(null);
    expect(component.isEdit()).toBeFalse();
    expect(component.name()).toBe('');
    expect(component.icon()).toBe(CATEGORY_ICONS[0]);
    expect(component.colour()).toBe(CATEGORY_COLOURS[0]);
    expect(component.displayOrder()).toBe(100);
    expect(component.active()).toBeTrue();
  });

  it('edit mode hydrates every field from the existing category', () => {
    open(makeCategory());
    expect(component.isEdit()).toBeTrue();
    expect(component.name()).toBe('Existing');
    expect(component.icon()).toBe('pi pi-bell');
    expect(component.colour()).toBe('#dc2626');
    expect(component.displayOrder()).toBe(5);
    expect(component.active()).toBeFalse();
  });

  it('edit mode falls back to defaults when icon/colour are missing on the existing category', () => {
    open(makeCategory({ icon: '', colourCode: '' }));
    expect(component.icon()).toBe(CATEGORY_ICONS[0]);
    expect(component.colour()).toBe(CATEGORY_COLOURS[0]);
  });

  // ─── isValid ────────────────────────────────────────────────────────────

  it('isValid is false when the name is empty or whitespace', () => {
    open(null);
    expect(component.isValid()).toBeFalse();
    component.name.set('  ');
    expect(component.isValid()).toBeFalse();
    component.name.set('Notices');
    expect(component.isValid()).toBeTrue();
  });

  // ─── save() ─────────────────────────────────────────────────────────────

  it('save() in create mode posts the trimmed payload, toasts, and emits saved', () => {
    open(null);
    component.name.set('  Notices  ');
    component.icon.set('pi pi-megaphone');
    component.colour.set('#4f46e5');
    component.displayOrder.set(3);
    component.active.set(true);

    const saved = jasmine.createSpy('saved');
    component.saved.subscribe(saved);

    component.save();

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
    expect(component.submitting()).toBeFalse();
  });

  it('save() in edit mode calls updateCategory with expectedVersion', () => {
    open(makeCategory({ id: 'c1', version: 9 }));
    component.name.set('Renamed');

    component.save();

    expect(data.updateCategory).toHaveBeenCalled();
    const [id, payload] = data.updateCategory.calls.mostRecent().args;
    expect(id).toBe('c1');
    expect(payload.expectedVersion).toBe(9);
    expect(payload.name).toBe('Renamed');
    expect(data.createCategory).not.toHaveBeenCalled();
  });

  it('save() guards against invalid input', () => {
    open(null); // name empty
    component.save();
    expect(data.createCategory).not.toHaveBeenCalled();
  });

  it('save() guards against re-entrant submissions while one is in flight', () => {
    open(null);
    component.name.set('X');
    component.submitting.set(true);

    component.save();

    expect(data.createCategory).not.toHaveBeenCalled();
  });

  it('save() shows an apiError toast and clears submitting on failure', () => {
    data.createCategory.and.returnValue(throwError(() => new Error('boom')));
    open(null);
    component.name.set('X');

    component.save();

    expect(notify.apiError).toHaveBeenCalled();
    expect(component.submitting()).toBeFalse();
  });

  // ─── close ──────────────────────────────────────────────────────────────

  it('onCancel/onHide emit closed', () => {
    open(null);
    const closed = jasmine.createSpy('closed');
    component.closed.subscribe(closed);
    component.onCancel();
    component.onHide();
    expect(closed).toHaveBeenCalledTimes(2);
  });
});
