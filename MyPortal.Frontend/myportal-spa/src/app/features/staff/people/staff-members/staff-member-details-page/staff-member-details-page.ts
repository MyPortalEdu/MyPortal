import {
  ChangeDetectionStrategy,
  Component,
  HostListener,
  OnInit,
  computed,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormField, form, submit, validate } from '@angular/forms/signals';
import { ActivatedRoute, Router } from '@angular/router';
import { MpDatePicker, MpButton, MpCard, MpInput, MpBadge, MpSpinner, MpMenu, type MpMenuItem } from '@myportal/ui';
import { firstValueFrom } from 'rxjs';
import {
  MAX_ATTACHMENT_BYTES,
  MAX_ATTACHMENT_LABEL,
} from '../../../../../shared/types/document';
import {
  TranslocoDirective,
  TranslocoService,
  provideTranslocoScope,
} from '@jsverse/transloco';

import { PageHeader } from '../../../../../shared/components/page-header/page-header';
import { Loading } from '../../../../../shared/components/loading/loading';
import { ErrorState } from '../../../../../shared/components/error-state/error-state';
import { EmptyState } from '../../../../../shared/components/empty-state/empty-state';
import { SectionHeader } from '../../../../../shared/components/section-header/section-header';
import { Field } from '../../../../../shared/components/field/field';
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
import { GenderSelect } from '../../../../../shared/components/gender-select/gender-select';
import { GenderLabelPipe } from '../../../../../shared/pipes/gender-label.pipe';
import { DirectoryBrowser } from '../../../../../shared/components/documents/directory-browser/directory-browser';
import { StaffTimetable } from '../../../../../shared/components/staff-timetable/staff-timetable';
import { DirectoryDataService } from '../../../../../shared/services/directory-data.service';
import { LookupResponse } from '../../../../../shared/types/lookup';
import { StaffAreaPanel } from './panels/staff-area-panel';
import { StaffAbsencesPanel } from './panels/staff-absences-panel';
import { StaffEqualityPanel } from './panels/staff-equality-panel';
import { StaffProfessionalPanel } from './panels/staff-professional-panel';
import { StaffEmploymentPanel } from './panels/staff-employment-panel';
import { StaffPreEmploymentPanel } from './panels/staff-pre-employment-panel';
import { StaffPerformancePanel } from './panels/staff-performance-panel';
import { StaffContactPanel } from './panels/staff-contact-panel';

