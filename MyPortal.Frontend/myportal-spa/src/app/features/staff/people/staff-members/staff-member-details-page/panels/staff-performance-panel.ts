import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  forwardRef,
  inject,
  input,
  signal,
} from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Button } from 'primeng/button';
import { InputText } from 'primeng/inputtext';
import { InputNumber } from 'primeng/inputnumber';
import { DatePicker } from 'primeng/datepicker';
import { Textarea } from 'primeng/textarea';
import { ProgressSpinner } from 'primeng/progressspinner';
import { firstValueFrom } from 'rxjs';
import {
  TranslocoDirective,
  TranslocoService,
  provideTranslocoScope,
} from '@jsverse/transloco';

import { NotificationService } from '../../../../../../core/services/notification.service';
import { Permissions } from '../../../../../../core/constants/permissions';
import { StaffMembersDataService } from '../../../../../../shared/services/staff-members-data.service';
import { StaffRelationship } from '../../../../../../shared/types/staff-member-header';
import { LookupSelect } from '../../../../../../shared/components/lookup-select/lookup-select';
import {
  PerformanceReviewUpsertItem,
  StaffObjectiveUpsertItem,
  StaffObservationUpsertItem,
  StaffPerformanceResponse,
  StaffPerformanceUpsertRequest,
  StaffTrainingRecordUpsertItem,
} from '../../../../../../shared/types/staff-performance';
import { StaffAreaPanel } from './staff-area-panel';

/**
 * Performance (appraisal) area: review cycles, objectives, lesson observations and CPD/training.
 * Manager (Managed) or HR (All) — no self scope (staff don't see their own appraisal data here).
 * Self-loads on mount.
 */
@Component({
  selector: 'mp-staff-performance-panel',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    DatePipe,
    FormsModule,
    Button,
    InputText,
    InputNumber,
    DatePicker,
    Textarea,
    ProgressSpinner,
    LookupSelect,
    TranslocoDirective,
  ],
  providers: [
    provideTranslocoScope('staff-members'),
    { provide: StaffAreaPanel, useExisting: forwardRef(() => StaffPerformancePanel) },
  ],
  templateUrl: './staff-performance-panel.html',
})
export class StaffPerformancePanel extends StaffAreaPanel implements OnInit {
  private readonly data = inject(StaffMembersDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);

  readonly staffMemberId = input.required<string>();
  readonly permissions = input.required<Set<string>>();
  readonly relationship = input<StaffRelationship>();

  protected readonly loading = signal(false);
  override readonly saving = signal(false);
  override readonly editing = signal(false);

  protected readonly performance = signal<StaffPerformanceResponse | null>(null);

  // Record lists (item dates kept as ISO strings, bound via toDate()).
  protected readonly reviews = signal<PerformanceReviewUpsertItem[]>([]);
  protected readonly objectives = signal<StaffObjectiveUpsertItem[]>([]);
  protected readonly observations = signal<StaffObservationUpsertItem[]>([]);
  protected readonly trainingRecords = signal<StaffTrainingRecordUpsertItem[]>([]);
  private readonly snapshot = signal<string>('');

  // Option lists travel with the performance payload so the editor is self-contained.
  protected readonly reviewStatuses = computed(() => this.performance()?.reviewStatuses ?? []);
  protected readonly objectiveStatuses = computed(() => this.performance()?.objectiveStatuses ?? []);
  protected readonly objectiveCategories = computed(() => this.performance()?.objectiveCategories ?? []);
  protected readonly observationOutcomes = computed(() => this.performance()?.outcomes ?? []);
  // All staff, backing both the reviewer and observer pickers.
  protected readonly staff = computed(() => this.performance()?.staff ?? []);
  protected readonly trainingCourses = computed(() => this.performance()?.trainingCourses ?? []);
  protected readonly trainingStatuses = computed(() => this.performance()?.trainingStatuses ?? []);

  // Persisted review cycles as options for linking an objective. Only saved reviews (with a cycle
  // name) appear — a brand-new review must be saved before objectives can reference it.
  protected readonly reviewOptions = computed(() =>
    (this.performance()?.reviews ?? [])
      .filter(r => !!r.cycleName)
      .map(r => ({ id: r.id, description: r.cycleName as string })),
  );

