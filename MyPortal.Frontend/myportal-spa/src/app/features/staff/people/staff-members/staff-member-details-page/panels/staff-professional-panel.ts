import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  effect,
  forwardRef,
  inject,
  input,
  signal,
  untracked,
} from '@angular/core';
import { DatePipe } from '@angular/common';
import { FieldTree, FormField, applyEach, form, maxLength, required, submit, validate } from '@angular/forms/signals';
import { MpButton, MpCheckbox, MpDatePicker, MpInput } from '@myportal/ui';
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
import {
  StaffProfessionalDetailsResponse,
  StaffProfessionalDetailsUpsertRequest,
} from '../../../../../../shared/types/staff-professional-details';
import { StaffAreaPanel } from './staff-area-panel';

interface QualificationRow {
  id: string | null;
  qualificationLevelId: string | null;
  title: string;
  subject: string;
  awardingBody: string;
  grade: string;
  classOfDegreeId: string | null;
  yearAwarded: number | null;
}
interface ProfessionalModel {
  isTeachingStaff: boolean;
  teacherCategoryId: string | null;
  teacherStatusId: string | null;
  eligibleForSwr: boolean;
  hasQts: boolean;
  hasHlta: boolean;
  hasQtls: boolean;
  hasEyts: boolean;
  isSeniorLeadership: boolean;
  teacherReferenceNumber: string;
  qtsRouteId: string | null;
  qtsAwardedDate: Date | null;
  inductionStatusId: string | null;
  inductionStartDate: Date | null;
  inductionCompletedDate: Date | null;
  qualificationsSummary: string;
  qualifications: QualificationRow[];
}

@Component({
  selector: 'mp-staff-professional-panel',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    DatePipe,
    FormField,
    MpButton,
    MpCheckbox,
    MpDatePicker,
    MpInput,
    LookupSelect,
    Loading,
    EmptyState,
    SectionHeader,
    Field,
    TranslocoDirective,
  ],
  providers: [
    provideTranslocoScope('staff-members'),
    { provide: StaffAreaPanel, useExisting: forwardRef(() => StaffProfessionalPanel) },
  ],
  templateUrl: './staff-professional-panel.html',
})
export class StaffProfessionalPanel extends StaffAreaPanel implements OnInit {
  private readonly data = inject(StaffMembersDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);

  readonly staffMemberId = input.required<string>();
  readonly permissions = input.required<Set<string>>();
  readonly relationship = input<StaffRelationship>();

  protected readonly loading = signal(false);
  override readonly editing = signal(false);

  protected readonly professional = signal<StaffProfessionalDetailsResponse | null>(null);

  protected readonly model = signal<ProfessionalModel>({
    isTeachingStaff: false,
    teacherCategoryId: null,
    teacherStatusId: null,
    eligibleForSwr: false,
    hasQts: false,
    hasHlta: false,
    hasQtls: false,
    hasEyts: false,
    isSeniorLeadership: false,
    teacherReferenceNumber: '',
    qtsRouteId: null,
    qtsAwardedDate: null,
    inductionStatusId: null,
    inductionStartDate: null,
    inductionCompletedDate: null,
    qualificationsSummary: '',
    qualifications: [],
  });
  protected readonly f = form(this.model, path => {
    validate(path.teacherReferenceNumber, ({ value }) => {
      const trn = value().trim();
      return !trn || /^\d{7}$/.test(trn)
        ? undefined
        : { kind: 'pattern', message: 'staff-members.professional.trnInvalid' };
    });
    maxLength(path.teacherReferenceNumber, 7);
    maxLength(path.qualificationsSummary, 128);
    applyEach(path.qualifications, item => {
      required(item.title);
      validate(item.title, ({ value }) =>
        value().trim().length ? undefined : { kind: 'blank', message: 'common.validation.required' },
      );
      maxLength(item.grade, 20);
    });
  });
  private readonly snapshot = signal<string>('');

