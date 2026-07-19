import {
  ChangeDetectionStrategy,
  Component,
  HostListener,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { FormField, disabled, form, maxLength, required, submit, validate } from '@angular/forms/signals';
import { MpButton, MpSelect, MpDatePicker, MpFormField, MpInput, MpInputNumber, MpCheckbox } from '@myportal/ui';
import { firstValueFrom } from 'rxjs';
import { TranslocoDirective, TranslocoService, provideTranslocoScope } from '@jsverse/transloco';

import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { Loading } from '../../../../shared/components/loading/loading';
import { SectionHeader } from '../../../../shared/components/section-header/section-header';
import { Field } from '../../../../shared/components/field/field';
import { HeaderAction } from '../../../../shared/types/header-action.type';
import { CanComponentDeactivate } from '../../../../core/guards/can-deactivate.guard';
import { ConfirmationDialog } from '../../../../core/services/confirmation.service';
import { LocalAuthorityPicker } from '../../../../shared/components/pickers/local-authority-picker/local-authority-picker';
import { StaffMemberPicker } from '../../../../shared/components/pickers/staff-member-picker/staff-member-picker';
import { LookupsDataService } from '../../../../shared/services/lookups-data.service';
import { SchoolsDataService } from '../../../../shared/services/schools-data.service';
import { SchoolService } from '../../../../core/services/school-service';
import { NotificationService } from '../../../../core/services/notification.service';
import { MeService } from '../../../../core/services/me-service';
import { LookupResponse } from '../../../../shared/types/lookup';
import {
  LocalAuthoritySummaryResponse,
  SchoolDetailsResponse,
  SchoolUpsertRequest,
} from '../../../../shared/types/school';
import { StaffMemberSummaryResponse } from '../../../../shared/types/staff-member';
import { Permissions } from '../../../../core/constants/permissions';

type FormSnapshot = {
  name: string;
  website: string | null;
  urn: string;
  uprn: string;
  establishmentNumber: number | null;
  localAuthorityId: string | null;
  schoolPhaseId: string | null;
  schoolTypeId: string | null;
  governanceTypeId: string | null;
  intakeTypeId: string | null;
  headTeacherId: string | null;
  ukprn: string | null;
  payZoneId: string | null;
  lowestAge: number | null;
  highestAge: number | null;
  netCapacity: number | null;
  netCapacityAssessmentDate: string | null;
  isSpecialSchool: boolean;
  specialSchoolOrganisationId: string | null;
  specialSchoolTypeId: string | null;
  maxBoarders: number | null;
  telephone: string | null;
  email: string | null;
};

interface SchoolFormModel {
  name: string;
  website: string;
  urn: string;
  uprn: string;
  establishmentNumber: number | null;
  schoolPhaseId: string | null;
  schoolTypeId: string | null;
  governanceTypeId: string | null;
  intakeTypeId: string | null;
  ukprn: string;
  payZoneId: string | null;
  lowestAge: number | null;
  highestAge: number | null;
  netCapacity: number | null;
  netCapacityAssessmentDate: Date | null;
  isSpecialSchool: boolean;
  specialSchoolOrganisationId: string | null;
  specialSchoolTypeId: string | null;
  maxBoarders: number | null;
  telephone: string;
  email: string;
}

@Component({
  selector: 'mp-school-details-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    FormField,
    MpButton,
    MpInput,
    MpInputNumber,
    MpSelect,
    MpCheckbox,
    MpDatePicker,
    MpFormField,
    PageHeader,
    Loading,
    SectionHeader,
    Field,
    LocalAuthorityPicker,
    StaffMemberPicker,
    TranslocoDirective,
  ],
  providers: [provideTranslocoScope('school-details')],
  templateUrl: './school-details-page.html',
})
export class SchoolDetailsPage implements OnInit, CanComponentDeactivate {
  private readonly schools = inject(SchoolsDataService);
  private readonly schoolNameCache = inject(SchoolService);
  private readonly lookups = inject(LookupsDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);
  private readonly me = inject(MeService);
  private readonly confirm = inject(ConfirmationDialog);

  readonly canEdit = signal(false);
  readonly loading = signal(false);

  readonly current = signal<SchoolDetailsResponse | null>(null);

  readonly governanceTypes = signal<LookupResponse[]>([]);
  readonly intakeTypes = signal<LookupResponse[]>([]);
  readonly schoolPhases = signal<LookupResponse[]>([]);
  readonly schoolTypes = signal<LookupResponse[]>([]);
  readonly payZones = signal<LookupResponse[]>([]);
  readonly specialSchoolOrganisations = signal<LookupResponse[]>([]);
  readonly specialSchoolTypes = signal<LookupResponse[]>([]);

