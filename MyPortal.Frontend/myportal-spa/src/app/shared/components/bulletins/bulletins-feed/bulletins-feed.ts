import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { MpButton, MpSkeleton } from '@myportal/ui';
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
  imports: [MpButton, MpSkeleton, BulletinDetailDialog, BulletinFormDialog, TranslocoDirective],
  providers: [provideTranslocoScope('bulletins')],
  templateUrl: './bulletins-feed.html',
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
  private readonly me = signal<Me | null>(null);
  readonly canPost = computed(() => {
    const me = this.me();
    if (!me || me.userType !== UserType.Staff) return false;
    return !!me.permissions?.includes(Permissions.School.EditSchoolBulletins);
  });
  readonly detailId = signal<string | null>(null);
  readonly formOpen = signal(false);
  readonly editingBulletin = signal<BulletinDetailsResponse | null>(null);

  readonly newCount = computed(() =>
    this.bulletins().filter(b => b.requiresAcknowledgement && !b.hasAcknowledged).length,
  );

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
    const id = this.detailId();
    if (!id) return;
    this.bulletins.update(list =>
      list.map(b => (b.id === id ? { ...b, hasAcknowledged: true } : b)),
    );
  }

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

  tint(hex: string, alphaHex = '22'): string {
    if (!hex) return '';
    return hex.length === 7 ? `${hex}${alphaHex}` : hex;
  }

  isExpired(b: BulletinSummaryResponse): boolean {
    return !!b.expiresAt && new Date(b.expiresAt).getTime() < Date.now();
  }
}