  protected readonly qtsRoutes = computed(() => this.professional()?.qtsRoutes ?? []);
  protected readonly teacherCategories = computed(() => this.professional()?.teacherCategories ?? []);
  protected readonly teacherStatuses = computed(() => this.professional()?.teacherStatuses ?? []);
  protected readonly inductionStatuses = computed(() => this.professional()?.inductionStatuses ?? []);
  protected readonly qualificationLevels = computed(() => this.professional()?.qualificationLevels ?? []);
  protected readonly classesOfDegree = computed(() => this.professional()?.classesOfDegree ?? []);

  protected readonly professionalFlags: { key: string; field: FieldTree<boolean> }[] = [
    { key: 'isTeachingStaff', field: this.f.isTeachingStaff },
    { key: 'hasQts', field: this.f.hasQts },
    { key: 'hasHlta', field: this.f.hasHlta },
    { key: 'hasQtls', field: this.f.hasQtls },
    { key: 'hasEyts', field: this.f.hasEyts },
    { key: 'isSeniorLeadership', field: this.f.isSeniorLeadership },
    { key: 'eligibleForSwr', field: this.f.eligibleForSwr },
  ];

  override readonly canEdit = computed(() => {
    const perms = this.permissions();
    const rel = this.relationship();
    if (perms.has(Permissions.Staff.EditAllStaffProfessionalDetails)) return true;
    if (rel === 'LineManaged' && perms.has(Permissions.Staff.EditManagedStaffProfessionalDetails)) return true;
    return false;
  });

  override readonly saving = computed(() => this.f().submitting());
  override readonly valid = computed(() => this.f().valid());

  private readonly form = computed(() => JSON.stringify(this.model()));

  override readonly dirty = computed(
    () => this.professional() != null && this.snapshot() !== this.form(),
  );

