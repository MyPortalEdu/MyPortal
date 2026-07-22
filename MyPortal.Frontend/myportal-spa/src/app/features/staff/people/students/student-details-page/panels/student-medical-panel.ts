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
import { FormField, applyEach, form, required, submit, validate } from '@angular/forms/signals';
import { DatePipe } from '@angular/common';
import { MpButton, MpCheckbox, MpDatePicker, MpInput, MpMultiSelect, MpTextarea } from '@myportal/ui';
import { firstValueFrom } from 'rxjs';
import {
  TranslocoDirective,
  TranslocoService,
  provideTranslocoScope,
} from '@jsverse/transloco';

import { NotificationService } from '../../../../../../core/services/notification.service';
import { Permissions } from '../../../../../../core/constants/permissions';
import { StudentsDataService } from '../../../../../../shared/services/students-data.service';
import { LookupSelect } from '../../../../../../shared/components/lookup-select/lookup-select';
import { Loading } from '../../../../../../shared/components/loading/loading';
import { SectionHeader } from '../../../../../../shared/components/section-header/section-header';
import { Field } from '../../../../../../shared/components/field/field';
import {
  StudentMedicalDetailsResponse,
  StudentMedicalDetailsUpsertRequest,
} from '../../../../../../shared/types/student-medical';
import { StudentAreaPanel } from './student-area-panel';

interface ConditionRow {
  medicalConditionId: string | null;
  requiresMedication: boolean;
  medication: string;
  startDate: Date | null;
  endDate: Date | null;
  infoReceivedDate: Date | null;
  notes: string;
}

interface MedicalModel {
  hasMedicalNeeds: boolean;
  conditions: ConditionRow[];
  dietaryRequirementIds: string[];
  disabilityIds: string[];
}

@Component({
  selector: 'mp-student-medical-panel',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormField, DatePipe, MpButton, MpCheckbox, MpDatePicker, MpInput, MpMultiSelect, MpTextarea, LookupSelect, Loading, SectionHeader, Field, TranslocoDirective],
  providers: [
    provideTranslocoScope('students'),
    { provide: StudentAreaPanel, useExisting: forwardRef(() => StudentMedicalPanel) },
  ],
  templateUrl: './student-medical-panel.html',
})
export class StudentMedicalPanel extends StudentAreaPanel implements OnInit {
  private readonly data = inject(StudentsDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);

  readonly studentId = input.required<string>();
  readonly permissions = input.required<Set<string>>();

  protected readonly loading = signal(false);
  override readonly editing = signal(false);

  protected readonly medical = signal<StudentMedicalDetailsResponse | null>(null);

  protected readonly model = signal<MedicalModel>({
    hasMedicalNeeds: false,
    conditions: [],
    dietaryRequirementIds: [],
    disabilityIds: [],
  });
  protected readonly f = form(this.model, path => {
    applyEach(path.conditions, item => {
      required(item.medicalConditionId);
      validate(item.medication, ({ value, valueOf }) =>
        valueOf(item.requiresMedication) && !value().trim() ? { kind: 'required' } : undefined,
      );
      validate(item.endDate, ({ value, valueOf }) => {
        const start = valueOf(item.startDate);
        const end = value();
        return start && end && end.getTime() < start.getTime()
          ? { kind: 'range', message: 'students.medical.resolvedBeforeStart' }
          : undefined;
      });
    });
  });
  private readonly snapshot = signal<string>('');

  constructor() {
    super();
    effect(() => {
      const conditions = this.model().conditions;
      if (!conditions.some(c => !c.requiresMedication && c.medication !== '')) return;
      untracked(() =>
        this.model.update(m => ({
          ...m,
          conditions: m.conditions.map(c =>
            !c.requiresMedication && c.medication !== '' ? { ...c, medication: '' } : c,
          ),
        })),
      );
    });
  }

  protected readonly medicalConditions = computed(() => this.medical()?.medicalConditions ?? []);
  protected readonly dietaryRequirements = computed(() => this.medical()?.dietaryRequirements ?? []);
  protected readonly disabilities = computed(() => this.medical()?.disabilities ?? []);

  override readonly canEdit = computed(() =>
    this.permissions().has(Permissions.Student.EditStudentMedical),
  );

