import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { TranslocoService } from '@jsverse/transloco';

import { BulletinDetailDialog } from './bulletin-detail-dialog';
import { BulletinsDataService } from '../../../services/bulletins-data.service';
import { ConfirmationDialog } from '../../../services/confirmation.service';
import { NotificationService } from '../../../services/notification.service';
import { MeService } from '../../../../core/services/me-service';
import { UserType } from '../../../../core/enums/user-type';
import { Me } from '../../../../core/interfaces/me';
import {
  BulletinAudienceKind,
  BulletinAudienceResponse,
  BulletinDetailsResponse,
} from '../../../types/bulletin';

function makeDetail(overrides: Partial<BulletinDetailsResponse> = {}): BulletinDetailsResponse {
  return {
    id: 'b1',
    directoryId: 'dir',
    title: 'Hello',
    detail: 'World',
    categoryId: 'cat',
    categoryName: 'Cat',
    categoryIcon: 'pi pi-info',
    categoryColourCode: '#0066CC',
    createdById: 'author',
    createdByName: 'Ada Lovelace',
    createdByIpAddress: '::1',
    createdAt: '2026-05-01T12:00:00Z',
    lastModifiedById: 'author',
    lastModifiedByName: 'Ada Lovelace',
    lastModifiedByIpAddress: '::1',
    lastModifiedAt: '2026-05-01T12:00:00Z',
    requiresAcknowledgement: false,
    hasAcknowledged: null,
    acknowledgedCount: null,
    pinnedAt: null,
    expiresAt: null,
    audiences: [],
    attachmentCount: 0,
    version: 1,
    ...overrides,
  };
}

function makeMe(overrides: Partial<Me> = {}): Me {
  return {
    id: 'me',
    userName: 'me',
    userType: UserType.Staff,
    isEnabled: true,
    isSystem: false,
    displayName: 'Me',
    permissions: [],
    ...overrides,
  };
}

