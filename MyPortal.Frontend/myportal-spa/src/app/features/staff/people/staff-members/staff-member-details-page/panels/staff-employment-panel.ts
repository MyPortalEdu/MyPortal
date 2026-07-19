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
import { MpBadge, MpButton, MpCheckbox, MpDatePicker, MpInput, MpInputNumber } from '@myportal/ui';
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
import { EmptyState } from '../../../../../../shared/components/empty-state/empty-state';
import { SectionHeader } from '../../../../../../shared/components/section-header/section-header';
import { Field } from '../../../../../../shared/components/field/field';
import { Callout } from '../../../../../../shared/components/callout/callout';
import {
  PayScalePointResponse,
  StaffContractUpsertItem,
  StaffEmploymentDetailsResponse,
  StaffEmploymentDetailsUpsertRequest,
  StaffEmploymentUpsertItem,
} from '../../../../../../shared/types/staff-employment-details';
import { StaffAreaPanel } from './staff-area-panel';

@Component({
  selector: 'mp-staff-employment-panel',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    DatePipe,
    FormsModule,
    MpButton,
    MpInput,
    MpInputNumber,
    MpDatePicker,
    MpCheckbox,
    MpBadge,
    LookupSelect,
    Loading,
    EmptyState,
    SectionHeader,
    Field,
    Callout,
    TranslocoDirective,
  ],
  providers: [
    provideTranslocoScope('staff-members'),
    { provide: StaffAreaPanel, useExisting: forwardRef(() => StaffEmploymentPanel) },
  ],
  templateUrl: './staff-employment-panel.html',
})
export class StaffEmploymentPanel extends StaffAreaPanel implements OnInit {
  private readonly data = inject(StaffMembersDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);

  readonly staffMemberId = input.required<string>();
  readonly permissions = input.required<Set<string>>();

  protected readonly loading = signal(false);
  override readonly saving = signal(false);
  override readonly editing = signal(false);

  protected readonly employment = signal<StaffEmploymentDetailsResponse | null>(null);

  protected readonly bankName = signal<string | null>(null);
  protected readonly bankAccount = signal<string | null>(null);
  protected readonly bankSortCode = signal<string | null>(null);
  protected readonly niNumber = signal<string | null>(null);
  protected readonly employments = signal<StaffEmploymentUpsertItem[]>([]);
  private readonly snapshot = signal<string>('');

  protected readonly leavingReasons = computed(() => this.employment()?.leavingReasons ?? []);
  protected readonly origins = computed(() => this.employment()?.origins ?? []);
  protected readonly destinations = computed(() => this.employment()?.destinations ?? []);
  protected readonly contractTypes = computed(() => this.employment()?.contractTypes ?? []);
  protected readonly staffRoles = computed(() => this.employment()?.staffRoles ?? []);
  protected readonly serviceTerms = computed(() => this.employment()?.serviceTerms ?? []);
  protected readonly departments = computed(() => this.employment()?.departments ?? []);
  protected readonly payScales = computed(() => this.employment()?.payScales ?? []);
  protected readonly payScalePoints = computed(() => this.employment()?.payScalePoints ?? []);
  protected readonly payZoneName = computed(() => this.employment()?.payZoneName ?? null);

  private static readonly NO_POINTS: PayScalePointResponse[] = [];
  private readonly payScalePointsByScale = computed(() => {
    const map = new Map<string, PayScalePointResponse[]>();
    for (const point of this.payScalePoints()) {
      const list = map.get(point.payScaleId);
      if (list) list.push(point);
      else map.set(point.payScaleId, [point]);
    }
    return map;
  });

  override readonly canEdit = computed(() =>
    this.permissions().has(Permissions.Staff.EditAllStaffEmploymentDetails),
  );

  override readonly valid = computed(() =>
    this.employments().every(
      e =>
        !!e.startDate &&
        e.contracts.every(
          c =>
            !!c.contractTypeId &&
            c.postTitle.trim().length > 0 &&
            !!c.startDate &&
            c.fte != null &&
            c.fte >= 0 &&
            c.fte <= 1,
        ),
    ),
  );

  private readonly form = computed(() =>
    JSON.stringify({
      bankName: this.bankName(),
      bankAccount: this.bankAccount(),
      bankSortCode: this.bankSortCode(),
      niNumber: this.niNumber(),
      employments: this.employments(),
    }),
  );

