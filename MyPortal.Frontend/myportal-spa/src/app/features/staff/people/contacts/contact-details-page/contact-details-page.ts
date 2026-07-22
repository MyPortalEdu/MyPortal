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
import { MpDatePicker, MpButton, MpCard, MpCheckbox, MpInput, MpSpinner, MpMenu, type MpMenuItem } from '@myportal/ui';
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
import { ContactsDataService } from '../../../../../shared/services/contacts-data.service';
import { ContactHeaderResponse } from '../../../../../shared/types/contact-header';
import {
  ContactBasicDetailsResponse,
  ContactBasicDetailsUpsertRequest,
} from '../../../../../shared/types/contact-basic-details';
import { GenderSelect } from '../../../../../shared/components/gender-select/gender-select';
import { GenderLabelPipe } from '../../../../../shared/pipes/gender-label.pipe';
import { ContactAreaPanel } from './panels/contact-area-panel';
import { ContactCommunicationsPanel } from './panels/contact-communications-panel';
import { ContactAssociatedStudentsPanel } from './panels/contact-associated-students-panel';

interface BasicModel {
  title: string;
  firstName: string;
  middleName: string;
  lastName: string;
  preferredFirstName: string;
  preferredLastName: string;
  gender: string;
  dob: Date | null;
  parentalBallot: boolean;
  placeOfWork: string;
  jobTitle: string;
  niNumber: string;
}

type AreaKey = 'basicDetails' | 'communications' | 'associatedStudents';

interface AreaTab {
  key: AreaKey;
  icon: string;
  enabled: boolean;
}

const AREAS: AreaTab[] = [
  { key: 'basicDetails',       icon: 'fa-solid fa-user',            enabled: true  },
  { key: 'communications',     icon: 'fa-solid fa-address-book',    enabled: false },
  { key: 'associatedStudents', icon: 'fa-solid fa-graduation-cap',  enabled: false },
];

@Component({
  selector: 'mp-contact-details-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    DatePipe,
    MpButton,
    MpCard,
    FormField,
    MpCheckbox,
    MpDatePicker,
    MpInput,
    MpMenu,
    MpSpinner,
    PageHeader,
    Loading,
    ErrorState,
    EmptyState,
    SectionHeader,
    Field,
    GenderSelect,
    GenderLabelPipe,
    ContactCommunicationsPanel,
    ContactAssociatedStudentsPanel,
    TranslocoDirective,
  ],
  providers: [provideTranslocoScope('contacts')],
  templateUrl: './contact-details-page.html',
})
export class ContactDetailsPage implements OnInit, CanComponentDeactivate {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly data = inject(ContactsDataService);
  private readonly me = inject(MeService);
  private readonly notify = inject(NotificationService);
  private readonly confirm = inject(ConfirmationDialog);
  private readonly transloco = inject(TranslocoService);

  protected readonly areas = computed<AreaTab[]>(() =>
    AREAS.map(a => {
      if (a.key === 'communications') return { ...a, enabled: this.canViewCommunications() };
      if (a.key === 'associatedStudents') return { ...a, enabled: this.canViewAssociatedStudents() };
      return a;
    }),
  );
  protected readonly activeArea = signal<AreaKey>('basicDetails');

  private static readonly EXTRACTED_AREAS: ReadonlySet<AreaKey> = new Set<AreaKey>([
    'communications',
    'associatedStudents',
  ]);
  private readonly activePanel = viewChild(ContactAreaPanel);

  protected readonly loadingHeader = signal(false);
  protected readonly headerError = signal(false);
  protected readonly loadingBasic = signal(false);
  protected readonly editing = signal(false);

  protected readonly contactId = signal<string>('');
  protected readonly header = signal<ContactHeaderResponse | null>(null);
  protected readonly current = signal<ContactBasicDetailsResponse | null>(null);

  protected readonly model = signal<BasicModel>({
    title: '',
    firstName: '',
    middleName: '',
    lastName: '',
    preferredFirstName: '',
    preferredLastName: '',
    gender: '',
    dob: null,
    parentalBallot: false,
    placeOfWork: '',
    jobTitle: '',
    niNumber: '',
  });
  protected readonly f = form(this.model, path => {
    for (const field of [path.firstName, path.lastName, path.gender]) {
      validate(field, ({ value }) =>
        value().trim().length ? undefined : { kind: 'required' },
      );
    }
  });
  private readonly snapshot = signal<string>('');

  protected readonly heldPerms = signal<Set<string>>(new Set());

  protected readonly canEditBasic = computed(() =>
    this.heldPerms().has(Permissions.Contact.EditContactDetails),
  );

  protected readonly canViewCommunications = computed(() =>
    this.heldPerms().has(Permissions.Contact.ViewContactDetails),
  );

  protected readonly canViewAssociatedStudents = computed(() =>
    this.heldPerms().has(Permissions.Contact.ViewContactDetails),
  );

  protected readonly canEditActiveArea = computed(() => {
    switch (this.activeArea()) {
      case 'basicDetails':
        return this.canEditBasic();
      case 'communications':
        return this.heldPerms().has(Permissions.Contact.EditContactDetails);
      default:
        return false;
    }
  });

  protected readonly canDelete = computed(() =>
    this.heldPerms().has(Permissions.Contact.EditContactDetails),
  );

  protected readonly deleting = signal(false);

