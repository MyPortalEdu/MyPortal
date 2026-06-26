import {
  ChangeDetectionStrategy,
  Component,
  HostListener,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MenuItem } from 'primeng/api';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { DatePicker } from 'primeng/datepicker';
import { InputText } from 'primeng/inputtext';
import { Menu } from 'primeng/menu';
import { ProgressSpinner } from 'primeng/progressspinner';
import { Tag } from 'primeng/tag';
import { Checkbox } from 'primeng/checkbox';
import { Textarea } from 'primeng/textarea';
import { MultiSelect } from 'primeng/multiselect';
import { firstValueFrom } from 'rxjs';
import {
  TranslocoDirective,
  TranslocoService,
  provideTranslocoScope,
} from '@jsverse/transloco';

import { PageHeader } from '../../../../../shared/components/page-header/page-header';
import { HeaderAction } from '../../../../../shared/types/header-action.type';
import { CanComponentDeactivate } from '../../../../../core/guards/can-deactivate.guard';
import { ConfirmationDialog } from '../../../../../core/services/confirmation.service';
import { NotificationService } from '../../../../../core/services/notification.service';
import { MeService } from '../../../../../core/services/me-service';
import { Permissions } from '../../../../../core/constants/permissions';
import { StaffMembersDataService } from '../../../../../shared/services/staff-members-data.service';
import {
  StaffMemberHeaderResponse,
  StaffRelationship,
} from '../../../../../shared/types/staff-member-header';
import {
  StaffBasicDetailsResponse,
  StaffBasicDetailsUpsertRequest,
} from '../../../../../shared/types/staff-basic-details';
import {
  PersonEmailUpsertItem,
  PersonPhoneUpsertItem,
  StaffContactDetailsResponse,
  StaffContactDetailsUpsertRequest,
} from '../../../../../shared/types/staff-contact-details';
import { PersonEmails } from '../../../../../shared/components/contact/person-emails/person-emails';
import { PersonPhones } from '../../../../../shared/components/contact/person-phones/person-phones';
import { PersonAddresses } from '../../../../../shared/components/contact/person-addresses/person-addresses';
import { GenderSelect } from '../../../../../shared/components/gender-select/gender-select';
import { GenderLabelPipe } from '../../../../../shared/pipes/gender-label.pipe';
import { LookupSelect } from '../../../../../shared/components/lookup-select/lookup-select';
import { DirectoryBrowser } from '../../../../../shared/components/documents/directory-browser/directory-browser';
import { DirectoryDataService } from '../../../../../shared/services/directory-data.service';
import { LookupResponse } from '../../../../../shared/types/lookup';
import {
  StaffEqualityDetailsResponse,
  StaffEqualityDetailsUpsertRequest,
} from '../../../../../shared/types/staff-equality-details';
import {
  StaffProfessionalDetailsResponse,
  StaffProfessionalDetailsUpsertRequest,
  StaffQualificationUpsertItem,
} from '../../../../../shared/types/staff-professional-details';

type BasicFormSnapshot = {
  code: string;
  title: string | null;
  firstName: string;
  middleName: string | null;
  lastName: string;
  preferredFirstName: string | null;
  preferredLastName: string | null;
  gender: string;
  dob: string | null;
};

// The 10 sidebar areas the staff profile will eventually surface. Only Basic
// details is wired up in this slice; the rest render as disabled placeholders
// so the layout reads right and adding a section is a focused follow-up.
type AreaKey =
  | 'basicDetails'
  | 'contactDetails'
  | 'equalityDetails'
  | 'professionalDetails'
  | 'employmentDetails'
  | 'preEmploymentChecks'
  | 'absences'
  | 'timetable'
  | 'performanceDetails'
  | 'documents';

interface AreaTab {
  key: AreaKey;
  icon: string;
  enabled: boolean;
}

