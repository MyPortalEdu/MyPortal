import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  effect,
  forwardRef,
  inject,
  input,
  signal,
  untracked,
} from '@angular/core';
import { FormField, applyEach, form, maxLength, required, submit } from '@angular/forms/signals';
import { DatePipe } from '@angular/common';
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
import { SectionHeader } from '../../../../../../shared/components/section-header/section-header';
import { Field } from '../../../../../../shared/components/field/field';
import {
  StaffEqualityDetailsResponse,
  StaffEqualityDetailsUpsertRequest,
} from '../../../../../../shared/types/staff-equality-details';
import { StaffAreaPanel } from './staff-area-panel';

interface DisabilityFormRow {
  disabilityId: string | null;
  dateAdvised: Date | null;
  isLongTerm: boolean;
  affectsWorkingAbility: boolean;
  assistanceRequired: string;
}

interface EqualityModel {
  ethnicityId: string | null;
  nationalityId: string | null;
  firstLanguageId: string | null;
  maritalStatusId: string | null;
  religionId: string | null;
  sexualOrientationId: string | null;
  genderIdentityId: string | null;
  hasDisability: boolean;
  disabilityDetails: string;
  disabilityNumber: string;
  impairmentEffectId: string | null;
  declaredDisabilities: DisabilityFormRow[];
}

