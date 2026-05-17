import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  effect,
  inject,
  input,
  output,
  signal,
  untracked,
} from '@angular/core';
import { Dialog } from 'primeng/dialog';
import { Button } from 'primeng/button';
import { ProgressSpinner } from 'primeng/progressspinner';
import { TranslocoDirective, TranslocoService, provideTranslocoScope } from '@jsverse/transloco';

import { BulletinsDataService } from '../../../services/bulletins-data.service';
import { ConfirmationDialog } from '../../../../core/services/confirmation.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { MeService } from '../../../../core/services/me-service';
import { Permissions } from '../../../../core/constants/permissions';
import { BulletinAttachments } from '../bulletin-attachments/bulletin-attachments';
import { UserType } from '../../../../core/types/user-type';
import { Me } from '../../../../core/types/me';
import {
  BulletinAudienceKind,
  BulletinAudienceResponse,
  BulletinDetailsResponse,
} from '../../../types/bulletin';

@Component({
  selector: 'mp-bulletin-detail-dialog',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Dialog, Button, ProgressSpinner, TranslocoDirective, BulletinAttachments],
  providers: [provideTranslocoScope('bulletins')],
  templateUrl: './bulletin-detail-dialog.html',
})
export class BulletinDetailDialog implements OnInit {
  private readonly data = inject(BulletinsDataService);
  private readonly meService = inject(MeService);
  private readonly notify = inject(NotificationService);
  private readonly confirm = inject(ConfirmationDialog);
  private readonly transloco = inject(TranslocoService);

  /** When set to a non-null id the dialog opens and fetches the bulletin. */
  readonly bulletinId = input<string | null>(null);

  readonly closed = output<void>();
  readonly acknowledged = output<void>();
  /** Emitted with the full bulletin so the parent can hand it to the form dialog without re-fetching. */
  readonly editRequested = output<BulletinDetailsResponse>();
  /** Emitted after the user confirms a delete. Parent owns the API call + feed refresh. */
  readonly deleteRequested = output<string>();

  readonly bulletin = signal<BulletinDetailsResponse | null>(null);
  readonly loading = signal(false);
  readonly acknowledging = signal(false);
  private readonly me = signal<Me | null>(null);

  readonly visible = computed(() => this.bulletinId() !== null);

  // Mirrors the feed's helper: only staff creators ever see this dialog with an
  // expired bulletin (audience visibility is gated server-side). Expired hides
  // the Acknowledge action — there's no point acking a bulletin that's no
  // longer in the audience's feed.
  readonly isExpired = computed(() => {
    const exp = this.bulletin()?.expiresAt;
    return !!exp && new Date(exp).getTime() < Date.now();
  });

  readonly initials = computed(() => {
    const name = this.bulletin()?.createdByName ?? '';
    return name
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map(part => part[0]?.toUpperCase() ?? '')
      .join('');
  });

  readonly audienceSummary = computed(() => {
    const list = this.bulletin()?.audiences ?? [];
    return list.map(a => this.formatAudience(a)).join(', ');
  });

  /**
   * Mirror of BulletinAccessPolicy.CanEdit on the server: staff only; pinners
   * can edit any bulletin, otherwise the caller needs Edit + must be the author.
   * The server is still the authority — this just decides whether to render the
   * button so non-editors don't see an action that would 403.
   */
  readonly canEdit = computed(() => {
    const b = this.bulletin();
    const me = this.me();
    if (!b || !me || me.userType !== UserType.Staff) return false;
    const perms = me.permissions ?? [];
    if (perms.includes(Permissions.School.PinSchoolBulletins)) return true;
    return perms.includes(Permissions.School.EditSchoolBulletins) && b.createdById === me.id;
  });

  constructor() {
    // React to the parent's bulletinId binding. Reads of fetch state / bulletin
    // writes are wrapped in untracked so they don't form an effect loop.
    effect(() => {
      const id = this.bulletinId();
      untracked(() => {
        if (id) this.fetch(id);
        else this.bulletin.set(null);
      });
    });
  }

  ngOnInit(): void {
    this.meService.me().subscribe(me => this.me.set(me));
  }

  onHide(): void {
    // p-dialog calls onHide on backdrop click / X / Esc. We bubble it up so the
    // parent can clear its `detailId` signal — that's the source of truth, not
    // a local boolean.
    this.closed.emit();
  }

  acknowledge(): void {
    const b = this.bulletin();
    if (!b || this.acknowledging()) return;
    this.acknowledging.set(true);
    this.data.acknowledge(b.id).subscribe({
      next: () => {
        this.bulletin.update(curr => (curr ? { ...curr, hasAcknowledged: true } : curr));
        this.acknowledging.set(false);
        this.acknowledged.emit();
      },
      error: (err: unknown) => {
        this.acknowledging.set(false);
        this.notify.apiError(err, this.transloco.translate('bulletins.detail.errorAcknowledge'));
      },
    });
  }

  requestEdit(): void {
    const b = this.bulletin();
    if (b) this.editRequested.emit(b);
  }

  async requestDelete(): Promise<void> {
    const b = this.bulletin();
    if (!b) return;
    const ok = await this.confirm.danger({
      message: this.transloco.translate('bulletins.detail.deleteConfirm', { title: b.title }),
    });
    if (ok) this.deleteRequested.emit(b.id);
  }

  formatDateTime(iso: string | null | undefined): string {
    if (!iso) return '';
    const d = new Date(iso);
    return d.toLocaleString(this.transloco.getActiveLang(), {
      weekday: 'short',
      day: '2-digit',
      month: 'short',
      hour: '2-digit',
      minute: '2-digit',
    });
  }

  private fetch(id: string): void {
    this.loading.set(true);
    this.data.getById(id).subscribe({
      next: b => {
        this.bulletin.set(b);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  // Mirrors the feed's helper: append the alpha suffix only when the value is
  // a 6-digit hex (#RRGGBB). Backend validation allows 8-digit (#RRGGBBAA), and
  // unconditionally concatenating would produce an invalid colour string.
  tint(hex: string, alphaHex = '1A'): string {
    if (!hex) return '';
    return hex.length === 7 ? `${hex}${alphaHex}` : hex;
  }

  private formatAudience(a: BulletinAudienceResponse): string {
    const t = (key: string) => this.transloco.translate(`bulletins.audience.${key}`);
    switch (a.audienceKind) {
      case BulletinAudienceKind.AllStaff:     return t('allStaff');
      case BulletinAudienceKind.AllPupils:    return t('allPupils');
      case BulletinAudienceKind.AllParents:   return t('allParents');
      case BulletinAudienceKind.StudentGroup: return a.studentGroupName ?? t('group');
      default: return '';
    }
  }
}