  protected readonly deleteMenuItems = computed<MpMenuItem[]>(() => [
    {
      label: this.transloco.translate('contacts.delete.menuItem'),
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
    if (panel && ContactDetailsPage.EXTRACTED_AREAS.has(this.activeArea())) {
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
    this.contactId.set(id);

    this.me.me().subscribe(me => this.heldPerms.set(new Set(me.permissions ?? [])));

    this.loadHeader();
    this.loadBasic();
  }

  private loadHeader(): void {
    this.loadingHeader.set(true);
    this.headerError.set(false);
    this.data.getHeader(this.contactId()).subscribe({
      next: row => {
        this.header.set(row);
        this.loadingHeader.set(false);
      },
      error: err => {
        this.loadingHeader.set(false);
        this.headerError.set(true);
        this.notify.apiError(err, this.transloco.translate('contacts.loadHeaderError'));
      },
    });
  }

  protected retry(): void {
    this.loadHeader();
    this.loadBasic();
  }

  private loadBasic(): void {
    this.loadingBasic.set(true);
    this.data.getBasicDetails(this.contactId()).subscribe({
      next: row => {
        this.current.set(row);
        this.applyToForm(row);
        this.loadingBasic.set(false);
      },
      error: err => {
        this.loadingBasic.set(false);
        this.notify.apiError(err, this.transloco.translate('contacts.loadBasicError'));
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

  protected initials(): string {
    const first = this.model().firstName.trim().charAt(0);
    const last = this.model().lastName.trim().charAt(0);
    return (first + last).toUpperCase();
  }

  protected readonly photoUploading = signal(false);

  protected photoSrc(photoId: string): string {
    return this.data.photoUrl(this.contactId(), photoId);
  }

  protected async onPhotoSelected(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file) return;

    if (!file.type.startsWith('image/')) {
      this.notify.error(this.transloco.translate('contacts.photo.invalidType'));
      return;
    }
    if (file.size > MAX_ATTACHMENT_BYTES) {
      this.notify.error(
        this.transloco.translate('contacts.photo.tooLarge', { size: MAX_ATTACHMENT_LABEL }),
      );
      return;
    }

    this.photoUploading.set(true);
    try {
      await firstValueFrom(this.data.uploadPhoto(this.contactId(), file));
      this.notify.success(this.transloco.translate('contacts.photo.savedToast'));
      this.loadHeader();
      this.loadBasic();
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('contacts.photo.saveError'));
    } finally {
      this.photoUploading.set(false);
    }
  }

  protected async removePhoto(): Promise<void> {
    const ok = await this.confirm.confirm({
      header: this.transloco.translate('contacts.photo.removeHeader'),
      message: this.transloco.translate('contacts.photo.removeConfirm'),
      acceptLabel: this.transloco.translate('contacts.photo.remove'),
      acceptSeverity: 'danger',
    });
    if (!ok) return;

    this.photoUploading.set(true);
    try {
      await firstValueFrom(this.data.deletePhoto(this.contactId()));
      this.notify.success(this.transloco.translate('contacts.photo.removedToast'));
      this.loadHeader();
      this.loadBasic();
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('contacts.photo.removeError'));
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
      const payload: ContactBasicDetailsUpsertRequest = {
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
        parentalBallot: m.parentalBallot,
        placeOfWork: this.normalise(m.placeOfWork),
        jobTitle: this.normalise(m.jobTitle),
        niNumber: this.normalise(m.niNumber),
      };
      try {
        await firstValueFrom(this.data.updateBasicDetails(this.contactId(), payload));
      } catch (err) {
        this.notify.apiError(err, this.transloco.translate('contacts.saveError'));
        return;
      }
      this.notify.success(this.transloco.translate('contacts.savedToast'));
      this.editing.set(false);
      this.loadHeader();
      this.loadBasic();
    });
  }

  protected backToList(): void {
    this.router.navigate(['/staff/people/contacts']);
  }

  protected async confirmDelete(): Promise<void> {
    if (this.deleting()) return;

    const ok = await this.confirm.confirm({
      header: this.transloco.translate('contacts.delete.header'),
      message: this.transloco.translate('contacts.delete.confirm'),
      acceptLabel: this.transloco.translate('common.delete'),
      acceptSeverity: 'danger',
    });

    if (!ok) return;

    this.deleting.set(true);
    try {
      await firstValueFrom(this.data.delete(this.contactId()));
      this.notify.success(this.transloco.translate('contacts.delete.toast'));
      this.router.navigate(['/staff/people/contacts']);
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('contacts.delete.error'));
    } finally {
      this.deleting.set(false);
    }
  }

  private applyToForm(row: ContactBasicDetailsResponse | null): void {
    this.model.set({
      title: row?.title ?? '',
      firstName: row?.firstName ?? '',
      middleName: row?.middleName ?? '',
      lastName: row?.lastName ?? '',
      preferredFirstName: row?.preferredFirstName ?? '',
      preferredLastName: row?.preferredLastName ?? '',
      gender: row?.gender ?? '',
      dob: row?.dob ? new Date(row.dob) : null,
      parentalBallot: row?.parentalBallot ?? false,
      placeOfWork: row?.placeOfWork ?? '',
      jobTitle: row?.jobTitle ?? '',
      niNumber: row?.niNumber ?? '',
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
