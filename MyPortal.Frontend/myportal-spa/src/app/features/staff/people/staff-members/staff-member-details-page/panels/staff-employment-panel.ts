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
import { FormField, applyEach, form, maxLength, submit, validate } from '@angular/forms/signals';
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
  StaffEmploymentDetailsResponse,
  StaffEmploymentDetailsUpsertRequest,
} from '../../../../../../shared/types/staff-employment-details';
import { StaffAreaPanel } from './staff-area-panel';

interface ContractRow {
  id: string | null;
  contractTypeId: string | null;
  staffRoleId: string | null;
  serviceTermId: string | null;
  departmentId: string | null;
  payScaleId: string | null;
  payScalePointId: string | null;
  postTitle: string;
  startDate: Date | null;
  endDate: Date | null;
  fte: number | null;
  hoursPerWeek: number | null;
  weeksPerYear: number | null;
  annualSalary: number | null;
  isAgencySupply: boolean;
  safeguardedSalary: boolean;
  dailyRate: boolean;
}

interface EmploymentRow {
  id: string | null;
  startDate: Date | null;
  endDate: Date | null;
  leavingReasonId: string | null;
  originId: string | null;
  destinationId: string | null;
  notes: string;
  contracts: ContractRow[];
}

interface EmploymentModel {
  bankName: string;
  bankAccount: string;
  bankSortCode: string;
  niNumber: string;
  employments: EmploymentRow[];
}

