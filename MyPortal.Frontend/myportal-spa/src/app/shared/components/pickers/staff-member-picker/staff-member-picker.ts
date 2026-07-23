import { ChangeDetectionStrategy, Component, computed, inject, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  MpButton,
  MpDialog,
  MpTable,
  MpTableHeader,
  MpTableBody,
  MpTableEmpty,
  MpSortable,
  MpSortIcon,
  MpSelectableRow,
  MpColumnFilter,
  MpColumnSelectFilter,
  type MpFilterMetadata,
  type MpTableLazyLoadEvent,
} from '@myportal/ui';
import { TranslocoDirective, TranslocoPipe, TranslocoService } from '@jsverse/transloco';

import { StaffMembersDataService } from '../../../services/staff-members-data.service';
import { StaffMemberSummaryResponse } from '../../../types/staff-member';
import { StaffStatus } from '../../../types/staff-member-header';
import { toQueryKitParams } from '../../../utils/querykit';

@Component({
  selector: 'mp-staff-member-picker',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule, MpButton, MpDialog, MpTable, MpTableHeader, MpTableBody, MpTableEmpty, MpSortable, MpSortIcon, MpSelectableRow, MpColumnFilter, MpColumnSelectFilter, TranslocoDirective, TranslocoPipe],
  templateUrl: './staff-member-picker.html',
})
export class StaffMemberPicker {
  private readonly data = inject(StaffMembersDataService);
  private readonly transloco = inject(TranslocoService);

  readonly buttonLabel = input<string | undefined>(undefined);

  readonly picked = output<StaffMemberSummaryResponse>();

  protected readonly visible = signal(false);
  protected readonly rows = signal<StaffMemberSummaryResponse[]>([]);
  protected readonly totalRecords = signal(0);
  protected readonly loading = signal(false);

  // Open on current staff; the status filter widens to leavers/future/all when actually needed.
  protected readonly initialFilters: Record<string, MpFilterMetadata> = {
    status: { value: 'Active', matchMode: 'equals' },
  };

  protected readonly statusOptions = computed<{ label: string; value: StaffStatus | 'All' }[]>(() => [
    { label: this.transloco.translate('common.staffMemberPicker.status.all'), value: 'All' },
    { label: this.transloco.translate('common.staffMemberPicker.status.Active'), value: 'Active' },
    { label: this.transloco.translate('common.staffMemberPicker.status.Future'), value: 'Future' },
    { label: this.transloco.translate('common.staffMemberPicker.status.Leaver'), value: 'Leaver' },
    { label: this.transloco.translate('common.staffMemberPicker.status.None'), value: 'None' },
  ]);

  protected statusLabel(status: StaffStatus): string {
    return this.transloco.translate('common.staffMemberPicker.status.' + status);
  }

  load(event: MpTableLazyLoadEvent): void {
    this.loading.set(true);
    const params = toQueryKitParams(event);
    this.data.list(params).subscribe({
      next: page => {
        this.rows.set(page.items ?? []);
        this.totalRecords.set(page.totalItems ?? 0);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  onRowSelect(row: StaffMemberSummaryResponse): void {
    this.picked.emit(row);
    this.visible.set(false);
  }
}