  protected readonly model = signal<SchoolFormModel>({
    name: '',
    website: '',
    urn: '',
    uprn: '',
    establishmentNumber: null,
    schoolPhaseId: null,
    schoolTypeId: null,
    governanceTypeId: null,
    intakeTypeId: null,
    ukprn: '',
    payZoneId: null,
    lowestAge: null,
    highestAge: null,
    netCapacity: null,
    netCapacityAssessmentDate: null,
    isSpecialSchool: false,
    specialSchoolOrganisationId: null,
    specialSchoolTypeId: null,
    maxBoarders: null,
    telephone: '',
    email: '',
  });
  protected readonly f = form(this.model, path => {
    disabled(path, () => !this.canEdit());
    required(path.name);
    validate(path.name, ({ value }) =>
      value().trim().length ? undefined : { kind: 'blank', message: 'common.validation.required' },
    );
    required(path.urn);
    validate(path.urn, ({ value }) =>
      value().trim().length ? undefined : { kind: 'blank', message: 'common.validation.required' },
    );
    required(path.uprn);
    validate(path.uprn, ({ value }) =>
      value().trim().length ? undefined : { kind: 'blank', message: 'common.validation.required' },
    );
    required(path.establishmentNumber);
    required(path.schoolPhaseId);
    required(path.schoolTypeId);
    required(path.governanceTypeId);
    required(path.intakeTypeId);
    maxLength(path.ukprn, 8);
    maxLength(path.telephone, 30);
    maxLength(path.email, 256);
  });

  readonly localAuthorityId = signal<string | null>(null);
  readonly localAuthorityName = signal<string | null>(null);
  readonly headTeacherId = signal<string | null>(null);
  readonly headTeacherFullName = signal<string | null>(null);

  private readonly snapshot = signal<FormSnapshot | null>(null);

  private readonly currentForm = computed<FormSnapshot>(() => {
    const m = this.model();
    return {
      name: m.name,
      website: m.website,
      urn: m.urn,
      uprn: m.uprn,
      establishmentNumber: m.establishmentNumber,
      localAuthorityId: this.localAuthorityId(),
      schoolPhaseId: m.schoolPhaseId,
      schoolTypeId: m.schoolTypeId,
      governanceTypeId: m.governanceTypeId,
      intakeTypeId: m.intakeTypeId,
      headTeacherId: this.headTeacherId(),
      ukprn: m.ukprn,
      payZoneId: m.payZoneId,
      lowestAge: m.lowestAge,
      highestAge: m.highestAge,
      netCapacity: m.netCapacity,
      netCapacityAssessmentDate: m.netCapacityAssessmentDate?.toISOString() ?? null,
      isSpecialSchool: m.isSpecialSchool,
      specialSchoolOrganisationId: m.specialSchoolOrganisationId,
      specialSchoolTypeId: m.specialSchoolTypeId,
      maxBoarders: m.maxBoarders,
      telephone: m.telephone,
      email: m.email,
    };
  });

  readonly isDirty = computed(() => {
    const s = this.snapshot();
    if (!s) return false;
    return JSON.stringify(s) !== JSON.stringify(this.currentForm());
  });

  readonly headerActions = computed<HeaderAction[]>(() =>
    this.canEdit()
      ? [{
          label: this.transloco.translate('school-details.save'),
          icon: 'fa-solid fa-check',
          disabled: !this.isDirty(),
          loading: this.f().submitting(),
          command: () => this.save(),
        }]
      : []
  );

  ngOnInit(): void {
    this.me.me().subscribe(me => {
      this.canEdit.set(me.permissions?.includes(Permissions.Agencies.EditAgencies) ?? false);
    });

    this.refresh();

    this.lookups.governanceTypes().subscribe(rows => this.governanceTypes.set(rows ?? []));
    this.lookups.intakeTypes().subscribe(rows => this.intakeTypes.set(rows ?? []));
    this.lookups.schoolPhases().subscribe(rows => this.schoolPhases.set(rows ?? []));
    this.lookups.schoolTypes().subscribe(rows => this.schoolTypes.set(rows ?? []));
    this.lookups.payZones().subscribe(rows => this.payZones.set(rows ?? []));
    this.lookups.specialSchoolOrganisations().subscribe(rows => this.specialSchoolOrganisations.set(rows ?? []));
    this.lookups.specialSchoolTypes().subscribe(rows => this.specialSchoolTypes.set(rows ?? []));
  }

  refresh(): void {
    this.loading.set(true);
    this.schools.getLocalDetails().subscribe({
      next: school => {
        this.current.set(school);
        this.applyToForm(school);
        this.loading.set(false);
      },
      error: err => {
        this.loading.set(false);
        this.notify.apiError(err, this.transloco.translate('school-details.loadError'));
      },
    });
  }

  onLocalAuthorityPicked(la: LocalAuthoritySummaryResponse): void {
    this.localAuthorityId.set(la.id);
    this.localAuthorityName.set(la.name);
  }

  clearLocalAuthority(): void {
    this.localAuthorityId.set(null);
    this.localAuthorityName.set(null);
  }

