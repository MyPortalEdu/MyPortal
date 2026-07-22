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
import { MpButton, MpCheckbox, MpDatePicker, MpInput, MpInputNumber, MpTextarea } from '@myportal/ui';
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
import { Callout } from '../../../../../../shared/components/callout/callout';
import {
  StaffAbsencesResponse,
  StaffAbsencesUpsertRequest,
} from '../../../../../../shared/types/staff-absences';
import { StaffAreaPanel } from './staff-area-panel';

interface CertificateFormRow {
  id: string | null;
  dateReceived: Date | null;
  dateSigned: Date | null;
  isSelfCertified: boolean;
  isReturnToWork: boolean;
  signedBy: string;
  notes: string;
}

interface AbsenceFormRow {
  id: string | null;
  absenceTypeId: string | null;
  illnessTypeId: string | null;
  startDate: Date | null;
  endDate: Date | null;
  isConfidential: boolean;
  notes: string;
  authorisedPayRateId: string | null;
  payrollReasonId: string | null;
  sspExcluded: boolean;
  workingDaysLost: number | null;
  hoursLost: number | null;
  isIndustrialInjury: boolean;
  certificates: CertificateFormRow[];
}

function absencesOverlap(rows: readonly AbsenceFormRow[]): boolean {
  const ordered = rows
    .filter(a => a.startDate && a.endDate)
    .slice()
    .sort((a, b) => a.startDate!.getTime() - b.startDate!.getTime());
  for (let i = 1; i < ordered.length; i++) {
    if (ordered[i].startDate!.getTime() <= ordered[i - 1].endDate!.getTime()) return true;
  }
  return false;
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
    MpInput,
    MpInputNumber,
    MpTextarea,
    MpCheckbox,
    LookupSelect,
    Loading,
    EmptyState,
    Field,
    Callout,
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

      applyEach(item.certificates, cert => {
        required(cert.dateReceived);
        maxLength(cert.signedBy, 256);
        maxLength(cert.notes, 256);
        validate(cert.dateSigned, ({ value, valueOf }) => {
          const signed = value();
          const received = valueOf(cert.dateReceived);
          return !signed || !received || signed.getTime() <= received.getTime()
            ? undefined
            : { kind: 'signedAfterReceived', message: 'staff-members.absences.signedAfterReceived' };
        });
      });
    });
    validate(path.absences, ({ value }) =>
      absencesOverlap(value())
        ? { kind: 'overlap', message: 'staff-members.absences.overlap' }
        : undefined,
    );
  });
  private readonly snapshot = signal<string>('');

  protected readonly absenceTypes = computed(() => this.absences()?.absenceTypes ?? []);
  protected readonly illnessTypes = computed(() => this.absences()?.illnessTypes ?? []);
  protected readonly payRates = computed(() => this.absences()?.payRates ?? []);
  protected readonly payrollReasons = computed(() => this.absences()?.payrollReasons ?? []);

  protected readonly canEditPayroll = computed(() => this.absences()?.canEditPayroll ?? false);

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

  protected readonly hasOverlap = computed(() => absencesOverlap(this.model().absences));

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
          authorisedPayRateId: a.authorisedPayRateId,
          payrollReasonId: a.payrollReasonId,
          sspExcluded: a.sspExcluded,
          workingDaysLost: a.workingDaysLost,
          hoursLost: a.hoursLost,
          isIndustrialInjury: a.isIndustrialInjury,
          certificates: a.certificates.map(c => ({
            id: c.id,
            dateReceived: c.dateReceived?.toISOString() ?? null,
            dateSigned: c.dateSigned?.toISOString() ?? null,
            isSelfCertified: c.isSelfCertified,
            isReturnToWork: c.isReturnToWork,
            signedBy: this.normalise(c.signedBy),
            notes: this.normalise(c.notes),
          })),
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
        authorisedPayRateId: a.authorisedPayRateId ?? null,
        payrollReasonId: a.payrollReasonId ?? null,
        sspExcluded: a.sspExcluded,
        workingDaysLost: a.workingDaysLost ?? null,
        hoursLost: a.hoursLost ?? null,
        isIndustrialInjury: a.isIndustrialInjury,
        certificates: (a.certificates ?? []).map(c => ({
          id: c.id,
          dateReceived: c.dateReceived ? new Date(c.dateReceived) : null,
          dateSigned: c.dateSigned ? new Date(c.dateSigned) : null,
          isSelfCertified: c.isSelfCertified,
          isReturnToWork: c.isReturnToWork,
          signedBy: c.signedBy ?? '',
          notes: c.notes ?? '',
        })),
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
          authorisedPayRateId: null,
          payrollReasonId: null,
          sspExcluded: false,
          workingDaysLost: null,
          hoursLost: null,
          isIndustrialInjury: false,
          certificates: [],
        },
      ],
    }));
  }

  protected removeAbsence(index: number): void {
    this.model.update(m => ({ absences: m.absences.filter((_, i) => i !== index) }));
  }

  protected addCertificate(absenceIndex: number): void {
    this.mutateAbsence(absenceIndex, a => ({
      ...a,
      certificates: [
        ...a.certificates,
        {
          id: null,
          dateReceived: null,
          dateSigned: null,
          isSelfCertified: true,
          isReturnToWork: false,
          signedBy: '',
          notes: '',
        },
      ],
    }));
  }

  protected removeCertificate(absenceIndex: number, certificateIndex: number): void {
    this.mutateAbsence(absenceIndex, a => ({
      ...a,
      certificates: a.certificates.filter((_, j) => j !== certificateIndex),
    }));
  }

  private mutateAbsence(index: number, fn: (a: AbsenceFormRow) => AbsenceFormRow): void {
    this.model.update(m => ({
      absences: m.absences.map((a, i) => (i === index ? fn(a) : a)),
    }));
  }

  protected absenceDays(a: AbsenceFormRow): number | null {
    if (!a.startDate || !a.endDate) return null;
    const ms = a.endDate.getTime() - a.startDate.getTime();
    if (ms < 0) return null;
    return Math.round(ms / 86_400_000) + 1;
  }
}
