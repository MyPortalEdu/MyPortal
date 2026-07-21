import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  HostListener,
  OnInit,
  computed,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
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
import { Callout } from '../../../../../shared/components/callout/callout';
import { focusFirstInvalid } from '../../../../../shared/utils/focus-first-invalid';
import { HeaderAction } from '../../../../../shared/types/header-action.type';
import { CanComponentDeactivate } from '../../../../../core/guards/can-deactivate.guard';
import { ConfirmationDialog } from '../../../../../core/services/confirmation.service';
import { NotificationService } from '../../../../../core/services/notification.service';
import { MeService } from '../../../../../core/services/me-service';
import { Permissions } from '../../../../../core/constants/permissions';
import { StudentsDataService } from '../../../../../shared/services/students-data.service';
import { StudentHeaderResponse } from '../../../../../shared/types/student-header';
import {
  StudentBasicDetailsResponse,
  StudentBasicDetailsUpsertRequest,
} from '../../../../../shared/types/student-basic-details';
import { GenderSelect } from '../../../../../shared/components/gender-select/gender-select';
import { GenderLabelPipe } from '../../../../../shared/pipes/gender-label.pipe';
import { StudentAreaPanel } from './panels/student-area-panel';
import { StudentRegistrationPanel } from './panels/student-registration-panel';

type BasicFormSnapshot = {
  title: string | null;
  firstName: string;
  middleName: string | null;
  lastName: string;
  preferredFirstName: string | null;
  preferredLastName: string | null;
  gender: string;
  dob: string | null;
};

type AreaKey = 'basicDetails' | 'registration';

interface AreaTab {
  key: AreaKey;
  icon: string;
  enabled: boolean;
}

const AREAS: AreaTab[] = [
  { key: 'basicDetails', icon: 'fa-solid fa-user',           enabled: true  },
  { key: 'registration', icon: 'fa-solid fa-clipboard-list', enabled: false },
];

@Component({
  selector: 'mp-student-details-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    DatePipe,
    FormsModule,
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
    Callout,
    GenderSelect,
    GenderLabelPipe,
    StudentRegistrationPanel,
    TranslocoDirective,
  ],
  providers: [provideTranslocoScope('students')],
  templateUrl: './student-details-page.html',
})
export class StudentDetailsPage implements OnInit, CanComponentDeactivate {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly data = inject(StudentsDataService);
  private readonly me = inject(MeService);
  private readonly notify = inject(NotificationService);
  private readonly confirm = inject(ConfirmationDialog);
  private readonly transloco = inject(TranslocoService);
  private readonly host = inject(ElementRef<HTMLElement>);

  protected readonly areas = computed<AreaTab[]>(() =>
    AREAS.map(a => {
      if (a.key === 'registration') return { ...a, enabled: this.canViewRegistration() };
      return a;
    }),
  );
  protected readonly activeArea = signal<AreaKey>('basicDetails');

  private static readonly EXTRACTED_AREAS: ReadonlySet<AreaKey> = new Set<AreaKey>(['registration']);
  private readonly activePanel = viewChild(StudentAreaPanel);

  protected readonly loadingHeader = signal(false);
  protected readonly headerError = signal(false);
  protected readonly loadingBasic = signal(false);
  protected readonly saving = signal(false);
  protected readonly editing = signal(false);

  protected readonly studentId = signal<string>('');
  protected readonly header = signal<StudentHeaderResponse | null>(null);
  protected readonly current = signal<StudentBasicDetailsResponse | null>(null);

  protected readonly admissionNumber = signal<number | null>(null);
  protected readonly title = signal<string | null>(null);
  protected readonly firstName = signal('');
  protected readonly middleName = signal<string | null>(null);
  protected readonly lastName = signal('');
  protected readonly preferredFirstName = signal<string | null>(null);
  protected readonly preferredLastName = signal<string | null>(null);
  protected readonly gender = signal('');
  protected readonly dob = signal<Date | null>(null);

  protected readonly heldPerms = signal<Set<string>>(new Set());

