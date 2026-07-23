import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CurrencyPipe } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import {
  MpBadge,
  MpButton,
  MpCard,
  MpCheckbox,
  MpDatePicker,
  MpInput,
  MpInputNumber,
  MpSelect,
  MpSkeleton,
} from '@myportal/ui';
import { firstValueFrom } from 'rxjs';
import { TranslocoDirective, TranslocoService, provideTranslocoScope } from '@jsverse/transloco';

import { PageHeader } from '../../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../../shared/components/empty-state/empty-state';
import { Callout } from '../../../../../shared/components/callout/callout';
import { HeaderAction } from '../../../../../shared/types/header-action.type';
import { ConfirmationDialog } from '../../../../../core/services/confirmation.service';
import { NotificationService } from '../../../../../core/services/notification.service';
import { ServiceTermsDataService } from '../../../../../shared/services/service-terms-data.service';
import {
  PayScaleUpsertItem,
  PointSalaryItem,
  ServiceTermPayResponse,
  ServiceTermResponse,
} from '../../../../../shared/types/staff-setup';
import { ServiceTermEditorDialog } from '../service-term-editor-dialog/service-term-editor-dialog';
import { PayAwardDialog } from '../pay-award-dialog/pay-award-dialog';
import { IncrementDialog } from '../increment-dialog/increment-dialog';
import { defaultGenerationDate, formatPoint, generatePointValues, toDateOnly } from '../pay-spine';

const ALL_ZONES = 'all';

const MONTHS = [
  'january', 'february', 'march', 'april', 'may', 'june',
  'july', 'august', 'september', 'october', 'november', 'december',
];

/** A scale as it is being edited, before the points it implies exist. */
interface ScaleDraft {
  id: string | null;
  code: string;
  description: string;
  active: boolean;
  minimumPoint: number | null;
  maximumPoint: number | null;
  pointInterval: number | null;
  contractCount: number;
  /** Point values that already exist and are on a contract, so cannot be generated away. */
  lockedPoints: Record<number, string>;
}

function salaryKey(pointValue: number, zoneId: string): string {
  return `${pointValue}|${zoneId}`;
}

const CODE_MAX = 10;
const POINT_CAP = 250;

/** i18n key per field, or undefined when the field is fine. */
interface ScaleErrors {
  code?: string;
  description?: string;
  minimumPoint?: string;
  maximumPoint?: string;
  pointInterval?: string;
}

interface PanelErrors {
  spineMinimum?: string;
  spineMaximum?: string;
  spineInterval?: string;
  scales: ScaleErrors[];
}

function generatesTooManyPoints(min: number, max: number, interval: number): boolean {
  return interval > 0 && (max - min) / interval + 1 > POINT_CAP;
}

