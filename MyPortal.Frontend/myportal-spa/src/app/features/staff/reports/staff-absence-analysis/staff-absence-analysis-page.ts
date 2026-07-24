import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import {
  MpButton,
  MpCard,
  MpDatePicker,
  MpSelect,
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
import { ReportOption, StaffAbsenceAnalysisReportItem } from '../../../../shared/types/staff-reports';
import { exportToCsv, CsvColumn } from '../../../../shared/utils/csv-export';
import { fteLabel, dateLabel, toDateOnly } from '../../../../shared/utils/report-format';

function addYears(date: Date, years: number): Date {
  const next = new Date(date);
  next.setFullYear(next.getFullYear() + years);
  return next;
}

@Component({
  selector: 'mp-staff-absence-analysis-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    FormsModule,
    RouterLink,
    MpButton,
    MpCard,
    MpDatePicker,
    MpSelect,
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
  templateUrl: './staff-absence-analysis-page.html',
})
export class StaffAbsenceAnalysisPage implements OnInit {
  private readonly data = inject(StaffReportsDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);

  protected readonly fteLabel = fteLabel;
  protected readonly dateLabel = dateLabel;

  protected readonly absenceTypeId = signal<string>('');
  protected readonly startDate = signal<Date>(addYears(new Date(), -1));
  protected readonly endDate = signal<Date>(new Date());

  protected readonly loading = signal(false);
  protected readonly hasRun = signal(false);
  protected readonly rows = signal<StaffAbsenceAnalysisReportItem[]>([]);
  protected readonly absenceTypes = signal<ReportOption[]>([]);

  protected readonly typeOptions = computed(() => [
    { id: '', name: this.transloco.translate('staff-reports.absenceType.all') },
    ...this.absenceTypes(),
  ]);

  protected readonly totalAbsences = computed(() =>
    this.rows().reduce((sum, r) => sum + r.absenceCount, 0),
  );
  protected readonly totalStaff = computed(() =>
    this.rows().reduce((sum, r) => sum + r.staffCount, 0),
  );
  protected readonly totalDays = computed(() =>
    this.rows().reduce((sum, r) => sum + (r.totalWorkingDaysLost ?? 0), 0),
  );

  ngOnInit(): void {
    this.data.getAbsenceTypes().subscribe({ next: t => this.absenceTypes.set(t), error: () => {} });
  }

  protected run(): void {
    this.loading.set(true);
    this.data
      .getStaffAbsenceAnalysis(
        this.absenceTypeId() || null,
        toDateOnly(this.startDate()),
        toDateOnly(this.endDate()),
      )
      .subscribe({
        next: items => {
          this.rows.set(items);
          this.hasRun.set(true);
          this.loading.set(false);
        },
        error: err => {
          this.loading.set(false);
          this.notify.apiError(err, this.transloco.translate('staff-reports.staffAbsenceAnalysis.loadError'));
        },
      });
  }

  protected exportCsv(): void {
    const columns: CsvColumn<StaffAbsenceAnalysisReportItem>[] = [
      { header: this.col('serviceTerm'), value: r => r.serviceTerm },
      { header: this.col('absences'), value: r => r.absenceCount },
      { header: this.col('staff'), value: r => r.staffCount },
      { header: this.col('daysLost'), value: r => r.totalWorkingDaysLost },
    ];
    exportToCsv(
      `staff-absence-analysis-${toDateOnly(this.startDate())}-to-${toDateOnly(this.endDate())}`,
      columns,
      this.rows(),
    );
  }

  protected print(): void {
    window.print();
  }

  private col(key: string): string {
    return this.transloco.translate(`staff-reports.staffAbsenceAnalysis.columns.${key}`);
  }
}
