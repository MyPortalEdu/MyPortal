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
import { NotificationService } from '../../../../core/services/notification.service';
import { StaffReportsDataService } from '../../../../shared/services/staff-reports-data.service';
import { TerminatingContractReportItem } from '../../../../shared/types/staff-reports';
import { exportToCsv, CsvColumn } from '../../../../shared/utils/csv-export';
import { fteLabel, dateLabel, toDateOnly } from '../../../../shared/utils/report-format';

function addMonths(date: Date, months: number): Date {
  const next = new Date(date);
  next.setMonth(next.getMonth() + months);
  return next;
}

@Component({
  selector: 'mp-terminating-contracts-page',
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
    PageHeader,
    EmptyState,
    TranslocoDirective,
  ],
  providers: [provideTranslocoScope('staff-reports')],
  templateUrl: './terminating-contracts-page.html',
})
export class TerminatingContractsPage {
  private readonly data = inject(StaffReportsDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);

  protected readonly fteLabel = fteLabel;
  protected readonly dateLabel = dateLabel;

  protected readonly startDate = signal<Date>(new Date());
  protected readonly endDate = signal<Date>(addMonths(new Date(), 3));

  protected readonly loading = signal(false);
  protected readonly hasRun = signal(false);
  protected readonly rows = signal<TerminatingContractReportItem[]>([]);

  protected readonly count = computed(() => this.rows().length);
  protected readonly totalFte = computed(() =>
    this.rows().reduce((sum, r) => sum + (r.fte ?? 0), 0),
  );

  protected run(): void {
    this.loading.set(true);
    this.data
      .getTerminatingContracts(toDateOnly(this.startDate()), toDateOnly(this.endDate()))
      .subscribe({
        next: items => {
          this.rows.set(items);
          this.hasRun.set(true);
          this.loading.set(false);
        },
        error: err => {
          this.loading.set(false);
          this.notify.apiError(err, this.transloco.translate('staff-reports.terminatingContracts.loadError'));
        },
      });
  }

  protected exportCsv(): void {
    const columns: CsvColumn<TerminatingContractReportItem>[] = [
      { header: this.col('code'), value: r => r.staffCode },
      { header: this.col('name'), value: r => r.staffName },
      { header: this.col('post'), value: r => r.postTitle },
      { header: this.col('type'), value: r => r.contractType },
      { header: this.col('serviceTerm'), value: r => r.serviceTerm },
      { header: this.col('fte'), value: r => r.fte },
      { header: this.col('end'), value: r => r.endDate },
    ];
    exportToCsv(
      `terminating-contracts-${toDateOnly(this.startDate())}-to-${toDateOnly(this.endDate())}`,
      columns,
      this.rows(),
    );
  }

  protected print(): void {
    window.print();
  }

  private col(key: string): string {
    return this.transloco.translate(`staff-reports.terminatingContracts.columns.${key}`);
  }
}
