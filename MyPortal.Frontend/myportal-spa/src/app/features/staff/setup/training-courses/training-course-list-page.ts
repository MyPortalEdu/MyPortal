import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal, viewChild } from '@angular/core';
import {
  MpBadge,
  MpButton,
  MpCard,
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
import { MeService } from '../../../../core/services/me-service';
import { Permissions } from '../../../../core/constants/permissions';
import { TrainingCoursesDataService } from '../../../../shared/services/training-courses-data.service';
import { TrainingCourse } from '../../../../shared/types/training-course';
import { HeaderAction } from '../../../../shared/types/header-action.type';
import { TrainingCourseFormDialog } from './training-course-form-dialog';

@Component({
  selector: 'mp-training-course-list-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    MpBadge,
    MpButton,
    MpCard,
    MpSkeleton,
    MpTable,
    MpTableBody,
    MpTableEmpty,
    MpTableHeader,
    PageHeader,
    EmptyState,
    TrainingCourseFormDialog,
    TranslocoDirective,
  ],
  providers: [provideTranslocoScope('training-courses')],
  templateUrl: './training-course-list-page.html',
})
export class TrainingCourseListPage implements OnInit {
  private readonly data = inject(TrainingCoursesDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);
  private readonly me = inject(MeService);

  protected readonly courses = signal<TrainingCourse[]>([]);
  protected readonly loading = signal(false);
  protected readonly canEdit = signal(false);

  private readonly formDialog = viewChild(TrainingCourseFormDialog);

  protected readonly headerActions = computed<HeaderAction[]>(() =>
    this.canEdit()
      ? [
          {
            label: this.transloco.translate('training-courses.new'),
            icon: 'fa-solid fa-plus',
            severity: 'primary',
            command: () => this.formDialog()?.open(),
          },
        ]
      : [],
  );

  ngOnInit(): void {
    this.me.me().subscribe(me =>
      this.canEdit.set(me.permissions?.includes(Permissions.Staff.EditStaffSetup) ?? false),
    );
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.data.list().subscribe({
      next: rows => {
        this.courses.set(rows);
        this.loading.set(false);
      },
      error: err => {
        this.loading.set(false);
        this.notify.apiError(err, this.transloco.translate('training-courses.loadError'));
      },
    });
  }

  protected edit(course: TrainingCourse): void {
    this.formDialog()?.open(course);
  }

  protected remove(course: TrainingCourse): void {
    if (!confirm(this.transloco.translate('training-courses.confirmDelete', { name: course.name }))) return;
    this.data.delete(course.id).subscribe({
      next: () => {
        this.notify.success(this.transloco.translate('training-courses.deleted'));
        this.load();
      },
      error: err => this.notify.apiError(err, this.transloco.translate('training-courses.deleteError')),
    });
  }
}