const AREAS: AreaTab[] = [
  { key: 'basicDetails',         icon: 'fa-solid fa-user',            enabled: true  },
  { key: 'contactDetails',       icon: 'fa-solid fa-address-book',    enabled: true  },
  { key: 'equalityDetails',      icon: 'fa-solid fa-scale-balanced',  enabled: false },
  { key: 'professionalDetails',  icon: 'fa-solid fa-graduation-cap',  enabled: false },
  { key: 'employmentDetails',    icon: 'fa-solid fa-briefcase',       enabled: false },
  { key: 'preEmploymentChecks',  icon: 'fa-solid fa-shield-halved',   enabled: false },
  { key: 'absences',             icon: 'fa-solid fa-calendar-xmark',  enabled: false },
  { key: 'timetable',            icon: 'fa-solid fa-calendar-days',   enabled: false },
  { key: 'performanceDetails',   icon: 'fa-solid fa-chart-line',      enabled: false },
  { key: 'documents',            icon: 'fa-solid fa-folder',          enabled: true },
];

@Component({
  selector: 'mp-staff-member-details-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    DatePipe,
    FormsModule,
    Button,
    Card,
    DatePicker,
    InputText,
    Menu,
    ProgressSpinner,
    Tag,
    Checkbox,
    Textarea,
    MultiSelect,
    PageHeader,
    PersonEmails,
    PersonPhones,
    PersonAddresses,
    GenderSelect,
    GenderLabelPipe,
    LookupSelect,
    DirectoryBrowser,
    TranslocoDirective,
  ],
  providers: [provideTranslocoScope('staff-members')],
  templateUrl: './staff-member-details-page.html',
})
export class StaffMemberDetailsPage implements OnInit, CanComponentDeactivate {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly data = inject(StaffMembersDataService);
  private readonly me = inject(MeService);
  private readonly notify = inject(NotificationService);
  private readonly confirm = inject(ConfirmationDialog);
  private readonly transloco = inject(TranslocoService);
  private readonly directoryData = inject(DirectoryDataService);

  // A couple of areas gate their own tab on the viewer's access rather than the
  // static flag: equality (special-category, no Managed scope) and professional.
  protected readonly areas = computed<AreaTab[]>(() =>
    AREAS.map(a => {
      if (a.key === 'equalityDetails') return { ...a, enabled: this.canViewEquality() };
      if (a.key === 'professionalDetails') return { ...a, enabled: this.canViewProfessional() };
      return a;
    }),
  );
  protected readonly activeArea = signal<AreaKey>('basicDetails');

  protected readonly loadingHeader = signal(false);
  protected readonly loadingBasic = signal(false);
  protected readonly loadingContact = signal(false);
  protected readonly saving = signal(false);
  protected readonly editing = signal(false);

  protected readonly staffMemberId = signal<string>('');
  protected readonly header = signal<StaffMemberHeaderResponse | null>(null);
  protected readonly current = signal<StaffBasicDetailsResponse | null>(null);

  // Contact-details panel: editable email/phone lists + their type options.
  protected readonly contact = signal<StaffContactDetailsResponse | null>(null);
  protected readonly emails = signal<PersonEmailUpsertItem[]>([]);
  protected readonly phones = signal<PersonPhoneUpsertItem[]>([]);
  protected readonly emailTypes = computed(() => this.contact()?.emailTypes ?? []);
  protected readonly phoneTypes = computed(() => this.contact()?.phoneTypes ?? []);
  private readonly contactSnapshot = signal<string>('');

  // Equality & Diversity panel. Lazy-loaded the first time the area is opened
  // (it 403s for viewers without the equality scope, so we don't fetch eagerly).
  protected readonly loadingEquality = signal(false);
  protected readonly equality = signal<StaffEqualityDetailsResponse | null>(null);
  private equalityLoaded = false;

  protected readonly ethnicityId = signal<string | null>(null);
  protected readonly nationalityId = signal<string | null>(null);
  protected readonly firstLanguageId = signal<string | null>(null);
  protected readonly maritalStatusId = signal<string | null>(null);
  protected readonly religionId = signal<string | null>(null);
  protected readonly sexualOrientationId = signal<string | null>(null);
  protected readonly genderIdentityId = signal<string | null>(null);
  protected readonly hasDisability = signal<boolean>(false);
  protected readonly disabilityDetails = signal<string | null>(null);
  protected readonly disabilityIds = signal<string[]>([]);
  private readonly equalitySnapshot = signal<string>('');

