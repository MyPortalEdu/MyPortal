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
import { FieldTree, FormField, applyEach, form, maxLength, required, submit, validate } from '@angular/forms/signals';
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
  StaffPreEmploymentChecksResponse,
  StaffPreEmploymentChecksUpsertRequest,
} from '../../../../../../shared/types/staff-pre-employment-checks';
import { StaffAreaPanel } from './staff-area-panel';

interface DbsRow {
  id: string | null;
  dbsCheckTypeId: string | null;
  certificateNumber: string;
  issueDate: Date | null;
  expiryDate: Date | null;
  updateServiceEnrolled: boolean;
  lastUpdateServiceCheck: Date | null;
  notes: string;
}
interface RtwRow {
  id: string | null;
  documentTypeId: string | null;
  documentNumber: string;
  checkDate: Date | null;
  documentExpiryDate: Date | null;
  followUpDate: Date | null;
  notes: string;
}
interface ReferenceRow {
  id: string | null;
  referenceTypeId: string | null;
  referenceStatusId: string | null;
  refereeName: string;
  refereeOrganisation: string;
  refereeEmail: string;
  requestedDate: Date | null;
  receivedDate: Date | null;
  notes: string;
}
interface OverseasRow {
  id: string | null;
  nationalityId: string | null;
  checkedDate: Date | null;
  isClear: boolean;
  notes: string;
}
interface PreEmploymentModel {
  identityCheckedDate: Date | null;
  prohibitionFromTeachingCheckedDate: Date | null;
  prohibitionFromManagementCheckedDate: Date | null;
  childcareDisqualificationCheckedDate: Date | null;
  medicalFitnessCheckedDate: Date | null;
  qualificationsVerifiedDate: Date | null;
  notes: string;
  dbsChecks: DbsRow[];
  rightToWorkChecks: RtwRow[];
  references: ReferenceRow[];
  overseasChecks: OverseasRow[];
}

