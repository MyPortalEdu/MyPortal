import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import {
  MpButton,
  MpCard,
  MpDatePicker,
  MpSkeleton,
  MpTable,
  MpTableBody,
  MpTableEmpty,
  MpTableHeader,
} from '@myportal/ui';
import { TranslocoDirective, TranslocoService, provideTranslocoScope } from '@jsverse/transloco';

import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { StaffMemberPicker } from '../../../../shared/components/pickers/staff-member-picker/staff-member-picker';
import { NotificationService } from '../../../../core/services/notification.service';
import { StaffReportsDataService } from '../../../../shared/services/staff-reports-data.service';
import { StaffMemberSummaryResponse } from '../../../../shared/types/staff-member';
import { StaffTrainingReportItem } from '../../../../shared/types/staff-reports';
import { exportToCsv, CsvColumn } from '../../../../shared/utils/csv-export';
import { fteLabel, dateLabel, toDateOnly } from '../../../../shared/utils/report-format';

function addYears(date: Date, years: number): Date {
  const next = new Date(date);
  next.setFullYear(next.getFullYear() + years);
  return next;
}

@Component({
  selector: 'mp-staff-training-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    FormsModule,
    RouterLink,
    MpButton,
    MpCard,
    MpDatePicker,
    MpSkeleton,
    MpTable,
    MpTableBody,
    MpTableEmpty,
    MpTableHeader,
    StaffMemberPicker,
    PageHeader,
    EmptyState,
    TranslocoDirective,
  ],
  providers: [provideTranslocoScope('staff-reports')],
  templateUrl: './staff-training-page.html',
})
export class StaffTrainingPage {
  private readonly data = inject(StaffReportsDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);

  protected readonly dateLabel = dateLabel;
  protected readonly fteLabel = fteLabel;

  protected readonly staff = signal<{ id: string; name: string } | null>(null);
  protected readonly startDate = signal<Date>(addYears(new Date(), -1));
  protected readonly endDate = signal<Date>(new Date());

  protected readonly loading = signal(false);
  protected readonly hasRun = signal(false);
  protected readonly rows = signal<StaffTrainingReportItem[]>([]);

  protected readonly count = computed(() => this.rows().length);

  protected onStaffPicked(row: StaffMemberSummaryResponse): void {
    const first = row.preferredFirstName?.trim() || row.firstName;
    const last = row.preferredLastName?.trim() || row.lastName;
    this.staff.set({ id: row.id, name: last ? `${last}, ${first}` : first });
  }

  protected clearStaff(): void {
    this.staff.set(null);
  }

  protected run(): void {
    this.loading.set(true);
    this.data
      .getStaffTraining(this.staff()?.id ?? null, toDateOnly(this.startDate()), toDateOnly(this.endDate()))
      .subscribe({
        next: items => {
          this.rows.set(items);
          this.hasRun.set(true);
          this.loading.set(false);
        },
        error: err => {
          this.loading.set(false);
          this.notify.apiError(err, this.transloco.translate('staff-reports.staffTraining.loadError'));
        },
      });
  }

  protected exportCsv(): void {
    const columns: CsvColumn<StaffTrainingReportItem>[] = [
      { header: this.col('code'), value: r => r.staffCode },
      { header: this.col('name'), value: r => r.staffName },
      { header: this.col('course'), value: r => r.course },
      { header: this.col('status'), value: r => r.status },
      { header: this.col('completed'), value: r => r.completedDate },
      { header: this.col('expiry'), value: r => r.expiryDate },
      { header: this.col('hours'), value: r => r.hours ?? '' },
      { header: this.col('provider'), value: r => r.provider },
    ];
    exportToCsv(
      `staff-training-${toDateOnly(this.startDate())}-to-${toDateOnly(this.endDate())}`,
      columns,
      this.rows(),
    );
  }

  protected print(): void {
    window.print();
  }

  private col(key: string): string {
    return this.transloco.translate(`staff-reports.staffTraining.columns.${key}`);
  }
}