  // Performance edit: HR (All) or the line manager (Managed) — no self-edit.
  override readonly canEdit = computed(() => {
    const perms = this.permissions();
    const rel = this.relationship();
    if (perms.has(Permissions.Staff.EditAllStaffPerformanceDetails)) return true;
    if (rel === 'LineManaged' && perms.has(Permissions.Staff.EditManagedStaffPerformanceDetails)) return true;
    return false;
  });

  // Each objective needs a title; each observation a date, observer and outcome; each training record
  // a course and status.
  override readonly valid = computed(
    () =>
      this.objectives().every(o => o.title.trim().length > 0) &&
      this.observations().every(o => !!o.date && !!o.observerId && !!o.outcomeId) &&
      this.trainingRecords().every(t => !!t.trainingCourseId && !!t.statusId),
  );

  private readonly form = computed(() =>
    JSON.stringify({
      reviews: this.reviews(),
      objectives: this.objectives(),
      observations: this.observations(),
      trainingRecords: this.trainingRecords(),
    }),
  );

  override readonly dirty = computed(
    () => this.performance() != null && this.snapshot() !== this.form(),
  );

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.data.getPerformance(this.staffMemberId()).subscribe({
      next: row => {
        this.performance.set(row);
        this.apply(row);
        this.loading.set(false);
      },
      error: err => {
        this.loading.set(false);
        this.notify.apiError(err, this.transloco.translate('staff-members.loadPerformanceError'));
      },
    });
  }

  override startEdit(): void {
    this.editing.set(true);
  }

  override cancel(): void {
    this.apply(this.performance());
    this.editing.set(false);
  }

  override async save(): Promise<void> {
    if (!this.canEdit() || !this.valid() || this.saving()) return;
    this.saving.set(true);

    const payload: StaffPerformanceUpsertRequest = {
      reviews: this.reviews().map(r => ({
        id: r.id ?? null,
        cycleName: this.normalise(r.cycleName),
        reviewerId: r.reviewerId ?? null,
        statusId: r.statusId ?? null,
        reviewDate: r.reviewDate ?? null,
        nextReviewDate: r.nextReviewDate ?? null,
        overallRatingId: r.overallRatingId ?? null,
        summary: this.normalise(r.summary),
      })),
      objectives: this.objectives().map(o => ({
        id: o.id ?? null,
        reviewId: o.reviewId ?? null,
        categoryId: o.categoryId ?? null,
        title: o.title.trim(),
        description: this.normalise(o.description),
        successCriteria: this.normalise(o.successCriteria),
        dueDate: o.dueDate ?? null,
        statusId: o.statusId ?? null,
        progressNotes: this.normalise(o.progressNotes),
      })),
      observations: this.observations().map(o => ({
        id: o.id ?? null,
        date: o.date,
        observerId: o.observerId,
        outcomeId: o.outcomeId,
        focus: this.normalise(o.focus),
        subjectObserved: this.normalise(o.subjectObserved),
        strengths: this.normalise(o.strengths),
        areasForDevelopment: this.normalise(o.areasForDevelopment),
        notes: this.normalise(o.notes),
      })),
      trainingRecords: this.trainingRecords().map(t => ({
        id: t.id ?? null,
        trainingCourseId: t.trainingCourseId,
        statusId: t.statusId,
        completedDate: t.completedDate ?? null,
        expiryDate: t.expiryDate ?? null,
        provider: this.normalise(t.provider),
        hours: t.hours ?? null,
        certificateReference: this.normalise(t.certificateReference),
      })),
    };

    try {
      await firstValueFrom(this.data.updatePerformance(this.staffMemberId(), payload));
      this.notify.success(this.transloco.translate('staff-members.savedPerformanceToast'));
      this.editing.set(false);
      // Refetch so server-assigned ids (new rows) become the baseline.
      this.load();
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('staff-members.savePerformanceError'));
    } finally {
      this.saving.set(false);
    }
  }

  private apply(row: StaffPerformanceResponse | null): void {
    this.reviews.set(
      (row?.reviews ?? []).map(r => ({
        id: r.id,
        cycleName: r.cycleName ?? null,
        reviewerId: r.reviewerId ?? null,
        statusId: r.statusId ?? null,
        reviewDate: r.reviewDate ?? null,
        nextReviewDate: r.nextReviewDate ?? null,
        overallRatingId: r.overallRatingId ?? null,
        summary: r.summary ?? null,
      })),
    );
    this.objectives.set(
      (row?.objectives ?? []).map(o => ({
        id: o.id,
        reviewId: o.reviewId ?? null,
        categoryId: o.categoryId ?? null,
        title: o.title,
        description: o.description ?? null,
        successCriteria: o.successCriteria ?? null,
        dueDate: o.dueDate ?? null,
        statusId: o.statusId ?? null,
        progressNotes: o.progressNotes ?? null,
      })),
    );
    this.observations.set(
      (row?.observations ?? []).map(o => ({
        id: o.id,
        date: o.date,
        observerId: o.observerId,
        outcomeId: o.outcomeId,
        focus: o.focus ?? null,
        subjectObserved: o.subjectObserved ?? null,
        strengths: o.strengths ?? null,
        areasForDevelopment: o.areasForDevelopment ?? null,
        notes: o.notes ?? null,
      })),
    );
    this.trainingRecords.set(
      (row?.trainingRecords ?? []).map(t => ({
        id: t.id,
        trainingCourseId: t.trainingCourseId,
        statusId: t.statusId,
        completedDate: t.completedDate ?? null,
        expiryDate: t.expiryDate ?? null,
        provider: t.provider ?? null,
        hours: t.hours ?? null,
        certificateReference: t.certificateReference ?? null,
      })),
    );
    this.snapshot.set(this.form());
  }

  protected addReview(): void {
    this.reviews.update(rows => [
      ...rows,
      {
        id: null,
        cycleName: null,
        reviewerId: null,
        statusId: null,
        reviewDate: null,
        nextReviewDate: null,
        overallRatingId: null,
        summary: null,
      },
    ]);
  }

  protected removeReview(index: number): void {
    this.reviews.update(rows => rows.filter((_, i) => i !== index));
  }

  protected patchReview<K extends keyof PerformanceReviewUpsertItem>(
    index: number,
    key: K,
    value: PerformanceReviewUpsertItem[K],
  ): void {
    this.reviews.update(rows => rows.map((row, i) => (i === index ? { ...row, [key]: value } : row)));
  }

  protected addObjective(): void {
    this.objectives.update(rows => [
      ...rows,
      {
        id: null,
        reviewId: null,
        categoryId: null,
        title: '',
        description: null,
        successCriteria: null,
        dueDate: null,
        statusId: null,
        progressNotes: null,
      },
    ]);
  }

  protected removeObjective(index: number): void {
    this.objectives.update(rows => rows.filter((_, i) => i !== index));
  }

  protected patchObjective<K extends keyof StaffObjectiveUpsertItem>(
    index: number,
    key: K,
    value: StaffObjectiveUpsertItem[K],
  ): void {
    this.objectives.update(rows => rows.map((row, i) => (i === index ? { ...row, [key]: value } : row)));
  }

  protected addObservation(): void {
    this.observations.update(rows => [
      ...rows,
      {
        id: null,
        date: null,
        observerId: null,
        outcomeId: null,
        focus: null,
        subjectObserved: null,
        strengths: null,
        areasForDevelopment: null,
        notes: null,
      },
    ]);
  }

  protected removeObservation(index: number): void {
    this.observations.update(rows => rows.filter((_, i) => i !== index));
  }

  protected patchObservation<K extends keyof StaffObservationUpsertItem>(
    index: number,
    key: K,
    value: StaffObservationUpsertItem[K],
  ): void {
    this.observations.update(rows => rows.map((row, i) => (i === index ? { ...row, [key]: value } : row)));
  }

  protected addTrainingRecord(): void {
    this.trainingRecords.update(rows => [
      ...rows,
      {
        id: null,
        trainingCourseId: null,
        statusId: null,
        completedDate: null,
        expiryDate: null,
        provider: null,
        hours: null,
        certificateReference: null,
      },
    ]);
  }

  protected removeTrainingRecord(index: number): void {
    this.trainingRecords.update(rows => rows.filter((_, i) => i !== index));
  }

  protected patchTrainingRecord<K extends keyof StaffTrainingRecordUpsertItem>(
    index: number,
    key: K,
    value: StaffTrainingRecordUpsertItem[K],
  ): void {
    this.trainingRecords.update(rows => rows.map((row, i) => (i === index ? { ...row, [key]: value } : row)));
  }
}
