import { createSpyObj, type SpyObj } from '@testing/spy';
import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { TranslocoService } from '@jsverse/transloco';

import { BulletinsFeed } from './bulletins-feed';
import { BulletinsDataService } from '../../../services/bulletins-data.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { MeService } from '../../../../core/services/me-service';
import { Permissions } from '../../../../core/constants/permissions';
import { UserType } from '../../../../core/types/user-type';
import { Me } from '../../../../core/types/me';
import {
  BulletinDetailsResponse,
  BulletinSummaryResponse,
  PageResult,
} from '../../../types/bulletin';

function makeSummary(overrides: Partial<BulletinSummaryResponse> = {}): BulletinSummaryResponse {
  return {
    id: 'b1',
    categoryId: 'cat',
    categoryName: 'Cat',
    categoryIcon: 'pi pi-info',
    categoryColourCode: '#0066CC',
    title: 'Title',
    detail: 'Detail',
    requiresAcknowledgement: false,
    hasAcknowledged: null,
    attachmentCount: 0,
    pinnedAt: null,
    expiresAt: null,
    createdAt: '2026-05-01T12:00:00Z',
    createdByName: 'Author',
    ...overrides,
  };
}

function makeMe(overrides: Partial<Me> = {}): Me {
  return {
    id: 'me-1',
    username: 'me',
    userType: UserType.Staff,
    isEnabled: true,
    isSystem: false,
    displayName: 'Me',
    permissions: [Permissions.School.EditSchoolBulletins],
    ...overrides,
  };
}

