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
import { ContractInformationReportItem, StaffTypeFilter } from '../../../../shared/types/staff-reports';
import { exportToCsv, CsvColumn } from '../../../../shared/utils/csv-export';
import { fteLabel, dateLabel, toDateOnly } from '../../../../shared/utils/report-format';

const STAFF_TYPES: StaffTypeFilter[] = ['All', 'Teaching', 'Support'];

@Component({
  selector: 'mp-contract-information-page',
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
  templateUrl: './contract-information-page.html',
})
export class ContractInformationPage {
  private readonly data = inject(StaffReportsDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);

  protected readonly fteLabel = fteLabel;
  protected readonly dateLabel = dateLabel;

  protected readonly staffType = signal<StaffTypeFilter>('All');
  protected readonly effectiveDate = signal<Date>(new Date());

  protected readonly loading = signal(false);
  protected readonly hasRun = signal(false);
  protected readonly rows = signal<ContractInformationReportItem[]>([]);

  protected readonly staffTypeOptions = computed(() =>
    STAFF_TYPES.map(value => ({
      value,
      label: this.transloco.translate(`staff-reports.staffType.${value}`),
    })),
  );

  protected readonly contractCount = computed(() => this.rows().length);
  protected readonly totalFte = computed(() =>
    this.rows().reduce((sum, r) => sum + (r.fte ?? 0), 0),
  );

  protected run(): void {
    this.loading.set(true);
    this.data.getContractInformation(this.staffType(), toDateOnly(this.effectiveDate())).subscribe({
      next: items => {
        this.rows.set(items);
        this.hasRun.set(true);
        this.loading.set(false);
      },
      error: err => {
        this.loading.set(false);
        this.notify.apiError(err, this.transloco.translate('staff-reports.contractInformation.loadError'));
      },
    });
  }

  protected exportCsv(): void {
    const columns: CsvColumn<ContractInformationReportItem>[] = [
      { header: this.col('code'), value: r => r.staffCode },
      { header: this.col('name'), value: r => r.staffName },
      { header: this.col('serviceTerm'), value: r => r.serviceTerm },
      { header: this.col('post'), value: r => r.postTitle },
      { header: this.col('role'), value: r => r.role },
      { header: this.col('type'), value: r => r.contractType },
      { header: this.col('fte'), value: r => r.fte },
      { header: this.col('hours'), value: r => r.hoursPerWeek ?? '' },
      { header: this.col('weeks'), value: r => r.weeksPerYear ?? '' },
      { header: this.col('scale'), value: r => r.payScale },
      { header: this.col('point'), value: r => r.payPoint },
      { header: this.col('start'), value: r => r.startDate },
      { header: this.col('end'), value: r => r.endDate },
    ];
    const stamp = toDateOnly(this.effectiveDate());
    exportToCsv(`contract-information-${this.staffType().toLowerCase()}-${stamp}`, columns, this.rows());
  }

  protected print(): void {
    window.print();
  }

  private col(key: string): string {
    return this.transloco.translate(`staff-reports.contractInformation.columns.${key}`);
  }
}
