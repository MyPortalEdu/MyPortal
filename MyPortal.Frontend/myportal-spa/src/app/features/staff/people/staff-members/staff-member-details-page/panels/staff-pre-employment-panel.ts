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
import { MpButton, MpCheckbox, MpDatePicker, MpInput, MpTextarea } from '@myportal/ui';
import { firstValueFrom } from 'rxjs';
import {
  TranslocoDirective,
  TranslocoService,
  provideTranslocoScope,
} from '@jsverse/transloco';

import { NotificationService } from '../../../../../../core/services/notification.service';
import { Permissions } from '../../../../../../core/constants/permissions';
import { StaffMembersDataService } from '../../../../../../shared/services/staff-members-data.service';
import { LookupSelect } from '../../../../../../shared/components/lookup-select/lookup-select';
import { Loading } from '../../../../../../shared/components/loading/loading';
import { EmptyState } from '../../../../../../shared/components/empty-state/empty-state';
import { SectionHeader } from '../../../../../../shared/components/section-header/section-header';
import { Field } from '../../../../../../shared/components/field/field';
import { Callout } from '../../../../../../shared/components/callout/callout';
import {
  DbsCheckUpsertItem,
  RightToWorkCheckUpsertItem,
  StaffOverseasCheckUpsertItem,
  StaffPreEmploymentChecksResponse,
  StaffPreEmploymentChecksUpsertRequest,
  StaffReferenceUpsertItem,
} from '../../../../../../shared/types/staff-pre-employment-checks';
import { StaffAreaPanel } from './staff-area-panel';

@Component({
  selector: 'mp-staff-pre-employment-panel',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    DatePipe,
    FormsModule,
    MpButton,
    MpCheckbox,
    MpDatePicker,
    MpInput,
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
    { provide: StaffAreaPanel, useExisting: forwardRef(() => StaffPreEmploymentPanel) },
  ],
  templateUrl: './staff-pre-employment-panel.html',
})
export class StaffPreEmploymentPanel extends StaffAreaPanel implements OnInit {
  private readonly data = inject(StaffMembersDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);

  readonly staffMemberId = input.required<string>();
  readonly permissions = input.required<Set<string>>();

  protected readonly loading = signal(false);
  override readonly saving = signal(false);
  override readonly editing = signal(false);

  protected readonly preEmployment = signal<StaffPreEmploymentChecksResponse | null>(null);

  protected readonly identityCheckedDate = signal<Date | null>(null);
  protected readonly prohibitionFromTeachingCheckedDate = signal<Date | null>(null);
  protected readonly prohibitionFromManagementCheckedDate = signal<Date | null>(null);
  protected readonly childcareDisqualificationCheckedDate = signal<Date | null>(null);
  protected readonly medicalFitnessCheckedDate = signal<Date | null>(null);
  protected readonly qualificationsVerifiedDate = signal<Date | null>(null);
  protected readonly preEmploymentNotes = signal<string | null>(null);

  protected readonly dbsChecks = signal<DbsCheckUpsertItem[]>([]);
  protected readonly rightToWorkChecks = signal<RightToWorkCheckUpsertItem[]>([]);
  protected readonly references = signal<StaffReferenceUpsertItem[]>([]);
  protected readonly overseasChecks = signal<StaffOverseasCheckUpsertItem[]>([]);
  private readonly snapshot = signal<string>('');

  protected readonly dbsCheckTypes = computed(() => this.preEmployment()?.dbsCheckTypes ?? []);
  protected readonly rightToWorkDocumentTypes = computed(
    () => this.preEmployment()?.rightToWorkDocumentTypes ?? [],
  );
  protected readonly referenceTypes = computed(() => this.preEmployment()?.referenceTypes ?? []);
  protected readonly referenceStatuses = computed(() => this.preEmployment()?.referenceStatuses ?? []);
  protected readonly countries = computed(() => this.preEmployment()?.countries ?? []);