  // Option lists travel with the equality payload so the editor is self-contained.
  protected readonly ethnicities = computed(() => this.equality()?.ethnicities ?? []);
  protected readonly nationalities = computed(() => this.equality()?.nationalities ?? []);
  protected readonly languages = computed(() => this.equality()?.languages ?? []);
  protected readonly maritalStatuses = computed(() => this.equality()?.maritalStatuses ?? []);
  protected readonly religions = computed(() => this.equality()?.religions ?? []);
  protected readonly sexualOrientations = computed(() => this.equality()?.sexualOrientations ?? []);
  protected readonly genderIdentities = computed(() => this.equality()?.genderIdentities ?? []);
  protected readonly disabilities = computed(() => this.equality()?.disabilities ?? []);

  // Professional Details panel. Lazy-loaded the first time the area is opened
  // (it 403s for viewers without professional view scope, so we don't fetch eagerly).
  protected readonly loadingProfessional = signal(false);
  protected readonly professional = signal<StaffProfessionalDetailsResponse | null>(null);
  private professionalLoaded = false;

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
  private readonly professionalSnapshot = signal<string>('');

  // Option lists travel with the professional payload so the editor is self-contained.
  protected readonly qtsRoutes = computed(() => this.professional()?.qtsRoutes ?? []);
  protected readonly inductionStatuses = computed(() => this.professional()?.inductionStatuses ?? []);
  protected readonly qualificationLevels = computed(() => this.professional()?.qualificationLevels ?? []);
  protected readonly classesOfDegree = computed(() => this.professional()?.classesOfDegree ?? []);

  // The teaching-status checkboxes, driven by a small descriptor list so the template
  // doesn't repeat the same get/set block six times. The i18n key is the field name.
  protected readonly professionalFlags: {
    key: string;
    get: () => boolean;
    set: (value: boolean) => void;
  }[] = [
    { key: 'isTeachingStaff', get: () => this.isTeachingStaff(), set: v => this.isTeachingStaff.set(v) },
    { key: 'hasQts', get: () => this.hasQts(), set: v => this.hasQts.set(v) },
    { key: 'hasHlta', get: () => this.hasHlta(), set: v => this.hasHlta.set(v) },
    { key: 'hasQtls', get: () => this.hasQtls(), set: v => this.hasQtls.set(v) },
    { key: 'hasEyts', get: () => this.hasEyts(), set: v => this.hasEyts.set(v) },
    { key: 'isSeniorLeadership', get: () => this.isSeniorLeadership(), set: v => this.isSeniorLeadership.set(v) },
  ];

  // Form-field signals for the basic-details panel. Mirror the upsert request.
  protected readonly code = signal('');
  protected readonly title = signal<string | null>(null);
  protected readonly firstName = signal('');
  protected readonly middleName = signal<string | null>(null);
  protected readonly lastName = signal('');
  protected readonly preferredFirstName = signal<string | null>(null);
  protected readonly preferredLastName = signal<string | null>(null);
  protected readonly gender = signal('');
  protected readonly dob = signal<Date | null>(null);

  // Held permissions claim, used (with Relationship) to compute canEdit.
  private readonly heldPerms = signal<Set<string>>(new Set());

  protected readonly canEditBasic = computed(() => {
    const perms = this.heldPerms();
    const rel = this.header()?.relationship;
    if (perms.has(Permissions.Staff.EditAllStaffBasicDetails)) return true;
    if (rel === 'LineManaged' && perms.has(Permissions.Staff.EditManagedStaffBasicDetails)) return true;
    return false;
  });

  // Attachments endpoints are inherited on the staff-members controller; the
  // directory browser keys off this base URL (entityId = staff member id).
  protected readonly documentsBaseUrl = computed(
    () => `/api/v1/staffmembers/${this.staffMemberId()}/attachments`,
  );

  // Staff-facet document types for the upload/classify picker (lazy-loaded the
  // first time the Documents area is opened).
  protected readonly documentTypes = signal<LookupResponse[]>([]);
  private documentTypesLoaded = false;

