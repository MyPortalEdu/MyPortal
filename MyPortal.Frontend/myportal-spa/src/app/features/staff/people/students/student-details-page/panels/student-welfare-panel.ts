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
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MpBadge, MpButton, MpSelect } from '@myportal/ui';
import { firstValueFrom } from 'rxjs';
import {
  TranslocoDirective,
  TranslocoService,
  provideTranslocoScope,
} from '@jsverse/transloco';

import { NotificationService } from '../../../../../../core/services/notification.service';
import { ConfirmationDialog } from '../../../../../../core/services/confirmation.service';
import { Permissions } from '../../../../../../core/constants/permissions';
import { StudentsDataService } from '../../../../../../shared/services/students-data.service';
import { Loading } from '../../../../../../shared/components/loading/loading';
import { SectionHeader } from '../../../../../../shared/components/section-header/section-header';
import { EmptyState } from '../../../../../../shared/components/empty-state/empty-state';
import {
  CareEpisodeResponse,
  CareEpisodeUpsertRequest,
  ChildProtectionPlanResponse,
  ChildProtectionPlanUpsertRequest,
  PepResponse,
  PepUpsertRequest,
  StudentWelfareDetailsResponse,
  WelfareIndicatorsUpsertRequest,
} from '../../../../../../shared/types/student-welfare';
import { StudentAreaPanel } from './student-area-panel';
import { StudentWelfareCareEpisodeDialog } from './student-welfare-care-episode-dialog';
import { StudentWelfarePepDialog } from './student-welfare-pep-dialog';
import { StudentWelfareCpDialog } from './student-welfare-cp-dialog';

interface IndicatorsModel {
  postLookedAfterArrangementId: string | null;
  serviceChildIndicatorId: string | null;
  youngCarerIndicatorId: string | null;
  kinshipCareIndicatorId: string | null;
}

@Component({
  selector: 'mp-student-welfare-panel',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    DatePipe,
    FormsModule,
    MpBadge,
    MpButton,
    MpSelect,
    Loading,
    SectionHeader,
    EmptyState,
    StudentWelfareCareEpisodeDialog,
    StudentWelfarePepDialog,
    StudentWelfareCpDialog,
    TranslocoDirective,
  ],
  providers: [
    provideTranslocoScope('students'),
    { provide: StudentAreaPanel, useExisting: forwardRef(() => StudentWelfarePanel) },
  ],
  templateUrl: './student-welfare-panel.html',
})
export class StudentWelfarePanel extends StudentAreaPanel implements OnInit {
  private readonly data = inject(StudentsDataService);
  private readonly notify = inject(NotificationService);
  private readonly confirm = inject(ConfirmationDialog);
  private readonly transloco = inject(TranslocoService);

  readonly studentId = input.required<string>();
  readonly permissions = input.required<Set<string>>();

  override readonly selfManaged = true;
  override readonly editing = signal(false);
  override readonly dirty = signal(false);
  override readonly valid = signal(true);
  override readonly saving = signal(false);

  protected readonly loading = signal(false);
  protected readonly welfare = signal<StudentWelfareDetailsResponse | null>(null);

  protected readonly careDialogOpen = signal(false);
  protected readonly pepDialogOpen = signal(false);
  protected readonly cpDialogOpen = signal(false);

  protected readonly careTarget = signal<CareEpisodeResponse | null>(null);
  protected readonly pepTarget = signal<PepResponse | null>(null);
  protected readonly cpTarget = signal<ChildProtectionPlanResponse | null>(null);

  protected readonly savingIndicators = signal(false);
  protected readonly savingCare = signal(false);
  protected readonly savingPep = signal(false);
  protected readonly savingCp = signal(false);

  protected readonly indicatorsEditing = signal(false);
  protected readonly indicatorsModel = signal<IndicatorsModel>({
    postLookedAfterArrangementId: null,
    serviceChildIndicatorId: null,
    youngCarerIndicatorId: null,
    kinshipCareIndicatorId: null,
  });

  protected readonly livingArrangements = computed(() => this.welfare()?.livingArrangements ?? []);
  protected readonly caringAuthorities = computed(() => this.welfare()?.caringAuthorities ?? []);
  protected readonly postLookedAfterArrangements = computed(
    () => this.welfare()?.postLookedAfterArrangements ?? [],
  );
  protected readonly serviceChildIndicators = computed(
    () => this.welfare()?.serviceChildIndicators ?? [],
  );
  protected readonly youngCarerIndicators = computed(
    () => this.welfare()?.youngCarerIndicators ?? [],
  );
  protected readonly kinshipCareIndicators = computed(
    () => this.welfare()?.kinshipCareIndicators ?? [],
  );