  protected readonly summaryFlags: { key: string; get: () => Date | null; set: (v: Date | null) => void }[] = [
    { key: 'identityChecked', get: () => this.identityCheckedDate(), set: v => this.identityCheckedDate.set(v) },
    {
      key: 'prohibitionTeaching',
      get: () => this.prohibitionFromTeachingCheckedDate(),
      set: v => this.prohibitionFromTeachingCheckedDate.set(v),
    },
    {
      key: 'prohibitionManagement',
      get: () => this.prohibitionFromManagementCheckedDate(),
      set: v => this.prohibitionFromManagementCheckedDate.set(v),
    },
    {
      key: 'childcareDisqualification',
      get: () => this.childcareDisqualificationCheckedDate(),
      set: v => this.childcareDisqualificationCheckedDate.set(v),
    },
    {
      key: 'medicalFitness',
      get: () => this.medicalFitnessCheckedDate(),
      set: v => this.medicalFitnessCheckedDate.set(v),
    },
    {
      key: 'qualificationsVerified',
      get: () => this.qualificationsVerifiedDate(),
      set: v => this.qualificationsVerifiedDate.set(v),
    },
  ];

  override readonly canEdit = computed(() =>
    this.permissions().has(Permissions.Staff.EditAllStaffPreEmploymentChecks),
  );

  override readonly valid = computed(
    () =>
      this.dbsChecks().every(
        d => !!d.dbsCheckTypeId && d.certificateNumber.trim().length > 0 && !!d.issueDate,
      ) &&
      this.rightToWorkChecks().every(r => !!r.documentTypeId && !!r.checkDate) &&
      this.references().every(r => r.refereeName.trim().length > 0) &&
      this.overseasChecks().every(o => !!o.nationalityId),
  );

  private readonly form = computed(() =>
    JSON.stringify({
      identityCheckedDate: this.identityCheckedDate()?.toISOString() ?? null,
      prohibitionFromTeachingCheckedDate: this.prohibitionFromTeachingCheckedDate()?.toISOString() ?? null,
      prohibitionFromManagementCheckedDate: this.prohibitionFromManagementCheckedDate()?.toISOString() ?? null,
      childcareDisqualificationCheckedDate:
        this.childcareDisqualificationCheckedDate()?.toISOString() ?? null,
      medicalFitnessCheckedDate: this.medicalFitnessCheckedDate()?.toISOString() ?? null,
      qualificationsVerifiedDate: this.qualificationsVerifiedDate()?.toISOString() ?? null,
      notes: this.preEmploymentNotes(),
      dbsChecks: this.dbsChecks(),
      rightToWorkChecks: this.rightToWorkChecks(),
      references: this.references(),
      overseasChecks: this.overseasChecks(),
    }),
  );