interface BasicModel {
  code: string;
  title: string;
  firstName: string;
  middleName: string;
  lastName: string;
  preferredFirstName: string;
  preferredLastName: string;
  gender: string;
  dob: Date | null;
}

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
    FormField,
    MpButton,
    MpCard,
    MpDatePicker,
    MpInput,
    MpMenu,
    MpSpinner,
    MpBadge,
    PageHeader,
    Loading,
    ErrorState,
    EmptyState,
    SectionHeader,
    Field,
    GenderSelect,
    GenderLabelPipe,
    DirectoryBrowser,
    StaffTimetable,
    StaffAbsencesPanel,
    StaffEqualityPanel,
    StaffProfessionalPanel,
    StaffEmploymentPanel,
    StaffPreEmploymentPanel,
    StaffPerformancePanel,
    StaffContactPanel,
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

  protected readonly areas = computed<AreaTab[]>(() =>
    AREAS.map(a => {
      if (a.key === 'equalityDetails') return { ...a, enabled: this.canViewEquality() };
      if (a.key === 'professionalDetails') return { ...a, enabled: this.canViewProfessional() };
      if (a.key === 'employmentDetails') return { ...a, enabled: this.canViewEmployment() };
      if (a.key === 'preEmploymentChecks') return { ...a, enabled: this.canViewPreEmployment() };
      if (a.key === 'absences') return { ...a, enabled: this.canViewAbsences() };
      if (a.key === 'timetable') return { ...a, enabled: this.canViewTimetable() };
      if (a.key === 'performanceDetails') return { ...a, enabled: this.canViewPerformance() };
      return a;
    }),
  );
  protected readonly activeArea = signal<AreaKey>('basicDetails');

  private static readonly EXTRACTED_AREAS: ReadonlySet<AreaKey> = new Set<AreaKey>([
    'contactDetails',
    'absences',
    'equalityDetails',
    'professionalDetails',
    'employmentDetails',
    'preEmploymentChecks',
    'performanceDetails',
  ]);
  private readonly activePanel = viewChild(StaffAreaPanel);

  protected readonly loadingHeader = signal(false);
  protected readonly headerError = signal(false);
  protected readonly loadingBasic = signal(false);
  protected readonly editing = signal(false);

  protected readonly staffMemberId = signal<string>('');
  protected readonly header = signal<StaffMemberHeaderResponse | null>(null);
  protected readonly current = signal<StaffBasicDetailsResponse | null>(null);

  protected readonly model = signal<BasicModel>({
    code: '',
    title: '',
    firstName: '',
    middleName: '',
    lastName: '',
    preferredFirstName: '',
    preferredLastName: '',
    gender: '',
    dob: null,
  });
  protected readonly f = form(this.model, path => {
    for (const field of [path.code, path.firstName, path.lastName, path.gender]) {
      validate(field, ({ value }) =>
        value().trim().length ? undefined : { kind: 'required' },
      );
    }
  });
  private readonly snapshot = signal<string>('');

  protected readonly heldPerms = signal<Set<string>>(new Set());

  protected readonly canEditBasic = computed(() => {
    const perms = this.heldPerms();
    const rel = this.header()?.relationship;
    if (perms.has(Permissions.Staff.EditAllStaffBasicDetails)) return true;
    if (rel === 'LineManaged' && perms.has(Permissions.Staff.EditManagedStaffBasicDetails)) return true;
    return false;
  });

  protected readonly documentsBaseUrl = computed(
    () => `/api/v1/staffmembers/${this.staffMemberId()}/attachments`,
  );

  protected readonly documentTypes = signal<LookupResponse[]>([]);
  private documentTypesLoaded = false;

  protected readonly canEditDocuments = computed(() => {
    const perms = this.heldPerms();
    const rel = this.header()?.relationship;
    if (perms.has(Permissions.Staff.EditAllStaffDocuments)) return true;
    if (rel === 'LineManaged' && perms.has(Permissions.Staff.EditManagedStaffDocuments)) return true;
    if (rel === 'Self' && perms.has(Permissions.Staff.EditOwnStaffDocuments)) return true;
    return false;
  });

  protected readonly canViewEquality = computed(() => {
    const perms = this.heldPerms();
    const rel = this.header()?.relationship;
    if (perms.has(Permissions.Staff.ViewAllStaffEqualityDetails)) return true;
    if (perms.has(Permissions.Staff.EditAllStaffEqualityDetails)) return true;
    if (rel === 'Self' && perms.has(Permissions.Staff.ViewOwnStaffEqualityDetails)) return true;
    return false;
  });

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

  protected readonly canViewEmployment = computed(() => {
    const perms = this.heldPerms();
    const rel = this.header()?.relationship;
    if (
      perms.has(Permissions.Staff.ViewAllStaffEmploymentDetails) ||
      perms.has(Permissions.Staff.EditAllStaffEmploymentDetails)
    )
      return true;
    if (rel === 'Self' && perms.has(Permissions.Staff.ViewOwnStaffEmploymentDetails)) return true;
    return false;
  });

  protected readonly canViewPreEmployment = computed(() => {
    const perms = this.heldPerms();
    return (
      perms.has(Permissions.Staff.ViewAllStaffPreEmploymentChecks) ||
      perms.has(Permissions.Staff.EditAllStaffPreEmploymentChecks)
    );
  });

  protected readonly canViewAbsences = computed(() => {
    const perms = this.heldPerms();
    const rel = this.header()?.relationship;
    if (
      perms.has(Permissions.Staff.ViewAllStaffAbsences) ||
      perms.has(Permissions.Staff.EditAllStaffAbsences)
    )
      return true;
    if (
      rel === 'LineManaged' &&
      (perms.has(Permissions.Staff.ViewManagedStaffAbsences) ||
        perms.has(Permissions.Staff.EditManagedStaffAbsences))
    )
      return true;
    if (rel === 'Self' && perms.has(Permissions.Staff.ViewOwnStaffAbsences)) return true;
    return false;
  });

  protected readonly canViewTimetable = computed(() => {
    const perms = this.heldPerms();
    const rel = this.header()?.relationship;
    if (perms.has(Permissions.Staff.ViewAllStaffTimetable)) return true;
    if (rel === 'LineManaged' && perms.has(Permissions.Staff.ViewManagedStaffTimetable)) return true;
    if (rel === 'Self' && perms.has(Permissions.Staff.ViewOwnStaffTimetable)) return true;
    return false;
  });

  protected readonly canViewPerformance = computed(() => {
    const perms = this.heldPerms();
    const rel = this.header()?.relationship;
    if (
      perms.has(Permissions.Staff.ViewAllStaffPerformanceDetails) ||
      perms.has(Permissions.Staff.EditAllStaffPerformanceDetails)
    )
      return true;
    if (
      rel === 'LineManaged' &&
      (perms.has(Permissions.Staff.ViewManagedStaffPerformanceDetails) ||
        perms.has(Permissions.Staff.EditManagedStaffPerformanceDetails))
    )
      return true;
    return false;
  });

  protected readonly canEditActiveArea = computed(() =>
    this.activeArea() === 'basicDetails' ? this.canEditBasic() : false,
  );

  protected readonly canDelete = computed(() =>
    this.heldPerms().has(Permissions.Staff.EditAllStaffBasicDetails),
  );

  protected readonly deleting = signal(false);

  protected readonly deleteMenuItems = computed<MpMenuItem[]>(() => [
    {
      label: this.transloco.translate('staff-members.delete.menuItem'),
      icon: 'fa-solid fa-trash',
      styleClass: 'text-destructive hover:!text-destructive hover:!bg-destructive/10',
      command: () => this.confirmDelete(),
    },
  ]);

  private readonly formState = computed(() => JSON.stringify(this.model()));

  private readonly basicDirty = computed(
    () => this.current() != null && this.snapshot() !== this.formState(),
  );

  protected readonly isDirty = computed(() => this.basicDirty());

  private readonly activeEdit = computed(() => {
    const panel = this.activePanel();
    if (panel && StaffMemberDetailsPage.EXTRACTED_AREAS.has(this.activeArea())) {
      return {
        canEdit: panel.canEdit(),
        editing: panel.editing(),
        dirty: panel.dirty(),
        valid: panel.valid(),
        saving: panel.saving(),
        explainsInvalid: panel.explainsInvalid,
        start: () => panel.startEdit(),
        cancel: () => panel.cancel(),
        save: () => panel.save(),
      };
    }
    return {
      canEdit: this.canEditActiveArea(),
      editing: this.editing(),
      dirty: this.isDirty(),
      valid: this.f().valid(),
      saving: this.f().submitting(),
      explainsInvalid: true,
      start: () => this.startEdit(),
      cancel: () => this.cancelEdit(),
      save: () => this.save(),
    };
  });

  protected readonly headerActions = computed<HeaderAction[]>(() => {
    const edit = this.activeEdit();
    if (!edit.canEdit) return [];
    if (edit.editing) {
      return [
        {
          label: this.transloco.translate('common.cancel'),
          icon: 'fa-solid fa-xmark',
          outlined: true,
          severity: 'secondary',
          disabled: edit.saving,
          command: () => edit.cancel(),
        },
        {
          label: this.transloco.translate('common.save'),
          icon: 'fa-solid fa-check',
          severity: 'primary',
          disabled: !edit.dirty || (!edit.valid && !edit.explainsInvalid),
          loading: edit.saving,
          command: () => edit.save(),
        },
      ];
    }
    return [
      {
        label: this.transloco.translate('common.edit'),
        icon: 'fa-solid fa-pen',
        severity: 'primary',
        command: () => edit.start(),
      },
    ];
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id') ?? '';
    this.staffMemberId.set(id);

    this.me.me().subscribe(me => this.heldPerms.set(new Set(me.permissions ?? [])));

    this.loadHeader();
    this.loadBasic();
  }

  private loadHeader(): void {
    this.loadingHeader.set(true);
    this.headerError.set(false);
    this.data.getHeader(this.staffMemberId()).subscribe({
      next: row => {
        this.header.set(row);
        this.loadingHeader.set(false);
      },
      error: err => {
        this.loadingHeader.set(false);
        this.headerError.set(true);
        this.notify.apiError(err, this.transloco.translate('staff-members.loadHeaderError'));
      },
    });
  }

  protected retry(): void {
    this.loadHeader();
    this.loadBasic();
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

  protected pickArea(area: AreaTab): void {
    if (!area.enabled || area.key === this.activeArea()) return;
    if (area.key === 'documents') this.loadDocumentTypes();
    if (this.activeEdit().dirty) {
      this.confirmDiscard().then(ok => {
        if (ok) {
          this.activeEdit().cancel();
          this.editing.set(false);
          this.activeArea.set(area.key);
        }
      });
      return;
    }
    this.editing.set(false);
    this.activeArea.set(area.key);
  }

  private loadDocumentTypes(): void {
    if (this.documentTypesLoaded) return;
    this.documentTypesLoaded = true;
    this.directoryData.getDocumentTypes({ staff: true }).subscribe({
      next: types => this.documentTypes.set(types),
      error: () => (this.documentTypesLoaded = false),
    });
  }

  protected statusSeverity(): 'success' | 'info' | 'warn' | 'secondary' | 'contrast' {
    switch (this.header()?.status) {
      case 'Active':
        return 'success';
      case 'Future':
        return 'info';
      case 'Leaver':
        return 'warn';
      case 'Archived':
        return 'contrast';
      default:
        return 'secondary';
    }
  }

  protected initials(): string {
    const first = this.model().firstName.trim().charAt(0);
    const last = this.model().lastName.trim().charAt(0);
    return (first + last).toUpperCase();
  }

  protected relationshipLabel(rel: StaffRelationship): string {
    return this.transloco.translate(`staff-members.relationship.${rel}`);
  }

  protected readonly photoUploading = signal(false);

  protected photoSrc(photoId: string): string {
    return this.data.photoUrl(this.staffMemberId(), photoId);
  }

  protected async onPhotoSelected(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file) return;

    if (!file.type.startsWith('image/')) {
      this.notify.error(this.transloco.translate('staff-members.photo.invalidType'));
      return;
    }
    if (file.size > MAX_ATTACHMENT_BYTES) {
      this.notify.error(
        this.transloco.translate('staff-members.photo.tooLarge', { size: MAX_ATTACHMENT_LABEL }),
      );
      return;
    }

    this.photoUploading.set(true);
    try {
      await firstValueFrom(this.data.uploadPhoto(this.staffMemberId(), file));
      this.notify.success(this.transloco.translate('staff-members.photo.savedToast'));
      this.loadHeader();
      this.loadBasic();
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('staff-members.photo.saveError'));
    } finally {
      this.photoUploading.set(false);
    }
  }

  protected async removePhoto(): Promise<void> {
    const ok = await this.confirm.confirm({
      header: this.transloco.translate('staff-members.photo.removeHeader'),
      message: this.transloco.translate('staff-members.photo.removeConfirm'),
      acceptLabel: this.transloco.translate('staff-members.photo.remove'),
      acceptSeverity: 'danger',
    });
    if (!ok) return;

    this.photoUploading.set(true);
    try {
      await firstValueFrom(this.data.deletePhoto(this.staffMemberId()));
      this.notify.success(this.transloco.translate('staff-members.photo.removedToast'));
      this.loadHeader();
      this.loadBasic();
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('staff-members.photo.removeError'));
    } finally {
      this.photoUploading.set(false);
    }
  }

  protected startEdit(): void {
    this.editing.set(true);
  }

  protected cancelEdit(): void {
    this.applyToForm(this.current());
    this.editing.set(false);
  }

  async save(): Promise<void> {
    if (!this.canEditBasic()) return;
    await submit(this.f, async () => {
      const m = this.model();
      const c = this.current();
      const payload: StaffBasicDetailsUpsertRequest = {
        title: this.normalise(m.title),
        firstName: m.firstName.trim(),
        middleName: this.normalise(m.middleName),
        lastName: m.lastName.trim(),
        preferredFirstName: this.normalise(m.preferredFirstName),
        preferredLastName: this.normalise(m.preferredLastName),
        gender: m.gender.trim(),
        dob: m.dob?.toISOString() ?? null,
        photoId: c?.photoId ?? null,
        deceased: c?.deceased ?? null,
        code: m.code.trim(),
      };
      try {
        await firstValueFrom(this.data.updateBasicDetails(this.staffMemberId(), payload));
      } catch (err) {
        this.notify.apiError(err, this.transloco.translate('staff-members.saveError'));
        return;
      }
      this.notify.success(this.transloco.translate('staff-members.savedToast'));
      this.editing.set(false);
      this.loadHeader();
      this.loadBasic();
    });
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
    this.model.set({
      code: row?.code ?? '',
      title: row?.title ?? '',
      firstName: row?.firstName ?? '',
      middleName: row?.middleName ?? '',
      lastName: row?.lastName ?? '',
      preferredFirstName: row?.preferredFirstName ?? '',
      preferredLastName: row?.preferredLastName ?? '',
      gender: row?.gender ?? '',
      dob: row?.dob ? new Date(row.dob) : null,
    });
    this.f().reset();
    this.snapshot.set(this.formState());
  }

  private normalise(value: string | null | undefined): string | null {
    if (value == null) return null;
    const trimmed = value.trim();
    return trimmed.length === 0 ? null : trimmed;
  }

  async canDeactivate(): Promise<boolean> {
    if (!this.activeEdit().dirty) return true;
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
    if (this.activeEdit().dirty) {
      event.preventDefault();
      event.returnValue = '';
    }
  }
}
