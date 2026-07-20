import { createSpyObj, type SpyObj } from '@testing/spy';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { TranslocoService } from '@jsverse/transloco';

import { BulletinDetailDialog } from './bulletin-detail-dialog';
import { BulletinsDataService } from '../../../services/bulletins-data.service';
import { ConfirmationDialog } from '../../../../core/services/confirmation.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { MeService } from '../../../../core/services/me-service';
import { UserType } from '../../../../core/types/user-type';
import { Me } from '../../../../core/types/me';
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
    username: 'me',
    userType: UserType.Staff,
    isEnabled: true,
    isSystem: false,
    displayName: 'Me',
    permissions: [],
    ...overrides,
  };
}

describe('BulletinDetailDialog', () => {
  let fixture: ComponentFixture<BulletinDetailDialog>;
  let component: BulletinDetailDialog;
  let data: SpyObj<BulletinsDataService>;
  let notify: SpyObj<NotificationService>;
  let confirm: SpyObj<ConfirmationDialog>;
  let me$: SpyObj<MeService>;

  function setBulletinId(value: string | null) {
    fixture.componentRef.setInput('bulletinId', value);
    fixture.detectChanges();
  }

  beforeEach(async () => {
    data = createSpyObj<BulletinsDataService>(['getById', 'acknowledge']);
    notify = createSpyObj<NotificationService>(['apiError', 'success']);
    confirm = createSpyObj<ConfirmationDialog>(['danger']);
    me$ = createSpyObj<MeService>(['me']);

    me$.me.mockReturnValue(of(makeMe()));
    data.getById.mockReturnValue(of(makeDetail()));
    data.acknowledge.mockReturnValue(of(void 0));

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

    fixture = TestBed.createComponent(BulletinDetailDialog);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('setting bulletinId triggers a fetch and opens the dialog', () => {
    setBulletinId('b1');
    expect(data.getById).toHaveBeenCalledWith('b1');
    expect(component.visible()).toBe(true);
    expect(component.bulletin()?.id).toBe('b1');
  });

  it('setting bulletinId to null clears the bulletin and closes the dialog', () => {
    setBulletinId('b1');
    setBulletinId(null);
    expect(component.bulletin()).toBeNull();
    expect(component.visible()).toBe(false);
  });

  it('clears the loading flag on fetch error', () => {
    data.getById.mockReturnValue(throwError(() => new Error('boom')));
    setBulletinId('b1');
    expect(component.loading()).toBe(false);
  });

  it('initials uses the first two name parts', () => {
    setBulletinId('b1');
    expect(component.initials()).toBe('AL');
  });

  it('initials handles single-word names', () => {
    data.getById.mockReturnValue(of(makeDetail({ createdByName: 'Cher' })));
    setBulletinId('b1');
    expect(component.initials()).toBe('C');
  });

  it('initials handles empty name', () => {
    data.getById.mockReturnValue(of(makeDetail({ createdByName: '' })));
    setBulletinId('b1');
    expect(component.initials()).toBe('');
  });

  it('audienceSummary maps known kinds and uses studentGroupName for groups', () => {
    const audiences: BulletinAudienceResponse[] = [
      { id: 'a1', audienceKind: BulletinAudienceKind.AllStaff, studentGroupId: null, studentGroupName: null },
      { id: 'a2', audienceKind: BulletinAudienceKind.AllPupils, studentGroupId: null, studentGroupName: null },
      { id: 'a3', audienceKind: BulletinAudienceKind.StudentGroup, studentGroupId: 'g1', studentGroupName: 'Year 7A' },
    ];
    data.getById.mockReturnValue(of(makeDetail({ audiences })));

    setBulletinId('b1');

    expect(component.audienceSummary()).toBe('bulletins.audience.allStaff, bulletins.audience.allPupils, Year 7A');
  });

  it('audienceSummary falls back to translated "group" when student-group has no name', () => {
    data.getById.mockReturnValue(of(makeDetail({
      audiences: [{ id: 'a1', audienceKind: BulletinAudienceKind.StudentGroup, studentGroupId: 'g1', studentGroupName: null }],
    })));

    setBulletinId('b1');

    expect(component.audienceSummary()).toBe('bulletins.audience.group');
  });

  type MePoke = { me: { set: (v: Me) => void } };

  it('canEdit is false for non-staff', () => {
    setBulletinId('b1');
    (component as unknown as MePoke).me.set(
      makeMe({ userType: UserType.Student, permissions: ['School.PinSchoolBulletins'] }),
    );
    expect(component.canEdit()).toBe(false);
  });

  it('canEdit is true for staff pinners regardless of authorship', () => {
    data.getById.mockReturnValue(of(makeDetail({ createdById: 'someone-else' })));
    setBulletinId('b1');
    (component as unknown as MePoke).me.set(
      makeMe({ id: 'me', permissions: ['School.PinSchoolBulletins'] }),
    );
    expect(component.canEdit()).toBe(true);
  });

  it('canEdit is true for staff editors who authored the bulletin', () => {
    data.getById.mockReturnValue(of(makeDetail({ createdById: 'me' })));
    setBulletinId('b1');
    (component as unknown as MePoke).me.set(
      makeMe({ id: 'me', permissions: ['School.EditSchoolBulletins'] }),
    );
    expect(component.canEdit()).toBe(true);
  });

  it('canEdit is false for staff editors viewing someone else\'s bulletin', () => {
    data.getById.mockReturnValue(of(makeDetail({ createdById: 'someone-else' })));
    setBulletinId('b1');
    (component as unknown as MePoke).me.set(
      makeMe({ id: 'me', permissions: ['School.EditSchoolBulletins'] }),
    );
    expect(component.canEdit()).toBe(false);
  });

  it('acknowledge() calls the API, flips local state, and emits acknowledged', () => {
    data.getById.mockReturnValue(of(makeDetail({
      requiresAcknowledgement: true, hasAcknowledged: false,
    })));
    const emitted = vi.fn();
    component.acknowledged.subscribe(emitted);

    setBulletinId('b1');
    component.acknowledge();

    expect(data.acknowledge).toHaveBeenCalledWith('b1');
    expect(component.bulletin()!.hasAcknowledged).toBe(true);
    expect(component.acknowledging()).toBe(false);
    expect(emitted).toHaveBeenCalled();
  });

  it('acknowledge() guards against re-entrant clicks while in flight', () => {
    setBulletinId('b1');
    component.acknowledging.set(true);

    component.acknowledge();

    expect(data.acknowledge).not.toHaveBeenCalled();
  });

  it('acknowledge() toasts on error and clears the in-flight flag', () => {
    setBulletinId('b1');
    data.acknowledge.mockReturnValue(throwError(() => new Error('boom')));

    component.acknowledge();

    expect(notify.apiError).toHaveBeenCalled();
    expect(component.acknowledging()).toBe(false);
  });

  it('requestEdit() emits the full bulletin', () => {
    const detail = makeDetail();
    data.getById.mockReturnValue(of(detail));
    const emitted = vi.fn();
    component.editRequested.subscribe(emitted);

    setBulletinId('b1');
    component.requestEdit();

    expect(emitted).toHaveBeenCalledWith(detail);
  });

  it('requestDelete() emits the id only when the confirm prompt resolves true', async () => {
    confirm.danger.mockResolvedValue(true);
    const emitted = vi.fn();
    component.deleteRequested.subscribe(emitted);
    setBulletinId('b1');

    await component.requestDelete();

    expect(emitted).toHaveBeenCalledWith('b1');
  });

  it('requestDelete() does not emit when the user cancels the prompt', async () => {
    confirm.danger.mockResolvedValue(false);
    const emitted = vi.fn();
    component.deleteRequested.subscribe(emitted);
    setBulletinId('b1');

    await component.requestDelete();

    expect(emitted).not.toHaveBeenCalled();
  });

  it('tint() appends 1A to 6-digit hex and returns 8-digit hex unchanged', () => {
    expect(component.tint('#6366F1')).toBe('#6366F11A');
    expect(component.tint('#6366F1FF')).toBe('#6366F1FF');
    expect(component.tint('')).toBe('');
  });

  it('onHide emits closed', () => {
    const emitted = vi.fn();
    component.closed.subscribe(emitted);
    component.onHide();
    expect(emitted).toHaveBeenCalled();
  });

  it('isExpired reflects the bulletin expiresAt vs. now', () => {
    expect(component.isExpired()).toBe(false);

    data.getById.mockReturnValue(of(makeDetail({
      expiresAt: new Date(Date.now() + 60_000).toISOString(),
    })));
    setBulletinId('future');
    expect(component.isExpired()).toBe(false);

    data.getById.mockReturnValue(of(makeDetail({
      expiresAt: new Date(Date.now() - 60_000).toISOString(),
    })));
    setBulletinId('past');
    expect(component.isExpired()).toBe(true);
  });
});