  override readonly dirty = computed(
    () => this.employment() != null && this.snapshot() !== this.form(),
  );

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.data.getEmploymentDetails(this.staffMemberId()).subscribe({
      next: row => {
        this.employment.set(row);
        this.apply(row);
        this.loading.set(false);
      },
      error: err => {
        this.loading.set(false);
        this.notify.apiError(err, this.transloco.translate('staff-members.loadEmploymentError'));
      },
    });
  }

  override startEdit(): void {
    this.editing.set(true);
  }

  override cancel(): void {
    this.apply(this.employment());
    this.editing.set(false);
  }

  override async save(): Promise<void> {
    if (!this.canEdit() || !this.valid() || this.saving()) return;
    this.saving.set(true);

    const payload: StaffEmploymentDetailsUpsertRequest = {
      bankName: this.normalise(this.bankName()),
      bankAccount: this.normalise(this.bankAccount()),
      bankSortCode: this.normalise(this.bankSortCode()),
      niNumber: this.normalise(this.niNumber()),
      employments: this.employments().map(e => ({
        id: e.id ?? null,
        startDate: e.startDate,
        endDate: e.endDate ?? null,
        leavingReasonId: e.leavingReasonId ?? null,
        originId: e.originId ?? null,
        destinationId: e.destinationId ?? null,
        notes: this.normalise(e.notes),
        contracts: e.contracts.map(c => ({
          id: c.id ?? null,
          contractTypeId: c.contractTypeId,
          staffRoleId: c.staffRoleId ?? null,
          serviceTermId: c.serviceTermId ?? null,
          departmentId: c.departmentId ?? null,
          payScaleId: c.payScaleId ?? null,
          payScalePointId: c.payScalePointId ?? null,
          postTitle: c.postTitle.trim(),
          startDate: c.startDate,
          endDate: c.endDate ?? null,
          fte: c.fte,
          hoursPerWeek: c.hoursPerWeek ?? null,
          weeksPerYear: c.weeksPerYear ?? null,
          annualSalary: c.annualSalary ?? null,
          isAgencySupply: c.isAgencySupply,
          safeguardedSalary: c.safeguardedSalary,
          dailyRate: c.dailyRate,
        })),
      })),
    };

    try {
      await firstValueFrom(this.data.updateEmploymentDetails(this.staffMemberId(), payload));
      this.notify.success(this.transloco.translate('staff-members.savedEmploymentToast'));
      this.editing.set(false);
      this.load();
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('staff-members.saveEmploymentError'));
    } finally {
      this.saving.set(false);
    }
  }

  private apply(row: StaffEmploymentDetailsResponse | null): void {
    this.bankName.set(row?.bankName ?? null);
    this.bankAccount.set(row?.bankAccount ?? null);
    this.bankSortCode.set(row?.bankSortCode ?? null);
    this.niNumber.set(row?.niNumber ?? null);
    this.employments.set(
      (row?.employments ?? []).map(e => ({
        id: e.id,
        startDate: e.startDate,
        endDate: e.endDate ?? null,
        leavingReasonId: e.leavingReasonId ?? null,
        originId: e.originId ?? null,
        destinationId: e.destinationId ?? null,
        notes: e.notes ?? null,
        contracts: (e.contracts ?? []).map(c => ({
          id: c.id,
          contractTypeId: c.contractTypeId,
          staffRoleId: c.staffRoleId ?? null,
          serviceTermId: c.serviceTermId ?? null,
          departmentId: c.departmentId ?? null,
          payScaleId: c.payScaleId ?? null,
          payScalePointId: c.payScalePointId ?? null,
          postTitle: c.postTitle,
          startDate: c.startDate,
          endDate: c.endDate ?? null,
          fte: c.fte,
          hoursPerWeek: c.hoursPerWeek ?? null,
          weeksPerYear: c.weeksPerYear ?? null,
          annualSalary: c.annualSalary ?? null,
          isAgencySupply: c.isAgencySupply,
          safeguardedSalary: c.safeguardedSalary,
          dailyRate: c.dailyRate,
        })),
      })),
    );
    this.snapshot.set(this.form());
  }

  protected addEmployment(): void {
    this.employments.update(rows => [
      ...rows,
      {
        id: null,
        startDate: null,
        endDate: null,
        leavingReasonId: null,
        originId: null,
        destinationId: null,
        notes: null,
        contracts: [],
      },
    ]);
  }

  protected removeEmployment(index: number): void {
    this.employments.update(rows => rows.filter((_, i) => i !== index));
  }

  protected patchEmployment<K extends keyof StaffEmploymentUpsertItem>(
    index: number,
    key: K,
    value: StaffEmploymentUpsertItem[K],
  ): void {
    this.employments.update(rows =>
      rows.map((row, i) => (i === index ? { ...row, [key]: value } : row)),
    );
  }

  protected onEmploymentEndDate(index: number, value: Date | null): void {
    const iso = value ? value.toISOString() : null;
    this.employments.update(rows =>
      rows.map((row, i) => {
        if (i !== index) return row;
        if (iso) return { ...row, endDate: iso };
        return { ...row, endDate: null, leavingReasonId: null, destinationId: null };
      }),
    );
  }

  protected addContract(employmentIndex: number): void {
    this.employments.update(rows =>
      rows.map((row, i) =>
        i === employmentIndex
          ? {
              ...row,
              contracts: [
                ...row.contracts,
                {
                  id: null,
                  contractTypeId: null,
                  staffRoleId: null,
                  serviceTermId: null,
                  departmentId: null,
                  payScaleId: null,
                  payScalePointId: null,
                  postTitle: '',
                  startDate: row.startDate,
                  endDate: null,
                  fte: 1,
                  hoursPerWeek: null,
                  weeksPerYear: null,
                  annualSalary: null,
                  isAgencySupply: false,
                  safeguardedSalary: false,
                  dailyRate: false,
                },
              ],
            }
          : row,
      ),
    );
  }

  protected removeContract(employmentIndex: number, contractIndex: number): void {
    this.employments.update(rows =>
      rows.map((row, i) =>
        i === employmentIndex
          ? { ...row, contracts: row.contracts.filter((_, j) => j !== contractIndex) }
          : row,
      ),
    );
  }

  protected patchContract<K extends keyof StaffContractUpsertItem>(
    employmentIndex: number,
    contractIndex: number,
    key: K,
    value: StaffContractUpsertItem[K],
  ): void {
    this.employments.update(rows =>
      rows.map((row, i) =>
        i === employmentIndex
          ? {
              ...row,
              contracts: row.contracts.map((c, j) =>
                j === contractIndex ? { ...c, [key]: value } : c,
              ),
            }
          : row,
      ),
    );
  }

  private mutateContract(
    employmentIndex: number,
    contractIndex: number,
    fn: (c: StaffContractUpsertItem) => StaffContractUpsertItem,
  ): void {
    this.employments.update(rows =>
      rows.map((row, i) =>
        i === employmentIndex
          ? { ...row, contracts: row.contracts.map((c, j) => (j === contractIndex ? fn(c) : c)) }
          : row,
      ),
    );
  }

  protected statutoryFor(payScalePointId: string | null | undefined): number | null {
    if (!payScalePointId) return null;
    return this.payScalePoints().find(p => p.id === payScalePointId)?.fullTimeSalary ?? null;
  }

  private autoSalaryFor(payScalePointId: string | null | undefined, fte: number | null): number | null {
    const statutory = this.statutoryFor(payScalePointId);
    if (statutory == null || fte == null) return null;
    return Math.round(statutory * fte * 100) / 100;
  }

  private salaryIsAuto(c: StaffContractUpsertItem): boolean {
    if (c.annualSalary == null) return true;
    const auto = this.autoSalaryFor(c.payScalePointId, c.fte);
    return auto != null && Math.abs(c.annualSalary - auto) < 0.5;
  }

  protected onContractSpinePoint(
    employmentIndex: number,
    contractIndex: number,
    payScalePointId: string | null,
  ): void {
    this.mutateContract(employmentIndex, contractIndex, c => {
      const auto = this.salaryIsAuto(c);
      const next = { ...c, payScalePointId };
      if (auto) next.annualSalary = this.autoSalaryFor(payScalePointId, c.fte);
      return next;
    });
  }

  protected onContractFte(employmentIndex: number, contractIndex: number, fte: number | null): void {
    this.mutateContract(employmentIndex, contractIndex, c => {
      const auto = this.salaryIsAuto(c);
      const next = { ...c, fte: fte ?? 0 };
      if (auto) next.annualSalary = this.autoSalaryFor(c.payScalePointId, next.fte);
      return next;
    });
  }

  protected onContractPayScale(
    employmentIndex: number,
    contractIndex: number,
    payScaleId: string | null,
  ): void {
    this.mutateContract(employmentIndex, contractIndex, c => {
      const auto = this.salaryIsAuto(c);
      const next = { ...c, payScaleId, payScalePointId: null };
      if (auto) next.annualSalary = null;
      return next;
    });
  }

  protected contractPayScalePoints(payScaleId: string | null | undefined): PayScalePointResponse[] {
    if (!payScaleId) return StaffEmploymentPanel.NO_POINTS;
    return this.payScalePointsByScale().get(payScaleId) ?? StaffEmploymentPanel.NO_POINTS;
  }

  protected periodStatus(
    startDate: string | null | undefined,
    endDate: string | null | undefined,
  ): 'upcoming' | 'current' | 'ended' {
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    if (startDate) {
      const start = new Date(startDate);
      start.setHours(0, 0, 0, 0);
      if (start > today) return 'upcoming';
    }

    if (endDate) {
      const end = new Date(endDate);
      end.setHours(0, 0, 0, 0);
      if (end < today) return 'ended';
    }

    return 'current';
  }

  protected periodSeverity(status: 'upcoming' | 'current' | 'ended'): 'success' | 'info' | 'secondary' {
    return status === 'current' ? 'success' : status === 'upcoming' ? 'info' : 'secondary';
  }

  protected spellFteTotal(e: StaffEmploymentUpsertItem): number {
    const total = e.contracts.reduce((sum, c) => sum + (c.fte ?? 0), 0);
    return Math.round(total * 100) / 100;
  }

  protected formatMoney(value: number): string {
    return '£' + Math.round(value).toLocaleString('en-GB');
  }
}