  protected readonly postLookedAfterName = computed(() =>
    this.lookupLabel(this.postLookedAfterArrangements(), this.welfare()?.postLookedAfterArrangementId),
  );
  protected readonly serviceChildName = computed(() =>
    this.lookupLabel(this.serviceChildIndicators(), this.welfare()?.serviceChildIndicatorId),
  );
  protected readonly youngCarerName = computed(() =>
    this.lookupLabel(this.youngCarerIndicators(), this.welfare()?.youngCarerIndicatorId),
  );
  protected readonly kinshipCareName = computed(() =>
    this.lookupLabel(this.kinshipCareIndicators(), this.welfare()?.kinshipCareIndicatorId),
  );

  protected readonly careEpisodes = computed(() =>
    [...(this.welfare()?.careEpisodes ?? [])].sort((a, b) => {
      if (!a.endDate && b.endDate) return -1;
      if (a.endDate && !b.endDate) return 1;
      return new Date(b.startDate).getTime() - new Date(a.startDate).getTime();
    }),
  );
  protected readonly peps = computed(() =>
    [...(this.welfare()?.peps ?? [])].sort(
      (a, b) => new Date(b.startDate).getTime() - new Date(a.startDate).getTime(),
    ),
  );
  protected readonly childProtectionPlans = computed(() =>
    [...(this.welfare()?.childProtectionPlans ?? [])].sort((a, b) => {
      if (!a.endDate && b.endDate) return -1;
      if (a.endDate && !b.endDate) return 1;
      return new Date(b.startDate).getTime() - new Date(a.startDate).getTime();
    }),
  );

  override readonly canEdit = computed(() =>
    this.permissions().has(Permissions.Student.EditStudentWelfare),
  );

