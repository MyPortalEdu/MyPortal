import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { TranslocoService } from '@jsverse/transloco';

import { BulletinFormDialog } from './bulletin-form-dialog';
import { BulletinsDataService } from '../../../services/bulletins-data.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { ConfirmationDialog } from '../../../../core/services/confirmation.service';
import { MeService } from '../../../../core/services/me-service';
import { UserType } from '../../../../core/types/user-type';
import { Me } from '../../../../core/types/me';
import {
  BulletinAllowedGroupResponse,
  BulletinAudienceKind,
  BulletinCategoryResponse,
  BulletinDetailsResponse,
  BulletinUpsertRequest,
} from '../../../types/bulletin';

function makeDetail(overrides: Partial<BulletinDetailsResponse> = {}): BulletinDetailsResponse {
  return {
    id: 'b1',
    directoryId: 'dir',
    title: 'Existing title',
    detail: 'Existing detail',
    categoryId: 'cat-1',
    categoryName: 'Cat',
    categoryIcon: 'pi pi-info',
    categoryColourCode: '#0066CC',
    createdById: 'me',
    createdByName: 'Me',
    createdByIpAddress: '::1',
    createdAt: '2026-05-01T12:00:00Z',
    lastModifiedById: 'me',
    lastModifiedByName: 'Me',
    lastModifiedByIpAddress: '::1',
    lastModifiedAt: '2026-05-01T12:00:00Z',
    requiresAcknowledgement: true,
    pinnedAt: '2026-05-01T12:00:00Z',
    expiresAt: null,
    audiences: [
      { id: 'a1', audienceKind: BulletinAudienceKind.AllStaff, studentGroupId: null, studentGroupName: null },
    ],
    attachmentCount: 0,
    version: 7,
    ...overrides,
  };
}

function makeMe(overrides: Partial<Me> = {}): Me {
  return {
    id: 'me',
    username: 'me',
    userType: UserType.Staff,
    isEnabled: true,
    isSystem: false,
    displayName: 'Me',
    permissions: [],
    ...overrides,
  };
}