  protected readonly canEditBasic = computed(() =>
    this.heldPerms().has(Permissions.Student.EditStudentBasicDetails),
  );

  protected readonly canViewRegistration = computed(() => {
    const perms = this.heldPerms();
    return (
      perms.has(Permissions.Student.ViewStudentRegistration) ||
      perms.has(Permissions.Student.EditStudentRegistration)
    );
  });

  protected readonly canEditActiveArea = computed(() => {
    switch (this.activeArea()) {
      case 'basicDetails':
        return this.canEditBasic();
      case 'registration':
        return this.heldPerms().has(Permissions.Student.EditStudentRegistration);
      default:
        return false;
    }
  });

  protected readonly canDelete = computed(() =>
    this.heldPerms().has(Permissions.Student.EditStudentBasicDetails),
  );

  protected readonly deleting = signal(false);

  protected readonly deleteMenuItems = computed<MpMenuItem[]>(() => [
    {
      label: this.transloco.translate('students.delete.menuItem'),
      icon: 'fa-solid fa-trash',
      styleClass: 'text-destructive hover:!text-destructive hover:!bg-destructive/10',
      command: () => this.confirmDelete(),
    },
  ]);

  private readonly basicValid = computed(
    () =>
      this.firstName().trim().length > 0 &&
      this.lastName().trim().length > 0 &&
      this.gender().trim().length > 0,
  );

  protected readonly isValid = computed(() => this.basicValid());

  protected readonly basicSubmitAttempted = signal(false);

  protected basicFieldError(value: string | null): string | undefined {
    if (!this.editing() || !this.basicSubmitAttempted()) return undefined;
    return value?.trim() ? undefined : this.transloco.translate('common.validation.required');
  }

  private readonly snapshot = signal<BasicFormSnapshot | null>(null);

