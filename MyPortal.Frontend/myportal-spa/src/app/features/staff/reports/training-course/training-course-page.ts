import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import {
  MpButton,
  MpCard,
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
import { ReportOption, TrainingCourseAttendeeReportItem } from '../../../../shared/types/staff-reports';
import { exportToCsv, CsvColumn } from '../../../../shared/utils/csv-export';
import { fteLabel, dateLabel } from '../../../../shared/utils/report-format';

@Component({
  selector: 'mp-training-course-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    FormsModule,
    RouterLink,
    MpButton,
    MpCard,
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
  templateUrl: './training-course-page.html',
})
export class TrainingCoursePage implements OnInit {
  private readonly data = inject(StaffReportsDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);

  protected readonly dateLabel = dateLabel;
  protected readonly fteLabel = fteLabel;

  protected readonly courseId = signal<string>('');
  protected readonly courses = signal<ReportOption[]>([]);

  protected readonly loading = signal(false);
  protected readonly hasRun = signal(false);
  protected readonly rows = signal<TrainingCourseAttendeeReportItem[]>([]);

  protected readonly count = computed(() => this.rows().length);
  protected readonly courseName = computed(
    () => this.courses().find(c => c.id === this.courseId())?.name ?? '',
  );

  ngOnInit(): void {
    this.data.getTrainingCourses().subscribe({ next: c => this.courses.set(c), error: () => {} });
  }

  protected run(): void {
    if (!this.courseId()) return;

    this.loading.set(true);
    this.data.getTrainingCourseAttendees(this.courseId()).subscribe({
      next: items => {
        this.rows.set(items);
        this.hasRun.set(true);
        this.loading.set(false);
      },
      error: err => {
        this.loading.set(false);
        this.notify.apiError(err, this.transloco.translate('staff-reports.trainingCourse.loadError'));
      },
    });
  }

  protected exportCsv(): void {
    const columns: CsvColumn<TrainingCourseAttendeeReportItem>[] = [
      { header: this.col('code'), value: r => r.staffCode },
      { header: this.col('name'), value: r => r.staffName },
      { header: this.col('status'), value: r => r.status },
      { header: this.col('completed'), value: r => r.completedDate },
      { header: this.col('expiry'), value: r => r.expiryDate },
      { header: this.col('hours'), value: r => r.hours ?? '' },
    ];
    exportToCsv(`training-course-${this.courseName() || 'course'}`, columns, this.rows());
  }

  protected print(): void {
    window.print();
  }

  private col(key: string): string {
    return this.transloco.translate(`staff-reports.trainingCourse.columns.${key}`);
  }
}