  override readonly dirty = computed(
    () => this.preEmployment() != null && this.snapshot() !== this.form(),
  );

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.data.getPreEmploymentChecks(this.staffMemberId()).subscribe({
      next: row => {
        this.preEmployment.set(row);
        this.apply(row);
        this.loading.set(false);
      },
      error: err => {
        this.loading.set(false);
        this.notify.apiError(err, this.transloco.translate('staff-members.loadPreEmploymentError'));
      },
    });
  }

  override startEdit(): void {
    this.editing.set(true);
  }

  override cancel(): void {
    this.apply(this.preEmployment());
    this.editing.set(false);
  }

  override async save(): Promise<void> {
    if (!this.canEdit() || !this.valid() || this.saving()) return;
    this.saving.set(true);

    const payload: StaffPreEmploymentChecksUpsertRequest = {
      identityCheckedDate: this.identityCheckedDate()?.toISOString() ?? null,
      prohibitionFromTeachingCheckedDate: this.prohibitionFromTeachingCheckedDate()?.toISOString() ?? null,
      prohibitionFromManagementCheckedDate: this.prohibitionFromManagementCheckedDate()?.toISOString() ?? null,
      childcareDisqualificationCheckedDate:
        this.childcareDisqualificationCheckedDate()?.toISOString() ?? null,
      medicalFitnessCheckedDate: this.medicalFitnessCheckedDate()?.toISOString() ?? null,
      qualificationsVerifiedDate: this.qualificationsVerifiedDate()?.toISOString() ?? null,
      notes: this.normalise(this.preEmploymentNotes()),
      dbsChecks: this.dbsChecks().map(d => ({
        id: d.id ?? null,
        dbsCheckTypeId: d.dbsCheckTypeId,
        certificateNumber: d.certificateNumber.trim(),
        issueDate: d.issueDate,
        expiryDate: d.expiryDate ?? null,
        updateServiceEnrolled: d.updateServiceEnrolled,
        lastUpdateServiceCheck: d.lastUpdateServiceCheck ?? null,
        notes: this.normalise(d.notes),
      })),
      rightToWorkChecks: this.rightToWorkChecks().map(r => ({
        id: r.id ?? null,
        documentTypeId: r.documentTypeId,
        documentNumber: this.normalise(r.documentNumber),
        checkDate: r.checkDate,
        documentExpiryDate: r.documentExpiryDate ?? null,
        followUpDate: r.followUpDate ?? null,
        notes: this.normalise(r.notes),
      })),
      references: this.references().map(r => ({
        id: r.id ?? null,
        referenceTypeId: r.referenceTypeId ?? null,
        referenceStatusId: r.referenceStatusId ?? null,
        refereeName: r.refereeName.trim(),
        refereeOrganisation: this.normalise(r.refereeOrganisation),
        refereeEmail: this.normalise(r.refereeEmail),
        requestedDate: r.requestedDate ?? null,
        receivedDate: r.receivedDate ?? null,
        notes: this.normalise(r.notes),
      })),
      overseasChecks: this.overseasChecks().map(o => ({
        id: o.id ?? null,
        nationalityId: o.nationalityId,
        checkedDate: o.checkedDate ?? null,
        isClear: o.isClear,
        notes: this.normalise(o.notes),
      })),
    };

    try {
      await firstValueFrom(this.data.updatePreEmploymentChecks(this.staffMemberId(), payload));
      this.notify.success(this.transloco.translate('staff-members.savedPreEmploymentToast'));
      this.editing.set(false);
      this.load();
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('staff-members.savePreEmploymentError'));
    } finally {
      this.saving.set(false);
    }
  }

  private apply(row: StaffPreEmploymentChecksResponse | null): void {
    this.identityCheckedDate.set(this.toDate(row?.identityCheckedDate));
    this.prohibitionFromTeachingCheckedDate.set(this.toDate(row?.prohibitionFromTeachingCheckedDate));
    this.prohibitionFromManagementCheckedDate.set(this.toDate(row?.prohibitionFromManagementCheckedDate));
    this.childcareDisqualificationCheckedDate.set(this.toDate(row?.childcareDisqualificationCheckedDate));
    this.medicalFitnessCheckedDate.set(this.toDate(row?.medicalFitnessCheckedDate));
    this.qualificationsVerifiedDate.set(this.toDate(row?.qualificationsVerifiedDate));
    this.preEmploymentNotes.set(row?.notes ?? null);
    this.dbsChecks.set(
      (row?.dbsChecks ?? []).map(d => ({
        id: d.id,
        dbsCheckTypeId: d.dbsCheckTypeId,
        certificateNumber: d.certificateNumber,
        issueDate: d.issueDate,
        expiryDate: d.expiryDate ?? null,
        updateServiceEnrolled: d.updateServiceEnrolled,
        lastUpdateServiceCheck: d.lastUpdateServiceCheck ?? null,
        notes: d.notes ?? null,
      })),
    );
    this.rightToWorkChecks.set(
      (row?.rightToWorkChecks ?? []).map(r => ({
        id: r.id,
        documentTypeId: r.documentTypeId,
        documentNumber: r.documentNumber ?? null,
        checkDate: r.checkDate,
        documentExpiryDate: r.documentExpiryDate ?? null,
        followUpDate: r.followUpDate ?? null,
        notes: r.notes ?? null,
      })),
    );
    this.references.set(
      (row?.references ?? []).map(r => ({
        id: r.id,
        referenceTypeId: r.referenceTypeId ?? null,
        referenceStatusId: r.referenceStatusId ?? null,
        refereeName: r.refereeName,
        refereeOrganisation: r.refereeOrganisation ?? null,
        refereeEmail: r.refereeEmail ?? null,
        requestedDate: r.requestedDate ?? null,
        receivedDate: r.receivedDate ?? null,
        notes: r.notes ?? null,
      })),
    );
    this.overseasChecks.set(
      (row?.overseasChecks ?? []).map(o => ({
        id: o.id,
        nationalityId: o.nationalityId,
        checkedDate: o.checkedDate ?? null,
        isClear: o.isClear,
        notes: o.notes ?? null,
      })),
    );
    this.snapshot.set(this.form());
  }

  protected addDbsCheck(): void {
    this.dbsChecks.update(rows => [
      ...rows,
      {
        id: null,
        dbsCheckTypeId: null,
        certificateNumber: '',
        issueDate: null,
        expiryDate: null,
        updateServiceEnrolled: false,
        lastUpdateServiceCheck: null,
        notes: null,
      },
    ]);
  }

  protected removeDbsCheck(index: number): void {
    this.dbsChecks.update(rows => rows.filter((_, i) => i !== index));
  }

  protected patchDbsCheck<K extends keyof DbsCheckUpsertItem>(
    index: number,
    key: K,
    value: DbsCheckUpsertItem[K],
  ): void {
    this.dbsChecks.update(rows => rows.map((row, i) => (i === index ? { ...row, [key]: value } : row)));
  }

  protected addRightToWork(): void {
    this.rightToWorkChecks.update(rows => [
      ...rows,
      {
        id: null,
        documentTypeId: null,
        documentNumber: null,
        checkDate: null,
        documentExpiryDate: null,
        followUpDate: null,
        notes: null,
      },
    ]);
  }

  protected removeRightToWork(index: number): void {
    this.rightToWorkChecks.update(rows => rows.filter((_, i) => i !== index));
  }

  protected patchRightToWork<K extends keyof RightToWorkCheckUpsertItem>(
    index: number,
    key: K,
    value: RightToWorkCheckUpsertItem[K],
  ): void {
    this.rightToWorkChecks.update(rows =>
      rows.map((row, i) => (i === index ? { ...row, [key]: value } : row)),
    );
  }

  protected addReference(): void {
    this.references.update(rows => [
      ...rows,
      {
        id: null,
        referenceTypeId: null,
        referenceStatusId: null,
        refereeName: '',
        refereeOrganisation: null,
        refereeEmail: null,
        requestedDate: null,
        receivedDate: null,
        notes: null,
      },
    ]);
  }

  protected removeReference(index: number): void {
    this.references.update(rows => rows.filter((_, i) => i !== index));
  }

  protected patchReference<K extends keyof StaffReferenceUpsertItem>(
    index: number,
    key: K,
    value: StaffReferenceUpsertItem[K],
  ): void {
    this.references.update(rows => rows.map((row, i) => (i === index ? { ...row, [key]: value } : row)));
  }

  protected addOverseasCheck(): void {
    this.overseasChecks.update(rows => [
      ...rows,
      { id: null, nationalityId: null, checkedDate: null, isClear: false, notes: null },
    ]);
  }

  protected removeOverseasCheck(index: number): void {
    this.overseasChecks.update(rows => rows.filter((_, i) => i !== index));
  }

  protected patchOverseasCheck<K extends keyof StaffOverseasCheckUpsertItem>(
    index: number,
    key: K,
    value: StaffOverseasCheckUpsertItem[K],
  ): void {
    this.overseasChecks.update(rows => rows.map((row, i) => (i === index ? { ...row, [key]: value } : row)));
  }
}
