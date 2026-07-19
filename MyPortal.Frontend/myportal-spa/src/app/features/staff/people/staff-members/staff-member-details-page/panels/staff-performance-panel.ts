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
import { FormField, applyEach, form, maxLength, required, submit, validate } from '@angular/forms/signals';
import { MpButton, MpDatePicker, MpInput, MpInputNumber, MpTextarea } from '@myportal/ui';
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
import { Loading } from '../../../../../../shared/components/loading/loading';
import { EmptyState } from '../../../../../../shared/components/empty-state/empty-state';
import { SectionHeader } from '../../../../../../shared/components/section-header/section-header';
import { Field } from '../../../../../../shared/components/field/field';
import { Callout } from '../../../../../../shared/components/callout/callout';
import {
  StaffPerformanceResponse,
  StaffPerformanceUpsertRequest,
} from '../../../../../../shared/types/staff-performance';
import { StaffAreaPanel } from './staff-area-panel';

interface ReviewRow {
  id: string | null;
  cycleName: string;
  reviewerId: string | null;
  statusId: string | null;
  reviewDate: Date | null;
  nextReviewDate: Date | null;
  overallRatingId: string | null;
  summary: string;
}
interface ObjectiveRow {
  id: string | null;
  reviewId: string | null;
  categoryId: string | null;
  title: string;
  description: string;
  successCriteria: string;
  dueDate: Date | null;
  statusId: string | null;
  progressNotes: string;
}
interface ObservationRow {
  id: string | null;
  date: Date | null;
  observerId: string | null;
  outcomeId: string | null;
  focus: string;
  subjectObserved: string;
  strengths: string;
  areasForDevelopment: string;
  notes: string;
}
interface TrainingRow {
  id: string | null;
  trainingCourseId: string | null;
  statusId: string | null;
  completedDate: Date | null;
  expiryDate: Date | null;
  provider: string;
  hours: number | null;
  certificateReference: string;
}
interface PerformanceModel {
  reviews: ReviewRow[];
  objectives: ObjectiveRow[];
  observations: ObservationRow[];
  trainingRecords: TrainingRow[];
}

