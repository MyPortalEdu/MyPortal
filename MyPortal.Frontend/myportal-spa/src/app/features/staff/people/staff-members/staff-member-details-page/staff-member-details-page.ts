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
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { DatePicker } from 'primeng/datepicker';
import { InputText } from 'primeng/inputtext';
import { ProgressSpinner } from 'primeng/progressspinner';
import { Tag } from 'primeng/tag';
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
  { key: 'contactDetails',       icon: 'fa-solid fa-address-book',    enabled: false },
  { key: 'equalityDetails',      icon: 'fa-solid fa-scale-balanced',  enabled: false },
  { key: 'professionalDetails',  icon: 'fa-solid fa-graduation-cap',  enabled: false },
  { key: 'employmentDetails',    icon: 'fa-solid fa-briefcase',       enabled: false },
  { key: 'preEmploymentChecks',  icon: 'fa-solid fa-shield-halved',   enabled: false },
  { key: 'absences',             icon: 'fa-solid fa-calendar-xmark',  enabled: false },
  { key: 'timetable',            icon: 'fa-solid fa-calendar-days',   enabled: false },
  { key: 'performanceDetails',   icon: 'fa-solid fa-chart-line',      enabled: false },
  { key: 'documents',            icon: 'fa-solid fa-folder',          enabled: false },
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
    ProgressSpinner,
    Tag,
    PageHeader,
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

  protected readonly areas = AREAS;
  protected readonly activeArea = signal<AreaKey>('basicDetails');

  protected readonly loadingHeader = signal(false);
  protected readonly loadingBasic = signal(false);
  protected readonly saving = signal(false);
  protected readonly editing = signal(false);

  protected readonly staffMemberId = signal<string>('');
  protected readonly header = signal<StaffMemberHeaderResponse | null>(null);
  protected readonly current = signal<StaffBasicDetailsResponse | null>(null);

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

  protected readonly isValid = computed(
    () =>
      this.firstName().trim().length > 0 &&
      this.lastName().trim().length > 0 &&
      this.gender().trim().length > 0 &&
      this.code().trim().length > 0,
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

  protected readonly isDirty = computed(() => {
    const s = this.snapshot();
    if (!s) return false;
    return JSON.stringify(s) !== JSON.stringify(this.currentForm());
  });

  protected readonly headerActions = computed<HeaderAction[]>(() => {
    if (!this.canEditBasic() || this.activeArea() !== 'basicDetails') return [];
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

  protected pickArea(area: AreaTab): void {
    if (!area.enabled) return;
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
    this.activeArea.set(area.key);
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

  protected startEdit(): void {
    this.editing.set(true);
  }

  protected cancelEdit(): void {
    this.applyToForm(this.current());
    this.editing.set(false);
  }

  async save(): Promise<void> {
    if (!this.canEditBasic() || !this.isValid() || this.saving()) return;
    this.saving.set(true);

    const c = this.current();
    // Preserve fields we don't yet render an input for (Nationality / Language /
    // MaritalStatus / PhotoId / Deceased) — these come back to UI when their
    // lookup endpoints land. Sending the existing values keeps the row honest.
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
      nationalityId: c?.nationalityId ?? null,
      firstLanguageId: c?.firstLanguageId ?? null,
      maritalStatusId: c?.maritalStatusId ?? null,
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

  protected backToList(): void {
    this.router.navigate(['/staff/people/staff-members']);
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