  override startEdit(): void {}
  override cancel(): void {}
  override async save(): Promise<void> {}

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.data.getWelfare(this.studentId()).subscribe({
      next: row => {
        this.welfare.set(row);
        this.loading.set(false);
      },
      error: err => {
        this.loading.set(false);
        this.notify.apiError(err, this.transloco.translate('students.welfare.loadError'));
      },
    });
  }

  protected caringAuthorityName(id: string): string {
    return this.lookupLabel(this.caringAuthorities(), id);
  }

  protected livingArrangementName(id: string | null | undefined): string {
    return this.lookupLabel(this.livingArrangements(), id);
  }

  protected startIndicatorsEdit(): void {
    const w = this.welfare();
    this.indicatorsModel.set({
      postLookedAfterArrangementId: w?.postLookedAfterArrangementId ?? null,
      serviceChildIndicatorId: w?.serviceChildIndicatorId ?? null,
      youngCarerIndicatorId: w?.youngCarerIndicatorId ?? null,
      kinshipCareIndicatorId: w?.kinshipCareIndicatorId ?? null,
    });
    this.indicatorsEditing.set(true);
  }

  protected cancelIndicatorsEdit(): void {
    this.indicatorsEditing.set(false);
  }

  protected updateIndicator(key: keyof IndicatorsModel, value: unknown): void {
    this.indicatorsModel.update(m => ({ ...m, [key]: (value as string | null) ?? null }));
  }

  protected async saveIndicators(): Promise<void> {
    const m = this.indicatorsModel();
    const payload: WelfareIndicatorsUpsertRequest = {
      postLookedAfterArrangementId: m.postLookedAfterArrangementId,
      serviceChildIndicatorId: m.serviceChildIndicatorId,
      youngCarerIndicatorId: m.youngCarerIndicatorId,
      kinshipCareIndicatorId: m.kinshipCareIndicatorId,
    };
    this.savingIndicators.set(true);
    try {
      await firstValueFrom(this.data.updateWelfareIndicators(this.studentId(), payload));
      this.notify.success(this.transloco.translate('students.welfare.indicators.savedToast'));
      this.indicatorsEditing.set(false);
      this.load();
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('students.welfare.indicators.saveError'));
    } finally {
      this.savingIndicators.set(false);
    }
  }

  protected openAddCare(): void {
    this.careTarget.set(null);
    this.careDialogOpen.set(true);
  }

  protected openEditCare(episode: CareEpisodeResponse): void {
    this.careTarget.set(episode);
    this.careDialogOpen.set(true);
  }

  protected async onCareSave(item: CareEpisodeUpsertRequest): Promise<void> {
    const existing = this.careEpisodes().map(this.toCareUpsert);
    const next = item.id
      ? existing.map(e => (e.id === item.id ? item : e))
      : [...existing, item];
    await this.persistCare(next, 'students.welfare.careEpisodes.savedToast');
  }

  protected async removeCare(episode: CareEpisodeResponse): Promise<void> {
    const ok = await this.confirm.confirm({
      header: this.transloco.translate('students.welfare.careEpisodes.removeHeader'),
      message: this.transloco.translate('students.welfare.careEpisodes.removeConfirm'),
      acceptLabel: this.transloco.translate('common.delete'),
      acceptSeverity: 'danger',
    });
    if (!ok) return;
    const next = this.careEpisodes()
      .filter(e => e.id !== episode.id)
      .map(this.toCareUpsert);
    await this.persistCare(next, 'students.welfare.careEpisodes.removedToast');
  }

  private async persistCare(payload: CareEpisodeUpsertRequest[], toastKey: string): Promise<void> {
    this.savingCare.set(true);
    try {
      await firstValueFrom(this.data.updateWelfareCareEpisodes(this.studentId(), payload));
      this.notify.success(this.transloco.translate(toastKey));
      this.careDialogOpen.set(false);
      this.load();
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('students.welfare.careEpisodes.saveError'));
    } finally {
      this.savingCare.set(false);
    }
  }

  protected openAddPep(): void {
    this.pepTarget.set(null);
    this.pepDialogOpen.set(true);
  }

  protected openEditPep(pep: PepResponse): void {
    this.pepTarget.set(pep);
    this.pepDialogOpen.set(true);
  }

  protected async onPepSave(item: PepUpsertRequest): Promise<void> {
    const existing = this.peps().map(this.toPepUpsert);
    const next = item.id
      ? existing.map(e => (e.id === item.id ? item : e))
      : [...existing, item];
    await this.persistPeps(next, 'students.welfare.peps.savedToast');
  }

  protected async removePep(pep: PepResponse): Promise<void> {
    const ok = await this.confirm.confirm({
      header: this.transloco.translate('students.welfare.peps.removeHeader'),
      message: this.transloco.translate('students.welfare.peps.removeConfirm'),
      acceptLabel: this.transloco.translate('common.delete'),
      acceptSeverity: 'danger',
    });
    if (!ok) return;
    const next = this.peps()
      .filter(p => p.id !== pep.id)
      .map(this.toPepUpsert);
    await this.persistPeps(next, 'students.welfare.peps.removedToast');
  }

  private async persistPeps(payload: PepUpsertRequest[], toastKey: string): Promise<void> {
    this.savingPep.set(true);
    try {
      await firstValueFrom(this.data.updateWelfarePeps(this.studentId(), payload));
      this.notify.success(this.transloco.translate(toastKey));
      this.pepDialogOpen.set(false);
      this.load();
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('students.welfare.peps.saveError'));
    } finally {
      this.savingPep.set(false);
    }
  }

  protected openAddCp(): void {
    this.cpTarget.set(null);
    this.cpDialogOpen.set(true);
  }

  protected openEditCp(plan: ChildProtectionPlanResponse): void {
    this.cpTarget.set(plan);
    this.cpDialogOpen.set(true);
  }

  protected async onCpSave(item: ChildProtectionPlanUpsertRequest): Promise<void> {
    const existing = this.childProtectionPlans().map(this.toCpUpsert);
    const next = item.id
      ? existing.map(e => (e.id === item.id ? item : e))
      : [...existing, item];
    await this.persistCps(next, 'students.welfare.childProtectionPlans.savedToast');
  }

  protected async removeCp(plan: ChildProtectionPlanResponse): Promise<void> {
    const ok = await this.confirm.confirm({
      header: this.transloco.translate('students.welfare.childProtectionPlans.removeHeader'),
      message: this.transloco.translate('students.welfare.childProtectionPlans.removeConfirm'),
      acceptLabel: this.transloco.translate('common.delete'),
      acceptSeverity: 'danger',
    });
    if (!ok) return;
    const next = this.childProtectionPlans()
      .filter(p => p.id !== plan.id)
      .map(this.toCpUpsert);
    await this.persistCps(next, 'students.welfare.childProtectionPlans.removedToast');
  }

  private async persistCps(
    payload: ChildProtectionPlanUpsertRequest[],
    toastKey: string,
  ): Promise<void> {
    this.savingCp.set(true);
    try {
      await firstValueFrom(this.data.updateWelfareChildProtectionPlans(this.studentId(), payload));
      this.notify.success(this.transloco.translate(toastKey));
      this.cpDialogOpen.set(false);
      this.load();
    } catch (err) {
      this.notify.apiError(
        err,
        this.transloco.translate('students.welfare.childProtectionPlans.saveError'),
      );
    } finally {
      this.savingCp.set(false);
    }
  }

  private toCareUpsert(e: CareEpisodeResponse): CareEpisodeUpsertRequest {
    return {
      id: e.id,
      caringAuthorityId: e.caringAuthorityId,
      livingArrangementId: e.livingArrangementId ?? null,
      startDate: e.startDate,
      endDate: e.endDate ?? null,
      comment: e.comment ?? null,
    };
  }

  private toPepUpsert(p: PepResponse): PepUpsertRequest {
    return {
      id: p.id,
      startDate: p.startDate,
      endDate: p.endDate ?? null,
      comment: p.comment ?? null,
      contributorPersonIds: p.contributors.map(c => c.personId),
    };
  }

  private toCpUpsert(p: ChildProtectionPlanResponse): ChildProtectionPlanUpsertRequest {
    return {
      id: p.id,
      localAuthorityId: p.localAuthorityId ?? null,
      startDate: p.startDate,
      endDate: p.endDate ?? null,
      comment: p.comment ?? null,
    };
  }
}