  constructor() {
    super();
    effect(() => {
      if (this.model().isTeachingStaff) return;
      untracked(() =>
        this.model.update(m =>
          m.teacherReferenceNumber === '' &&
          !m.hasQts &&
          m.inductionStatusId === null &&
          m.inductionStartDate === null &&
          m.inductionCompletedDate === null
            ? m
            : {
                ...m,
                teacherReferenceNumber: '',
                hasQts: false,
                inductionStatusId: null,
                inductionStartDate: null,
                inductionCompletedDate: null,
              },
        ),
      );
    });
    effect(() => {
      if (this.model().hasQts) return;
      untracked(() =>
        this.model.update(m =>
          m.qtsRouteId === null && m.qtsAwardedDate === null
            ? m
            : { ...m, qtsRouteId: null, qtsAwardedDate: null },
        ),
      );
    });
  }

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.data.getProfessionalDetails(this.staffMemberId()).subscribe({
      next: row => {
        this.professional.set(row);
        this.apply(row);
        this.loading.set(false);
      },
      error: err => {
        this.loading.set(false);
        this.notify.apiError(err, this.transloco.translate('staff-members.loadProfessionalError'));
      },
    });
  }

  override startEdit(): void {
    this.editing.set(true);
  }

  override cancel(): void {
    this.apply(this.professional());
    this.editing.set(false);
  }

  override async save(): Promise<void> {
    if (!this.canEdit() || !this.dirty()) return;
    await submit(this.f, async () => {
      const m = this.model();
      const payload: StaffProfessionalDetailsUpsertRequest = {
        isTeachingStaff: m.isTeachingStaff,
        teacherCategoryId: m.teacherCategoryId,
        teacherStatusId: m.teacherStatusId,
        eligibleForSwr: m.eligibleForSwr,
        hasQts: m.hasQts,
        hasHlta: m.hasHlta,
        hasQtls: m.hasQtls,
        hasEyts: m.hasEyts,
        isSeniorLeadership: m.isSeniorLeadership,
        teacherReferenceNumber: this.normalise(m.teacherReferenceNumber),
        qtsRouteId: m.qtsRouteId,
        qtsAwardedDate: m.qtsAwardedDate?.toISOString() ?? null,
        inductionStatusId: m.inductionStatusId,
        inductionStartDate: m.inductionStartDate?.toISOString() ?? null,
        inductionCompletedDate: m.inductionCompletedDate?.toISOString() ?? null,
        qualificationsSummary: this.normalise(m.qualificationsSummary),
        qualifications: m.qualifications.map(q => ({
          id: q.id,
          qualificationLevelId: q.qualificationLevelId,
          title: q.title.trim(),
          subject: this.normalise(q.subject),
          awardingBody: this.normalise(q.awardingBody),
          grade: this.normalise(q.grade),
          classOfDegreeId: q.classOfDegreeId,
          yearAwarded: q.yearAwarded,
        })),
      };
      try {
        await firstValueFrom(this.data.updateProfessionalDetails(this.staffMemberId(), payload));
      } catch (err) {
        this.notify.apiError(err, this.transloco.translate('staff-members.saveProfessionalError'));
        return;
      }
      this.notify.success(this.transloco.translate('staff-members.savedProfessionalToast'));
      this.editing.set(false);
      this.load();
    });
  }

  private apply(row: StaffProfessionalDetailsResponse | null): void {
    this.model.set({
      isTeachingStaff: row?.isTeachingStaff ?? false,
      teacherCategoryId: row?.teacherCategoryId ?? null,
      teacherStatusId: row?.teacherStatusId ?? null,
      eligibleForSwr: row?.eligibleForSwr ?? false,
      hasQts: row?.hasQts ?? false,
      hasHlta: row?.hasHlta ?? false,
      hasQtls: row?.hasQtls ?? false,
      hasEyts: row?.hasEyts ?? false,
      isSeniorLeadership: row?.isSeniorLeadership ?? false,
      teacherReferenceNumber: row?.teacherReferenceNumber ?? '',
      qtsRouteId: row?.qtsRouteId ?? null,
      qtsAwardedDate: this.toDate(row?.qtsAwardedDate),
      inductionStatusId: row?.inductionStatusId ?? null,
      inductionStartDate: this.toDate(row?.inductionStartDate),
      inductionCompletedDate: this.toDate(row?.inductionCompletedDate),
      qualificationsSummary: row?.qualificationsSummary ?? '',
      qualifications: (row?.qualifications ?? []).map(q => ({
        id: q.id,
        qualificationLevelId: q.qualificationLevelId ?? null,
        title: q.title,
        subject: q.subject ?? '',
        awardingBody: q.awardingBody ?? '',
        grade: q.grade ?? '',
        classOfDegreeId: q.classOfDegreeId ?? null,
        yearAwarded: q.yearAwarded ?? null,
      })),
    });
    this.f().reset();
    this.snapshot.set(this.form());
  }

  protected addQualification(): void {
    this.model.update(m => ({
      ...m,
      qualifications: [
        ...m.qualifications,
        {
          id: null,
          qualificationLevelId: null,
          title: '',
          subject: '',
          awardingBody: '',
          grade: '',
          classOfDegreeId: null,
          yearAwarded: null,
        },
      ],
    }));
  }

  protected removeQualification(index: number): void {
    this.model.update(m => ({ ...m, qualifications: m.qualifications.filter((_, i) => i !== index) }));
  }

  protected qualificationLine(q: QualificationRow): string {
    const parts = [
      this.lookupLabel(this.qualificationLevels(), q.qualificationLevelId),
      q.subject,
      q.grade,
      this.lookupLabel(this.classesOfDegree(), q.classOfDegreeId),
      q.awardingBody,
      q.yearAwarded != null ? String(q.yearAwarded) : null,
    ];
    const shown = parts.filter(p => p && p !== '—');
    return shown.length ? shown.join(' · ') : '—';
  }
}