@Component({
  selector: 'mp-staff-equality-panel',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DatePipe, FormField, MpButton, MpCheckbox, MpDatePicker, MpInput, MpTextarea, LookupSelect, Loading, SectionHeader, Field, TranslocoDirective],
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
  override readonly editing = signal(false);

  protected readonly equality = signal<StaffEqualityDetailsResponse | null>(null);

  protected readonly model = signal<EqualityModel>({
    ethnicityId: null,
    nationalityId: null,
    firstLanguageId: null,
    maritalStatusId: null,
    religionId: null,
    sexualOrientationId: null,
    genderIdentityId: null,
    hasDisability: false,
    disabilityDetails: '',
    disabilityNumber: '',
    impairmentEffectId: null,
    declaredDisabilities: [],
  });
  protected readonly f = form(this.model, path => {
    applyEach(path.declaredDisabilities, item => {
      required(item.disabilityId);
      maxLength(item.assistanceRequired, 512);
    });
  });
  private readonly snapshot = signal<string>('');

  protected readonly ethnicities = computed(() => this.equality()?.ethnicities ?? []);
  protected readonly nationalities = computed(() => this.equality()?.nationalities ?? []);
  protected readonly languages = computed(() => this.equality()?.languages ?? []);
  protected readonly maritalStatuses = computed(() => this.equality()?.maritalStatuses ?? []);
  protected readonly religions = computed(() => this.equality()?.religions ?? []);
  protected readonly sexualOrientations = computed(() => this.equality()?.sexualOrientations ?? []);
  protected readonly genderIdentities = computed(() => this.equality()?.genderIdentities ?? []);
  protected readonly disabilities = computed(() => this.equality()?.disabilities ?? []);
  protected readonly impairmentEffects = computed(() => this.equality()?.impairmentEffects ?? []);

  override readonly canEdit = computed(() =>
    this.permissions().has(Permissions.Staff.EditAllStaffEqualityDetails),
  );

  override readonly saving = computed(() => this.f().submitting());
  override readonly valid = computed(() => this.f().valid());

  private readonly form = computed(() => {
    const m = this.model();
    return JSON.stringify({
      ethnicityId: m.ethnicityId,
      nationalityId: m.nationalityId,
      firstLanguageId: m.firstLanguageId,
      maritalStatusId: m.maritalStatusId,
      religionId: m.religionId,
      sexualOrientationId: m.sexualOrientationId,
      genderIdentityId: m.genderIdentityId,
      hasDisability: m.hasDisability,
      disabilityDetails: m.disabilityDetails,
      disabilityNumber: m.disabilityNumber,
      impairmentEffectId: m.impairmentEffectId,
      declaredDisabilities: m.declaredDisabilities,
    });
  });

  override readonly dirty = computed(
    () => this.equality() != null && this.snapshot() !== this.form(),
  );

  constructor() {
    super();
    effect(() => {
      if (!this.model().hasDisability) {
        untracked(() =>
          this.model.update(m =>
            m.declaredDisabilities.length === 0 && m.disabilityDetails === '' && !m.impairmentEffectId && m.disabilityNumber === ''
              ? m
              : { ...m, declaredDisabilities: [], disabilityDetails: '', impairmentEffectId: null, disabilityNumber: '' },
          ),
        );
      }
    });
  }

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
    if (!this.canEdit() || !this.dirty()) return;
    await submit(this.f, async () => {
      const m = this.model();
      const payload: StaffEqualityDetailsUpsertRequest = {
        ethnicityId: m.ethnicityId,
        nationalityId: m.nationalityId,
        firstLanguageId: m.firstLanguageId,
        maritalStatusId: m.maritalStatusId,
        religionId: m.religionId,
        sexualOrientationId: m.sexualOrientationId,
        genderIdentityId: m.genderIdentityId,
        hasDisability: m.hasDisability,
        disabilityDetails: this.normalise(m.disabilityDetails),
        disabilityNumber: this.normalise(m.disabilityNumber),
        impairmentEffectId: m.impairmentEffectId,
        declaredDisabilities: m.declaredDisabilities.map(d => ({
          disabilityId: d.disabilityId,
          dateAdvised: d.dateAdvised?.toISOString() ?? null,
          isLongTerm: d.isLongTerm,
          affectsWorkingAbility: d.affectsWorkingAbility,
          assistanceRequired: this.normalise(d.assistanceRequired),
        })),
      };
      try {
        await firstValueFrom(this.data.updateEqualityDetails(this.staffMemberId(), payload));
      } catch (err) {
        this.notify.apiError(err, this.transloco.translate('staff-members.saveEqualityError'));
        return;
      }
      this.notify.success(this.transloco.translate('staff-members.savedEqualityToast'));
      this.editing.set(false);
      this.load();
    });
  }

  protected addDisability(): void {
    this.model.update(m => ({
      ...m,
      declaredDisabilities: [
        ...m.declaredDisabilities,
        { disabilityId: null, dateAdvised: null, isLongTerm: false, affectsWorkingAbility: false, assistanceRequired: '' },
      ],
    }));
  }

  protected removeDisability(index: number): void {
    this.model.update(m => ({
      ...m,
      declaredDisabilities: m.declaredDisabilities.filter((_, i) => i !== index),
    }));
  }

  private apply(row: StaffEqualityDetailsResponse | null): void {
    this.model.set({
      ethnicityId: row?.ethnicityId ?? null,
      nationalityId: row?.nationalityId ?? null,
      firstLanguageId: row?.firstLanguageId ?? null,
      maritalStatusId: row?.maritalStatusId ?? null,
      religionId: row?.religionId ?? null,
      sexualOrientationId: row?.sexualOrientationId ?? null,
      genderIdentityId: row?.genderIdentityId ?? null,
      hasDisability: row?.hasDisability ?? false,
      disabilityDetails: row?.disabilityDetails ?? '',
      disabilityNumber: row?.disabilityNumber ?? '',
      impairmentEffectId: row?.impairmentEffectId ?? null,
      declaredDisabilities: (row?.declaredDisabilities ?? []).map(d => ({
        disabilityId: d.disabilityId,
        dateAdvised: d.dateAdvised ? new Date(d.dateAdvised) : null,
        isLongTerm: d.isLongTerm,
        affectsWorkingAbility: d.affectsWorkingAbility,
        assistanceRequired: d.assistanceRequired ?? '',
      })),
    });
    this.f().reset();
    this.snapshot.set(this.form());
  }
}
