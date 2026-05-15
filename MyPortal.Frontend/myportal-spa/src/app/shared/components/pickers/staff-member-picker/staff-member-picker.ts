import { ChangeDetectionStrategy, Component, inject, input, output, signal, viewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Button } from 'primeng/button';
import { Popover } from 'primeng/popover';
import { TableLazyLoadEvent, TableModule } from 'primeng/table';
import { TranslocoDirective, TranslocoPipe } from '@jsverse/transloco';

import { StaffMembersDataService } from '../../../services/staff-members-data.service';
import { StaffMemberSummaryResponse } from '../../../types/staff-member';
import { toQueryKitParams } from '../../../utils/primeng-querykit';

/**
 * Single-select staff-member browser. Trigger button opens a popover with a
 * lazy-loaded p-table backed by /api/people/staff, with column filters on
 * Code / FirstName / LastName. Returns the underlying Person.Id so the value
 * can be dropped straight into HeadTeacherId-style person FK fields.
 *
 * Translation keys live under `common.staffMemberPicker.*` in the root scope.
 */
@Component({
  selector: 'mp-staff-member-picker',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule, Button, Popover, TableModule, TranslocoDirective, TranslocoPipe],
  templateUrl: './staff-member-picker.html',
})
export class StaffMemberPicker {
  private readonly data = inject(StaffMembersDataService);

  /** Optional override for the trigger button label. Defaults to "Pick a staff member…". */
  readonly buttonLabel = input<string | undefined>(undefined);

  readonly picked = output<StaffMemberSummaryResponse>();

  private readonly op = viewChild<Popover>('op');

  protected readonly rows = signal<StaffMemberSummaryResponse[]>([]);
  protected readonly totalRecords = signal(0);
  protected readonly loading = signal(false);

  open(event: Event): void {
    this.op()?.toggle(event);
  }

  close(): void {
    this.op()?.hide();
  }

  load(event: TableLazyLoadEvent): void {
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
    this.close();
  }
}
