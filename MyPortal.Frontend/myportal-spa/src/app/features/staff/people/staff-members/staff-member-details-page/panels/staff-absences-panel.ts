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
import { MpButton, MpCheckbox, MpDatePicker, MpTextarea } from '@myportal/ui';
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
import { Field } from '../../../../../../shared/components/field/field';
import {
  StaffAbsencesResponse,
  StaffAbsencesUpsertRequest,
} from '../../../../../../shared/types/staff-absences';
import { StaffAreaPanel } from './staff-area-panel';

interface AbsenceFormRow {
  id: string | null;
  absenceTypeId: string | null;
  illnessTypeId: string | null;
  startDate: Date | null;
  endDate: Date | null;
  isConfidential: boolean;
  notes: string;
}

@Component({
  selector: 'mp-staff-absences-panel',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    DatePipe,
    FormField,
    MpButton,
    MpDatePicker,
    MpTextarea,
    MpCheckbox,
    LookupSelect,
    Loading,
    EmptyState,
    Field,
    TranslocoDirective,
  ],
  providers: [
    provideTranslocoScope('staff-members'),
    { provide: StaffAreaPanel, useExisting: forwardRef(() => StaffAbsencesPanel) },
  ],
  templateUrl: './staff-absences-panel.html',
})
export class StaffAbsencesPanel extends StaffAreaPanel implements OnInit {
  private readonly data = inject(StaffMembersDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);

  readonly staffMemberId = input.required<string>();
  readonly permissions = input.required<Set<string>>();
  readonly relationship = input<StaffRelationship>();

  protected readonly loading = signal(false);
  override readonly editing = signal(false);

  protected readonly absences = signal<StaffAbsencesResponse | null>(null);
  protected readonly model = signal<{ absences: AbsenceFormRow[] }>({ absences: [] });
  protected readonly f = form(this.model, path => {
    applyEach(path.absences, item => {
      required(item.absenceTypeId);
      required(item.startDate);
      required(item.endDate);
      maxLength(item.notes, 256);
      validate(item.endDate, ({ value, valueOf }) => {
        const end = value();
        const start = valueOf(item.startDate);
        return !end || !start || end.getTime() >= start.getTime()
          ? undefined
          : { kind: 'endBeforeStart', message: 'staff-members.absences.endBeforeStart' };
      });
    });
  });
  private readonly snapshot = signal<string>('');

  protected readonly absenceTypes = computed(() => this.absences()?.absenceTypes ?? []);
  protected readonly illnessTypes = computed(() => this.absences()?.illnessTypes ?? []);

  override readonly canEdit = computed(() => {
    const perms = this.permissions();
    const rel = this.relationship();
    if (perms.has(Permissions.Staff.EditAllStaffAbsences)) return true;
    if (rel === 'LineManaged' && perms.has(Permissions.Staff.EditManagedStaffAbsences)) return true;
    return false;
  });

  protected readonly canManageConfidential = computed(() =>
    this.permissions().has(Permissions.Staff.EditAllStaffAbsences),
  );

  override readonly saving = computed(() => this.f().submitting());
  override readonly valid = computed(() => this.f().valid());

  private readonly form = computed(() => JSON.stringify(this.model()));

  override readonly dirty = computed(
    () => this.absences() != null && this.snapshot() !== this.form(),
  );

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.data.getAbsences(this.staffMemberId()).subscribe({
      next: row => {
        this.absences.set(row);
        this.apply(row);
        this.loading.set(false);
      },
      error: err => {
        this.loading.set(false);
        this.notify.apiError(err, this.transloco.translate('staff-members.loadAbsencesError'));
      },
    });
  }

  override startEdit(): void {
    this.editing.set(true);
  }

  override cancel(): void {
    this.apply(this.absences());
    this.editing.set(false);
  }

  override async save(): Promise<void> {
    if (!this.canEdit() || !this.dirty()) return;
    await submit(this.f, async () => {
      const payload: StaffAbsencesUpsertRequest = {
        absences: this.model().absences.map(a => ({
          id: a.id,
          absenceTypeId: a.absenceTypeId,
          illnessTypeId: a.illnessTypeId,
          startDate: a.startDate?.toISOString() ?? null,
          endDate: a.endDate?.toISOString() ?? null,
          isConfidential: a.isConfidential,
          notes: this.normalise(a.notes),
        })),
      };
      try {
        await firstValueFrom(this.data.updateAbsences(this.staffMemberId(), payload));
      } catch (err) {
        this.notify.apiError(err, this.transloco.translate('staff-members.saveAbsencesError'));
        return;
      }
      this.notify.success(this.transloco.translate('staff-members.savedAbsencesToast'));
      this.editing.set(false);
      this.load();
    });
  }

  private apply(row: StaffAbsencesResponse | null): void {
    this.model.set({
      absences: (row?.absences ?? []).map(a => ({
        id: a.id,
        absenceTypeId: a.absenceTypeId,
        illnessTypeId: a.illnessTypeId ?? null,
        startDate: a.startDate ? new Date(a.startDate) : null,
        endDate: a.endDate ? new Date(a.endDate) : null,
        isConfidential: a.isConfidential,
        notes: a.notes ?? '',
      })),
    });
    this.f().reset();
    this.snapshot.set(this.form());
  }

  protected addAbsence(): void {
    this.model.update(m => ({
      absences: [
        ...m.absences,
        {
          id: null,
          absenceTypeId: null,
          illnessTypeId: null,
          startDate: null,
          endDate: null,
          isConfidential: false,
          notes: '',
        },
      ],
    }));
  }

  protected removeAbsence(index: number): void {
    this.model.update(m => ({ absences: m.absences.filter((_, i) => i !== index) }));
  }

  protected absenceDays(a: AbsenceFormRow): number | null {
    if (!a.startDate || !a.endDate) return null;
    const ms = a.endDate.getTime() - a.startDate.getTime();
    if (ms < 0) return null;
    return Math.round(ms / 86_400_000) + 1;
  }
}
