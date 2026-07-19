import { createSpyObj, type SpyObj } from '@testing/spy';
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

interface FormModel {
  title: string;
  detail: string;
  categoryId: string | null;
  isPinned: boolean;
  requiresAck: boolean;
  expiresAt: Date | null;
  audienceKeys: string[];
}
interface FieldLike {
  valid(): boolean;
  invalid(): boolean;
  submitting(): boolean;
  touched(): boolean;
}
interface Internals {
  model: { (): FormModel; set(v: FormModel): void; update(fn: (m: FormModel) => FormModel): void };
  f: (() => FieldLike) & { title: () => FieldLike };
}

describe('BulletinFormDialog', () => {
  let fixture: ComponentFixture<BulletinFormDialog>;
  let component: BulletinFormDialog;
  let internals: Internals;
  let data: SpyObj<BulletinsDataService>;
  let notify: SpyObj<NotificationService>;
  let me$: SpyObj<MeService>;
  let confirmDialog: SpyObj<ConfirmationDialog>;

  function setModel(patch: Partial<FormModel>) {
    internals.model.update(m => ({ ...m, ...patch }));
  }

  const categories: BulletinCategoryResponse[] = [
    { id: 'cat-1', name: 'Notices', icon: 'i', colourCode: '#000000', displayOrder: 1, active: true, isSystem: false, version: 1 },
    { id: 'cat-2', name: 'Sports',  icon: 'i', colourCode: '#000000', displayOrder: 2, active: true, isSystem: false, version: 1 },
  ];
  const allowedGroups: BulletinAllowedGroupResponse[] = [
    { studentGroupId: 'g-allowed', code: '7A', name: 'Year 7A' },
  ];

  beforeEach(async () => {
    data = createSpyObj<BulletinsDataService>(['listCategories', 'getSettings', 'create', 'update', 'getById']);
    notify = createSpyObj<NotificationService>(['success', 'error']);
    me$ = createSpyObj<MeService>(['me']);

    data.listCategories.mockReturnValue(of(categories));
    data.getSettings.mockReturnValue(of({ allowedAudienceGroups: allowedGroups }));
    data.create.mockReturnValue(of({ id: 'new-id' }));
    data.update.mockReturnValue(of(void 0));
    data.getById.mockReturnValue(of(makeDetail({ id: 'new-id', directoryId: 'new-dir' })));
    me$.me.mockReturnValue(of(makeMe()));

    confirmDialog = createSpyObj<ConfirmationDialog>(['confirm', 'danger']);
    confirmDialog.confirm.mockResolvedValue(true);
    confirmDialog.danger.mockResolvedValue(true);

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
    internals = component as unknown as Internals;
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
    me$.me.mockReturnValue(of(makeMe({ permissions: ['School.PinSchoolBulletins'] })));
    open();
    expect(component.canPin()).toBe(true);
  });

  it('create mode pre-selects All staff and defaults the category to the first option', () => {
    open(null);
    expect(internals.model().title).toBe('');
    expect(internals.model().detail).toBe('');
    expect(internals.model().isPinned).toBe(false);
    expect(internals.model().requiresAck).toBe(false);
    expect(component.isSelected('all-staff')).toBe(true);
    expect(internals.model().categoryId).toBe('cat-1');
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

    expect(component.isEdit()).toBe(true);
    expect(internals.model().title).toBe('Edit me');
    expect(internals.model().detail).toBe('Edit detail body');
    expect(internals.model().isPinned).toBe(true);
    expect(internals.model().requiresAck).toBe(true);
    expect(component.isSelected('all-pupils')).toBe(true);
    expect(component.isSelected('sg-g-x')).toBe(true);
  });

  it('audienceChoices surfaces groups from the existing bulletin even when not in the allowlist', () => {
    open(makeDetail({
      audiences: [
        { id: 'a1', audienceKind: BulletinAudienceKind.StudentGroup, studentGroupId: 'g-removed', studentGroupName: 'Old Group' },
      ],
    }));

    const keys = component.audienceChoices().map(c => c.key);
    expect(keys).toContain('sg-g-allowed');
    expect(keys).toContain('sg-g-removed');
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
    expect(internals.f().invalid()).toBe(true);

    setModel({ title: '   ' });
    expect(internals.f().invalid()).toBe(true);

    setModel({ title: 'T', detail: 'D' });
    expect(internals.f().valid()).toBe(true);

    setModel({ audienceKeys: [] });
    expect(internals.f().invalid()).toBe(true);
  });

  it('toggleAudience() flips membership in the selection set', () => {
    open();
    expect(component.isSelected('all-pupils')).toBe(false);
    component.toggleAudience('all-pupils');
    expect(component.isSelected('all-pupils')).toBe(true);
    component.toggleAudience('all-pupils');
    expect(component.isSelected('all-pupils')).toBe(false);
  });

  it('publish() in create mode posts the trimmed payload and emits saved on success', async () => {
    open();
    setModel({ title: '  Hello  ', detail: '  World  ' });
    component.toggleAudience('all-pupils');
    const saved = vi.fn();
    component.saved.subscribe(saved);

    await component.publish();

    const payload = data.create.mock.calls.at(-1)![0] as BulletinUpsertRequest;
    expect(payload.title).toBe('Hello');
    expect(payload.detail).toBe('World');
    expect(payload.categoryId).toBe('cat-1');
    expect(payload.expectedVersion).toBe(0);
    expect(payload.audiences.map(a => a.audienceKind).sort()).toEqual(
      [BulletinAudienceKind.AllStaff, BulletinAudienceKind.AllPupils].sort(),
    );
    expect(internals.f().submitting()).toBe(false);
    expect(notify.success).toHaveBeenCalled();
    expect(saved).toHaveBeenCalled();
  });

  it('publish() in edit mode calls update with expectedVersion and preserves expiresAt', async () => {
    const existing = makeDetail({ version: 9, expiresAt: '2026-12-31T00:00:00Z' });
    open(existing);
    setModel({ title: 'Updated', detail: 'Updated body' });

    await component.publish();

    expect(data.update).toHaveBeenCalled();
    const [id, payload] = data.update.mock.calls.at(-1)!;
    expect(id).toBe(existing.id);
    expect(payload.expectedVersion).toBe(9);
    expect(new Date(payload.expiresAt!).getTime())
      .toBe(new Date('2026-12-31T00:00:00Z').getTime());
    expect(data.create).not.toHaveBeenCalled();
  });

  it('publish() sends the user-picked expiresAt in create mode', async () => {
    open();
    const pick = new Date('2027-06-15T12:00:00Z');
    setModel({
      title: 'With expiry',
      detail: 'Body',
      categoryId: categories[0].id,
      audienceKeys: ['all-staff'],
      expiresAt: pick,
    });

    await component.publish();

    expect(data.create).toHaveBeenCalled();
    const [payload] = data.create.mock.calls.at(-1)!;
    expect(new Date(payload.expiresAt!).getTime()).toBe(pick.getTime());
  });

  it('publish() on an invalid form skips the API and marks fields touched to reveal errors', async () => {
    open();
    await component.publish();
    expect(data.create).not.toHaveBeenCalled();
    expect(internals.f().submitting()).toBe(false);
    expect(internals.f.title().touched()).toBe(true);
  });

  it('publish() shows an error toast and clears submitting when the server rejects', async () => {
    data.create.mockReturnValue(throwError(() => new Error('boom')));
    open();
    setModel({ title: 'T', detail: 'D' });

    await component.publish();

    expect(notify.error).toHaveBeenCalled();
    expect(internals.f().submitting()).toBe(false);
  });

  it('publish() with staged attachments fetches the new bulletin and uploads against its directoryId', async () => {
    open();
    setModel({ title: 'T', detail: 'D' });

    const uploadStaged = vi.fn().mockResolvedValue(undefined);
    const fake = { hasStaged: () => true, uploadStaged };
    (component as unknown as { attachments: () => unknown }).attachments = () => fake;

    await component.publish();

    expect(data.getById).toHaveBeenCalledWith('new-id');
    expect(uploadStaged).toHaveBeenCalledWith('new-id', 'new-dir');
  });

  it('publish() still finishes ok when getById fails after a successful create', async () => {
    open();
    setModel({ title: 'T', detail: 'D' });
    data.getById.mockReturnValue(throwError(() => new Error('boom')));

    const fake = { hasStaged: () => true, uploadStaged: () => Promise.resolve() };
    (component as unknown as { attachments: () => unknown }).attachments = () => fake;

    await component.publish();

    expect(notify.success).toHaveBeenCalled();
    expect(internals.f().submitting()).toBe(false);
  });

  it('onCancel/onHide emit closed', () => {
    open();
    const closed = vi.fn();
    component.closed.subscribe(closed);
    component.onCancel();
    component.onHide();
    expect(closed).toHaveBeenCalledTimes(2);
  });
});
