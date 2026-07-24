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
import { ContractAnalysisReportItem, StaffTypeFilter } from '../../../../shared/types/staff-reports';
import { exportToCsv, CsvColumn } from '../../../../shared/utils/csv-export';
import { fteLabel, dateLabel, toDateOnly } from '../../../../shared/utils/report-format';

const STAFF_TYPES: StaffTypeFilter[] = ['All', 'Teaching', 'Support'];

@Component({
  selector: 'mp-contract-analysis-page',
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
  templateUrl: './contract-analysis-page.html',
})
export class ContractAnalysisPage {
  private readonly data = inject(StaffReportsDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);

  protected readonly fteLabel = fteLabel;
  protected readonly dateLabel = dateLabel;

  protected readonly staffType = signal<StaffTypeFilter>('All');
  protected readonly effectiveDate = signal<Date>(new Date());

  protected readonly loading = signal(false);
  protected readonly hasRun = signal(false);
  protected readonly rows = signal<ContractAnalysisReportItem[]>([]);

  protected readonly staffTypeOptions = computed(() =>
    STAFF_TYPES.map(value => ({
      value,
      label: this.transloco.translate(`staff-reports.staffType.${value}`),
    })),
  );

  protected readonly serviceTermCount = computed(() => this.rows().length);
  protected readonly totalContracts = computed(() =>
    this.rows().reduce((sum, r) => sum + r.contractCount, 0),
  );
  protected readonly totalFte = computed(() =>
    this.rows().reduce((sum, r) => sum + (r.totalFte ?? 0), 0),
  );

  protected run(): void {
    this.loading.set(true);
    this.data.getContractAnalysis(this.staffType(), toDateOnly(this.effectiveDate())).subscribe({
      next: items => {
        this.rows.set(items);
        this.hasRun.set(true);
        this.loading.set(false);
      },
      error: err => {
        this.loading.set(false);
        this.notify.apiError(err, this.transloco.translate('staff-reports.contractAnalysis.loadError'));
      },
    });
  }

  protected exportCsv(): void {
    const columns: CsvColumn<ContractAnalysisReportItem>[] = [
      { header: this.col('serviceTerm'), value: r => r.serviceTerm },
      { header: this.col('contracts'), value: r => r.contractCount },
      { header: this.col('staff'), value: r => r.staffCount },
      { header: this.col('teaching'), value: r => r.teachingCount },
      { header: this.col('support'), value: r => r.supportCount },
      { header: this.col('totalFte'), value: r => r.totalFte },
    ];
    const stamp = toDateOnly(this.effectiveDate());
    exportToCsv(`contract-analysis-${this.staffType().toLowerCase()}-${stamp}`, columns, this.rows());
  }

  protected print(): void {
    window.print();
  }

  private col(key: string): string {
    return this.transloco.translate(`staff-reports.contractAnalysis.columns.${key}`);
  }
}
