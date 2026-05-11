import {
  ChangeDetectionStrategy,
  Component,
  EventEmitter,
  Input,
  OnInit,
  Output,
  computed,
  inject,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Dialog } from 'primeng/dialog';
import { Button } from 'primeng/button';

import { BulletinsDataService } from '../../../services/bulletins-data.service';
import { NotificationService } from '../../../services/notification.service';
import { MeService } from '../../../../core/services/me-service';
import { UserType } from '../../../../core/enums/user-type';
import { Me } from '../../../../core/interfaces/me';
import {
  BulletinAudienceKind,
  BulletinAudienceResponse,
  BulletinDetailsResponse,
} from '../../../types/bulletin';

@Component({
  selector: 'mp-bulletin-detail-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, RouterLink, Dialog, Button],
  templateUrl: './bulletin-detail-dialog.html',
  styleUrl: './bulletin-detail-dialog.scss',
})
export class BulletinDetailDialog implements OnInit {
  private readonly data = inject(BulletinsDataService);
  private readonly meService = inject(MeService);
  private readonly notify = inject(NotificationService);

  /** When set to a non-null id the dialog opens and fetches the bulletin. */
  @Input()
  set bulletinId(value: string | null) {
    this._bulletinId.set(value);
    if (value) {
      this.fetch(value);
    } else {
      this.bulletin.set(null);
    }
  }
  get bulletinId(): string | null { return this._bulletinId(); }

  @Output() readonly closed = new EventEmitter<void>();
  @Output() readonly acknowledged = new EventEmitter<void>();
  /** Emitted with the full bulletin so the parent can hand it to the form dialog without re-fetching. */
  @Output() readonly editRequested = new EventEmitter<BulletinDetailsResponse>();
  /** Emitted after the user confirms a delete. Parent owns the API call + feed refresh. */
  @Output() readonly deleteRequested = new EventEmitter<string>();

  private readonly _bulletinId = signal<string | null>(null);
  readonly bulletin = signal<BulletinDetailsResponse | null>(null);
  readonly loading = signal(false);
  readonly acknowledging = signal(false);
  private readonly me = signal<Me | null>(null);

  readonly visible = computed(() => this._bulletinId() !== null);

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
    if (perms.includes('School.PinSchoolBulletins')) return true;
    return perms.includes('School.EditSchoolBulletins') && b.createdById === me.id;
  });

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
        this.notify.apiError(err, "Couldn't acknowledge bulletin");
      },
    });
  }

  requestEdit(): void {
    const b = this.bulletin();
    if (b) this.editRequested.emit(b);
  }

  requestDelete(): void {
    const b = this.bulletin();
    if (!b) return;
    // Native confirm is the simplest destructive-action guard that doesn't
    // require provisioning PrimeNG's ConfirmationService. Swap in a styled
    // p-confirmdialog later if the UX bar moves up — the contract is the
    // same: only emit on positive confirmation.
    const ok = window.confirm(`Delete bulletin "${b.title}"? This can't be undone.`);
    if (ok) this.deleteRequested.emit(b.id);
  }

  formatDateTime(iso: string | null | undefined): string {
    if (!iso) return '';
    const d = new Date(iso);
    return d.toLocaleString(undefined, {
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

  private formatAudience(a: BulletinAudienceResponse): string {
    switch (a.audienceKind) {
      case BulletinAudienceKind.AllStaff:    return 'All staff';
      case BulletinAudienceKind.AllPupils:   return 'All pupils';
      case BulletinAudienceKind.AllParents:  return 'All parents';
      case BulletinAudienceKind.StudentGroup: return a.studentGroupName ?? 'Group';
      default: return '';
    }
  }
}