  override readonly saving = computed(() => this.f().submitting());
  override readonly valid = computed(() => this.f().valid());

  private readonly formState = computed(() => {
    const m = this.model();
    return JSON.stringify({
      hasMedicalNeeds: m.hasMedicalNeeds,
      conditions: m.conditions,
      dietaryRequirementIds: [...m.dietaryRequirementIds].sort(),
      disabilityIds: [...m.disabilityIds].sort(),
    });
  });

  override readonly dirty = computed(
    () => this.medical() != null && this.snapshot() !== this.formState(),
  );

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.data.getMedicalDetails(this.studentId()).subscribe({
      next: row => {
        this.medical.set(row);
        this.apply(row);
        this.loading.set(false);
      },
      error: err => {
        this.loading.set(false);
        this.notify.apiError(err, this.transloco.translate('students.medical.loadError'));
      },
    });
  }

  override startEdit(): void {
    this.editing.set(true);
  }

  override cancel(): void {
    this.apply(this.medical());
    this.editing.set(false);
  }

  override async save(): Promise<void> {
    if (!this.canEdit() || !this.dirty()) return;
    await submit(this.f, async () => {
      const m = this.model();
      const payload: StudentMedicalDetailsUpsertRequest = {
        hasMedicalNeeds: m.hasMedicalNeeds,
        conditions: m.conditions
          .filter(c => !!c.medicalConditionId)
          .map(c => ({
            medicalConditionId: c.medicalConditionId as string,
            requiresMedication: c.requiresMedication,
            medication: c.requiresMedication ? this.normalise(c.medication) : null,
            startDate: c.startDate?.toISOString() ?? null,
            endDate: c.endDate?.toISOString() ?? null,
            infoReceivedDate: c.infoReceivedDate?.toISOString() ?? null,
            notes: this.normalise(c.notes),
          })),
        dietaryRequirementIds: m.dietaryRequirementIds,
        disabilityIds: m.disabilityIds,
      };
      try {
        await firstValueFrom(this.data.updateMedicalDetails(this.studentId(), payload));
      } catch (err) {
        this.notify.apiError(err, this.transloco.translate('students.medical.saveError'));
        return;
      }
      this.notify.success(this.transloco.translate('students.medical.savedToast'));
      this.editing.set(false);
      this.load();
    });
  }

  private apply(row: StudentMedicalDetailsResponse | null): void {
    this.model.set({
      hasMedicalNeeds: row?.hasMedicalNeeds ?? false,
      conditions: (row?.conditions ?? []).map(c => ({
        medicalConditionId: c.medicalConditionId,
        requiresMedication: c.requiresMedication,
        medication: c.requiresMedication ? (c.medication ?? '') : '',
        startDate: c.startDate ? new Date(c.startDate) : null,
        endDate: c.endDate ? new Date(c.endDate) : null,
        infoReceivedDate: c.infoReceivedDate ? new Date(c.infoReceivedDate) : null,
        notes: c.notes ?? '',
      })),
      dietaryRequirementIds: [...(row?.dietaryRequirementIds ?? [])],
      disabilityIds: [...(row?.disabilityIds ?? [])],
    });
    this.f().reset();
    this.snapshot.set(this.formState());
  }

  protected addCondition(): void {
    this.model.update(m => ({
      ...m,
      conditions: [
        ...m.conditions,
        {
          medicalConditionId: null,
          requiresMedication: false,
          medication: '',
          startDate: null,
          endDate: null,
          infoReceivedDate: null,
          notes: '',
        },
      ],
    }));
  }

  protected removeCondition(index: number): void {
    this.model.update(m => ({ ...m, conditions: m.conditions.filter((_, i) => i !== index) }));
  }

  protected conditionLabel(c: ConditionRow): string {
    return this.lookupLabel(this.medicalConditions(), c.medicalConditionId);
  }

  protected medicationLine(c: ConditionRow): string | null {
    if (!c.requiresMedication) return null;
    return c.medication.trim()
      ? this.transloco.translate('students.medical.medicationLine', { medication: c.medication.trim() })
      : this.transloco.translate('students.medical.requiresMedication');
  }
}
