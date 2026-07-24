import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
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
import { SalaryInformationReportItem, StaffTypeFilter } from '../../../../shared/types/staff-reports';
import { exportToCsv, CsvColumn } from '../../../../shared/utils/csv-export';
import { money, fteLabel, dateLabel, toDateOnly } from '../../../../shared/utils/report-format';

const STAFF_TYPES: StaffTypeFilter[] = ['All', 'Teaching', 'Support'];

@Component({
  selector: 'mp-salary-information-page',
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
  templateUrl: './salary-information-page.html',
})
export class SalaryInformationPage {
  private readonly data = inject(StaffReportsDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);

  protected readonly money = money;
  protected readonly fteLabel = fteLabel;
  protected readonly dateLabel = dateLabel;

  protected readonly staffType = signal<StaffTypeFilter>('All');
  protected readonly effectiveDate = signal<Date>(new Date());

  protected readonly loading = signal(false);
  protected readonly hasRun = signal(false);
  protected readonly rows = signal<SalaryInformationReportItem[]>([]);

  protected readonly staffTypeOptions = computed(() =>
    STAFF_TYPES.map(value => ({
      value,
      label: this.transloco.translate(`staff-reports.staffType.${value}`),
    })),
  );

  // Summary strip: the numbers a payroll snapshot is actually read for.
  protected readonly headcount = computed(() => this.rows().length);
  protected readonly paybill = computed(() =>
    this.rows().reduce((sum, r) => sum + (r.actualSalary ?? 0), 0),
  );
  protected readonly totalFte = computed(() =>
    this.rows().reduce((sum, r) => sum + (r.fte ?? 0), 0),
  );

  protected run(): void {
    this.loading.set(true);
    this.data.getSalaryInformation(this.staffType(), toDateOnly(this.effectiveDate())).subscribe({
      next: items => {
        this.rows.set(items);
        this.hasRun.set(true);
        this.loading.set(false);
      },
      error: err => {
        this.loading.set(false);
        this.notify.apiError(err, this.transloco.translate('staff-reports.salaryInformation.loadError'));
      },
    });
  }

  protected exportCsv(): void {
    const columns: CsvColumn<SalaryInformationReportItem>[] = [
      { header: this.col('code'), value: r => r.staffCode },
      { header: this.col('name'), value: r => r.staffName },
      { header: this.col('serviceTerm'), value: r => r.serviceTerm },
      { header: this.col('post'), value: r => r.postTitle },
      { header: this.col('scale'), value: r => r.payScale },
      { header: this.col('point'), value: r => r.payPoint },
      { header: this.col('fte'), value: r => r.fte },
      { header: this.col('fteSalary'), value: r => r.fullTimeSalary ?? '' },
      { header: this.col('actualSalary'), value: r => r.actualSalary ?? '' },
      { header: this.col('pension'), value: r => r.pensionScheme },
      { header: this.col('start'), value: r => r.contractStartDate },
    ];
    const stamp = toDateOnly(this.effectiveDate());
    exportToCsv(`salary-information-${this.staffType().toLowerCase()}-${stamp}`, columns, this.rows());
  }

  protected print(): void {
    window.print();
  }

  private col(key: string): string {
    return this.transloco.translate(`staff-reports.salaryInformation.columns.${key}`);
  }
}
