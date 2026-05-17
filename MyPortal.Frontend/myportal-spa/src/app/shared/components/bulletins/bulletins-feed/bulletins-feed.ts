import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { Button } from 'primeng/button';
import { Skeleton } from 'primeng/skeleton';
import { TranslocoDirective, TranslocoService, provideTranslocoScope } from '@jsverse/transloco';
import { forkJoin } from 'rxjs';

import { BulletinsDataService } from '../../../services/bulletins-data.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { MeService } from '../../../../core/services/me-service';
import { Permissions } from '../../../../core/constants/permissions';
import { UserType } from '../../../../core/types/user-type';
import { Me } from '../../../../core/types/me';
import {
  BulletinCategoryResponse,
  BulletinDetailsResponse,
  BulletinSummaryResponse,
} from '../../../types/bulletin';
import { BulletinDetailDialog } from '../bulletin-detail-dialog/bulletin-detail-dialog';
import { BulletinFormDialog } from '../bulletin-form-dialog/bulletin-form-dialog';

@Component({
  selector: 'mp-bulletins-feed',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Button, Skeleton, BulletinDetailDialog, BulletinFormDialog, TranslocoDirective],
  // The bulletins scope lazy-loads public/i18n/bulletins/<lang>.json the first
  // time this component (or any other consumer) renders.
  providers: [provideTranslocoScope('bulletins')],
  templateUrl: './bulletins-feed.html',
  // display:block on the host so parent flex containers (e.g. <home> with
  // flex-1 on this element) can size it. Custom elements default to inline.
  host: { class: 'block' },
})
export class BulletinsFeed implements OnInit {
  private readonly data = inject(BulletinsDataService);
  private readonly notify = inject(NotificationService);
  private readonly meService = inject(MeService);
  private readonly transloco = inject(TranslocoService);

  readonly loading = signal(true);
  readonly bulletins = signal<BulletinSummaryResponse[]>([]);
  readonly categories = signal<BulletinCategoryResponse[]>([]);
  readonly selectedCategoryId = signal<string | null>(null);
  // Permission gate for the "+ New" button. The form / endpoint still enforce
  // server-side; hiding the button just keeps non-editors from seeing an
  // action that would 403.
  private readonly me = signal<Me | null>(null);
  readonly canPost = computed(() => {
    const me = this.me();
    if (!me || me.userType !== UserType.Staff) return false;
    return !!me.permissions?.includes(Permissions.School.EditSchoolBulletins);
  });
  readonly detailId = signal<string | null>(null);
  // Form-dialog state. `formOpen` controls visibility; `editingBulletin` switches
  // the dialog between create mode (null) and edit mode (the full bulletin to
  // hydrate fields from). Always clear `editingBulletin` when closing so a stale
  // edit doesn't leak into the next "+ New" click.
  readonly formOpen = signal(false);
  readonly editingBulletin = signal<BulletinDetailsResponse | null>(null);

  // "{n} new" in the header counts bulletins that require acknowledgement and that
  // the current caller hasn't acted on yet — those are the items the user is being
  // asked to read. Plain backlog counts felt noisy.
  readonly newCount = computed(() =>
    this.bulletins().filter(b => b.requiresAcknowledgement && !b.hasAcknowledged).length,
  );

  // Pinned-first, then most-recent-first. The API has no opinion on default sort;
  // we apply it client-side so this widget renders the feed the way the mockup shows.
  readonly orderedBulletins = computed(() => {
    const list = this.bulletins().slice();
    list.sort((a, b) => {
      const ap = a.pinnedAt ? new Date(a.pinnedAt).getTime() : 0;
      const bp = b.pinnedAt ? new Date(b.pinnedAt).getTime() : 0;
      if (ap !== bp) return bp - ap;
      const ac = a.createdAt ? new Date(a.createdAt).getTime() : 0;
      const bc = b.createdAt ? new Date(b.createdAt).getTime() : 0;
      return bc - ac;
    });
    return list;
  });

