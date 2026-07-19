import {
  ChangeDetectionStrategy,
  Component,
  HostListener,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MpButton, MpSelect, MpDatePicker, MpInput, MpInputNumber, MpCheckbox } from '@myportal/ui';
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

@Component({
  selector: 'mp-school-details-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    FormsModule,
    MpButton,
    MpInput,
    MpInputNumber,
    MpSelect,
    MpCheckbox,
    MpDatePicker,
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
  readonly saving = signal(false);

  readonly current = signal<SchoolDetailsResponse | null>(null);

  readonly governanceTypes = signal<LookupResponse[]>([]);
  readonly intakeTypes = signal<LookupResponse[]>([]);
  readonly schoolPhases = signal<LookupResponse[]>([]);
  readonly schoolTypes = signal<LookupResponse[]>([]);
  readonly payZones = signal<LookupResponse[]>([]);
  readonly specialSchoolOrganisations = signal<LookupResponse[]>([]);
  readonly specialSchoolTypes = signal<LookupResponse[]>([]);

  readonly name = signal('');
  readonly website = signal<string | null>(null);
  readonly urn = signal('');
  readonly uprn = signal('');
  readonly establishmentNumber = signal<number | null>(null);
  readonly localAuthorityId = signal<string | null>(null);
  readonly localAuthorityName = signal<string | null>(null);
  readonly schoolPhaseId = signal<string | null>(null);
  readonly schoolTypeId = signal<string | null>(null);
  readonly governanceTypeId = signal<string | null>(null);
  readonly intakeTypeId = signal<string | null>(null);
  readonly headTeacherId = signal<string | null>(null);
  readonly headTeacherFullName = signal<string | null>(null);
  readonly ukprn = signal<string | null>(null);
  readonly payZoneId = signal<string | null>(null);
  readonly lowestAge = signal<number | null>(null);
  readonly highestAge = signal<number | null>(null);
  readonly netCapacity = signal<number | null>(null);
  readonly netCapacityAssessmentDate = signal<Date | null>(null);
  readonly isSpecialSchool = signal<boolean>(false);
  readonly specialSchoolOrganisationId = signal<string | null>(null);
  readonly specialSchoolTypeId = signal<string | null>(null);
  readonly maxBoarders = signal<number | null>(null);
  readonly telephone = signal<string | null>(null);
  readonly email = signal<string | null>(null);

  readonly isValid = computed(() =>
    this.name().trim().length > 0 &&
    this.urn().trim().length > 0 &&
    this.uprn().trim().length > 0 &&
    this.establishmentNumber() != null &&
    !!this.schoolPhaseId() &&
    !!this.schoolTypeId() &&
    !!this.governanceTypeId() &&
    !!this.intakeTypeId(),
  );

  private readonly snapshot = signal<FormSnapshot | null>(null);

  private readonly currentForm = computed<FormSnapshot>(() => ({
    name: this.name(),
    website: this.website(),
    urn: this.urn(),
    uprn: this.uprn(),
    establishmentNumber: this.establishmentNumber(),
    localAuthorityId: this.localAuthorityId(),
    schoolPhaseId: this.schoolPhaseId(),
    schoolTypeId: this.schoolTypeId(),
    governanceTypeId: this.governanceTypeId(),
    intakeTypeId: this.intakeTypeId(),
    headTeacherId: this.headTeacherId(),
    ukprn: this.ukprn(),
    payZoneId: this.payZoneId(),
    lowestAge: this.lowestAge(),
    highestAge: this.highestAge(),
    netCapacity: this.netCapacity(),
    netCapacityAssessmentDate: this.netCapacityAssessmentDate()?.toISOString() ?? null,
    isSpecialSchool: this.isSpecialSchool(),
    specialSchoolOrganisationId: this.specialSchoolOrganisationId(),
    specialSchoolTypeId: this.specialSchoolTypeId(),
    maxBoarders: this.maxBoarders(),
    telephone: this.telephone(),
    email: this.email(),
  }));

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
          disabled: !this.isValid() || !this.isDirty(),
          loading: this.saving(),
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

  async save(): Promise<void> {
    if (!this.canEdit() || !this.isValid() || this.saving()) return;
    this.saving.set(true);

    const payload: SchoolUpsertRequest = {
      name: this.name().trim(),
      website: this.normalise(this.website()),
      expectedVersion: 0,
      urn: this.urn().trim(),
      uprn: this.uprn().trim(),
      establishmentNumber: this.establishmentNumber() ?? 0,
      localAuthorityId: this.localAuthorityId(),
      schoolPhaseId: this.schoolPhaseId()!,
      schoolTypeId: this.schoolTypeId()!,
      governanceTypeId: this.governanceTypeId()!,
      intakeTypeId: this.intakeTypeId()!,
      headTeacherId: this.headTeacherId(),
      ukprn: this.normalise(this.ukprn()),
      payZoneId: this.payZoneId(),
      lowestAge: this.lowestAge(),
      highestAge: this.highestAge(),
      netCapacity: this.netCapacity(),
      netCapacityAssessmentDate: this.netCapacityAssessmentDate()?.toISOString() ?? null,
      isSpecialSchool: this.isSpecialSchool(),
      specialSchoolOrganisationId: this.isSpecialSchool() ? this.specialSchoolOrganisationId() : null,
      specialSchoolTypeId: this.isSpecialSchool() ? this.specialSchoolTypeId() : null,
      maxBoarders: this.isSpecialSchool() ? this.maxBoarders() : null,
      telephone: this.normalise(this.telephone()),
      email: this.normalise(this.email()),
    };

    try {
      await firstValueFrom(this.schools.saveLocalDetails(payload));
      this.schoolNameCache.clearCache();
      this.notify.success(this.transloco.translate('school-details.savedToast'));
      this.refresh();
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('school-details.saveError'));
    } finally {
      this.saving.set(false);
    }
  }

  private applyToForm(school: SchoolDetailsResponse | null): void {
    if (!school) {
      this.name.set('');
      this.website.set(null);
      this.urn.set('');
      this.uprn.set('');
      this.establishmentNumber.set(null);
      this.localAuthorityId.set(null);
      this.localAuthorityName.set(null);
      this.schoolPhaseId.set(null);
      this.schoolTypeId.set(null);
      this.governanceTypeId.set(null);
      this.intakeTypeId.set(null);
      this.headTeacherId.set(null);
      this.headTeacherFullName.set(null);
      this.ukprn.set(null);
      this.payZoneId.set(null);
      this.lowestAge.set(null);
      this.highestAge.set(null);
      this.netCapacity.set(null);
      this.netCapacityAssessmentDate.set(null);
      this.isSpecialSchool.set(false);
      this.specialSchoolOrganisationId.set(null);
      this.specialSchoolTypeId.set(null);
      this.maxBoarders.set(null);
      this.telephone.set(null);
      this.email.set(null);
    } else {
      this.name.set(school.name ?? '');
      this.website.set(school.website ?? null);
      this.urn.set(school.urn ?? '');
      this.uprn.set(school.uprn ?? '');
      this.establishmentNumber.set(school.establishmentNumber ?? null);
      this.localAuthorityId.set(school.localAuthorityId ?? null);
      this.localAuthorityName.set(school.localAuthorityName ?? null);
      this.schoolPhaseId.set(school.schoolPhaseId ?? null);
      this.schoolTypeId.set(school.schoolTypeId ?? null);
      this.governanceTypeId.set(school.governanceTypeId ?? null);
      this.intakeTypeId.set(school.intakeTypeId ?? null);
      this.headTeacherId.set(school.headTeacherId ?? null);
      this.headTeacherFullName.set(school.headTeacherFullName ?? null);
      this.ukprn.set(school.ukprn ?? null);
      this.payZoneId.set(school.payZoneId ?? null);
      this.lowestAge.set(school.lowestAge ?? null);
      this.highestAge.set(school.highestAge ?? null);
      this.netCapacity.set(school.netCapacity ?? null);
      this.netCapacityAssessmentDate.set(
        school.netCapacityAssessmentDate ? new Date(school.netCapacityAssessmentDate) : null,
      );
      this.isSpecialSchool.set(school.isSpecialSchool ?? false);
      this.specialSchoolOrganisationId.set(school.specialSchoolOrganisationId ?? null);
      this.specialSchoolTypeId.set(school.specialSchoolTypeId ?? null);
      this.maxBoarders.set(school.maxBoarders ?? null);
      this.telephone.set(school.telephone ?? null);
      this.email.set(school.email ?? null);
    }
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
