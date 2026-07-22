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
import { CurrencyPipe, DatePipe } from '@angular/common';
import { MpBadge, MpButton } from '@myportal/ui';
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
  SenNeedResponse,
  SenNeedUpsertRequest,
  SenProvisionResponse,
  SenProvisionUpsertRequest,
  SenStatementResponse,
  SenStatementUpsertRequest,
  SetSenStatusRequest,
  StudentSenDetailsResponse,
} from '../../../../../../shared/types/student-sen';
import { StudentAreaPanel } from './student-area-panel';
import { StudentSenStatusDialog } from './student-sen-status-dialog';
import { StudentSenNeedDialog } from './student-sen-need-dialog';
import { StudentSenProvisionDialog } from './student-sen-provision-dialog';
import { StudentSenStatementDialog } from './student-sen-statement-dialog';

@Component({
  selector: 'mp-student-sen-panel',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CurrencyPipe,
    DatePipe,
    MpBadge,
    MpButton,
    Loading,
    SectionHeader,
    EmptyState,
    StudentSenStatusDialog,
    StudentSenNeedDialog,
    StudentSenProvisionDialog,
    StudentSenStatementDialog,
    TranslocoDirective,
  ],
  providers: [
    provideTranslocoScope('students'),
    { provide: StudentAreaPanel, useExisting: forwardRef(() => StudentSenPanel) },
  ],
  templateUrl: './student-sen-panel.html',
})
export class StudentSenPanel extends StudentAreaPanel implements OnInit {
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
  protected readonly sen = signal<StudentSenDetailsResponse | null>(null);

  protected readonly statusDialogOpen = signal(false);
  protected readonly needDialogOpen = signal(false);
  protected readonly provisionDialogOpen = signal(false);
  protected readonly statementDialogOpen = signal(false);

  protected readonly needTarget = signal<SenNeedResponse | null>(null);
  protected readonly provisionTarget = signal<SenProvisionResponse | null>(null);
  protected readonly statementTarget = signal<SenStatementResponse | null>(null);

  protected readonly savingStatus = signal(false);
  protected readonly savingNeed = signal(false);
  protected readonly savingProvision = signal(false);
  protected readonly savingStatement = signal(false);

  protected readonly senStatuses = computed(() => this.sen()?.senStatuses ?? []);
  protected readonly senTypes = computed(() => this.sen()?.senTypes ?? []);
  protected readonly provisionTypes = computed(() => this.sen()?.senProvisionTypes ?? []);
  protected readonly assessmentAgreedOptions = computed(
    () => this.sen()?.statutoryAssessmentAgreedOptions ?? [],
  );
  protected readonly assessmentOutcomeOptions = computed(
    () => this.sen()?.statutoryAssessmentOutcomeOptions ?? [],
  );

  protected readonly currentStatusId = computed(() => this.sen()?.currentSenStatusId ?? null);
  protected readonly currentStartDate = computed(() => this.sen()?.senStartDate ?? null);
  protected readonly currentStatusName = computed(() =>
    this.currentStatusId() ? this.lookupLabel(this.senStatuses(), this.currentStatusId()) : null,
  );

  protected readonly statusHistory = computed(() =>
    [...(this.sen()?.statusHistory ?? [])].sort((a, b) => {
      if (!a.endDate && b.endDate) return -1;
      if (a.endDate && !b.endDate) return 1;
      return new Date(b.startDate).getTime() - new Date(a.startDate).getTime();
    }),
  );
  protected readonly needs = computed(() =>
    [...(this.sen()?.needs ?? [])].sort((a, b) => a.rank - b.rank),
  );
  protected readonly provisions = computed(() => this.sen()?.provisions ?? []);
  protected readonly statements = computed(() => this.sen()?.statements ?? []);

  override readonly canEdit = computed(() =>
    this.permissions().has(Permissions.Student.EditStudentSen),
  );

