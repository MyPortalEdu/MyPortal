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
import { FormsModule } from '@angular/forms';
import { Checkbox } from 'primeng/checkbox';
import { Textarea } from 'primeng/textarea';
import { MultiSelect } from 'primeng/multiselect';
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
import { SectionHeader } from '../../../../../../shared/components/section-header/section-header';
import { Field } from '../../../../../../shared/components/field/field';
import {
  StaffEqualityDetailsResponse,
  StaffEqualityDetailsUpsertRequest,
} from '../../../../../../shared/types/staff-equality-details';
import { StaffAreaPanel } from './staff-area-panel';

/**
 * Equality & Diversity area of the staff profile. Special-category data: HR-edit-only (no self or
 * line-manager edit). Self-loads on mount; every field is optional so the form is always valid.
 */
@Component({
  selector: 'mp-staff-equality-panel',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule, Checkbox, Textarea, MultiSelect, LookupSelect, Loading, SectionHeader, Field, TranslocoDirective],
  providers: [
    provideTranslocoScope('staff-members'),
    { provide: StaffAreaPanel, useExisting: forwardRef(() => StaffEqualityPanel) },
  ],
  templateUrl: './staff-equality-panel.html',
})
export class StaffEqualityPanel extends StaffAreaPanel implements OnInit {
  private readonly data = inject(StaffMembersDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);

  readonly staffMemberId = input.required<string>();
  readonly permissions = input.required<Set<string>>();

  protected readonly loading = signal(false);
  override readonly saving = signal(false);
  override readonly editing = signal(false);

  protected readonly equality = signal<StaffEqualityDetailsResponse | null>(null);

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
  private readonly snapshot = signal<string>('');

  // Option lists travel with the equality payload so the editor is self-contained.
  protected readonly ethnicities = computed(() => this.equality()?.ethnicities ?? []);
  protected readonly nationalities = computed(() => this.equality()?.nationalities ?? []);
  protected readonly languages = computed(() => this.equality()?.languages ?? []);
  protected readonly maritalStatuses = computed(() => this.equality()?.maritalStatuses ?? []);
  protected readonly religions = computed(() => this.equality()?.religions ?? []);
  protected readonly sexualOrientations = computed(() => this.equality()?.sexualOrientations ?? []);
  protected readonly genderIdentities = computed(() => this.equality()?.genderIdentities ?? []);
  protected readonly disabilities = computed(() => this.equality()?.disabilities ?? []);

  // Equality is HR-edit-only — no self/managed edit.
  override readonly canEdit = computed(() =>
    this.permissions().has(Permissions.Staff.EditAllStaffEqualityDetails),
  );

  // Every equality field is optional — nothing to invalidate.
  override readonly valid = computed(() => true);

  // Serialised edit state for the dirty check; disability ids sorted so reorder alone isn't a change.
  private readonly form = computed(() =>
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

  override readonly dirty = computed(
    () => this.equality() != null && this.snapshot() !== this.form(),
  );

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.data.getEqualityDetails(this.staffMemberId()).subscribe({
      next: row => {
        this.equality.set(row);
        this.apply(row);
        this.loading.set(false);
      },
      error: err => {
        this.loading.set(false);
        this.notify.apiError(err, this.transloco.translate('staff-members.loadEqualityError'));
      },
    });
  }

  override startEdit(): void {
    this.editing.set(true);
  }

  override cancel(): void {
    this.apply(this.equality());
    this.editing.set(false);
  }

  override async save(): Promise<void> {
    if (!this.canEdit() || this.saving()) return;
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
      this.load();
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('staff-members.saveEqualityError'));
    } finally {
      this.saving.set(false);
    }
  }

  private apply(row: StaffEqualityDetailsResponse | null): void {
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
    this.snapshot.set(this.form());
  }

  // Disability detail (type + free text) only applies once a disability is declared; clearing the
  // declaration discards those values so nothing stale is saved.
  protected onHasDisability(value: boolean): void {
    this.hasDisability.set(value);
    if (!value) {
      this.disabilityIds.set([]);
      this.disabilityDetails.set(null);
    }
  }
}
