import { ChangeDetectionStrategy, Component, inject, input, output, signal } from '@angular/core';
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
  type MpTableLazyLoadEvent,
} from '@myportal/ui';
import { TranslocoDirective, TranslocoPipe } from '@jsverse/transloco';

import { StaffMembersDataService } from '../../../services/staff-members-data.service';
import { StaffMemberSummaryResponse } from '../../../types/staff-member';
import { toQueryKitParams } from '../../../utils/querykit';

@Component({
  selector: 'mp-staff-member-picker',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule, MpButton, MpDialog, MpTable, MpTableHeader, MpTableBody, MpTableEmpty, MpSortable, MpSortIcon, MpSelectableRow, MpColumnFilter, TranslocoDirective, TranslocoPipe],
  templateUrl: './staff-member-picker.html',
})
export class StaffMemberPicker {
  private readonly data = inject(StaffMembersDataService);

  readonly buttonLabel = input<string | undefined>(undefined);

  readonly picked = output<StaffMemberSummaryResponse>();

  protected readonly visible = signal(false);
  protected readonly rows = signal<StaffMemberSummaryResponse[]>([]);
  protected readonly totalRecords = signal(0);
  protected readonly loading = signal(false);

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