  private readonly currentForm = computed<BasicFormSnapshot>(() => ({
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

  protected readonly isDirty = computed(() => this.basicDirty());

  private readonly activeEdit = computed(() => {
    const panel = this.activePanel();
    if (panel && StudentDetailsPage.EXTRACTED_AREAS.has(this.activeArea())) {
      return {
        canEdit: panel.canEdit(),
        editing: panel.editing(),
        dirty: panel.dirty(),
        valid: panel.valid(),
        saving: panel.saving(),
        explainsInvalid: false,
        start: () => panel.startEdit(),
        cancel: () => panel.cancel(),
        save: () => panel.save(),
      };
    }
    return {
      canEdit: this.canEditActiveArea(),
      editing: this.editing(),
      dirty: this.isDirty(),
      valid: this.isValid(),
      saving: this.saving(),
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
    this.studentId.set(id);

    this.me.me().subscribe(me => this.heldPerms.set(new Set(me.permissions ?? [])));

    this.loadHeader();
    this.loadBasic();
  }

  private loadHeader(): void {
    this.loadingHeader.set(true);
    this.headerError.set(false);
    this.data.getHeader(this.studentId()).subscribe({
      next: row => {
        this.header.set(row);
        this.loadingHeader.set(false);
      },
      error: err => {
        this.loadingHeader.set(false);
        this.headerError.set(true);
        this.notify.apiError(err, this.transloco.translate('students.loadHeaderError'));
      },
    });
  }

  protected retry(): void {
    this.loadHeader();
    this.loadBasic();
  }

  private loadBasic(): void {
    this.loadingBasic.set(true);
    this.data.getBasicDetails(this.studentId()).subscribe({
      next: row => {
        this.current.set(row);
        this.applyToForm(row);
        this.loadingBasic.set(false);
      },
      error: err => {
        this.loadingBasic.set(false);
        this.notify.apiError(err, this.transloco.translate('students.loadBasicError'));
      },
    });
  }

  protected pickArea(area: AreaTab): void {
    if (!area.enabled || area.key === this.activeArea()) return;
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
    const first = this.firstName().trim().charAt(0);
    const last = this.lastName().trim().charAt(0);
    return (first + last).toUpperCase();
  }

  protected readonly photoUploading = signal(false);

  protected photoSrc(photoId: string): string {
    return this.data.photoUrl(this.studentId(), photoId);
  }

  protected async onPhotoSelected(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file) return;

    if (!file.type.startsWith('image/')) {
      this.notify.error(this.transloco.translate('students.photo.invalidType'));
      return;
    }
    if (file.size > MAX_ATTACHMENT_BYTES) {
      this.notify.error(
        this.transloco.translate('students.photo.tooLarge', { size: MAX_ATTACHMENT_LABEL }),
      );
      return;
    }

    this.photoUploading.set(true);
    try {
      await firstValueFrom(this.data.uploadPhoto(this.studentId(), file));
      this.notify.success(this.transloco.translate('students.photo.savedToast'));
      this.loadHeader();
      this.loadBasic();
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('students.photo.saveError'));
    } finally {
      this.photoUploading.set(false);
    }
  }

  protected async removePhoto(): Promise<void> {
    const ok = await this.confirm.confirm({
      header: this.transloco.translate('students.photo.removeHeader'),
      message: this.transloco.translate('students.photo.removeConfirm'),
      acceptLabel: this.transloco.translate('students.photo.remove'),
      acceptSeverity: 'danger',
    });
    if (!ok) return;

    this.photoUploading.set(true);
    try {
      await firstValueFrom(this.data.deletePhoto(this.studentId()));
      this.notify.success(this.transloco.translate('students.photo.removedToast'));
      this.loadHeader();
      this.loadBasic();
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('students.photo.removeError'));
    } finally {
      this.photoUploading.set(false);
    }
  }

  protected startEdit(): void {
    this.basicSubmitAttempted.set(false);
    this.editing.set(true);
  }

  protected cancelEdit(): void {
    this.applyToForm(this.current());
    this.basicSubmitAttempted.set(false);
    this.editing.set(false);
  }

  async save(): Promise<void> {
    if (!this.canEditBasic() || this.saving()) return;
    if (!this.basicValid()) {
      this.basicSubmitAttempted.set(true);
      focusFirstInvalid(this.host.nativeElement);
      return;
    }
    await this.saveBasic();
  }

  private async saveBasic(): Promise<void> {
    if (!this.canEditBasic() || !this.basicValid() || this.saving()) return;
    this.saving.set(true);

    const c = this.current();
    const payload: StudentBasicDetailsUpsertRequest = {
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
    };

    try {
      await firstValueFrom(this.data.updateBasicDetails(this.studentId(), payload));
      this.notify.success(this.transloco.translate('students.savedToast'));
      this.basicSubmitAttempted.set(false);
      this.editing.set(false);
      this.loadHeader();
      this.loadBasic();
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('students.saveError'));
    } finally {
      this.saving.set(false);
    }
  }

  protected backToList(): void {
    this.router.navigate(['/staff/people/students']);
  }

  protected async confirmDelete(): Promise<void> {
    if (this.deleting()) return;

    const ok = await this.confirm.confirm({
      header: this.transloco.translate('students.delete.header'),
      message: this.transloco.translate('students.delete.confirm'),
      acceptLabel: this.transloco.translate('common.delete'),
      acceptSeverity: 'danger',
    });

    if (!ok) return;

    this.deleting.set(true);
    try {
      await firstValueFrom(this.data.delete(this.studentId()));
      this.notify.success(this.transloco.translate('students.delete.toast'));
      this.router.navigate(['/staff/people/students']);
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('students.delete.error'));
    } finally {
      this.deleting.set(false);
    }
  }

  private applyToForm(row: StudentBasicDetailsResponse | null): void {
    if (!row) {
      this.admissionNumber.set(null);
      this.title.set(null);
      this.firstName.set('');
      this.middleName.set(null);
      this.lastName.set('');
      this.preferredFirstName.set(null);
      this.preferredLastName.set(null);
      this.gender.set('');
      this.dob.set(null);
    } else {
      this.admissionNumber.set(row.admissionNumber);
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