describe('BulletinFormDialog', () => {
  let fixture: ComponentFixture<BulletinFormDialog>;
  let component: BulletinFormDialog;
  let data: jasmine.SpyObj<BulletinsDataService>;
  let notify: jasmine.SpyObj<NotificationService>;
  let me$: jasmine.SpyObj<MeService>;
  let confirmDialog: jasmine.SpyObj<ConfirmationDialog>;

  const categories: BulletinCategoryResponse[] = [
    { id: 'cat-1', name: 'Notices', icon: 'i', colourCode: '#000000', displayOrder: 1, active: true, isSystem: false, version: 1 },
    { id: 'cat-2', name: 'Sports',  icon: 'i', colourCode: '#000000', displayOrder: 2, active: true, isSystem: false, version: 1 },
  ];
  const allowedGroups: BulletinAllowedGroupResponse[] = [
    { studentGroupId: 'g-allowed', code: '7A', name: 'Year 7A' },
  ];

  beforeEach(async () => {
    data = jasmine.createSpyObj<BulletinsDataService>('BulletinsDataService',
      ['listCategories', 'getSettings', 'create', 'update', 'getById']);
    notify = jasmine.createSpyObj<NotificationService>('NotificationService', ['success', 'error']);
    me$ = jasmine.createSpyObj<MeService>('MeService', ['me']);

    data.listCategories.and.returnValue(of(categories));
    data.getSettings.and.returnValue(of({ allowedAudienceGroups: allowedGroups }));
    data.create.and.returnValue(of({ id: 'new-id' }));
    data.update.and.returnValue(of(void 0));
    data.getById.and.returnValue(of(makeDetail({ id: 'new-id', directoryId: 'new-dir' })));
    me$.me.and.returnValue(of(makeMe()));

    confirmDialog = jasmine.createSpyObj<ConfirmationDialog>('ConfirmationDialog', ['confirm', 'danger']);
    confirmDialog.confirm.and.resolveTo(true);
    confirmDialog.danger.and.resolveTo(true);

    const translocoStub = {
      translate: (key: string) => key,
      getActiveLang: () => 'en',
    } as Partial<TranslocoService> as TranslocoService;

    TestBed.overrideComponent(BulletinFormDialog, { set: { template: '' } });

    await TestBed.configureTestingModule({
      imports: [BulletinFormDialog],
      providers: [
        { provide: BulletinsDataService, useValue: data },
        { provide: NotificationService, useValue: notify },
        { provide: MeService, useValue: me$ },
        { provide: ConfirmationDialog, useValue: confirmDialog },
        { provide: TranslocoService, useValue: translocoStub },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(BulletinFormDialog);
    component = fixture.componentInstance;
    // Required input; setting it before the first CD avoids a "Required input
    // not provided" error. We start hidden so the open() helper drives the
    // effect that re-runs reset()/loadDependencies().
    fixture.componentRef.setInput('visible', false);
    fixture.componentRef.setInput('existing', null);
    fixture.detectChanges();
  });

  function open(existing: BulletinDetailsResponse | null = null) {
    fixture.componentRef.setInput('existing', existing);
    fixture.componentRef.setInput('visible', true);
    fixture.detectChanges();
  }

  it('loads me, categories, and allowed groups on open', () => {
    open();
    expect(me$.me).toHaveBeenCalled();
    expect(data.listCategories).toHaveBeenCalledWith(false);
    expect(data.getSettings).toHaveBeenCalled();
    expect(component.categories().length).toBe(2);
    expect(component.allowedGroups().length).toBe(1);
  });

  it('canPin reflects whether the current user has the pin permission', () => {
    me$.me.and.returnValue(of(makeMe({ permissions: ['School.PinSchoolBulletins'] })));
    open();
    expect(component.canPin()).toBeTrue();
  });

  it('create mode pre-selects All staff and defaults the category to the first option', () => {
    open(null);
    expect(component.title()).toBe('');
    expect(component.detail()).toBe('');
    expect(component.isPinned()).toBeFalse();
    expect(component.requiresAck()).toBeFalse();
    expect(component.isSelected('all-staff')).toBeTrue();
    expect(component.categoryId()).toBe('cat-1');
  });

  it('edit mode hydrates every field from the existing bulletin', () => {
    open(makeDetail({
      title: 'Edit me', detail: 'Edit detail body',
      pinnedAt: '2026-05-01T12:00:00Z', requiresAcknowledgement: true,
      audiences: [
        { id: 'a1', audienceKind: BulletinAudienceKind.AllPupils,    studentGroupId: null, studentGroupName: null },
        { id: 'a2', audienceKind: BulletinAudienceKind.StudentGroup, studentGroupId: 'g-x', studentGroupName: 'Year 8B' },
      ],
    }));

    expect(component.isEdit()).toBeTrue();
    expect(component.title()).toBe('Edit me');
    expect(component.detail()).toBe('Edit detail body');
    expect(component.isPinned()).toBeTrue();
    expect(component.requiresAck()).toBeTrue();
    expect(component.isSelected('all-pupils')).toBeTrue();
    expect(component.isSelected('sg-g-x')).toBeTrue();
  });

  it('audienceChoices surfaces groups from the existing bulletin even when not in the allowlist', () => {
    // Admin removed the group from the allowlist after the bulletin was created;
    // we still need to render it so the editor can untick it.
    open(makeDetail({
      audiences: [
        { id: 'a1', audienceKind: BulletinAudienceKind.StudentGroup, studentGroupId: 'g-removed', studentGroupName: 'Old Group' },
      ],
    }));

    const keys = component.audienceChoices().map(c => c.key);
    expect(keys).toContain('sg-g-allowed');  // from allowlist
    expect(keys).toContain('sg-g-removed');  // surfaced from bulletin
  });

  it('audienceChoices de-duplicates a group present in both the allowlist and the bulletin', () => {
    open(makeDetail({
      audiences: [
        { id: 'a1', audienceKind: BulletinAudienceKind.StudentGroup, studentGroupId: 'g-allowed', studentGroupName: 'Year 7A' },
      ],
    }));

    const groupKeys = component.audienceChoices().filter(c => c.studentGroupId === 'g-allowed').map(c => c.key);
    expect(groupKeys).toEqual(['sg-g-allowed']);
  });

  it('isValid is false when title is blank, detail is blank, no category, or no audience', () => {
    open();
    expect(component.isValid()).toBeFalse(); // title and detail blank

    component.title.set('   ');
    expect(component.isValid()).toBeFalse(); // whitespace doesn't count

    component.title.set('T');
    component.detail.set('D');
    expect(component.isValid()).toBeTrue();

    component.selectedAudienceKeys.set(new Set());
    expect(component.isValid()).toBeFalse();
  });

  it('toggleAudience() flips membership in the selection set', () => {
    open();
    expect(component.isSelected('all-pupils')).toBeFalse();
    component.toggleAudience('all-pupils');
    expect(component.isSelected('all-pupils')).toBeTrue();
    component.toggleAudience('all-pupils');
    expect(component.isSelected('all-pupils')).toBeFalse();
  });

  it('publish() in create mode posts the trimmed payload and emits saved on success', () => {
    open();
    component.title.set('  Hello  ');
    component.detail.set('  World  ');
    component.toggleAudience('all-pupils');
    const saved = jasmine.createSpy('saved');
    component.saved.subscribe(saved);

    component.publish();

    const payload = data.create.calls.mostRecent().args[0] as BulletinUpsertRequest;
    expect(payload.title).toBe('Hello');
    expect(payload.detail).toBe('World');
    expect(payload.categoryId).toBe('cat-1');
    expect(payload.expectedVersion).toBe(0);
    expect(payload.audiences.map(a => a.audienceKind).sort()).toEqual(
      [BulletinAudienceKind.AllStaff, BulletinAudienceKind.AllPupils].sort(),
    );
    // No attachments component instance in the overridden template → straight to finish.
    expect(component.submitting()).toBeFalse();
    expect(notify.success).toHaveBeenCalled();
    expect(saved).toHaveBeenCalled();
  });

  it('publish() in edit mode calls update with expectedVersion and preserves expiresAt', () => {
    const existing = makeDetail({ version: 9, expiresAt: '2026-12-31T00:00:00Z' });
    open(existing);
    component.title.set('Updated');
    component.detail.set('Updated body');

    component.publish();

    expect(data.update).toHaveBeenCalled();
    const [id, payload] = data.update.calls.mostRecent().args;
    expect(id).toBe(existing.id);
    expect(payload.expectedVersion).toBe(9);
    // Compare instants rather than literal strings: the form parses the ISO to
    // a Date and serializes back via toISOString(), which normalizes to .000Z.
    expect(new Date(payload.expiresAt!).getTime())
      .toBe(new Date('2026-12-31T00:00:00Z').getTime());
    expect(data.create).not.toHaveBeenCalled();
  });

  it('publish() sends the user-picked expiresAt in create mode', () => {
    open();
    component.title.set('With expiry');
    component.detail.set('Body');
    component.categoryId.set(categories[0].id);
    component.selectedAudienceKeys.set(new Set(['all-staff']));
    const pick = new Date('2027-06-15T12:00:00Z');
    component.expiresAt.set(pick);

    component.publish();

    expect(data.create).toHaveBeenCalled();
    const [payload] = data.create.calls.mostRecent().args;
    expect(new Date(payload.expiresAt!).getTime()).toBe(pick.getTime());
  });

  it('publish() guards against being invoked when invalid', () => {
    open();
    // No title/detail set, so isValid is false.
    component.publish();
    expect(data.create).not.toHaveBeenCalled();
    expect(component.submitting()).toBeFalse();
  });

  it('publish() guards against re-entrant submissions while one is in flight', () => {
    open();
    component.title.set('T'); component.detail.set('D');
    component.submitting.set(true);

    component.publish();

    expect(data.create).not.toHaveBeenCalled();
  });

  it('publish() shows an error toast and clears submitting when the server rejects', () => {
    data.create.and.returnValue(throwError(() => new Error('boom')));
    open();
    component.title.set('T'); component.detail.set('D');

    component.publish();

    expect(notify.error).toHaveBeenCalled();
    expect(component.submitting()).toBeFalse();
  });

  it('publish() with staged attachments fetches the new bulletin and uploads against its directoryId', async () => {
    open();
    component.title.set('T'); component.detail.set('D');

    const uploadStaged = jasmine.createSpy('uploadStaged').and.resolveTo(undefined);
    // The ViewChild is now a signal query; replace it with a function that
    // returns a fake instance, since the test template is empty and no real
    // child component exists. Class fields are writable at runtime despite the
    // TS `readonly` modifier.
    const fake = { hasStaged: () => true, uploadStaged };
    (component as unknown as { attachments: () => unknown }).attachments = () => fake;

    component.publish();

    // Microtask flush.
    await Promise.resolve();
    await Promise.resolve();

    expect(data.getById).toHaveBeenCalledWith('new-id');
    expect(uploadStaged).toHaveBeenCalledWith('new-id', 'new-dir');
  });

  it('publish() still finishes ok when getById fails after a successful create', () => {
    open();
    component.title.set('T'); component.detail.set('D');
    data.getById.and.returnValue(throwError(() => new Error('boom')));

    const fake = { hasStaged: () => true, uploadStaged: () => Promise.resolve() };
    (component as unknown as { attachments: () => unknown }).attachments = () => fake;

    component.publish();

    // Bulletin was published; failure to fetch the new id just skips the upload step.
    expect(notify.success).toHaveBeenCalled();
    expect(component.submitting()).toBeFalse();
  });

  it('onCancel/onHide emit closed', () => {
    open();
    const closed = jasmine.createSpy('closed');
    component.closed.subscribe(closed);
    component.onCancel();
    component.onHide();
    expect(closed).toHaveBeenCalledTimes(2);
  });
});