@Component({
  selector: 'mp-staff-pre-employment-panel',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    DatePipe,
    FormField,
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
  override readonly editing = signal(false);

  protected readonly preEmployment = signal<StaffPreEmploymentChecksResponse | null>(null);

  protected readonly model = signal<PreEmploymentModel>({
    identityCheckedDate: null,
    prohibitionFromTeachingCheckedDate: null,
    prohibitionFromManagementCheckedDate: null,
    childcareDisqualificationCheckedDate: null,
    medicalFitnessCheckedDate: null,
    qualificationsVerifiedDate: null,
    notes: '',
    dbsChecks: [],
    rightToWorkChecks: [],
    references: [],
    overseasChecks: [],
  });
  protected readonly f = form(this.model, path => {
    applyEach(path.dbsChecks, item => {
      required(item.dbsCheckTypeId);
      required(item.certificateNumber);
      validate(item.certificateNumber, ({ value }) =>
        value().trim().length ? undefined : { kind: 'blank', message: 'common.validation.required' },
      );
      maxLength(item.certificateNumber, 20);
      required(item.issueDate);
    });
    applyEach(path.rightToWorkChecks, item => {
      required(item.documentTypeId);
      required(item.checkDate);
      maxLength(item.documentNumber, 64);
    });
    applyEach(path.references, item => {
      required(item.refereeName);
      validate(item.refereeName, ({ value }) =>
        value().trim().length ? undefined : { kind: 'blank', message: 'common.validation.required' },
      );
      maxLength(item.refereeName, 256);
      maxLength(item.refereeOrganisation, 256);
      maxLength(item.refereeEmail, 256);
    });
    applyEach(path.overseasChecks, item => {
      required(item.nationalityId);
    });
  });
  private readonly snapshot = signal<string>('');

  protected readonly summaryFlags: { key: string; field: FieldTree<Date | null> }[] = [
    { key: 'identityChecked', field: this.f.identityCheckedDate },
    { key: 'prohibitionTeaching', field: this.f.prohibitionFromTeachingCheckedDate },
    { key: 'prohibitionManagement', field: this.f.prohibitionFromManagementCheckedDate },
    { key: 'childcareDisqualification', field: this.f.childcareDisqualificationCheckedDate },
    { key: 'medicalFitness', field: this.f.medicalFitnessCheckedDate },
    { key: 'qualificationsVerified', field: this.f.qualificationsVerifiedDate },
  ];

  protected readonly dbsCheckTypes = computed(() => this.preEmployment()?.dbsCheckTypes ?? []);
  protected readonly rightToWorkDocumentTypes = computed(
    () => this.preEmployment()?.rightToWorkDocumentTypes ?? [],
  );
  protected readonly referenceTypes = computed(() => this.preEmployment()?.referenceTypes ?? []);
  protected readonly referenceStatuses = computed(() => this.preEmployment()?.referenceStatuses ?? []);
  protected readonly countries = computed(() => this.preEmployment()?.countries ?? []);

  override readonly canEdit = computed(() =>
    this.permissions().has(Permissions.Staff.EditAllStaffPreEmploymentChecks),
  );

  override readonly saving = computed(() => this.f().submitting());
  override readonly valid = computed(() => this.f().valid());

  private readonly form = computed(() => JSON.stringify(this.model()));

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
    if (!this.canEdit() || !this.dirty()) return;
    await submit(this.f, async () => {
      const m = this.model();
      const payload: StaffPreEmploymentChecksUpsertRequest = {
        identityCheckedDate: m.identityCheckedDate?.toISOString() ?? null,
        prohibitionFromTeachingCheckedDate: m.prohibitionFromTeachingCheckedDate?.toISOString() ?? null,
        prohibitionFromManagementCheckedDate: m.prohibitionFromManagementCheckedDate?.toISOString() ?? null,
        childcareDisqualificationCheckedDate: m.childcareDisqualificationCheckedDate?.toISOString() ?? null,
        medicalFitnessCheckedDate: m.medicalFitnessCheckedDate?.toISOString() ?? null,
        qualificationsVerifiedDate: m.qualificationsVerifiedDate?.toISOString() ?? null,
        notes: this.normalise(m.notes),
        dbsChecks: m.dbsChecks.map(d => ({
          id: d.id,
          dbsCheckTypeId: d.dbsCheckTypeId,
          certificateNumber: d.certificateNumber.trim(),
          issueDate: d.issueDate?.toISOString() ?? null,
          expiryDate: d.expiryDate?.toISOString() ?? null,
          updateServiceEnrolled: d.updateServiceEnrolled,
          lastUpdateServiceCheck: d.lastUpdateServiceCheck?.toISOString() ?? null,
          notes: this.normalise(d.notes),
        })),
        rightToWorkChecks: m.rightToWorkChecks.map(r => ({
          id: r.id,
          documentTypeId: r.documentTypeId,
          documentNumber: this.normalise(r.documentNumber),
          checkDate: r.checkDate?.toISOString() ?? null,
          documentExpiryDate: r.documentExpiryDate?.toISOString() ?? null,
          followUpDate: r.followUpDate?.toISOString() ?? null,
          notes: this.normalise(r.notes),
        })),
        references: m.references.map(r => ({
          id: r.id,
          referenceTypeId: r.referenceTypeId,
          referenceStatusId: r.referenceStatusId,
          refereeName: r.refereeName.trim(),
          refereeOrganisation: this.normalise(r.refereeOrganisation),
          refereeEmail: this.normalise(r.refereeEmail),
          requestedDate: r.requestedDate?.toISOString() ?? null,
          receivedDate: r.receivedDate?.toISOString() ?? null,
          notes: this.normalise(r.notes),
        })),
        overseasChecks: m.overseasChecks.map(o => ({
          id: o.id,
          nationalityId: o.nationalityId,
          checkedDate: o.checkedDate?.toISOString() ?? null,
          isClear: o.isClear,
          notes: this.normalise(o.notes),
        })),
      };
      try {
        await firstValueFrom(this.data.updatePreEmploymentChecks(this.staffMemberId(), payload));
      } catch (err) {
        this.notify.apiError(err, this.transloco.translate('staff-members.savePreEmploymentError'));
        return;
      }
      this.notify.success(this.transloco.translate('staff-members.savedPreEmploymentToast'));
      this.editing.set(false);
      this.load();
    });
  }

  private apply(row: StaffPreEmploymentChecksResponse | null): void {
    this.model.set({
      identityCheckedDate: this.toDate(row?.identityCheckedDate),
      prohibitionFromTeachingCheckedDate: this.toDate(row?.prohibitionFromTeachingCheckedDate),
      prohibitionFromManagementCheckedDate: this.toDate(row?.prohibitionFromManagementCheckedDate),
      childcareDisqualificationCheckedDate: this.toDate(row?.childcareDisqualificationCheckedDate),
      medicalFitnessCheckedDate: this.toDate(row?.medicalFitnessCheckedDate),
      qualificationsVerifiedDate: this.toDate(row?.qualificationsVerifiedDate),
      notes: row?.notes ?? '',
      dbsChecks: (row?.dbsChecks ?? []).map(d => ({
        id: d.id,
        dbsCheckTypeId: d.dbsCheckTypeId,
        certificateNumber: d.certificateNumber,
        issueDate: this.toDate(d.issueDate),
        expiryDate: this.toDate(d.expiryDate),
        updateServiceEnrolled: d.updateServiceEnrolled,
        lastUpdateServiceCheck: this.toDate(d.lastUpdateServiceCheck),
        notes: d.notes ?? '',
      })),
      rightToWorkChecks: (row?.rightToWorkChecks ?? []).map(r => ({
        id: r.id,
        documentTypeId: r.documentTypeId,
        documentNumber: r.documentNumber ?? '',
        checkDate: this.toDate(r.checkDate),
        documentExpiryDate: this.toDate(r.documentExpiryDate),
        followUpDate: this.toDate(r.followUpDate),
        notes: r.notes ?? '',
      })),
      references: (row?.references ?? []).map(r => ({
        id: r.id,
        referenceTypeId: r.referenceTypeId ?? null,
        referenceStatusId: r.referenceStatusId ?? null,
        refereeName: r.refereeName,
        refereeOrganisation: r.refereeOrganisation ?? '',
        refereeEmail: r.refereeEmail ?? '',
        requestedDate: this.toDate(r.requestedDate),
        receivedDate: this.toDate(r.receivedDate),
        notes: r.notes ?? '',
      })),
      overseasChecks: (row?.overseasChecks ?? []).map(o => ({
        id: o.id,
        nationalityId: o.nationalityId,
        checkedDate: this.toDate(o.checkedDate),
        isClear: o.isClear,
        notes: o.notes ?? '',
      })),
    });
    this.f().reset();
    this.snapshot.set(this.form());
  }

  protected addDbsCheck(): void {
    this.model.update(m => ({
      ...m,
      dbsChecks: [
        ...m.dbsChecks,
        {
          id: null,
          dbsCheckTypeId: null,
          certificateNumber: '',
          issueDate: null,
          expiryDate: null,
          updateServiceEnrolled: false,
          lastUpdateServiceCheck: null,
          notes: '',
        },
      ],
    }));
  }

  protected removeDbsCheck(index: number): void {
    this.model.update(m => ({ ...m, dbsChecks: m.dbsChecks.filter((_, i) => i !== index) }));
  }

  protected addRightToWork(): void {
    this.model.update(m => ({
      ...m,
      rightToWorkChecks: [
        ...m.rightToWorkChecks,
        {
          id: null,
          documentTypeId: null,
          documentNumber: '',
          checkDate: null,
          documentExpiryDate: null,
          followUpDate: null,
          notes: '',
        },
      ],
    }));
  }

  protected removeRightToWork(index: number): void {
    this.model.update(m => ({ ...m, rightToWorkChecks: m.rightToWorkChecks.filter((_, i) => i !== index) }));
  }

  protected addReference(): void {
    this.model.update(m => ({
      ...m,
      references: [
        ...m.references,
        {
          id: null,
          referenceTypeId: null,
          referenceStatusId: null,
          refereeName: '',
          refereeOrganisation: '',
          refereeEmail: '',
          requestedDate: null,
          receivedDate: null,
          notes: '',
        },
      ],
    }));
  }

  protected removeReference(index: number): void {
    this.model.update(m => ({ ...m, references: m.references.filter((_, i) => i !== index) }));
  }

  protected addOverseasCheck(): void {
    this.model.update(m => ({
      ...m,
      overseasChecks: [
        ...m.overseasChecks,
        { id: null, nationalityId: null, checkedDate: null, isClear: false, notes: '' },
      ],
    }));
  }

  protected removeOverseasCheck(index: number): void {
    this.model.update(m => ({ ...m, overseasChecks: m.overseasChecks.filter((_, i) => i !== index) }));
  }
}