describe('BulletinsFeed', () => {
  let component: BulletinsFeed;
  let data: SpyObj<BulletinsDataService>;
  let notify: SpyObj<NotificationService>;
  let meService: SpyObj<MeService>;

  beforeEach(async () => {
    data = createSpyObj<BulletinsDataService>(['list', 'listCategories', 'delete']);
    notify = createSpyObj<NotificationService>(['success', 'apiError']);
    meService = createSpyObj<MeService>(['me', 'clearCache']);

    data.list.mockReturnValue(of({ items: [], totalItems: 0 } as PageResult<BulletinSummaryResponse>));
    data.listCategories.mockReturnValue(of([]));
    data.delete.mockReturnValue(of(void 0));
    meService.me.mockReturnValue(of(makeMe()));

    const translocoStub = {
      translate: (key: string) => key,
      getActiveLang: () => 'en',
    } as Partial<TranslocoService> as TranslocoService;

    TestBed.overrideComponent(BulletinsFeed, { set: { template: '' } });

    await TestBed.configureTestingModule({
      imports: [BulletinsFeed],
      providers: [
        { provide: BulletinsDataService, useValue: data },
        { provide: NotificationService, useValue: notify },
        { provide: MeService, useValue: meService },
        { provide: TranslocoService, useValue: translocoStub },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(BulletinsFeed);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('refresh() loads bulletins and categories and clears the loading flag', () => {
    const items = [makeSummary({ id: 'a' }), makeSummary({ id: 'b' })];
    data.list.mockReturnValue(of({ items, totalItems: 2 }));

    component.refresh();

    expect(component.loading()).toBe(false);
    expect(component.bulletins().map(b => b.id)).toEqual(['a', 'b']);
  });

  it('refresh() clears the loading flag on error', () => {
    data.list.mockReturnValue(throwError(() => new Error('boom')));

    component.refresh();

    expect(component.loading()).toBe(false);
  });

  it('newCount counts only bulletins that require ack and have not been acknowledged', () => {
    data.list.mockReturnValue(of({
      items: [
        makeSummary({ id: '1', requiresAcknowledgement: true,  hasAcknowledged: false }),
        makeSummary({ id: '2', requiresAcknowledgement: true,  hasAcknowledged: true  }),
        makeSummary({ id: '3', requiresAcknowledgement: false, hasAcknowledged: null  }),
        makeSummary({ id: '4', requiresAcknowledgement: true,  hasAcknowledged: false }),
      ],
      totalItems: 4,
    }));

    component.refresh();

    expect(component.newCount()).toBe(2);
  });

  it('orders pinned bulletins ahead of unpinned, then by createdAt descending', () => {
    data.list.mockReturnValue(of({
      items: [
        makeSummary({ id: 'old',     pinnedAt: null, createdAt: '2026-01-01T00:00:00Z' }),
        makeSummary({ id: 'pinned1', pinnedAt: '2026-05-01T00:00:00Z', createdAt: '2026-02-01T00:00:00Z' }),
        makeSummary({ id: 'new',     pinnedAt: null, createdAt: '2026-04-01T00:00:00Z' }),
        makeSummary({ id: 'pinned2', pinnedAt: '2026-05-10T00:00:00Z', createdAt: '2026-03-01T00:00:00Z' }),
      ],
      totalItems: 4,
    }));

    component.refresh();

    expect(component.orderedBulletins().map(b => b.id)).toEqual(['pinned2', 'pinned1', 'new', 'old']);
  });

  it('filteredBulletins returns all when no category selected, filters when one is', () => {
    data.list.mockReturnValue(of({
      items: [
        makeSummary({ id: 'a', categoryId: 'cat-1' }),
        makeSummary({ id: 'b', categoryId: 'cat-2' }),
        makeSummary({ id: 'c', categoryId: 'cat-1' }),
      ],
      totalItems: 3,
    }));

    component.refresh();

    expect(component.filteredBulletins().map(b => b.id)).toEqual(['a', 'b', 'c']);

    component.selectCategory('cat-1');
    expect(component.filteredBulletins().map(b => b.id)).toEqual(['a', 'c']);

    component.selectCategory(null);
    expect(component.filteredBulletins().length).toBe(3);
  });

  type MePoke = { me: { set: (v: Me) => void } };

  it('canPost is true for staff with EditSchoolBulletins', () => {
    (component as unknown as MePoke).me.set(
      makeMe({ userType: UserType.Staff, permissions: [Permissions.School.EditSchoolBulletins] }),
    );
    expect(component.canPost()).toBe(true);
  });

  it('canPost is false for staff without EditSchoolBulletins', () => {
    (component as unknown as MePoke).me.set(
      makeMe({ userType: UserType.Staff, permissions: [Permissions.School.ViewSchoolBulletins] }),
    );
    expect(component.canPost()).toBe(false);
  });

  it('canPost is false for non-staff even with EditSchoolBulletins', () => {
    (component as unknown as MePoke).me.set(
      makeMe({ userType: UserType.Student, permissions: [Permissions.School.EditSchoolBulletins] }),
    );
    expect(component.canPost()).toBe(false);
  });

  it('openNew() clears any stale edit state before opening the form', () => {
    component.editingBulletin.set({ id: 'stale' } as BulletinDetailsResponse);
    component.openNew();
    expect(component.editingBulletin()).toBeNull();
    expect(component.formOpen()).toBe(true);
  });

  it('closeForm() clears form state and editing state together', () => {
    component.editingBulletin.set({ id: 'b1' } as BulletinDetailsResponse);
    component.formOpen.set(true);

    component.closeForm();

    expect(component.formOpen()).toBe(false);
    expect(component.editingBulletin()).toBeNull();
  });

  it('onEditRequested() closes the detail dialog before opening the form in edit mode', () => {
    component.detailId.set('b1');
    const bulletin = { id: 'b1', title: 'T' } as BulletinDetailsResponse;

    component.onEditRequested(bulletin);

    expect(component.detailId()).toBeNull();
    expect(component.editingBulletin()).toBe(bulletin);
    expect(component.formOpen()).toBe(true);
  });

  it('onAcknowledged() flips hasAcknowledged for the currently-open bulletin in local state', () => {
    data.list.mockReturnValue(of({
      items: [
        makeSummary({ id: 'a', requiresAcknowledgement: true, hasAcknowledged: false }),
        makeSummary({ id: 'b', requiresAcknowledgement: true, hasAcknowledged: false }),
      ],
      totalItems: 2,
    }));
    component.refresh();
    component.detailId.set('b');

    component.onAcknowledged();

    const byId = (id: string) => component.bulletins().find(x => x.id === id);
    expect(byId('a')!.hasAcknowledged).toBe(false);
    expect(byId('b')!.hasAcknowledged).toBe(true);
    expect(component.newCount()).toBe(1);
  });

  it('onAcknowledged() no-ops when no detail is open', () => {
    data.list.mockReturnValue(of({
      items: [makeSummary({ id: 'a', requiresAcknowledgement: true, hasAcknowledged: false })],
      totalItems: 1,
    }));
    component.refresh();
    component.detailId.set(null);

    component.onAcknowledged();

    expect(component.bulletins()[0].hasAcknowledged).toBe(false);
  });

  it('onDeleteRequested() deletes, closes detail, toasts, and refreshes', () => {
    component.detailId.set('b1');

    component.onDeleteRequested('b1');

    expect(data.delete).toHaveBeenCalledWith('b1');
    expect(component.detailId()).toBeNull();
    expect(notify.success).toHaveBeenCalled();
    expect(data.list).toHaveBeenCalledTimes(2);
  });

  it('onDeleteRequested() surfaces an apiError toast on failure and leaves the detail dialog open', () => {
    data.delete.mockReturnValue(throwError(() => new Error('boom')));
    component.detailId.set('b1');

    component.onDeleteRequested('b1');

    expect(notify.apiError).toHaveBeenCalled();
    expect(component.detailId()).toBe('b1');
    expect(data.list).toHaveBeenCalledTimes(1);
  });

  it('tint() appends the alpha suffix only when the value is a 6-digit hex', () => {
    expect(component.tint('#6366F1')).toBe('#6366F122');
    expect(component.tint('#6366F1', '1A')).toBe('#6366F11A');
    expect(component.tint('#6366F1FF')).toBe('#6366F1FF');
    expect(component.tint('')).toBe('');
  });

  it('isExpired() is true only when expiresAt has passed', () => {
    const past = new Date(Date.now() - 60_000).toISOString();
    const future = new Date(Date.now() + 60_000).toISOString();
    expect(component.isExpired(makeSummary({ expiresAt: null }))).toBe(false);
    expect(component.isExpired(makeSummary({ expiresAt: future }))).toBe(false);
    expect(component.isExpired(makeSummary({ expiresAt: past }))).toBe(true);
  });
});