describe('BulletinDetailDialog', () => {
  let component: BulletinDetailDialog;
  let data: jasmine.SpyObj<BulletinsDataService>;
  let notify: jasmine.SpyObj<NotificationService>;
  let confirm: jasmine.SpyObj<ConfirmationDialog>;
  let me$: jasmine.SpyObj<MeService>;

  beforeEach(async () => {
    data = jasmine.createSpyObj<BulletinsDataService>('BulletinsDataService', ['getById', 'acknowledge']);
    notify = jasmine.createSpyObj<NotificationService>('NotificationService', ['apiError', 'success']);
    confirm = jasmine.createSpyObj<ConfirmationDialog>('ConfirmationDialog', ['danger']);
    me$ = jasmine.createSpyObj<MeService>('MeService', ['me']);

    me$.me.and.returnValue(of(makeMe()));
    data.getById.and.returnValue(of(makeDetail()));
    data.acknowledge.and.returnValue(of(void 0));

    const translocoStub = {
      translate: (key: string, params?: Record<string, unknown>) =>
        params ? `${key}:${JSON.stringify(params)}` : key,
      getActiveLang: () => 'en',
    } as Partial<TranslocoService> as TranslocoService;

    TestBed.overrideComponent(BulletinDetailDialog, { set: { template: '' } });

    await TestBed.configureTestingModule({
      imports: [BulletinDetailDialog],
      providers: [
        { provide: BulletinsDataService, useValue: data },
        { provide: NotificationService, useValue: notify },
        { provide: ConfirmationDialog, useValue: confirm },
        { provide: MeService, useValue: me$ },
        { provide: TranslocoService, useValue: translocoStub },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(BulletinDetailDialog);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  // ─── input binding / visibility ──────────────────────────────────────────

  it('setting bulletinId triggers a fetch and opens the dialog', () => {
    component.bulletinId = 'b1';
    expect(data.getById).toHaveBeenCalledWith('b1');
    expect(component.visible()).toBeTrue();
    expect(component.bulletin()?.id).toBe('b1');
  });

  it('setting bulletinId to null clears the bulletin and closes the dialog', () => {
    component.bulletinId = 'b1';
    component.bulletinId = null;
    expect(component.bulletin()).toBeNull();
    expect(component.visible()).toBeFalse();
  });

  it('clears the loading flag on fetch error', () => {
    data.getById.and.returnValue(throwError(() => new Error('boom')));
    component.bulletinId = 'b1';
    expect(component.loading()).toBeFalse();
  });

  // ─── initials ─────────────────────────────────────────────────────────────

  it('initials uses the first two name parts', () => {
    component.bulletinId = 'b1';
    expect(component.initials()).toBe('AL'); // "Ada Lovelace"
  });

  it('initials handles single-word names', () => {
    data.getById.and.returnValue(of(makeDetail({ createdByName: 'Cher' })));
    component.bulletinId = 'b1';
    expect(component.initials()).toBe('C');
  });

  it('initials handles empty name', () => {
    data.getById.and.returnValue(of(makeDetail({ createdByName: '' })));
    component.bulletinId = 'b1';
    expect(component.initials()).toBe('');
  });

  // ─── audienceSummary ─────────────────────────────────────────────────────

  it('audienceSummary maps known kinds and uses studentGroupName for groups', () => {
    const audiences: BulletinAudienceResponse[] = [
      { id: 'a1', audienceKind: BulletinAudienceKind.AllStaff, studentGroupId: null, studentGroupName: null },
      { id: 'a2', audienceKind: BulletinAudienceKind.AllPupils, studentGroupId: null, studentGroupName: null },
      { id: 'a3', audienceKind: BulletinAudienceKind.StudentGroup, studentGroupId: 'g1', studentGroupName: 'Year 7A' },
    ];
    data.getById.and.returnValue(of(makeDetail({ audiences })));

    component.bulletinId = 'b1';

    expect(component.audienceSummary()).toBe('bulletins.audience.allStaff, bulletins.audience.allPupils, Year 7A');
  });

  it('audienceSummary falls back to translated "group" when student-group has no name', () => {
    data.getById.and.returnValue(of(makeDetail({
      audiences: [{ id: 'a1', audienceKind: BulletinAudienceKind.StudentGroup, studentGroupId: 'g1', studentGroupName: null }],
    })));

    component.bulletinId = 'b1';

    expect(component.audienceSummary()).toBe('bulletins.audience.group');
  });

  // ─── canEdit ─────────────────────────────────────────────────────────────

  it('canEdit is false for non-staff', () => {
    me$.me.and.returnValue(of(makeMe({ userType: UserType.Student, permissions: ['School.PinSchoolBulletins'] })));
    TestBed.resetTestingModule();
    // Recreate with the new me$. (Easier than juggling Subject inside the test.)
    return (async () => {
      await TestBed.configureTestingModule({
        imports: [BulletinDetailDialog],
        providers: [
          { provide: BulletinsDataService, useValue: data },
          { provide: NotificationService, useValue: notify },
          { provide: ConfirmationDialog, useValue: confirm },
          { provide: MeService, useValue: me$ },
          { provide: TranslocoService, useValue: { translate: (k: string) => k, getActiveLang: () => 'en' } as Partial<TranslocoService> as TranslocoService },
        ],
      });
      TestBed.overrideComponent(BulletinDetailDialog, { set: { template: '' } }).compileComponents();
      const fixture = TestBed.createComponent(BulletinDetailDialog);
      const local = fixture.componentInstance;
      fixture.detectChanges();
      local.bulletinId = 'b1';
      expect(local.canEdit()).toBeFalse();
    })();
  });

  it('canEdit is true for staff pinners regardless of authorship', () => {
    me$.me.and.returnValue(of(makeMe({ id: 'me', permissions: ['School.PinSchoolBulletins'] })));
    data.getById.and.returnValue(of(makeDetail({ createdById: 'someone-else' })));

    // Re-run ngOnInit with the new me() result by re-setting bulletinId.
    component.bulletinId = 'b1';

    // The me() Observable was already subscribed in ngOnInit before this test re-stubbed it;
    // explicitly call the public surface by writing the signal — same effect for the computed.
    (component as unknown as { me: { set: (v: Me) => void } }).me.set(
      makeMe({ id: 'me', permissions: ['School.PinSchoolBulletins'] }),
    );

    expect(component.canEdit()).toBeTrue();
  });

  it('canEdit is true for staff editors who authored the bulletin', () => {
    data.getById.and.returnValue(of(makeDetail({ createdById: 'me' })));
    component.bulletinId = 'b1';
    (component as unknown as { me: { set: (v: Me) => void } }).me.set(
      makeMe({ id: 'me', permissions: ['School.EditSchoolBulletins'] }),
    );

    expect(component.canEdit()).toBeTrue();
  });

  it('canEdit is false for staff editors viewing someone else\'s bulletin', () => {
    data.getById.and.returnValue(of(makeDetail({ createdById: 'someone-else' })));
    component.bulletinId = 'b1';
    (component as unknown as { me: { set: (v: Me) => void } }).me.set(
      makeMe({ id: 'me', permissions: ['School.EditSchoolBulletins'] }),
    );

    expect(component.canEdit()).toBeFalse();
  });

  // ─── acknowledge ─────────────────────────────────────────────────────────

  it('acknowledge() calls the API, flips local state, and emits acknowledged', () => {
    data.getById.and.returnValue(of(makeDetail({
      requiresAcknowledgement: true, hasAcknowledged: false,
    })));
    const emitted = jasmine.createSpy('acknowledged');
    component.acknowledged.subscribe(emitted);

    component.bulletinId = 'b1';
    component.acknowledge();

    expect(data.acknowledge).toHaveBeenCalledWith('b1');
    expect(component.bulletin()!.hasAcknowledged).toBeTrue();
    expect(component.acknowledging()).toBeFalse();
    expect(emitted).toHaveBeenCalled();
  });

  it('acknowledge() guards against re-entrant clicks while in flight', () => {
    component.bulletinId = 'b1';
    component.acknowledging.set(true);

    component.acknowledge();

    expect(data.acknowledge).not.toHaveBeenCalled();
  });

  it('acknowledge() toasts on error and clears the in-flight flag', () => {
    component.bulletinId = 'b1';
    data.acknowledge.and.returnValue(throwError(() => new Error('boom')));

    component.acknowledge();

    expect(notify.apiError).toHaveBeenCalled();
    expect(component.acknowledging()).toBeFalse();
  });

  // ─── edit + delete ────────────────────────────────────────────────────────

  it('requestEdit() emits the full bulletin', () => {
    const detail = makeDetail();
    data.getById.and.returnValue(of(detail));
    const emitted = jasmine.createSpy('editRequested');
    component.editRequested.subscribe(emitted);

    component.bulletinId = 'b1';
    component.requestEdit();

    expect(emitted).toHaveBeenCalledWith(detail);
  });

  it('requestDelete() emits the id only when the confirm prompt resolves true', async () => {
    confirm.danger.and.resolveTo(true);
    const emitted = jasmine.createSpy('deleteRequested');
    component.deleteRequested.subscribe(emitted);
    component.bulletinId = 'b1';

    await component.requestDelete();

    expect(emitted).toHaveBeenCalledWith('b1');
  });

  it('requestDelete() does not emit when the user cancels the prompt', async () => {
    confirm.danger.and.resolveTo(false);
    const emitted = jasmine.createSpy('deleteRequested');
    component.deleteRequested.subscribe(emitted);
    component.bulletinId = 'b1';

    await component.requestDelete();

    expect(emitted).not.toHaveBeenCalled();
  });

  // ─── tint ────────────────────────────────────────────────────────────────

  it('tint() appends 1A to 6-digit hex and returns 8-digit hex unchanged', () => {
    expect(component.tint('#6366F1')).toBe('#6366F11A');
    expect(component.tint('#6366F1FF')).toBe('#6366F1FF');
    expect(component.tint('')).toBe('');
  });

  // ─── onHide ──────────────────────────────────────────────────────────────

  it('onHide emits closed', () => {
    const emitted = jasmine.createSpy('closed');
    component.closed.subscribe(emitted);
    component.onHide();
    expect(emitted).toHaveBeenCalled();
  });
});