@Component({
  selector: 'mp-staff-employment-panel',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    FormField,
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
  override readonly editing = signal(false);

  protected readonly employment = signal<StaffEmploymentDetailsResponse | null>(null);

  protected readonly model = signal<EmploymentModel>({
    bankName: '',
    bankAccount: '',
    bankSortCode: '',
    niNumber: '',
    employments: [],
  });
  protected readonly f = form(this.model, path => {
    maxLength(path.bankName, 50);
    maxLength(path.bankAccount, 15);
    maxLength(path.bankSortCode, 10);
    maxLength(path.niNumber, 9);
    applyEach(path.employments, emp => {
      maxLength(emp.notes, 1024);
      validate(emp.startDate, ({ value }) => (value() ? undefined : { kind: 'required' }));
      validate(emp.endDate, ({ value, valueOf }) => {
        const start = valueOf(emp.startDate);
        const end = value();
        return start && end && end.getTime() < start.getTime()
          ? { kind: 'range', message: 'staff-members.employment.datesReversed' }
          : undefined;
      });
      applyEach(emp.contracts, c => {
        validate(c.contractTypeId, ({ value }) => (value() ? undefined : { kind: 'required' }));
        validate(c.postTitle, ({ value }) => (value().trim().length ? undefined : { kind: 'required' }));
        validate(c.startDate, ({ value }) => (value() ? undefined : { kind: 'required' }));
        validate(c.fte, ({ value }) => {
          const v = value();
          if (v == null) return { kind: 'required' };
          if (v < 0) return { kind: 'min' };
          if (v > 1) return { kind: 'max' };
          return undefined;
        });
        validate(c.endDate, ({ value, valueOf }) => {
          const start = valueOf(c.startDate);
          const end = value();
          return start && end && end.getTime() < start.getTime()
            ? { kind: 'range', message: 'staff-members.employment.datesReversed' }
            : undefined;
        });
      });
    });
  });
  private readonly snapshot = signal<string>('');

  constructor() {
    super();
    effect(() => {
      const employments = this.model().employments;
      if (!employments.some(e => !e.endDate && (e.leavingReasonId || e.destinationId))) return;
      untracked(() =>
        this.model.update(m => ({
          ...m,
          employments: m.employments.map(e =>
            !e.endDate && (e.leavingReasonId || e.destinationId)
              ? { ...e, leavingReasonId: null, destinationId: null }
              : e,
          ),
        })),
      );
    });
  }

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

  protected readonly employmentsOverlap = computed(
    () => !employmentsDoNotOverlap(this.model().employments),
  );
  protected readonly contractOutOfRange = computed(
    () => !this.model().employments.every(contractsWithinEmployment),
  );

  override readonly saving = computed(() => this.f().submitting());
  override readonly valid = computed(
    () =>
      this.f().valid() &&
      employmentsDoNotOverlap(this.model().employments) &&
      this.model().employments.every(contractsWithinEmployment),
  );

  private readonly formState = computed(() => JSON.stringify(this.model()));

  override readonly dirty = computed(
    () => this.employment() != null && this.snapshot() !== this.formState(),
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
    if (!this.canEdit()) return;
    await submit(this.f, async () => {
      const employments = this.model().employments;
      if (
        !employmentsDoNotOverlap(employments) ||
        !employments.every(contractsWithinEmployment)
      )
        return;

      const payload: StaffEmploymentDetailsUpsertRequest = {
        bankName: this.normalise(this.model().bankName),
        bankAccount: this.normalise(this.model().bankAccount),
        bankSortCode: this.normalise(this.model().bankSortCode),
        niNumber: this.normalise(this.model().niNumber),
        employments: employments.map(e => ({
          id: e.id,
          startDate: e.startDate?.toISOString() ?? null,
          endDate: e.endDate?.toISOString() ?? null,
          leavingReasonId: e.leavingReasonId,
          originId: e.originId,
          destinationId: e.destinationId,
          notes: this.normalise(e.notes),
          contracts: e.contracts.map(c => ({
            id: c.id,
            contractTypeId: c.contractTypeId as string,
            staffRoleId: c.staffRoleId,
            serviceTermId: c.serviceTermId,
            departmentId: c.departmentId,
            payScaleId: c.payScaleId,
            payScalePointId: c.payScalePointId,
            postTitle: c.postTitle.trim(),
            startDate: c.startDate?.toISOString() ?? null,
            endDate: c.endDate?.toISOString() ?? null,
            fte: c.fte ?? 0,
            hoursPerWeek: c.hoursPerWeek,
            weeksPerYear: c.weeksPerYear,
            annualSalary: c.annualSalary,
            isAgencySupply: c.isAgencySupply,
            safeguardedSalary: c.safeguardedSalary,
            dailyRate: c.dailyRate,
          })),
        })),
      };

      try {
        await firstValueFrom(this.data.updateEmploymentDetails(this.staffMemberId(), payload));
      } catch (err) {
        this.notify.apiError(err, this.transloco.translate('staff-members.saveEmploymentError'));
        return;
      }
      this.notify.success(this.transloco.translate('staff-members.savedEmploymentToast'));
      this.editing.set(false);
      this.load();
    });
  }

  private apply(row: StaffEmploymentDetailsResponse | null): void {
    this.model.set({
      bankName: row?.bankName ?? '',
      bankAccount: row?.bankAccount ?? '',
      bankSortCode: row?.bankSortCode ?? '',
      niNumber: row?.niNumber ?? '',
      employments: (row?.employments ?? []).map(e => ({
        id: e.id,
        startDate: e.startDate ? new Date(e.startDate) : null,
        endDate: e.endDate ? new Date(e.endDate) : null,
        leavingReasonId: e.leavingReasonId ?? null,
        originId: e.originId ?? null,
        destinationId: e.destinationId ?? null,
        notes: e.notes ?? '',
        contracts: (e.contracts ?? []).map(c => ({
          id: c.id,
          contractTypeId: c.contractTypeId,
          staffRoleId: c.staffRoleId ?? null,
          serviceTermId: c.serviceTermId ?? null,
          departmentId: c.departmentId ?? null,
          payScaleId: c.payScaleId ?? null,
          payScalePointId: c.payScalePointId ?? null,
          postTitle: c.postTitle,
          startDate: c.startDate ? new Date(c.startDate) : null,
          endDate: c.endDate ? new Date(c.endDate) : null,
          fte: c.fte,
          hoursPerWeek: c.hoursPerWeek ?? null,
          weeksPerYear: c.weeksPerYear ?? null,
          annualSalary: c.annualSalary ?? null,
          isAgencySupply: c.isAgencySupply,
          safeguardedSalary: c.safeguardedSalary,
          dailyRate: c.dailyRate,
        })),
      })),
    });
    this.f().reset();
    this.snapshot.set(this.formState());
  }

  protected addEmployment(): void {
    this.model.update(m => ({
      ...m,
      employments: [
        ...m.employments,
        {
          id: null,
          startDate: null,
          endDate: null,
          leavingReasonId: null,
          originId: null,
          destinationId: null,
          notes: '',
          contracts: [],
        },
      ],
    }));
  }

  protected removeEmployment(index: number): void {
    this.model.update(m => ({
      ...m,
      employments: m.employments.filter((_, i) => i !== index),
    }));
  }

  protected patchEmployment<K extends keyof EmploymentRow>(
    index: number,
    key: K,
    value: EmploymentRow[K],
  ): void {
    this.model.update(m => ({
      ...m,
      employments: m.employments.map((row, i) => (i === index ? { ...row, [key]: value } : row)),
    }));
  }

  protected addContract(employmentIndex: number): void {
    this.model.update(m => ({
      ...m,
      employments: m.employments.map((row, i) =>
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
    }));
  }

  protected removeContract(employmentIndex: number, contractIndex: number): void {
    this.model.update(m => ({
      ...m,
      employments: m.employments.map((row, i) =>
        i === employmentIndex
          ? { ...row, contracts: row.contracts.filter((_, j) => j !== contractIndex) }
          : row,
      ),
    }));
  }

  protected patchContract<K extends keyof ContractRow>(
    employmentIndex: number,
    contractIndex: number,
    key: K,
    value: ContractRow[K],
  ): void {
    this.mutateContract(employmentIndex, contractIndex, c => ({ ...c, [key]: value }));
  }

  private mutateContract(
    employmentIndex: number,
    contractIndex: number,
    fn: (c: ContractRow) => ContractRow,
  ): void {
    this.model.update(m => ({
      ...m,
      employments: m.employments.map((row, i) =>
        i === employmentIndex
          ? { ...row, contracts: row.contracts.map((c, j) => (j === contractIndex ? fn(c) : c)) }
          : row,
      ),
    }));
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

  private salaryIsAuto(c: ContractRow): boolean {
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
      const next = { ...c, fte };
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
    startDate: Date | null,
    endDate: Date | null,
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

  protected spellFteTotal(e: EmploymentRow): number {
    const total = e.contracts.reduce((sum, c) => sum + (c.fte ?? 0), 0);
    return Math.round(total * 100) / 100;
  }

  protected formatMoney(value: number): string {
    return '£' + Math.round(value).toLocaleString('en-GB');
  }
}

function employmentsDoNotOverlap(employments: EmploymentRow[]): boolean {
  const ordered = employments
    .filter(e => e.startDate)
    .slice()
    .sort((a, b) => a.startDate!.getTime() - b.startDate!.getTime());
  for (let i = 1; i < ordered.length; i++) {
    const previousEnd = ordered[i - 1].endDate ? ordered[i - 1].endDate!.getTime() : Infinity;
    if (ordered[i].startDate!.getTime() <= previousEnd) return false;
  }
  return true;
}

function contractsWithinEmployment(employment: EmploymentRow): boolean {
  if (!employment.startDate) return true;
  const empStart = employment.startDate.getTime();
  const empEnd = employment.endDate ? employment.endDate.getTime() : null;
  for (const contract of employment.contracts) {
    if (!contract.startDate) continue;
    const contractStart = contract.startDate.getTime();
    if (contractStart < empStart) return false;
    if (empEnd !== null) {
      if (contractStart > empEnd) return false;
      if (contract.endDate && contract.endDate.getTime() > empEnd) return false;
    }
  }
  return true;
}