  // Documents edit gate — same relationship-scoped shape as basic details, but on
  // the Staff Documents permission domain.
  protected readonly canEditDocuments = computed(() => {
    const perms = this.heldPerms();
    const rel = this.header()?.relationship;
    if (perms.has(Permissions.Staff.EditAllStaffDocuments)) return true;
    if (rel === 'LineManaged' && perms.has(Permissions.Staff.EditManagedStaffDocuments)) return true;
    if (rel === 'Self' && perms.has(Permissions.Staff.EditOwnStaffDocuments)) return true;
    return false;
  });

  // Equality is special-category: HR (All) or the person themselves (view-own). No Managed.
  protected readonly canViewEquality = computed(() => {
    const perms = this.heldPerms();
    const rel = this.header()?.relationship;
    if (perms.has(Permissions.Staff.ViewAllStaffEqualityDetails)) return true;
    if (perms.has(Permissions.Staff.EditAllStaffEqualityDetails)) return true;
    if (rel === 'Self' && perms.has(Permissions.Staff.ViewOwnStaffEqualityDetails)) return true;
    return false;
  });

  // Equality is HR-edit-only — no self/managed edit.
  protected readonly canEditEquality = computed(() =>
    this.heldPerms().has(Permissions.Staff.EditAllStaffEqualityDetails),
  );

  // Professional details: relationship-scoped view (Own/Managed/All); an edit grant
  // implies view. Self can view-own but never edit (HR-verified data).
  protected readonly canViewProfessional = computed(() => {
    const perms = this.heldPerms();
    const rel = this.header()?.relationship;
    if (
      perms.has(Permissions.Staff.ViewAllStaffProfessionalDetails) ||
      perms.has(Permissions.Staff.EditAllStaffProfessionalDetails)
    )
      return true;
    if (
      rel === 'LineManaged' &&
      (perms.has(Permissions.Staff.ViewManagedStaffProfessionalDetails) ||
        perms.has(Permissions.Staff.EditManagedStaffProfessionalDetails))
    )
      return true;
    if (rel === 'Self' && perms.has(Permissions.Staff.ViewOwnStaffProfessionalDetails)) return true;
    return false;
  });

  // Professional edit: HR (All) or the line manager (Managed) — no self-edit.
  protected readonly canEditProfessional = computed(() => {
    const perms = this.heldPerms();
    const rel = this.header()?.relationship;
    if (perms.has(Permissions.Staff.EditAllStaffProfessionalDetails)) return true;
    if (rel === 'LineManaged' && perms.has(Permissions.Staff.EditManagedStaffProfessionalDetails))
      return true;
    return false;
  });

  // Contact methods live under the BasicDetails permission domain, so the same gate covers them.
  // Other editable areas have their own gate.
  protected readonly canEditActiveArea = computed(() => {
    const area = this.activeArea();
    if (area === 'basicDetails' || area === 'contactDetails') return this.canEditBasic();
    if (area === 'equalityDetails') return this.canEditEquality();
    if (area === 'professionalDetails') return this.canEditProfessional();
    return false;
  });

  // Deleting a staff record is HR-only (All scope) — the kebab doesn't render otherwise.
  protected readonly canDelete = computed(() =>
    this.heldPerms().has(Permissions.Staff.EditAllStaffBasicDetails),
  );

  protected readonly deleting = signal(false);

  // Single destructive action tucked behind the kebab. styleClass paints it
  // danger-red so it reads as destructive even inside the menu.
  protected readonly deleteMenuItems = computed<MenuItem[]>(() => [
    {
      label: this.transloco.translate('staff-members.delete.menuItem'),
      icon: 'fa-solid fa-trash',
      styleClass: 'mp-menu-item--danger',
      command: () => this.confirmDelete(),
    },
  ]);

  private readonly basicValid = computed(
    () =>
      this.firstName().trim().length > 0 &&
      this.lastName().trim().length > 0 &&
      this.gender().trim().length > 0 &&
      this.code().trim().length > 0,
  );

  private readonly contactValid = computed(
    () =>
      this.emails().every(e => !!e.typeId && e.address.trim().length > 0) &&
      this.phones().every(p => !!p.typeId && p.number.trim().length > 0),
  );

