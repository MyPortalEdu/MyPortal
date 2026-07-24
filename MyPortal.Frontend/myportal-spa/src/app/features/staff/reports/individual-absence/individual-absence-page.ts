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
import { StaffMemberPicker } from '../../../../shared/components/pickers/staff-member-picker/staff-member-picker';
import { NotificationService } from '../../../../core/services/notification.service';
import { StaffReportsDataService } from '../../../../shared/services/staff-reports-data.service';
import { StaffMemberSummaryResponse } from '../../../../shared/types/staff-member';
import { IndividualAbsenceReportItem, ReportOption } from '../../../../shared/types/staff-reports';
import { exportToCsv, CsvColumn } from '../../../../shared/utils/csv-export';
import { fteLabel, dateLabel, toDateOnly } from '../../../../shared/utils/report-format';

function addYears(date: Date, years: number): Date {
  const next = new Date(date);
  next.setFullYear(next.getFullYear() + years);
  return next;
}

@Component({
  selector: 'mp-individual-absence-page',
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
    StaffMemberPicker,
    PageHeader,
    EmptyState,
    TranslocoDirective,
  ],
  providers: [provideTranslocoScope('staff-reports')],
  templateUrl: './individual-absence-page.html',
})
export class IndividualAbsencePage implements OnInit {
  private readonly data = inject(StaffReportsDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);

  protected readonly fteLabel = fteLabel;
  protected readonly dateLabel = dateLabel;

  protected readonly staff = signal<{ id: string; name: string } | null>(null);
  protected readonly absenceTypeId = signal<string>('');
  protected readonly startDate = signal<Date>(addYears(new Date(), -1));
  protected readonly endDate = signal<Date>(new Date());

  protected readonly loading = signal(false);
  protected readonly hasRun = signal(false);
  protected readonly rows = signal<IndividualAbsenceReportItem[]>([]);
  protected readonly absenceTypes = signal<ReportOption[]>([]);

  protected readonly typeOptions = computed(() => [
    { id: '', name: this.transloco.translate('staff-reports.absenceType.all') },
    ...this.absenceTypes(),
  ]);

  protected readonly count = computed(() => this.rows().length);
  protected readonly totalDays = computed(() =>
    this.rows().reduce((sum, r) => sum + (r.workingDaysLost ?? 0), 0),
  );

  ngOnInit(): void {
    this.data.getAbsenceTypes().subscribe({ next: t => this.absenceTypes.set(t), error: () => {} });
  }

  protected onStaffPicked(row: StaffMemberSummaryResponse): void {
    const first = row.preferredFirstName?.trim() || row.firstName;
    const last = row.preferredLastName?.trim() || row.lastName;
    this.staff.set({ id: row.id, name: last ? `${last}, ${first}` : first });
  }

  protected run(): void {
    const staff = this.staff();
    if (!staff) return;

    this.loading.set(true);
    this.data
      .getIndividualAbsence(
        staff.id,
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
          this.notify.apiError(err, this.transloco.translate('staff-reports.individualAbsence.loadError'));
        },
      });
  }

  protected exportCsv(): void {
    const columns: CsvColumn<IndividualAbsenceReportItem>[] = [
      { header: this.col('type'), value: r => r.absenceType },
      { header: this.col('illness'), value: r => r.illnessType },
      { header: this.col('start'), value: r => r.startDate },
      { header: this.col('end'), value: r => r.endDate },
      { header: this.col('daysLost'), value: r => r.workingDaysLost ?? '' },
      { header: this.col('hoursLost'), value: r => r.hoursLost ?? '' },
      { header: this.col('notes'), value: r => r.notes },
    ];
    exportToCsv(`individual-absence-${this.staff()?.name ?? 'staff'}`, columns, this.rows());
  }

  protected print(): void {
    window.print();
  }

  private col(key: string): string {
    return this.transloco.translate(`staff-reports.individualAbsence.columns.${key}`);
  }
}
