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
import { DatePicker } from 'primeng/datepicker';
import { Checkbox } from 'primeng/checkbox';
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
  StaffQualificationUpsertItem,
} from '../../../../../../shared/types/staff-professional-details';
import { StaffAreaPanel } from './staff-area-panel';

/**
 * Professional Details area: teaching status, QTS, statutory induction and qualifications.
 * Relationship-scoped edit (HR All or line-manager Managed) — self can view but never edit.
 * Self-loads on mount.
 */
@Component({
  selector: 'mp-staff-professional-panel',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    DatePipe,
    FormsModule,
    Button,
    InputText,
    DatePicker,
    Checkbox,
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
  override readonly saving = signal(false);
  override readonly editing = signal(false);

  protected readonly professional = signal<StaffProfessionalDetailsResponse | null>(null);

  protected readonly isTeachingStaff = signal<boolean>(false);
  protected readonly hasQts = signal<boolean>(false);
  protected readonly hasHlta = signal<boolean>(false);
  protected readonly hasQtls = signal<boolean>(false);
  protected readonly hasEyts = signal<boolean>(false);
  protected readonly isSeniorLeadership = signal<boolean>(false);
  protected readonly teacherReferenceNumber = signal<string | null>(null);
  protected readonly qtsRouteId = signal<string | null>(null);
  protected readonly qtsAwardedDate = signal<Date | null>(null);
  protected readonly inductionStatusId = signal<string | null>(null);
  protected readonly inductionStartDate = signal<Date | null>(null);
  protected readonly inductionCompletedDate = signal<Date | null>(null);
  protected readonly qualificationsSummary = signal<string | null>(null);
  protected readonly qualifications = signal<StaffQualificationUpsertItem[]>([]);
  private readonly snapshot = signal<string>('');

  // Option lists travel with the professional payload so the editor is self-contained.
  protected readonly qtsRoutes = computed(() => this.professional()?.qtsRoutes ?? []);
  protected readonly inductionStatuses = computed(() => this.professional()?.inductionStatuses ?? []);
  protected readonly qualificationLevels = computed(() => this.professional()?.qualificationLevels ?? []);
  protected readonly classesOfDegree = computed(() => this.professional()?.classesOfDegree ?? []);

  // The teaching-status checkboxes, driven by a small descriptor list so the template doesn't
  // repeat the same get/set block six times. The i18n key is the field name.
  protected readonly professionalFlags: {
    key: string;
    get: () => boolean;
    set: (value: boolean) => void;
  }[] = [
    { key: 'isTeachingStaff', get: () => this.isTeachingStaff(), set: v => this.onIsTeachingStaff(v) },
    { key: 'hasQts', get: () => this.hasQts(), set: v => this.onHasQts(v) },
    { key: 'hasHlta', get: () => this.hasHlta(), set: v => this.hasHlta.set(v) },
    { key: 'hasQtls', get: () => this.hasQtls(), set: v => this.hasQtls.set(v) },
    { key: 'hasEyts', get: () => this.hasEyts(), set: v => this.hasEyts.set(v) },
    { key: 'isSeniorLeadership', get: () => this.isSeniorLeadership(), set: v => this.isSeniorLeadership.set(v) },
  ];

  // Professional edit: HR (All) or the line manager (Managed) — no self-edit.
  override readonly canEdit = computed(() => {
    const perms = this.permissions();
    const rel = this.relationship();
    if (perms.has(Permissions.Staff.EditAllStaffProfessionalDetails)) return true;
    if (rel === 'LineManaged' && perms.has(Permissions.Staff.EditManagedStaffProfessionalDetails)) return true;
    return false;
  });

  // A 7-digit TRN when provided, and every qualification row needs a title.
  override readonly valid = computed(() => {
    const trn = (this.teacherReferenceNumber() ?? '').trim();
    if (trn.length > 0 && !/^\d{7}$/.test(trn)) return false;
    return this.qualifications().every(q => q.title.trim().length > 0);
  });

  // Serialised edit state for the dirty check. Dates normalised to ISO so a Date vs string doesn't
  // read as a change.
  private readonly form = computed(() =>
    JSON.stringify({
      isTeachingStaff: this.isTeachingStaff(),
      hasQts: this.hasQts(),
      hasHlta: this.hasHlta(),
      hasQtls: this.hasQtls(),
      hasEyts: this.hasEyts(),
      isSeniorLeadership: this.isSeniorLeadership(),
      teacherReferenceNumber: this.teacherReferenceNumber(),
      qtsRouteId: this.qtsRouteId(),
      qtsAwardedDate: this.qtsAwardedDate()?.toISOString() ?? null,
      inductionStatusId: this.inductionStatusId(),
      inductionStartDate: this.inductionStartDate()?.toISOString() ?? null,
      inductionCompletedDate: this.inductionCompletedDate()?.toISOString() ?? null,
      qualificationsSummary: this.qualificationsSummary(),
      qualifications: this.qualifications(),
    }),
  );

  override readonly dirty = computed(
    () => this.professional() != null && this.snapshot() !== this.form(),
  );

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
    if (!this.canEdit() || !this.valid() || this.saving()) return;
    this.saving.set(true);

    const payload: StaffProfessionalDetailsUpsertRequest = {
      isTeachingStaff: this.isTeachingStaff(),
      hasQts: this.hasQts(),
      hasHlta: this.hasHlta(),
      hasQtls: this.hasQtls(),
      hasEyts: this.hasEyts(),
      isSeniorLeadership: this.isSeniorLeadership(),
      teacherReferenceNumber: this.normalise(this.teacherReferenceNumber()),
      qtsRouteId: this.qtsRouteId(),
      qtsAwardedDate: this.qtsAwardedDate()?.toISOString() ?? null,
      inductionStatusId: this.inductionStatusId(),
      inductionStartDate: this.inductionStartDate()?.toISOString() ?? null,
      inductionCompletedDate: this.inductionCompletedDate()?.toISOString() ?? null,
      qualificationsSummary: this.normalise(this.qualificationsSummary()),
      qualifications: this.qualifications().map(q => ({
        id: q.id ?? null,
        qualificationLevelId: q.qualificationLevelId ?? null,
        title: q.title.trim(),
        subject: this.normalise(q.subject),
        awardingBody: this.normalise(q.awardingBody),
        grade: this.normalise(q.grade),
        classOfDegreeId: q.classOfDegreeId ?? null,
        yearAwarded: q.yearAwarded ?? null,
      })),
    };

    try {
      await firstValueFrom(this.data.updateProfessionalDetails(this.staffMemberId(), payload));
      this.notify.success(this.transloco.translate('staff-members.savedProfessionalToast'));
      this.editing.set(false);
      // Refetch so server-assigned ids (new rows) and any normalisation become the baseline.
      this.load();
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('staff-members.saveProfessionalError'));
    } finally {
      this.saving.set(false);
    }
  }

  private apply(row: StaffProfessionalDetailsResponse | null): void {
    this.isTeachingStaff.set(row?.isTeachingStaff ?? false);
    this.hasQts.set(row?.hasQts ?? false);
    this.hasHlta.set(row?.hasHlta ?? false);
    this.hasQtls.set(row?.hasQtls ?? false);
    this.hasEyts.set(row?.hasEyts ?? false);
    this.isSeniorLeadership.set(row?.isSeniorLeadership ?? false);
    this.teacherReferenceNumber.set(row?.teacherReferenceNumber ?? null);
    this.qtsRouteId.set(row?.qtsRouteId ?? null);
    this.qtsAwardedDate.set(row?.qtsAwardedDate ? new Date(row.qtsAwardedDate) : null);
    this.inductionStatusId.set(row?.inductionStatusId ?? null);
    this.inductionStartDate.set(row?.inductionStartDate ? new Date(row.inductionStartDate) : null);
    this.inductionCompletedDate.set(
      row?.inductionCompletedDate ? new Date(row.inductionCompletedDate) : null,
    );
    this.qualificationsSummary.set(row?.qualificationsSummary ?? null);
    this.qualifications.set(
      (row?.qualifications ?? []).map(q => ({
        id: q.id,
        qualificationLevelId: q.qualificationLevelId ?? null,
        title: q.title,
        subject: q.subject ?? null,
        awardingBody: q.awardingBody ?? null,
        grade: q.grade ?? null,
        classOfDegreeId: q.classOfDegreeId ?? null,
        yearAwarded: q.yearAwarded ?? null,
      })),
    );
    this.snapshot.set(this.form());
  }

  // Qualifications grid — a new row has no id (the server inserts it); editing a field rewrites the
  // array immutably so the dirty check and OnPush both fire.
  protected addQualification(): void {
    this.qualifications.update(rows => [
      ...rows,
      {
        id: null,
        qualificationLevelId: null,
        title: '',
        subject: null,
        awardingBody: null,
        grade: null,
        classOfDegreeId: null,
        yearAwarded: null,
      },
    ]);
  }

  protected removeQualification(index: number): void {
    this.qualifications.update(rows => rows.filter((_, i) => i !== index));
  }

  // One-line read-only summary of a qualification's secondary fields (level, subject, grade, class,
  // awarding body, year) — empty fields dropped.
  protected qualificationLine(q: StaffQualificationUpsertItem): string {
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

  protected patchQualification<K extends keyof StaffQualificationUpsertItem>(
    index: number,
    key: K,
    value: StaffQualificationUpsertItem[K],
  ): void {
    this.qualifications.update(rows =>
      rows.map((row, i) => (i === index ? { ...row, [key]: value } : row)),
    );
  }

  // "Teaching staff" means qualified/unqualified teachers — not teaching assistants, who are support
  // staff even as HLTAs. QTS and statutory induction only apply to teachers, so clear and hide those
  // fields when the teaching-staff flag is off.
  protected onIsTeachingStaff(value: boolean): void {
    this.isTeachingStaff.set(value);
    if (!value) {
      this.teacherReferenceNumber.set(null);
      this.onHasQts(false);
      this.inductionStatusId.set(null);
      this.inductionStartDate.set(null);
      this.inductionCompletedDate.set(null);
    }
  }

  // Route to QTS and award date are meaningless without QTS; clear them when the QTS flag is turned
  // off so the hidden fields don't persist stale data.
  protected onHasQts(value: boolean): void {
    this.hasQts.set(value);
    if (!value) {
      this.qtsRouteId.set(null);
      this.qtsAwardedDate.set(null);
    }
  }
}
