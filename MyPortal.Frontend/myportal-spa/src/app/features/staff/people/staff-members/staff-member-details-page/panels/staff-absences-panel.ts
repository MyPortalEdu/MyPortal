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
import { DatePicker } from 'primeng/datepicker';
import { Textarea } from 'primeng/textarea';
import { Checkbox } from 'primeng/checkbox';
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
  StaffAbsenceUpsertItem,
  StaffAbsencesResponse,
  StaffAbsencesUpsertRequest,
} from '../../../../../../shared/types/staff-absences';
import { StaffAreaPanel } from './staff-area-panel';

/**
 * Absences & Leave area of the staff profile. Self-loads on mount (the shell only renders it while
 * its tab is active). Absences are relationship-scoped: HR (All) sees/edits everyone, a line manager
 * their reports (Managed), and only HR may mark a row confidential.
 */
@Component({
  selector: 'mp-staff-absences-panel',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    DatePipe,
    FormsModule,
    Button,
    DatePicker,
    Textarea,
    Checkbox,
    ProgressSpinner,
    LookupSelect,
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
  override readonly saving = signal(false);
  override readonly editing = signal(false);

  protected readonly absences = signal<StaffAbsencesResponse | null>(null);
  protected readonly absenceRows = signal<StaffAbsenceUpsertItem[]>([]);
  private readonly snapshot = signal<string>('');

  // Option lists travel with the absence payload so the editor is self-contained.
  protected readonly absenceTypes = computed(() => this.absences()?.absenceTypes ?? []);
  protected readonly illnessTypes = computed(() => this.absences()?.illnessTypes ?? []);

  // Absence edit: HR (All) or the line manager (Managed) — no self-edit (the record is HR/manager-owned).
  override readonly canEdit = computed(() => {
    const perms = this.permissions();
    const rel = this.relationship();
    if (perms.has(Permissions.Staff.EditAllStaffAbsences)) return true;
    if (rel === 'LineManaged' && perms.has(Permissions.Staff.EditManagedStaffAbsences)) return true;
    return false;
  });

  // Only HR (All scope) may mark an absence confidential; a line manager never sees or sets the flag.
  protected readonly canManageConfidential = computed(() =>
    this.permissions().has(Permissions.Staff.EditAllStaffAbsences),
  );

  // Each absence row needs a type, a start date and an end date no earlier than it.
  override readonly valid = computed(() =>
    this.absenceRows().every(
      a => !!a.absenceTypeId && !!a.startDate && !!a.endDate && a.endDate >= a.startDate,
    ),
  );

  private readonly form = computed(() => JSON.stringify({ absences: this.absenceRows() }));

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
    if (!this.canEdit() || !this.valid() || this.saving()) return;
    this.saving.set(true);

    const payload: StaffAbsencesUpsertRequest = {
      absences: this.absenceRows().map(a => ({
        id: a.id ?? null,
        absenceTypeId: a.absenceTypeId,
        illnessTypeId: a.illnessTypeId ?? null,
        startDate: a.startDate,
        endDate: a.endDate,
        isConfidential: a.isConfidential,
        notes: this.normalise(a.notes),
      })),
    };

    try {
      await firstValueFrom(this.data.updateAbsences(this.staffMemberId(), payload));
      this.notify.success(this.transloco.translate('staff-members.savedAbsencesToast'));
      this.editing.set(false);
      // Refetch so server-assigned ids (new rows) become the baseline.
      this.load();
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('staff-members.saveAbsencesError'));
    } finally {
      this.saving.set(false);
    }
  }

  private apply(row: StaffAbsencesResponse | null): void {
    this.absenceRows.set(
      (row?.absences ?? []).map(a => ({
        id: a.id,
        absenceTypeId: a.absenceTypeId,
        illnessTypeId: a.illnessTypeId ?? null,
        startDate: a.startDate,
        endDate: a.endDate,
        isConfidential: a.isConfidential,
        notes: a.notes ?? null,
      })),
    );
    this.snapshot.set(this.form());
  }

  protected addAbsence(): void {
    this.absenceRows.update(rows => [
      ...rows,
      {
        id: null,
        absenceTypeId: null,
        illnessTypeId: null,
        startDate: null,
        endDate: null,
        isConfidential: false,
        notes: null,
      },
    ]);
  }

  protected removeAbsence(index: number): void {
    this.absenceRows.update(rows => rows.filter((_, i) => i !== index));
  }

  protected patchAbsence<K extends keyof StaffAbsenceUpsertItem>(
    index: number,
    key: K,
    value: StaffAbsenceUpsertItem[K],
  ): void {
    this.absenceRows.update(rows => rows.map((row, i) => (i === index ? { ...row, [key]: value } : row)));
  }

  // Whole days inclusive of both ends — the simple span a school cares about.
  protected absenceDays(a: StaffAbsenceUpsertItem): number | null {
    if (!a.startDate || !a.endDate) return null;
    const start = new Date(a.startDate);
    const end = new Date(a.endDate);
    const ms = end.getTime() - start.getTime();
    if (ms < 0) return null;
    return Math.round(ms / 86_400_000) + 1;
  }
}