@Component({
  selector: 'mp-staff-performance-panel',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    DatePipe,
    FormField,
    MpButton,
    MpDatePicker,
    MpInput,
    MpInputNumber,
    MpTextarea,
    LookupSelect,
    Loading,
    EmptyState,
    SectionHeader,
    Field,
    Callout,
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
  override readonly editing = signal(false);

  protected readonly performance = signal<StaffPerformanceResponse | null>(null);

  protected readonly model = signal<PerformanceModel>({
    reviews: [],
    objectives: [],
    observations: [],
    trainingRecords: [],
  });
  protected readonly f = form(this.model, path => {
    applyEach(path.reviews, item => {
      maxLength(item.cycleName, 128);
    });
    applyEach(path.objectives, item => {
      required(item.title);
      validate(item.title, ({ value }) =>
        value().trim().length ? undefined : { kind: 'blank', message: 'common.validation.required' },
      );
      maxLength(item.title, 256);
    });
    applyEach(path.observations, item => {
      required(item.date);
      required(item.observerId);
      required(item.outcomeId);
      maxLength(item.focus, 128);
      maxLength(item.subjectObserved, 128);
    });
    applyEach(path.trainingRecords, item => {
      required(item.trainingCourseId);
      required(item.statusId);
      maxLength(item.provider, 128);
      maxLength(item.certificateReference, 64);
    });
  });
  private readonly snapshot = signal<string>('');

  protected readonly reviewStatuses = computed(() => this.performance()?.reviewStatuses ?? []);
  protected readonly objectiveStatuses = computed(() => this.performance()?.objectiveStatuses ?? []);
  protected readonly objectiveCategories = computed(() => this.performance()?.objectiveCategories ?? []);
  protected readonly observationOutcomes = computed(() => this.performance()?.outcomes ?? []);
  protected readonly staff = computed(() => this.performance()?.staff ?? []);
  protected readonly trainingCourses = computed(() => this.performance()?.trainingCourses ?? []);
  protected readonly trainingStatuses = computed(() => this.performance()?.trainingStatuses ?? []);

  protected readonly reviewOptions = computed(() =>
    (this.performance()?.reviews ?? [])
      .filter(r => !!r.cycleName)
      .map(r => ({ id: r.id, description: r.cycleName as string })),
  );

  override readonly canEdit = computed(() => {
    const perms = this.permissions();
    const rel = this.relationship();
    if (perms.has(Permissions.Staff.EditAllStaffPerformanceDetails)) return true;
    if (rel === 'LineManaged' && perms.has(Permissions.Staff.EditManagedStaffPerformanceDetails)) return true;
    return false;
  });

  override readonly saving = computed(() => this.f().submitting());
  override readonly valid = computed(() => this.f().valid());

  private readonly form = computed(() => JSON.stringify(this.model()));

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
    if (!this.canEdit() || !this.dirty()) return;
    await submit(this.f, async () => {
      const m = this.model();
      const payload: StaffPerformanceUpsertRequest = {
        reviews: m.reviews.map(r => ({
          id: r.id,
          cycleName: this.normalise(r.cycleName),
          reviewerId: r.reviewerId,
          statusId: r.statusId,
          reviewDate: r.reviewDate?.toISOString() ?? null,
          nextReviewDate: r.nextReviewDate?.toISOString() ?? null,
          overallRatingId: r.overallRatingId,
          summary: this.normalise(r.summary),
        })),
        objectives: m.objectives.map(o => ({
          id: o.id,
          reviewId: o.reviewId,
          categoryId: o.categoryId,
          title: o.title.trim(),
          description: this.normalise(o.description),
          successCriteria: this.normalise(o.successCriteria),
          dueDate: o.dueDate?.toISOString() ?? null,
          statusId: o.statusId,
          progressNotes: this.normalise(o.progressNotes),
        })),
        observations: m.observations.map(o => ({
          id: o.id,
          date: o.date?.toISOString() ?? null,
          observerId: o.observerId,
          outcomeId: o.outcomeId,
          focus: this.normalise(o.focus),
          subjectObserved: this.normalise(o.subjectObserved),
          strengths: this.normalise(o.strengths),
          areasForDevelopment: this.normalise(o.areasForDevelopment),
          notes: this.normalise(o.notes),
        })),
        trainingRecords: m.trainingRecords.map(t => ({
          id: t.id,
          trainingCourseId: t.trainingCourseId,
          statusId: t.statusId,
          completedDate: t.completedDate?.toISOString() ?? null,
          expiryDate: t.expiryDate?.toISOString() ?? null,
          provider: this.normalise(t.provider),
          hours: t.hours,
          certificateReference: this.normalise(t.certificateReference),
        })),
      };
      try {
        await firstValueFrom(this.data.updatePerformance(this.staffMemberId(), payload));
      } catch (err) {
        this.notify.apiError(err, this.transloco.translate('staff-members.savePerformanceError'));
        return;
      }
      this.notify.success(this.transloco.translate('staff-members.savedPerformanceToast'));
      this.editing.set(false);
      this.load();
    });
  }

  private apply(row: StaffPerformanceResponse | null): void {
    this.model.set({
      reviews: (row?.reviews ?? []).map(r => ({
        id: r.id,
        cycleName: r.cycleName ?? '',
        reviewerId: r.reviewerId ?? null,
        statusId: r.statusId ?? null,
        reviewDate: r.reviewDate ? new Date(r.reviewDate) : null,
        nextReviewDate: r.nextReviewDate ? new Date(r.nextReviewDate) : null,
        overallRatingId: r.overallRatingId ?? null,
        summary: r.summary ?? '',
      })),
      objectives: (row?.objectives ?? []).map(o => ({
        id: o.id,
        reviewId: o.reviewId ?? null,
        categoryId: o.categoryId ?? null,
        title: o.title,
        description: o.description ?? '',
        successCriteria: o.successCriteria ?? '',
        dueDate: o.dueDate ? new Date(o.dueDate) : null,
        statusId: o.statusId ?? null,
        progressNotes: o.progressNotes ?? '',
      })),
      observations: (row?.observations ?? []).map(o => ({
        id: o.id,
        date: o.date ? new Date(o.date) : null,
        observerId: o.observerId ?? null,
        outcomeId: o.outcomeId ?? null,
        focus: o.focus ?? '',
        subjectObserved: o.subjectObserved ?? '',
        strengths: o.strengths ?? '',
        areasForDevelopment: o.areasForDevelopment ?? '',
        notes: o.notes ?? '',
      })),
      trainingRecords: (row?.trainingRecords ?? []).map(t => ({
        id: t.id,
        trainingCourseId: t.trainingCourseId ?? null,
        statusId: t.statusId ?? null,
        completedDate: t.completedDate ? new Date(t.completedDate) : null,
        expiryDate: t.expiryDate ? new Date(t.expiryDate) : null,
        provider: t.provider ?? '',
        hours: t.hours ?? null,
        certificateReference: t.certificateReference ?? '',
      })),
    });
    this.f().reset();
    this.snapshot.set(this.form());
  }

  protected addReview(): void {
    this.model.update(m => ({
      ...m,
      reviews: [
        ...m.reviews,
        {
          id: null,
          cycleName: '',
          reviewerId: null,
          statusId: null,
          reviewDate: null,
          nextReviewDate: null,
          overallRatingId: null,
          summary: '',
        },
      ],
    }));
  }

  protected removeReview(index: number): void {
    this.model.update(m => ({ ...m, reviews: m.reviews.filter((_, i) => i !== index) }));
  }

  protected addObjective(): void {
    this.model.update(m => ({
      ...m,
      objectives: [
        ...m.objectives,
        {
          id: null,
          reviewId: null,
          categoryId: null,
          title: '',
          description: '',
          successCriteria: '',
          dueDate: null,
          statusId: null,
          progressNotes: '',
        },
      ],
    }));
  }

  protected removeObjective(index: number): void {
    this.model.update(m => ({ ...m, objectives: m.objectives.filter((_, i) => i !== index) }));
  }

  protected addObservation(): void {
    this.model.update(m => ({
      ...m,
      observations: [
        ...m.observations,
        {
          id: null,
          date: null,
          observerId: null,
          outcomeId: null,
          focus: '',
          subjectObserved: '',
          strengths: '',
          areasForDevelopment: '',
          notes: '',
        },
      ],
    }));
  }

  protected removeObservation(index: number): void {
    this.model.update(m => ({ ...m, observations: m.observations.filter((_, i) => i !== index) }));
  }

  protected addTrainingRecord(): void {
    this.model.update(m => ({
      ...m,
      trainingRecords: [
        ...m.trainingRecords,
        {
          id: null,
          trainingCourseId: null,
          statusId: null,
          completedDate: null,
          expiryDate: null,
          provider: '',
          hours: null,
          certificateReference: '',
        },
      ],
    }));
  }

  protected removeTrainingRecord(index: number): void {
    this.model.update(m => ({ ...m, trainingRecords: m.trainingRecords.filter((_, i) => i !== index) }));
  }
}