  // A 7-digit TRN when provided, and every qualification row needs a title.
  private readonly professionalValid = computed(() => {
    const trn = (this.teacherReferenceNumber() ?? '').trim();
    if (trn.length > 0 && !/^\d{7}$/.test(trn)) return false;
    return this.qualifications().every(q => q.title.trim().length > 0);
  });

  protected readonly isValid = computed(() => {
    switch (this.activeArea()) {
      case 'contactDetails':
        return this.contactValid();
      case 'equalityDetails':
        // Every equality field is optional — nothing to invalidate.
        return true;
      case 'professionalDetails':
        return this.professionalValid();
      default:
        return this.basicValid();
    }
  });

  // Serialised edit state for the dirty check; disability ids sorted so reorder
  // alone isn't treated as a change.
  private readonly equalityForm = computed(() =>
    JSON.stringify({
      ethnicityId: this.ethnicityId(),
      nationalityId: this.nationalityId(),
      firstLanguageId: this.firstLanguageId(),
      maritalStatusId: this.maritalStatusId(),
      religionId: this.religionId(),
      sexualOrientationId: this.sexualOrientationId(),
      genderIdentityId: this.genderIdentityId(),
      hasDisability: this.hasDisability(),
      disabilityDetails: this.disabilityDetails(),
      disabilityIds: [...this.disabilityIds()].sort(),
    }),
  );

  private readonly equalityDirty = computed(
    () => this.equality() != null && this.equalitySnapshot() !== this.equalityForm(),
  );

