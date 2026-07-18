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
  let data: jasmine.SpyObj<BulletinsDataService>;
  let notify: jasmine.SpyObj<NotificationService>;
  let meService: jasmine.SpyObj<MeService>;

  beforeEach(async () => {
    data = jasmine.createSpyObj<BulletinsDataService>('BulletinsDataService',
      ['list', 'listCategories', 'delete']);
    notify = jasmine.createSpyObj<NotificationService>('NotificationService', ['success', 'apiError']);
    meService = jasmine.createSpyObj<MeService>('MeService', ['me', 'clearCache']);

    // Default refresh() inputs: empty page + empty categories.
    data.list.and.returnValue(of({ items: [], totalItems: 0 } as PageResult<BulletinSummaryResponse>));
    data.listCategories.and.returnValue(of([]));
    data.delete.and.returnValue(of(void 0));
    meService.me.and.returnValue(of(makeMe()));

    // Stub TranslocoService so .translate() returns the key, avoiding the JSON loader.
    const translocoStub = {
      translate: (key: string) => key,
      getActiveLang: () => 'en',
    } as Partial<TranslocoService> as TranslocoService;

    // Skip template compilation: we only care about the component's class logic
    // (signals + methods). The real template pulls in PrimeNG, transloco directives,
    // and two child dialog components — none of which add value to these tests.
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
    fixture.detectChanges(); // triggers ngOnInit → refresh()
  });

  it('refresh() loads bulletins and categories and clears the loading flag', () => {
    const items = [makeSummary({ id: 'a' }), makeSummary({ id: 'b' })];
    data.list.and.returnValue(of({ items, totalItems: 2 }));

    component.refresh();

    expect(component.loading()).toBeFalse();
    expect(component.bulletins().map(b => b.id)).toEqual(['a', 'b']);
  });

  it('refresh() clears the loading flag on error', () => {
    data.list.and.returnValue(throwError(() => new Error('boom')));

    component.refresh();

    expect(component.loading()).toBeFalse();
  });

  it('newCount counts only bulletins that require ack and have not been acknowledged', () => {
    data.list.and.returnValue(of({
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
    data.list.and.returnValue(of({
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
    data.list.and.returnValue(of({
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

  // `me` is private — poke it directly so we don't have to recreate the
  // fixture per variation. Server still enforces; this is just so non-editors
  // don't see a button that would 403.

  type MePoke = { me: { set: (v: Me) => void } };

  it('canPost is true for staff with EditSchoolBulletins', () => {
    (component as unknown as MePoke).me.set(
      makeMe({ userType: UserType.Staff, permissions: [Permissions.School.EditSchoolBulletins] }),
    );
    expect(component.canPost()).toBeTrue();
  });

  it('canPost is false for staff without EditSchoolBulletins', () => {
    (component as unknown as MePoke).me.set(
      makeMe({ userType: UserType.Staff, permissions: [Permissions.School.ViewSchoolBulletins] }),
    );
    expect(component.canPost()).toBeFalse();
  });

  it('canPost is false for non-staff even with EditSchoolBulletins', () => {
    (component as unknown as MePoke).me.set(
      makeMe({ userType: UserType.Student, permissions: [Permissions.School.EditSchoolBulletins] }),
    );
    expect(component.canPost()).toBeFalse();
  });

  it('openNew() clears any stale edit state before opening the form', () => {
    component.editingBulletin.set({ id: 'stale' } as BulletinDetailsResponse);
    component.openNew();
    expect(component.editingBulletin()).toBeNull();
    expect(component.formOpen()).toBeTrue();
  });

  it('closeForm() clears form state and editing state together', () => {
    component.editingBulletin.set({ id: 'b1' } as BulletinDetailsResponse);
    component.formOpen.set(true);

    component.closeForm();

    expect(component.formOpen()).toBeFalse();
    expect(component.editingBulletin()).toBeNull();
  });

  it('onEditRequested() closes the detail dialog before opening the form in edit mode', () => {
    component.detailId.set('b1');
    const bulletin = { id: 'b1', title: 'T' } as BulletinDetailsResponse;

    component.onEditRequested(bulletin);

    expect(component.detailId()).toBeNull();
    expect(component.editingBulletin()).toBe(bulletin);
    expect(component.formOpen()).toBeTrue();
  });

  it('onAcknowledged() flips hasAcknowledged for the currently-open bulletin in local state', () => {
    data.list.and.returnValue(of({
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
    expect(byId('a')!.hasAcknowledged).toBeFalse();
    expect(byId('b')!.hasAcknowledged).toBeTrue();
    expect(component.newCount()).toBe(1);
  });

  it('onAcknowledged() no-ops when no detail is open', () => {
    data.list.and.returnValue(of({
      items: [makeSummary({ id: 'a', requiresAcknowledgement: true, hasAcknowledged: false })],
      totalItems: 1,
    }));
    component.refresh();
    component.detailId.set(null);

    component.onAcknowledged();

    expect(component.bulletins()[0].hasAcknowledged).toBeFalse();
  });

  it('onDeleteRequested() deletes, closes detail, toasts, and refreshes', () => {
    component.detailId.set('b1');

    component.onDeleteRequested('b1');

    expect(data.delete).toHaveBeenCalledWith('b1');
    expect(component.detailId()).toBeNull();
    expect(notify.success).toHaveBeenCalled();
    // refresh() = one initial + one post-delete = two calls to list/listCategories.
    expect(data.list).toHaveBeenCalledTimes(2);
  });

  it('onDeleteRequested() surfaces an apiError toast on failure and leaves the detail dialog open', () => {
    data.delete.and.returnValue(throwError(() => new Error('boom')));
    component.detailId.set('b1');

    component.onDeleteRequested('b1');

    expect(notify.apiError).toHaveBeenCalled();
    expect(component.detailId()).toBe('b1');
    // No second refresh on error.
    expect(data.list).toHaveBeenCalledTimes(1);
  });

  it('tint() appends the alpha suffix only when the value is a 6-digit hex', () => {
    expect(component.tint('#6366F1')).toBe('#6366F122');
    expect(component.tint('#6366F1', '1A')).toBe('#6366F11A');
    // 8-digit hex (#RRGGBBAA) is returned as-is — backend validator allows it,
    // and appending another alpha would yield an invalid colour string.
    expect(component.tint('#6366F1FF')).toBe('#6366F1FF');
    expect(component.tint('')).toBe('');
  });

  it('isExpired() is true only when expiresAt has passed', () => {
    const past = new Date(Date.now() - 60_000).toISOString();
    const future = new Date(Date.now() + 60_000).toISOString();
    expect(component.isExpired(makeSummary({ expiresAt: null }))).toBeFalse();
    expect(component.isExpired(makeSummary({ expiresAt: future }))).toBeFalse();
    expect(component.isExpired(makeSummary({ expiresAt: past }))).toBeTrue();
  });
});