  readonly filteredBulletins = computed(() => {
    const cat = this.selectedCategoryId();
    if (!cat) return this.orderedBulletins();
    return this.orderedBulletins().filter(b => b.categoryId === cat);
  });

  ngOnInit(): void {
    this.refresh();
    this.meService.me().subscribe(me => this.me.set(me));
  }

  refresh(): void {
    this.loading.set(true);
    forkJoin({
      page: this.data.list(1, 25),
      categories: this.data.listCategories(false),
    }).subscribe({
      next: ({ page, categories }) => {
        this.bulletins.set(page.items ?? []);
        this.categories.set(categories ?? []);
        this.loading.set(false);
      },
      error: err => {
        this.loading.set(false);
        this.notify.apiError(err, this.transloco.translate('bulletins.toasts.errorLoad'));
      },
    });
  }

  selectCategory(id: string | null): void {
    this.selectedCategoryId.set(id);
  }

  openDetail(id: string): void {
    this.detailId.set(id);
  }

  closeDetail(): void {
    this.detailId.set(null);
  }

  openNew(): void {
    // Defensive: clear any stale edit state before opening in create mode.
    this.editingBulletin.set(null);
    this.formOpen.set(true);
  }

  closeForm(): void {
    this.formOpen.set(false);
    this.editingBulletin.set(null);
  }

  onSaved(): void {
    this.closeForm();
    this.refresh();
  }

  onEditRequested(bulletin: BulletinDetailsResponse): void {
    // Close the detail dialog first so we don't stack two modals.
    this.closeDetail();
    this.editingBulletin.set(bulletin);
    this.formOpen.set(true);
  }

  onDeleteRequested(id: string): void {
    this.data.delete(id).subscribe({
      next: () => {
        this.closeDetail();
        this.notify.success(this.transloco.translate('bulletins.toasts.deleted'));
        this.refresh();
      },
      error: err => this.notify.apiError(err, this.transloco.translate('bulletins.toasts.errorDelete')),
    });
  }

  onAcknowledged(): void {
    // Mark the locally cached row as acknowledged so the "{n} new" badge updates
    // without a round-trip. The detail dialog re-fetches its own state separately.
    const id = this.detailId();
    if (!id) return;
    this.bulletins.update(list =>
      list.map(b => (b.id === id ? { ...b, hasAcknowledged: true } : b)),
    );
  }

  // Lightweight time-ago. The numeric thresholds are locale-neutral; only the
  // resulting label is translated. For the >7-day fallback we hand off to the
  // platform's locale-aware date formatter.
  timeAgo(iso?: string | null): string {
    if (!iso) return '';
    const then = new Date(iso).getTime();
    const diffSec = Math.max(0, Math.floor((Date.now() - then) / 1000));
    const t = (key: string, params?: Record<string, unknown>) =>
      this.transloco.translate(`bulletins.timeAgo.${key}`, params);
    if (diffSec < 60) return t('justNow');
    const diffMin = Math.floor(diffSec / 60);
    if (diffMin < 60) return t('minutes', { value: diffMin });
    const diffHr = Math.floor(diffMin / 60);
    if (diffHr < 24) return t('hours', { value: diffHr });
    const diffDay = Math.floor(diffHr / 24);
    if (diffDay === 1) return t('yesterday');
    if (diffDay < 7) return t('days', { value: diffDay });
    const date = new Date(iso);
    return date.toLocaleDateString(this.transloco.getActiveLang(), { day: '2-digit', month: 'short' });
  }

  // PrimeNG hex colours don't have built-in alpha helpers; we just append an opacity
  // suffix for the tinted icon-square background.
  tint(hex: string, alphaHex = '22'): string {
    if (!hex) return '';
    return hex.length === 7 ? `${hex}${alphaHex}` : hex;
  }

  // True when the bulletin's expiresAt is in the past. Only ever seen by users
  // who can still view post-expiry bulletins (staff creators); for everyone
  // else the server filters expired rows out of the feed entirely.
  isExpired(b: BulletinSummaryResponse): boolean {
    return !!b.expiresAt && new Date(b.expiresAt).getTime() < Date.now();
  }
}