  // Serialised professional edit state for the dirty check. Dates normalised to ISO
  // so a Date instance vs string doesn't read as a change.
  private readonly professionalForm = computed(() =>
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

  private readonly professionalDirty = computed(
    () => this.professional() != null && this.professionalSnapshot() !== this.professionalForm(),
  );

  private readonly snapshot = signal<BasicFormSnapshot | null>(null);

  private readonly currentForm = computed<BasicFormSnapshot>(() => ({
    code: this.code(),
    title: this.title(),
    firstName: this.firstName(),
    middleName: this.middleName(),
    lastName: this.lastName(),
    preferredFirstName: this.preferredFirstName(),
    preferredLastName: this.preferredLastName(),
    gender: this.gender(),
    dob: this.dob()?.toISOString() ?? null,
  }));

  private readonly basicDirty = computed(() => {
    const s = this.snapshot();
    if (!s) return false;
    return JSON.stringify(s) !== JSON.stringify(this.currentForm());
  });

  private readonly contactDirty = computed(
    () =>
      this.contactSnapshot() !==
      JSON.stringify({ emails: this.emails(), phones: this.phones() }),
  );

  protected readonly isDirty = computed(() => {
    switch (this.activeArea()) {
      case 'contactDetails':
        return this.contactDirty();
      case 'equalityDetails':
        return this.equalityDirty();
      case 'professionalDetails':
        return this.professionalDirty();
      default:
        return this.basicDirty();
    }
  });

  protected readonly headerActions = computed<HeaderAction[]>(() => {
    if (!this.canEditActiveArea()) return [];
    if (this.editing()) {
      return [
        {
          label: this.transloco.translate('common.cancel'),
          icon: 'fa-solid fa-xmark',
          outlined: true,
          severity: 'secondary',
          disabled: this.saving(),
          command: () => this.cancelEdit(),
        },
        {
          label: this.transloco.translate('common.save'),
          icon: 'fa-solid fa-floppy-disk',
          severity: 'primary',
          disabled: !this.isValid() || !this.isDirty(),
          loading: this.saving(),
          command: () => this.save(),
        },
      ];
    }
    return [
      {
        label: this.transloco.translate('common.edit'),
        icon: 'fa-solid fa-pen',
        severity: 'primary',
        command: () => this.startEdit(),
      },
    ];
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id') ?? '';
    this.staffMemberId.set(id);

    this.me.me().subscribe(me => this.heldPerms.set(new Set(me.permissions ?? [])));

    this.loadHeader();
    this.loadBasic();
    this.loadContact();
  }

  private loadHeader(): void {
    this.loadingHeader.set(true);
    this.data.getHeader(this.staffMemberId()).subscribe({
      next: row => {
        this.header.set(row);
        this.loadingHeader.set(false);
      },
      error: err => {
        this.loadingHeader.set(false);
        this.notify.apiError(err, this.transloco.translate('staff-members.loadHeaderError'));
      },
    });
  }

  private loadBasic(): void {
    this.loadingBasic.set(true);
    this.data.getBasicDetails(this.staffMemberId()).subscribe({
      next: row => {
        this.current.set(row);
        this.applyToForm(row);
        this.loadingBasic.set(false);
      },
      error: err => {
        this.loadingBasic.set(false);
        this.notify.apiError(err, this.transloco.translate('staff-members.loadBasicError'));
      },
    });
  }

  private loadContact(): void {
    this.loadingContact.set(true);
    this.data.getContactDetails(this.staffMemberId()).subscribe({
      next: row => {
        this.contact.set(row);
        this.applyContact(row);
        this.loadingContact.set(false);
      },
      error: err => {
        this.loadingContact.set(false);
        this.notify.apiError(err, this.transloco.translate('staff-members.loadContactError'));
      },
    });
  }

  private loadEquality(): void {
    if (this.equalityLoaded) return;
    this.equalityLoaded = true;
    this.loadingEquality.set(true);
    this.data.getEqualityDetails(this.staffMemberId()).subscribe({
      next: row => {
        this.equality.set(row);
        this.applyEquality(row);
        this.loadingEquality.set(false);
      },
      error: err => {
        this.loadingEquality.set(false);
        this.equalityLoaded = false;
        this.notify.apiError(err, this.transloco.translate('staff-members.loadEqualityError'));
      },
    });
  }

  protected pickArea(area: AreaTab): void {
    if (!area.enabled || area.key === this.activeArea()) return;
    if (area.key === 'documents') this.loadDocumentTypes();
    if (area.key === 'equalityDetails') this.loadEquality();
    if (area.key === 'professionalDetails') this.loadProfessional();
    if (this.isDirty()) {
      // Don't lose edits on accidental tab switch.
      this.confirmDiscard().then(ok => {
        if (ok) {
          this.cancelEdit();
          this.activeArea.set(area.key);
        }
      });
      return;
    }
    // Leaving edit mode behind when switching to a fresh, clean area.
    this.editing.set(false);
    this.activeArea.set(area.key);
  }

  // Staff-facet types only (plus the catch-all "Other", which is flagged on
  // every facet). Failure is non-fatal — the picker just stays hidden.
  private loadDocumentTypes(): void {
    if (this.documentTypesLoaded) return;
    this.documentTypesLoaded = true;
    this.directoryData.getDocumentTypes({ staff: true }).subscribe({
      next: types => this.documentTypes.set(types),
      error: () => (this.documentTypesLoaded = false),
    });
  }

  protected statusSeverity(): 'success' | 'secondary' {
    return this.header()?.status === 'Active' ? 'success' : 'secondary';
  }

  // Initials for the avatar. Use the actual first/last name rather than parsing
  // displayName — that leads with the title (e.g. "Ms Jessica … Pearson"), which
  // would yield "MP" instead of "JP". Empty until basic details load.
  protected initials(): string {
    const first = this.firstName().trim().charAt(0);
    const last = this.lastName().trim().charAt(0);
    return (first + last).toUpperCase();
  }

  protected relationshipLabel(rel: StaffRelationship): string {
    return this.transloco.translate(`staff-members.relationship.${rel}`);
  }

  // Resolve a lookup id to its description for read-only display; em dash if unset.
  protected lookupLabel(list: LookupResponse[], id: string | null | undefined): string {
    if (!id) return '—';
    return list.find(x => x.id === id)?.description ?? '—';
  }

  // Comma-joined descriptions for a set of lookup ids (read-only multi-select view).
  protected selectedLabels(list: LookupResponse[], ids: string[]): string {
    if (!ids.length) return '—';
    const byId = new Map(list.map(x => [x.id, x.description]));
    return ids.map(id => byId.get(id)).filter(Boolean).join(', ') || '—';
  }

  protected startEdit(): void {
    this.editing.set(true);
  }

  protected cancelEdit(): void {
    switch (this.activeArea()) {
      case 'contactDetails':
        this.applyContact(this.contact());
        break;
      case 'equalityDetails':
        this.applyEquality(this.equality());
        break;
      case 'professionalDetails':
        this.applyProfessional(this.professional());
        break;
      default:
        this.applyToForm(this.current());
    }
    this.editing.set(false);
  }

  async save(): Promise<void> {
    if (this.activeArea() === 'contactDetails') {
      await this.saveContact();
    } else if (this.activeArea() === 'equalityDetails') {
      await this.saveEquality();
    } else if (this.activeArea() === 'professionalDetails') {
      await this.saveProfessional();
    } else {
      await this.saveBasic();
    }
  }

  private async saveBasic(): Promise<void> {
    if (!this.canEditBasic() || !this.basicValid() || this.saving()) return;
    this.saving.set(true);

    const c = this.current();
    // Preserve fields we don't render an input for (PhotoId / Deceased) by echoing
    // their existing values. Equality fields are owned by the Equality area now.
    const payload: StaffBasicDetailsUpsertRequest = {
      title: this.normalise(this.title()),
      firstName: this.firstName().trim(),
      middleName: this.normalise(this.middleName()),
      lastName: this.lastName().trim(),
      preferredFirstName: this.normalise(this.preferredFirstName()),
      preferredLastName: this.normalise(this.preferredLastName()),
      gender: this.gender().trim(),
      dob: this.dob()?.toISOString() ?? null,
      photoId: c?.photoId ?? null,
      deceased: c?.deceased ?? null,
      code: this.code().trim(),
    };

    try {
      await firstValueFrom(this.data.updateBasicDetails(this.staffMemberId(), payload));
      this.notify.success(this.transloco.translate('staff-members.savedToast'));
      this.editing.set(false);
      this.loadHeader();
      this.loadBasic();
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('staff-members.saveError'));
    } finally {
      this.saving.set(false);
    }
  }

  private async saveContact(): Promise<void> {
    if (!this.canEditBasic() || !this.contactValid() || this.saving()) return;
    this.saving.set(true);

    const payload: StaffContactDetailsUpsertRequest = {
      emails: this.emails(),
      phones: this.phones(),
    };

    try {
      await firstValueFrom(this.data.updateContactDetails(this.staffMemberId(), payload));
      this.notify.success(this.transloco.translate('staff-members.savedContactToast'));
      this.editing.set(false);
      this.loadContact();
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('staff-members.saveContactError'));
    } finally {
      this.saving.set(false);
    }
  }