@Component({
  selector: 'mp-service-term-details-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    FormsModule,
    CurrencyPipe,
    MpBadge,
    MpButton,
    MpCard,
    MpCheckbox,
    MpDatePicker,
    MpInput,
    MpInputNumber,
    MpSelect,
    MpSkeleton,
    PageHeader,
    EmptyState,
    Callout,
    ServiceTermEditorDialog,
    PayAwardDialog,
    IncrementDialog,
    TranslocoDirective,
  ],
  providers: [provideTranslocoScope('staff-setup')],
  templateUrl: './service-term-details-page.html',
})
export class ServiceTermDetailsPage implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly data = inject(ServiceTermsDataService);
  private readonly notify = inject(NotificationService);
  private readonly confirm = inject(ConfirmationDialog);
  private readonly transloco = inject(TranslocoService);

  protected readonly loading = signal(false);
  protected readonly saving = signal(false);
  protected readonly term = signal<ServiceTermResponse | null>(null);
  protected readonly pay = signal<ServiceTermPayResponse | null>(null);
  protected readonly schemes = signal<{ id: string; description: string }[]>([]);

  protected readonly editorOpen = signal(false);
  protected readonly awardOpen = signal(false);
  protected readonly incrementOpen = signal(false);
  protected readonly dueIncrements = signal(0);

  protected readonly selectedGeneration = signal<string | null>(null);
  protected readonly zoneFilter = signal<string>(ALL_ZONES);

  // A term with no salaries yet has no generation to pick, so the first one is dated here.
  // Later versions come from the pay award routine, which is what keeps history intact.
  protected readonly firstGenerationDate = signal<Date | null>(null);

  protected readonly hasGenerations = computed(() => this.generations().length > 0);

  protected readonly effectiveFrom = computed(() => {
    if (this.hasGenerations()) return this.selectedGeneration();
    const date = this.firstGenerationDate();
    return date ? toDateOnly(date) : null;
  });

  // The whole pay panel is one editable unit — ranges and salaries move together, so a point
  // typed into a range gets a salary row before it exists in the database.
  protected readonly scaleDrafts = signal<ScaleDraft[]>([]);
  protected readonly spineMinimum = signal<number | null>(null);
  protected readonly spineMaximum = signal<number | null>(null);
  protected readonly spineInterval = signal<number | null>(null);
  protected readonly salaries = signal<Record<string, Record<string, number | null>>>({});

  private snapshot = '';

  protected readonly serviceTermId = computed(() => this.route.snapshot.paramMap.get('id') ?? '');
  protected readonly canEdit = computed(() => this.pay()?.canEdit ?? false);

  // Edited here rather than on the term dialog: flipping it moves points between the term and its
  // scales, and only the pay save can reconcile that in one transaction.
  protected readonly singleSpine = signal(false);
  protected readonly generations = computed(() => this.pay()?.generations ?? []);

  protected readonly zones = computed(() => this.pay()?.payZones ?? []);

  protected readonly visibleZones = computed(() => {
    const filter = this.zoneFilter();
    return filter === ALL_ZONES ? this.zones() : this.zones().filter(z => z.id === filter);
  });

  protected readonly generationOptions = computed(() =>
    this.generations()
      .slice()
      .reverse()
      .map(g => ({
        value: g.effectiveFrom,
        label: g.isCurrent
          ? `${this.formatDate(g.effectiveFrom)} — ${this.transloco.translate('staff-setup.payScales.current')}`
          : this.formatDate(g.effectiveFrom),
      })),
  );

  protected readonly zoneOptions = computed(() => [
    { value: ALL_ZONES, label: this.transloco.translate('staff-setup.payScales.allZones') },
    ...this.zones().map(z => ({ value: z.id, label: z.description })),
  ]);

  protected readonly spinePoints = computed(() =>
    generatePointValues(this.spineMinimum(), this.spineMaximum(), this.spineInterval() ?? 1),
  );

  protected readonly dirty = computed(() => this.currentSnapshot() !== this.snapshot);

  // Required-but-empty errors stay hidden until a save is attempted (the submit-reveals-errors
  // pattern); errors that need a value to occur — out of range, bad interval — show live.
  protected readonly attempted = signal(false);

  protected readonly errors = computed<PanelErrors>(() => this.computeErrors(this.attempted()));

  private readonly hardErrors = computed<PanelErrors>(() => this.computeErrors(true));

  protected readonly valid = computed(() => {
    const e = this.hardErrors();
    return (
      !e.spineMinimum &&
      !e.spineMaximum &&
      !e.spineInterval &&
      e.scales.every(s => !s.code && !s.description && !s.minimumPoint && !s.maximumPoint && !s.pointInterval)
    );
  });

  /** Why Save is unavailable, so the button is never just inert. */
  protected readonly saveBlockedReason = computed(() => {
    if (!this.effectiveFrom()) return 'staff-setup.payScales.needEffectiveFrom';
    if (this.anyLockedLoss()) return 'staff-setup.payScales.blockedByLockedPoints';
    if (this.attempted() && !this.valid()) return 'staff-setup.payScales.fixHighlighted';
    return null;
  });

  /**
   * Mirrors the server's rules so a bad range is caught inline, not on save. When
   * <paramref name="requireComplete"/> is false the empty-required errors are dropped, so a
   * half-typed row doesn't flash red before the user has finished with it.
   */
  private computeErrors(requireComplete: boolean): PanelErrors {
    const single = this.singleSpine();
    const result: PanelErrors = { scales: [] };

    if (single) {
      const min = this.spineMinimum();
      const max = this.spineMaximum();
      const interval = this.spineInterval();

      if (min == null) result.spineMinimum = requireComplete ? 'required' : undefined;
      else if (min <= 0) result.spineMinimum = 'mustBePositive';

      if (max == null) result.spineMaximum = requireComplete ? 'required' : undefined;
      else if (min != null && max < min) result.spineMaximum = 'maxBelowMin';

      if (interval == null) result.spineInterval = requireComplete ? 'required' : undefined;
      else if (interval <= 0) result.spineInterval = 'mustBePositive';

      if (min != null && max != null && interval != null && generatesTooManyPoints(min, max, interval)) {
        result.spineMaximum = 'tooManyPoints';
      }
    }

    const spineMin = this.spineMinimum();
    const spineMax = this.spineMaximum();

    const codeCounts = new Map<string, number>();
    for (const scale of this.scaleDrafts()) {
      const code = scale.code.trim().toUpperCase();
      if (code) codeCounts.set(code, (codeCounts.get(code) ?? 0) + 1);
    }

    for (const scale of this.scaleDrafts()) {
      const e: ScaleErrors = {};
      const code = scale.code.trim();

      if (!code) {
        if (requireComplete) e.code = 'required';
      } else if (code.length > CODE_MAX) {
        e.code = 'codeTooLong';
      } else if ((codeCounts.get(code.toUpperCase()) ?? 0) > 1) {
        e.code = 'codeDuplicate';
      }

      if (!scale.description.trim() && requireComplete) e.description = 'required';

      const min = scale.minimumPoint;
      const max = scale.maximumPoint;

      if (min != null && min <= 0) e.minimumPoint = 'mustBePositive';
      if (min != null && max == null) e.maximumPoint = 'rangeIncomplete';
      else if (max != null && min == null) e.minimumPoint = 'rangeIncomplete';
      else if (min != null && max != null && max < min) e.maximumPoint = 'maxBelowMin';

      // A grade must state its window; a separate scale can be left rangeless (it just has no points).
      if (single && requireComplete) {
        if (min == null) e.minimumPoint ??= 'required';
        if (max == null) e.maximumPoint ??= 'required';
      }

      if (single) {
        if (min != null && spineMin != null && min < spineMin) e.minimumPoint = 'belowSpine';
        if (max != null && spineMax != null && max > spineMax) e.maximumPoint = 'aboveSpine';
      } else {
        const interval = scale.pointInterval;
        if (interval == null) {
          if (requireComplete && (min != null || max != null)) e.pointInterval = 'required';
        } else if (interval <= 0) {
          e.pointInterval = 'mustBePositive';
        } else if (min != null && max != null && generatesTooManyPoints(min, max, interval)) {
          e.maximumPoint = 'tooManyPoints';
        }
      }

      result.scales.push(e);
    }

    return result;
  }

  protected scaleError(index: number, field: keyof ScaleErrors): string | undefined {
    const key = this.errors().scales[index]?.[field];
    return key ? `staff-setup.payScales.err.${key}` : undefined;
  }

  protected spineError(field: 'spineMinimum' | 'spineMaximum' | 'spineInterval'): string | undefined {
    const key = this.errors()[field];
    return key ? `staff-setup.payScales.err.${key}` : undefined;
  }

  protected readonly headerActions = computed<HeaderAction[]>(() => {
    if (!this.canEdit()) return [];
    const actions: HeaderAction[] = [
      {
        label: this.transloco.translate('staff-setup.serviceTerms.edit'),
        icon: 'fa-solid fa-pen',
        severity: 'secondary',
        command: () => this.editorOpen.set(true),
      },
    ];

    // The annual increment only means anything where the term progresses staff up a spine.
    if (this.term()?.spinalProgression) {
      actions.push({
        label: this.transloco.translate('staff-setup.increment.title'),
        icon: 'fa-solid fa-arrow-up-right-dots',
        severity: 'secondary',
        command: () => this.incrementOpen.set(true),
      });
    }

    actions.push({
      label: this.transloco.translate('staff-setup.payScales.newAward'),
      icon: 'fa-solid fa-arrow-trend-up',
      severity: 'primary',
      command: () => this.awardOpen.set(true),
    });

    return actions;
  });

  ngOnInit(): void {
    this.load(null);
  }

  protected load(effectiveFrom: string | null): void {
    const id = this.serviceTermId();
    if (!id) return;

    this.loading.set(true);

    this.data.getServiceTerms().subscribe({
      next: list => {
        this.term.set(list.serviceTerms.find(t => t.id === id) ?? null);
        this.schemes.set(list.superannuationSchemes);
      },
      error: err =>
        this.notify.apiError(err, this.transloco.translate('staff-setup.serviceTerms.loadError')),
    });

    this.loadDueIncrements();

    this.data.getPay(id, effectiveFrom).subscribe({
      next: row => {
        this.pay.set(row);
        this.selectedGeneration.set(row.selectedEffectiveFrom ?? null);

        if (this.zoneFilter() === ALL_ZONES && row.localPayZoneId) {
          this.zoneFilter.set(row.localPayZoneId);
        }

        this.firstGenerationDate.set(
          row.generations.length ? null : defaultGenerationDate(new Date()),
        );

        this.singleSpine.set(row.singlePaySpine);
        this.spineMinimum.set(row.minimumPoint ?? null);
        this.spineMaximum.set(row.maximumPoint ?? null);
        this.spineInterval.set(row.pointInterval ?? 1);

        this.scaleDrafts.set(
          row.scales.map(scale => ({
            id: scale.id,
            code: scale.code,
            description: scale.description,
            active: scale.active,
            minimumPoint: scale.minimumPoint ?? null,
            maximumPoint: scale.maximumPoint ?? null,
            pointInterval: scale.pointInterval ?? 1,
            contractCount: scale.contractCount,
            lockedPoints: Object.fromEntries(
              scale.points.filter(p => p.contractCount > 0).map(p => [p.pointValue, p.code]),
            ),
          })),
        );

        const map: Record<string, Record<string, number | null>> = { spine: {} };
        for (const salary of row.spineSalaries) {
          map['spine'][salaryKey(salary.pointValue, salary.payZoneId)] = salary.annualSalary;
        }
        for (const group of row.scaleSalaries) {
          map[group.payScaleId] = {};
          for (const salary of group.salaries) {
            map[group.payScaleId][salaryKey(salary.pointValue, salary.payZoneId)] =
              salary.annualSalary;
          }
        }
        this.salaries.set(map);

        this.attempted.set(false);
        this.snapshot = this.currentSnapshot();
        this.loading.set(false);
      },
      error: err => {
        this.loading.set(false);
        this.notify.apiError(err, this.transloco.translate('staff-setup.payScales.loadError'));
      },
    });
  }

  protected onGenerationChange(effectiveFrom: string): void {
    this.selectedGeneration.set(effectiveFrom);
    this.load(effectiveFrom);
  }

  protected pointsOf(scale: ScaleDraft): number[] {
    return this.singleSpine()
      ? this.spinePoints().filter(
          v =>
            (scale.minimumPoint == null || v >= scale.minimumPoint) &&
            (scale.maximumPoint == null || v <= scale.maximumPoint),
        )
      : generatePointValues(scale.minimumPoint, scale.maximumPoint, scale.pointInterval ?? 1);
  }

  /** Points a save would remove that somebody is still paid on. */
  protected lockedLoss(scale: ScaleDraft): string[] {
    if (this.singleSpine()) return [];
    const keep = new Set(this.pointsOf(scale));
    return Object.entries(scale.lockedPoints)
      .filter(([value]) => !keep.has(Number(value)))
      .map(([, code]) => code);
  }

  protected readonly anyLockedLoss = computed(() =>
    this.scaleDrafts().some(scale => this.lockedLoss(scale).length > 0),
  );

  protected salaryOf(bucket: string, pointValue: number, zoneId: string): number | null {
    return this.salaries()[bucket]?.[salaryKey(pointValue, zoneId)] ?? null;
  }

  protected setSalary(
    bucket: string,
    pointValue: number,
    zoneId: string,
    value: number | null,
  ): void {
    this.salaries.update(all => ({
      ...all,
      [bucket]: { ...(all[bucket] ?? {}), [salaryKey(pointValue, zoneId)]: value },
    }));
  }

  protected bucketOf(scale: ScaleDraft): string {
    return this.singleSpine() ? 'spine' : (scale.id ?? `new:${scale.code}`);
  }

  protected updateScale(index: number, patch: Partial<ScaleDraft>): void {
    this.scaleDrafts.update(scales =>
      scales.map((scale, i) => (i === index ? { ...scale, ...patch } : scale)),
    );
  }

  protected addScale(): void {
    this.scaleDrafts.update(scales => [
      ...scales,
      {
        id: null,
        code: '',
        description: '',
        active: true,
        minimumPoint: null,
        maximumPoint: null,
        pointInterval: 1,
        contractCount: 0,
        lockedPoints: {},
      },
    ]);
  }

  protected async removeScale(index: number): Promise<void> {
    const scale = this.scaleDrafts()[index];

    if (scale.id) {
      const ok = await this.confirm.confirm({
        header: this.transloco.translate('staff-setup.payScales.deleteScaleHeader'),
        message: this.transloco.translate('staff-setup.payScales.deleteScaleConfirm', {
          description: scale.description || scale.code,
        }),
        acceptLabel: this.transloco.translate('common.delete'),
        acceptSeverity: 'danger',
      });

      if (!ok) return;
    }

    this.scaleDrafts.update(scales => scales.filter((_, i) => i !== index));
  }

  protected formatPoint(value: number): string {
    return formatPoint(value);
  }

  /**
   * Switching ownership seeds the new owner's range from what is already there, so the grid does
   * not empty out and lose the salaries the user can see.
   */
  protected onSingleSpineChange(value: boolean): void {
    this.singleSpine.set(value);

    if (value && this.spineMinimum() == null) {
      const scales = this.scaleDrafts().filter(s => s.minimumPoint != null && s.maximumPoint != null);
      if (scales.length) {
        this.spineMinimum.set(Math.min(...scales.map(s => s.minimumPoint!)));
        this.spineMaximum.set(Math.max(...scales.map(s => s.maximumPoint!)));
        this.spineInterval.set(this.spineInterval() ?? 1);
      }
    }
  }

  protected incrementLabel(term: ServiceTermResponse): string {
    if (!term.spinalProgression || !term.incrementMonth) return '—';
    const month = this.transloco.translate(`staff-setup.months.${MONTHS[term.incrementMonth - 1]}`);
    return term.incrementDay ? `${term.incrementDay} ${month}` : month;
  }

  protected readonly schemeSummary = computed(() => {
    const links = this.term()?.superannuationSchemes ?? [];
    if (!links.length) return '—';
    const byId = new Map(this.schemes().map(s => [s.id, s.description]));
    return links
      .map(l => {
        const name = byId.get(l.superannuationSchemeId) ?? '';
        return l.isMain ? `${name} (${this.transloco.translate('staff-setup.serviceTerms.mainScheme')})` : name;
      })
      .filter(Boolean)
      .join(', ');
  });

  protected async save(): Promise<void> {
    // Reveal any required-but-empty errors, then stop if the panel isn't valid.
    this.attempted.set(true);

    const effectiveFrom = this.effectiveFrom();
    if (!effectiveFrom || !this.valid()) return;

    this.saving.set(true);

    const scales: PayScaleUpsertItem[] = this.scaleDrafts().map(scale => ({
      id: scale.id,
      code: scale.code.trim().toUpperCase(),
      description: scale.description.trim(),
      active: scale.active,
      minimumPoint: scale.minimumPoint,
      maximumPoint: scale.maximumPoint,
      pointInterval: this.singleSpine() ? null : scale.pointInterval,
      salaries: this.singleSpine() ? [] : this.collect(this.bucketOf(scale), this.pointsOf(scale)),
    }));

    try {
      await firstValueFrom(
        this.data.updatePay(this.serviceTermId(), {
          effectiveFrom,
          singlePaySpine: this.singleSpine(),
          minimumPoint: this.singleSpine() ? this.spineMinimum() : null,
          maximumPoint: this.singleSpine() ? this.spineMaximum() : null,
          pointInterval: this.singleSpine() ? this.spineInterval() : null,
          scales,
          spineSalaries: this.singleSpine()
            ? this.collect('spine', this.spinePoints())
            : [],
        }),
      );
      this.notify.success(this.transloco.translate('staff-setup.payScales.savedToast'));
      this.saving.set(false);
      this.firstGenerationDate.set(null);
      this.load(effectiveFrom);
    } catch (err) {
      this.saving.set(false);
      this.notify.apiError(err, this.transloco.translate('staff-setup.payScales.saveError'));
    }
  }

  private collect(bucket: string, pointValues: number[]): PointSalaryItem[] {
    const bucketSalaries = this.salaries()[bucket] ?? {};
    const items: PointSalaryItem[] = [];

    for (const pointValue of pointValues) {
      for (const zone of this.zones()) {
        const amount = bucketSalaries[salaryKey(pointValue, zone.id)];
        if (amount != null) {
          items.push({ pointValue, payZoneId: zone.id, annualSalary: amount });
        }
      }
    }

    return items;
  }

  protected onTermSaved(): void {
    this.editorOpen.set(false);
    this.load(this.selectedGeneration());
  }

  protected onAwardApplied(): void {
    this.awardOpen.set(false);
    this.load(null);
  }

  protected onIncrementApplied(): void {
    this.incrementOpen.set(false);
    this.load(this.selectedGeneration());
  }

  protected loadDueIncrements(): void {
    const id = this.serviceTermId();
    if (!id) return;
    this.data.getScheduledIncrements(id).subscribe({
      next: rows => this.dueIncrements.set(rows.filter(r => r.isDue).length),
      error: () => this.dueIncrements.set(0),
    });
  }

  protected back(): void {
    void this.router.navigate(['/staff/setup/service-terms']);
  }

  private currentSnapshot(): string {
    return JSON.stringify({
      singleSpine: this.singleSpine(),
      scales: this.scaleDrafts(),
      min: this.spineMinimum(),
      max: this.spineMaximum(),
      interval: this.spineInterval(),
      salaries: this.salaries(),
    });
  }

  private formatDate(iso: string): string {
    return new Date(iso).toLocaleDateString('en-GB', {
      day: 'numeric',
      month: 'short',
      year: 'numeric',
    });
  }
}
