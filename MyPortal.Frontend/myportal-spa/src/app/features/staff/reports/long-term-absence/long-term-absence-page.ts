import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import {
  MpButton,
  MpCard,
  MpDatePicker,
  MpInputNumber,
  MpSkeleton,
  MpTable,
  MpTableBody,
  MpTableEmpty,
  MpTableHeader,
} from '@myportal/ui';
import { TranslocoDirective, TranslocoService, provideTranslocoScope } from '@jsverse/transloco';

import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { NotificationService } from '../../../../core/services/notification.service';
import { StaffReportsDataService } from '../../../../shared/services/staff-reports-data.service';
import { LongTermAbsenceReportItem } from '../../../../shared/types/staff-reports';
import { exportToCsv, CsvColumn } from '../../../../shared/utils/csv-export';
import { fteLabel, dateLabel, toDateOnly } from '../../../../shared/utils/report-format';

function addYears(date: Date, years: number): Date {
  const next = new Date(date);
  next.setFullYear(next.getFullYear() + years);
  return next;
}

@Component({
  selector: 'mp-long-term-absence-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    FormsModule,
    RouterLink,
    MpButton,
    MpCard,
    MpDatePicker,
    MpInputNumber,
    MpSkeleton,
    MpTable,
    MpTableBody,
    MpTableEmpty,
    MpTableHeader,
    PageHeader,
    EmptyState,
    TranslocoDirective,
  ],
  providers: [provideTranslocoScope('staff-reports')],
  templateUrl: './long-term-absence-page.html',
})
export class LongTermAbsencePage {
  private readonly data = inject(StaffReportsDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);

  protected readonly fteLabel = fteLabel;
  protected readonly dateLabel = dateLabel;

  protected readonly startDate = signal<Date>(addYears(new Date(), -3));
  protected readonly endDate = signal<Date>(new Date());
  protected readonly minDays = signal<number>(20);

  protected readonly loading = signal(false);
  protected readonly hasRun = signal(false);
  protected readonly rows = signal<LongTermAbsenceReportItem[]>([]);

  protected readonly count = computed(() => this.rows().length);
  protected readonly totalDays = computed(() =>
    this.rows().reduce((sum, r) => sum + (r.workingDaysLost ?? 0), 0),
  );

  protected run(): void {
    this.loading.set(true);
    this.data
      .getLongTermAbsence(toDateOnly(this.startDate()), toDateOnly(this.endDate()), this.minDays() ?? 20)
      .subscribe({
        next: items => {
          this.rows.set(items);
          this.hasRun.set(true);
          this.loading.set(false);
        },
        error: err => {
          this.loading.set(false);
          this.notify.apiError(err, this.transloco.translate('staff-reports.longTermAbsence.loadError'));
        },
      });
  }

  protected exportCsv(): void {
    const columns: CsvColumn<LongTermAbsenceReportItem>[] = [
      { header: this.col('code'), value: r => r.staffCode },
      { header: this.col('name'), value: r => r.staffName },
      { header: this.col('type'), value: r => r.absenceType },
      { header: this.col('start'), value: r => r.startDate },
      { header: this.col('end'), value: r => r.endDate },
      { header: this.col('daysLost'), value: r => r.workingDaysLost ?? '' },
    ];
    exportToCsv(
      `long-term-absence-${toDateOnly(this.startDate())}-to-${toDateOnly(this.endDate())}`,
      columns,
      this.rows(),
    );
  }

  protected print(): void {
    window.print();
  }

  private col(key: string): string {
    return this.transloco.translate(`staff-reports.longTermAbsence.columns.${key}`);
  }
}