  override startEdit(): void {}
  override cancel(): void {}
  override async save(): Promise<void> {}

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.data.getSen(this.studentId()).subscribe({
      next: row => {
        this.sen.set(row);
        this.loading.set(false);
      },
      error: err => {
        this.loading.set(false);
        this.notify.apiError(err, this.transloco.translate('students.sen.loadError'));
      },
    });
  }

  protected statusName(senStatusId: string): string {
    return this.lookupLabel(this.senStatuses(), senStatusId);
  }

  protected needTypeName(need: SenNeedResponse): string {
    return this.lookupLabel(this.senTypes(), need.senTypeId);
  }

  protected provisionTypeName(provision: SenProvisionResponse): string {
    return this.lookupLabel(this.provisionTypes(), provision.senProvisionTypeId);
  }

  protected openStatusDialog(): void {
    this.statusDialogOpen.set(true);
  }

  protected async onStatusSave(payload: SetSenStatusRequest): Promise<void> {
    this.savingStatus.set(true);
    try {
      await firstValueFrom(this.data.setSenStatus(this.studentId(), payload));
      this.notify.success(this.transloco.translate('students.sen.status.savedToast'));
      this.statusDialogOpen.set(false);
      this.load();
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('students.sen.status.saveError'));
    } finally {
      this.savingStatus.set(false);
    }
  }

  protected async undoLatestStatus(): Promise<void> {
    const ok = await this.confirm.confirm({
      header: this.transloco.translate('students.sen.status.undoHeader'),
      message: this.transloco.translate('students.sen.status.undoConfirm'),
      acceptLabel: this.transloco.translate('students.sen.status.undo'),
      acceptSeverity: 'danger',
    });
    if (!ok) return;
    this.savingStatus.set(true);
    try {
      await firstValueFrom(this.data.undoLatestSenStatus(this.studentId()));
      this.notify.success(this.transloco.translate('students.sen.status.undoneToast'));
      this.load();
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('students.sen.status.undoError'));
    } finally {
      this.savingStatus.set(false);
    }
  }

  protected openAddNeed(): void {
    this.needTarget.set(null);
    this.needDialogOpen.set(true);
  }

  protected openEditNeed(need: SenNeedResponse): void {
    this.needTarget.set(need);
    this.needDialogOpen.set(true);
  }

  protected async onNeedSave(item: SenNeedUpsertRequest): Promise<void> {
    const existing = this.needs().map(this.toNeedUpsert);
    const next = item.id
      ? existing.map(e => (e.id === item.id ? item : e))
      : [...existing, item];
    await this.persistNeeds(next, 'students.sen.needs.savedToast');
  }

  protected async removeNeed(need: SenNeedResponse): Promise<void> {
    const ok = await this.confirm.confirm({
      header: this.transloco.translate('students.sen.needs.removeHeader'),
      message: this.transloco.translate('students.sen.needs.removeConfirm'),
      acceptLabel: this.transloco.translate('common.delete'),
      acceptSeverity: 'danger',
    });
    if (!ok) return;
    const next = this.needs()
      .filter(n => n.id !== need.id)
      .map(this.toNeedUpsert);
    await this.persistNeeds(next, 'students.sen.needs.removedToast');
  }

  private async persistNeeds(payload: SenNeedUpsertRequest[], toastKey: string): Promise<void> {
    this.savingNeed.set(true);
    try {
      await firstValueFrom(this.data.updateSenNeeds(this.studentId(), payload));
      this.notify.success(this.transloco.translate(toastKey));
      this.needDialogOpen.set(false);
      this.load();
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('students.sen.needs.saveError'));
    } finally {
      this.savingNeed.set(false);
    }
  }

  protected openAddProvision(): void {
    this.provisionTarget.set(null);
    this.provisionDialogOpen.set(true);
  }

  protected openEditProvision(provision: SenProvisionResponse): void {
    this.provisionTarget.set(provision);
    this.provisionDialogOpen.set(true);
  }

  protected async onProvisionSave(item: SenProvisionUpsertRequest): Promise<void> {
    const existing = this.provisions().map(this.toProvisionUpsert);
    const next = item.id
      ? existing.map(e => (e.id === item.id ? item : e))
      : [...existing, item];
    await this.persistProvisions(next, 'students.sen.provisions.savedToast');
  }

  protected async removeProvision(provision: SenProvisionResponse): Promise<void> {
    const ok = await this.confirm.confirm({
      header: this.transloco.translate('students.sen.provisions.removeHeader'),
      message: this.transloco.translate('students.sen.provisions.removeConfirm'),
      acceptLabel: this.transloco.translate('common.delete'),
      acceptSeverity: 'danger',
    });
    if (!ok) return;
    const next = this.provisions()
      .filter(p => p.id !== provision.id)
      .map(this.toProvisionUpsert);
    await this.persistProvisions(next, 'students.sen.provisions.removedToast');
  }

  private async persistProvisions(
    payload: SenProvisionUpsertRequest[],
    toastKey: string,
  ): Promise<void> {
    this.savingProvision.set(true);
    try {
      await firstValueFrom(this.data.updateSenProvisions(this.studentId(), payload));
      this.notify.success(this.transloco.translate(toastKey));
      this.provisionDialogOpen.set(false);
      this.load();
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('students.sen.provisions.saveError'));
    } finally {
      this.savingProvision.set(false);
    }
  }

  protected openAddStatement(): void {
    this.statementTarget.set(null);
    this.statementDialogOpen.set(true);
  }

  protected openEditStatement(statement: SenStatementResponse): void {
    this.statementTarget.set(statement);
    this.statementDialogOpen.set(true);
  }

  protected async onStatementSave(item: SenStatementUpsertRequest): Promise<void> {
    const existing = this.statements().map(this.toStatementUpsert);
    const next = item.id
      ? existing.map(e => (e.id === item.id ? item : e))
      : [...existing, item];
    await this.persistStatements(next, 'students.sen.statements.savedToast');
  }

  protected async removeStatement(statement: SenStatementResponse): Promise<void> {
    const ok = await this.confirm.confirm({
      header: this.transloco.translate('students.sen.statements.removeHeader'),
      message: this.transloco.translate('students.sen.statements.removeConfirm'),
      acceptLabel: this.transloco.translate('common.delete'),
      acceptSeverity: 'danger',
    });
    if (!ok) return;
    const next = this.statements()
      .filter(s => s.id !== statement.id)
      .map(this.toStatementUpsert);
    await this.persistStatements(next, 'students.sen.statements.removedToast');
  }

  private async persistStatements(
    payload: SenStatementUpsertRequest[],
    toastKey: string,
  ): Promise<void> {
    this.savingStatement.set(true);
    try {
      await firstValueFrom(this.data.updateSenStatements(this.studentId(), payload));
      this.notify.success(this.transloco.translate(toastKey));
      this.statementDialogOpen.set(false);
      this.load();
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('students.sen.statements.saveError'));
    } finally {
      this.savingStatement.set(false);
    }
  }

  private toNeedUpsert(n: SenNeedResponse): SenNeedUpsertRequest {
    return {
      id: n.id,
      senTypeId: n.senTypeId,
      description: n.description ?? null,
      startDate: n.startDate,
      endDate: n.endDate ?? null,
      rank: n.rank,
    };
  }

  private toProvisionUpsert(p: SenProvisionResponse): SenProvisionUpsertRequest {
    return {
      id: p.id,
      senProvisionTypeId: p.senProvisionTypeId,
      startDate: p.startDate,
      endDate: p.endDate ?? null,
      frequency: p.frequency ?? null,
      cost: p.cost ?? null,
      note: p.note,
    };
  }

  private toStatementUpsert(s: SenStatementResponse): SenStatementUpsertRequest {
    return {
      id: s.id,
      isEhcp: s.isEhcp,
      assessmentRequestDate: s.assessmentRequestDate,
      parentConsultDate: s.parentConsultDate ?? null,
      finalisedDate: s.finalisedDate ?? null,
      ceasedDate: s.ceasedDate ?? null,
      statutoryAssessmentAgreedId: s.statutoryAssessmentAgreedId ?? null,
      statutoryAssessmentOutcomeId: s.statutoryAssessmentOutcomeId ?? null,
      subjectToTribunal: s.subjectToTribunal,
      undergoingMediation: s.undergoingMediation,
      appealNotes: s.appealNotes ?? null,
      temporaryDisapplicationSubjects: s.temporaryDisapplicationSubjects ?? null,
      permanentDisapplicationSubjects: s.permanentDisapplicationSubjects ?? null,
      localAuthorityId: s.localAuthorityId ?? null,
      comments: s.comments ?? null,
    };
  }
}