  private applyContact(row: StaffContactDetailsResponse | null): void {
    this.emails.set(
      (row?.emails ?? []).map(e => ({
        id: e.id,
        typeId: e.typeId,
        address: e.address,
        isMain: e.isMain,
        notes: e.notes ?? null,
      })),
    );
    this.phones.set(
      (row?.phones ?? []).map(p => ({
        id: p.id,
        typeId: p.typeId,
        number: p.number,
        isMain: p.isMain,
      })),
    );
    this.contactSnapshot.set(JSON.stringify({ emails: this.emails(), phones: this.phones() }));
  }

  private async saveEquality(): Promise<void> {
    if (!this.canEditEquality() || this.saving()) return;
    this.saving.set(true);

    const payload: StaffEqualityDetailsUpsertRequest = {
      ethnicityId: this.ethnicityId(),
      nationalityId: this.nationalityId(),
      firstLanguageId: this.firstLanguageId(),
      maritalStatusId: this.maritalStatusId(),
      religionId: this.religionId(),
      sexualOrientationId: this.sexualOrientationId(),
      genderIdentityId: this.genderIdentityId(),
      hasDisability: this.hasDisability(),
      disabilityDetails: this.normalise(this.disabilityDetails()),
      disabilityIds: this.disabilityIds(),
    };

    try {
      await firstValueFrom(this.data.updateEqualityDetails(this.staffMemberId(), payload));
      this.notify.success(this.transloco.translate('staff-members.savedEqualityToast'));
      this.editing.set(false);
      // Refetch so the snapshot (and any server normalisation) is the new baseline.
      this.equalityLoaded = false;
      this.loadEquality();
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('staff-members.saveEqualityError'));
    } finally {
      this.saving.set(false);
    }
  }

  private applyEquality(row: StaffEqualityDetailsResponse | null): void {
    this.ethnicityId.set(row?.ethnicityId ?? null);
    this.nationalityId.set(row?.nationalityId ?? null);
    this.firstLanguageId.set(row?.firstLanguageId ?? null);
    this.maritalStatusId.set(row?.maritalStatusId ?? null);
    this.religionId.set(row?.religionId ?? null);
    this.sexualOrientationId.set(row?.sexualOrientationId ?? null);
    this.genderIdentityId.set(row?.genderIdentityId ?? null);
    this.hasDisability.set(row?.hasDisability ?? false);
    this.disabilityDetails.set(row?.disabilityDetails ?? null);
    this.disabilityIds.set([...(row?.disabilityIds ?? [])]);
    this.equalitySnapshot.set(this.equalityForm());
  }

  private loadProfessional(): void {
    if (this.professionalLoaded) return;
    this.professionalLoaded = true;
    this.loadingProfessional.set(true);
    this.data.getProfessionalDetails(this.staffMemberId()).subscribe({
      next: row => {
        this.professional.set(row);
        this.applyProfessional(row);
        this.loadingProfessional.set(false);
      },
      error: err => {
        this.loadingProfessional.set(false);
        this.professionalLoaded = false;
        this.notify.apiError(err, this.transloco.translate('staff-members.loadProfessionalError'));
      },
    });
  }

  private async saveProfessional(): Promise<void> {
    if (!this.canEditProfessional() || !this.professionalValid() || this.saving()) return;
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
      this.professionalLoaded = false;
      this.loadProfessional();
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('staff-members.saveProfessionalError'));
    } finally {
      this.saving.set(false);
    }
  }

  private applyProfessional(row: StaffProfessionalDetailsResponse | null): void {
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
    this.professionalSnapshot.set(this.professionalForm());
  }

  // Qualifications grid — a new row has no id (the server inserts it); editing a
  // field rewrites the array immutably so the dirty check and OnPush both fire.
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

  // One-line read-only summary of a qualification's secondary fields (level, subject,
  // grade, class, awarding body, year) — empty fields dropped.
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

  protected backToList(): void {
    this.router.navigate(['/staff/people/staff-members']);
  }

  protected async confirmDelete(): Promise<void> {
    if (this.deleting()) return;

    const ok = await this.confirm.confirm({
      header: this.transloco.translate('staff-members.delete.header'),
      message: this.transloco.translate('staff-members.delete.confirm'),
      acceptLabel: this.transloco.translate('common.delete'),
      acceptSeverity: 'danger',
    });

    if (!ok) return;

    this.deleting.set(true);
    try {
      await firstValueFrom(this.data.delete(this.staffMemberId()));
      this.notify.success(this.transloco.translate('staff-members.delete.toast'));
      this.router.navigate(['/staff/people/staff-members']);
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('staff-members.delete.error'));
    } finally {
      this.deleting.set(false);
    }
  }

  private applyToForm(row: StaffBasicDetailsResponse | null): void {
    if (!row) {
      this.code.set('');
      this.title.set(null);
      this.firstName.set('');
      this.middleName.set(null);
      this.lastName.set('');
      this.preferredFirstName.set(null);
      this.preferredLastName.set(null);
      this.gender.set('');
      this.dob.set(null);
    } else {
      this.code.set(row.code);
      this.title.set(row.title ?? null);
      this.firstName.set(row.firstName);
      this.middleName.set(row.middleName ?? null);
      this.lastName.set(row.lastName);
      this.preferredFirstName.set(row.preferredFirstName ?? null);
      this.preferredLastName.set(row.preferredLastName ?? null);
      this.gender.set(row.gender);
      this.dob.set(row.dob ? new Date(row.dob) : null);
    }
    this.snapshot.set(this.currentForm());
  }

  private normalise(value: string | null | undefined): string | null {
    if (value == null) return null;
    const trimmed = value.trim();
    return trimmed.length === 0 ? null : trimmed;
  }

  async canDeactivate(): Promise<boolean> {
    if (!this.isDirty()) return true;
    return this.confirmDiscard();
  }

  private confirmDiscard(): Promise<boolean> {
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
}