  onHeadTeacherPicked(s: StaffMemberSummaryResponse): void {
    this.headTeacherId.set(s.personId);
    this.headTeacherFullName.set(this.formatName(s));
  }

  clearHeadTeacher(): void {
    this.headTeacherId.set(null);
    this.headTeacherFullName.set(null);
  }

  save(): Promise<boolean> | void {
    if (!this.canEdit() || !this.isDirty()) return;
    return submit(this.f, async () => {
      const m = this.model();
      const payload: SchoolUpsertRequest = {
        name: m.name.trim(),
        website: this.normalise(m.website),
        expectedVersion: 0,
        urn: m.urn.trim(),
        uprn: m.uprn.trim(),
        establishmentNumber: m.establishmentNumber ?? 0,
        localAuthorityId: this.localAuthorityId(),
        schoolPhaseId: m.schoolPhaseId!,
        schoolTypeId: m.schoolTypeId!,
        governanceTypeId: m.governanceTypeId!,
        intakeTypeId: m.intakeTypeId!,
        headTeacherId: this.headTeacherId(),
        ukprn: this.normalise(m.ukprn),
        payZoneId: m.payZoneId,
        lowestAge: m.lowestAge,
        highestAge: m.highestAge,
        netCapacity: m.netCapacity,
        netCapacityAssessmentDate: m.netCapacityAssessmentDate?.toISOString() ?? null,
        isSpecialSchool: m.isSpecialSchool,
        specialSchoolOrganisationId: m.isSpecialSchool ? m.specialSchoolOrganisationId : null,
        specialSchoolTypeId: m.isSpecialSchool ? m.specialSchoolTypeId : null,
        maxBoarders: m.isSpecialSchool ? m.maxBoarders : null,
        telephone: this.normalise(m.telephone),
        email: this.normalise(m.email),
      };

      try {
        await firstValueFrom(this.schools.saveLocalDetails(payload));
      } catch (err) {
        this.notify.apiError(err, this.transloco.translate('school-details.saveError'));
        return;
      }
      this.schoolNameCache.clearCache();
      this.notify.success(this.transloco.translate('school-details.savedToast'));
      this.refresh();
    });
  }

  private applyToForm(school: SchoolDetailsResponse | null): void {
    this.model.set({
      name: school?.name ?? '',
      website: school?.website ?? '',
      urn: school?.urn ?? '',
      uprn: school?.uprn ?? '',
      establishmentNumber: school?.establishmentNumber ?? null,
      schoolPhaseId: school?.schoolPhaseId ?? null,
      schoolTypeId: school?.schoolTypeId ?? null,
      governanceTypeId: school?.governanceTypeId ?? null,
      intakeTypeId: school?.intakeTypeId ?? null,
      ukprn: school?.ukprn ?? '',
      payZoneId: school?.payZoneId ?? null,
      lowestAge: school?.lowestAge ?? null,
      highestAge: school?.highestAge ?? null,
      netCapacity: school?.netCapacity ?? null,
      netCapacityAssessmentDate: school?.netCapacityAssessmentDate
        ? new Date(school.netCapacityAssessmentDate)
        : null,
      isSpecialSchool: school?.isSpecialSchool ?? false,
      specialSchoolOrganisationId: school?.specialSchoolOrganisationId ?? null,
      specialSchoolTypeId: school?.specialSchoolTypeId ?? null,
      maxBoarders: school?.maxBoarders ?? null,
      telephone: school?.telephone ?? '',
      email: school?.email ?? '',
    });
    this.localAuthorityId.set(school?.localAuthorityId ?? null);
    this.localAuthorityName.set(school?.localAuthorityName ?? null);
    this.headTeacherId.set(school?.headTeacherId ?? null);
    this.headTeacherFullName.set(school?.headTeacherFullName ?? null);
    this.f().reset();
    this.snapshot.set(this.currentForm());
  }

  async canDeactivate(): Promise<boolean> {
    if (!this.isDirty()) return true;
    return this.confirm.confirm({
      header: this.transloco.translate('common.discardChanges'),
      message: this.transloco.translate('common.discardConfirm'),
      acceptLabel: this.transloco.translate('common.discard'),
      acceptSeverity: 'danger',
    });
  }

  @HostListener('window:beforeunload', ['$event'])
  onBeforeUnload(event: BeforeUnloadEvent): void {
    if (this.isDirty()) {
      event.preventDefault();
      event.returnValue = '';
    }
  }

  private formatName(s: StaffMemberSummaryResponse): string {
    const first = s.preferredFirstName ?? s.firstName;
    const last = s.preferredLastName ?? s.lastName;
    return [s.title, first, last].filter(Boolean).join(' ').trim();
  }

  private normalise(value: string | null | undefined): string | null {
    if (value == null) return null;
    const trimmed = value.trim();
    return trimmed.length === 0 ? null : trimmed;
  }
}
