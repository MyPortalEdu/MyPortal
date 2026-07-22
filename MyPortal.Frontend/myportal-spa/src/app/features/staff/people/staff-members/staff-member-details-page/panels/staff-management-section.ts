import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  input,
  signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import {
  TranslocoDirective,
  TranslocoService,
  provideTranslocoScope,
} from '@jsverse/transloco';
import { DatePipe } from '@angular/common';
import { MpSelect, MpSpinner } from '@myportal/ui';

import { NotificationService } from '../../../../../../core/services/notification.service';
import { StaffMembersDataService } from '../../../../../../shared/services/staff-members-data.service';
import { SectionHeader } from '../../../../../../shared/components/section-header/section-header';
import { StaffManagementResponse } from '../../../../../../shared/types/staff-management';

@Component({
  selector: 'mp-staff-management-section',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DatePipe, FormsModule, RouterLink, MpSelect, MpSpinner, SectionHeader, TranslocoDirective],
  providers: [provideTranslocoScope('staff-members')],
  templateUrl: './staff-management-section.html',
})
export class StaffManagementSection {
  private readonly data = inject(StaffMembersDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);

  readonly staffMemberId = input.required<string>();
  readonly editing = input(false);

  protected readonly loading = signal(false);
  protected readonly management = signal<StaffManagementResponse | null>(null);

  // The persisted manager vs the edit buffer — the picker mutates `pendingManagerId`; the change
  // is only pushed to the server when the page's Save calls commit().
  private readonly loadedManagerId = signal<string | null>(null);
  readonly pendingManagerId = signal<string | null>(null);
  readonly dirty = computed(() => this.pendingManagerId() !== this.loadedManagerId());

  protected readonly canEdit = computed(() => this.management()?.canEdit ?? false);
  protected readonly managerOptions = computed(() => this.management()?.managerOptions ?? []);
  protected readonly directReports = computed(() => this.management()?.directReports ?? []);
  protected readonly history = computed(() => this.management()?.history ?? []);

  // Reload when the routed staff member changes — the page reuses this component across
  // staff-member ids (e.g. clicking a direct report), so a one-shot ngOnInit fetch would go stale.
  constructor() {
    effect(() => {
      const id = this.staffMemberId();
      this.load(id, true);
    });
  }

  private load(staffMemberId: string, resetView: boolean): void {
    this.loading.set(true);
    if (resetView) {
      this.management.set(null);
    }
    this.data.getManagement(staffMemberId).subscribe({
      next: row => {
        this.management.set(row);
        this.loadedManagerId.set(row.lineManagerId ?? null);
        this.pendingManagerId.set(row.lineManagerId ?? null);
        this.loading.set(false);
      },
      error: err => {
        this.loading.set(false);
        this.notify.apiError(err, this.transloco.translate('staff-members.management.loadError'));
      },
    });
  }

  /** Revert the buffered selection — called by the page when the user cancels the edit. */
  reset(): void {
    this.pendingManagerId.set(this.loadedManagerId());
  }

  /** Persist the buffered selection if it changed — called by the page's Save. Throws on failure
   * so the page's save handler surfaces the error and keeps the form in edit mode. */
  async commit(): Promise<void> {
    if (!this.dirty()) {
      return;
    }

    await firstValueFrom(
      this.data.setLineManager(this.staffMemberId(), { lineManagerId: this.pendingManagerId() }),
    );
    this.load(this.staffMemberId(), false);
  }

  protected reportLink(staffMemberId: string): string[] {
    return ['/staff/people/staff-members', staffMemberId];
  }
}
