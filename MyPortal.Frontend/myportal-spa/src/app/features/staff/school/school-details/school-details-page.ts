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
import { Button } from 'primeng/button';
import { InputText } from 'primeng/inputtext';
import { InputNumber } from 'primeng/inputnumber';
import { Select } from 'primeng/select';
import { ProgressSpinner } from 'primeng/progressspinner';
import { firstValueFrom } from 'rxjs';
import { TranslocoDirective, TranslocoPipe, TranslocoService, provideTranslocoScope } from '@jsverse/transloco';

import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { HeaderAction } from '../../../../shared/types/header-action.type';
import { CanComponentDeactivate } from '../../../../core/guards/can-deactivate.guard';
import { ConfirmationDialog } from '../../../../core/services/confirmation.service';
import { LocalAuthorityPicker } from '../../../../shared/components/pickers/local-authority-picker/local-authority-picker';
import { StaffMemberPicker } from '../../../../shared/components/pickers/staff-member-picker/staff-member-picker';
import { LookupsDataService } from '../../../../shared/services/lookups-data.service';
import { SchoolsDataService } from '../../../../shared/services/schools-data.service';
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
};

/**
 * School details edit page. Single-tenant — there's only ever one local school.
 *
 * Permission model:
 *   • Agencies.ViewAgencies → read-only render (every input disabled, no Save).
 *   • Agencies.EditAgencies → editable form with Save.
 * The route guard accepts either; the page itself decides which mode it's in
 * by inspecting the user's permissions on load.
 */
@Component({
  selector: 'mp-school-details-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    FormsModule,
    Button,
    InputText,
    InputNumber,
    Select,
    ProgressSpinner,
    PageHeader,
    LocalAuthorityPicker,
    StaffMemberPicker,
    TranslocoDirective,
    TranslocoPipe,
  ],
  providers: [provideTranslocoScope('school-details')],
  templateUrl: './school-details-page.html',
})
export class SchoolDetailsPage implements OnInit, CanComponentDeactivate {
  private readonly schools = inject(SchoolsDataService);
  private readonly lookups = inject(LookupsDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);
  private readonly me = inject(MeService);
  private readonly confirm = inject(ConfirmationDialog);

  readonly canEdit = signal(false);
  readonly loading = signal(false);
  readonly saving = signal(false);

  // Existing school payload (null on first run, before any school is configured).
  readonly current = signal<SchoolDetailsResponse | null>(null);

  // Lookup catalogues — loaded once via the cached LookupsDataService.
  readonly governanceTypes = signal<LookupResponse[]>([]);
  readonly intakeTypes = signal<LookupResponse[]>([]);
  readonly schoolPhases = signal<LookupResponse[]>([]);
  readonly schoolTypes = signal<LookupResponse[]>([]);

  // Form-field signals. Mirror SchoolUpsertRequest 1:1 so save() is a flat copy.
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

  // Snapshot of the form as last loaded/saved. isDirty compares the live signals
  // against this so we can disable Save when nothing's changed and prompt the
  // user before they navigate away with unsaved edits.
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
          icon: 'fa-solid fa-floppy-disk',
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
    this.headTeacherId.set(s.id);
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
      // Agency carries an optimistic-concurrency version. Backend ignores it on
      // first-create; on update it must equal the row's current Version.
      // The school details payload doesn't surface Agency.Version yet, so we
      // send 0 — the server's update path doesn't enforce a non-zero check
      // (UpdateAsync is non-versioned in AgencyService.UpdateAsync).
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
    };

    try {
      await firstValueFrom(this.schools.saveLocalDetails(payload));
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

  // Browser-level guard for refresh/close/back-to-non-Angular. Setting
  // returnValue triggers the browser's generic "leave site?" prompt; the
  // string isn't displayed (modern browsers ignore it for safety).
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
