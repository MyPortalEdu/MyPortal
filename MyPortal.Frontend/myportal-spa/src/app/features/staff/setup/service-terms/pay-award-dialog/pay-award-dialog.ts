import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CurrencyPipe } from '@angular/common';
import {
  MpButton,
  MpDatePicker,
  MpDialog,
  MpDialogFooter,
  MpInputNumber,
  MpSelect,
} from '@myportal/ui';
import { firstValueFrom } from 'rxjs';
import { TranslocoDirective, TranslocoService, provideTranslocoScope } from '@jsverse/transloco';

import { NotificationService } from '../../../../../core/services/notification.service';
import { ServiceTermsDataService } from '../../../../../shared/services/service-terms-data.service';
import { LookupResponse } from '../../../../../shared/types/lookup';
import { formatPoint, toDateOnly } from '../pay-spine';
import {
  PayAwardPreviewItem,
  PayAwardRequest,
  PayScaleGenerationItem,
  PayScaleItem,
} from '../../../../../shared/types/staff-setup';

interface PreviewRow {
  key: string;
  scale: string;
  point: string;
  zone: string;
  before: number;
  after: number;
}

@Component({
  selector: 'mp-pay-award-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    FormsModule,
    CurrencyPipe,
    MpButton,
    MpDatePicker,
    MpDialog,
    MpDialogFooter,
    MpInputNumber,
    MpSelect,
    TranslocoDirective,
  ],
  providers: [provideTranslocoScope('staff-setup')],
  templateUrl: './pay-award-dialog.html',
})
export class PayAwardDialog {
  private readonly data = inject(ServiceTermsDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);

  readonly open = input(false);
  readonly serviceTermId = input.required<string>();
  readonly scales = input<PayScaleItem[]>([]);

  /** Per-scale overrides make no sense on one shared spine — there is a single set of values. */
  readonly singleSpine = input(false);

  readonly zones = input<LookupResponse[]>([]);
  readonly generations = input<PayScaleGenerationItem[]>([]);
  readonly sourceEffectiveFrom = input<string | null>(null);

  readonly closed = output<void>();
  readonly applied = output<void>();

  protected readonly effectiveFrom = signal<Date | null>(null);
  protected readonly source = signal<string | null>(null);
  protected readonly defaultPercentage = signal<number | null>(null);
  protected readonly overrides = signal<Record<string, number | null>>({});

  protected readonly busy = signal(false);
  protected readonly preview = signal<PreviewRow[] | null>(null);

  protected readonly generationOptions = computed(() =>
    this.generations()
      .slice()
      .reverse()
      .map(g => ({
        value: g.effectiveFrom,
        label: new Date(g.effectiveFrom).toLocaleDateString('en-GB', {
          day: 'numeric',
          month: 'short',
          year: 'numeric',
        }),
      })),
  );

  protected readonly canPreview = computed(
    () => !!this.effectiveFrom() && !!this.source() && this.defaultPercentage() != null,
  );

  constructor() {
    effect(() => {
      if (!this.open()) return;
      this.source.set(this.sourceEffectiveFrom() ?? this.generations().at(-1)?.effectiveFrom ?? null);
      this.effectiveFrom.set(null);
      this.defaultPercentage.set(null);
      this.overrides.set({});
      this.preview.set(null);
      this.busy.set(false);
    });
  }

  protected overrideFor(scaleId: string): number | null {
    return this.overrides()[scaleId] ?? null;
  }

  protected setOverride(scaleId: string, value: number | null): void {
    this.overrides.update(o => ({ ...o, [scaleId]: value }));
    // Any change to the inputs invalidates a preview the user may already be looking at.
    this.preview.set(null);
  }

  protected onInputChanged(): void {
    this.preview.set(null);
  }

  private buildRequest(): PayAwardRequest | null {
    const when = this.effectiveFrom();
    const src = this.source();
    const pct = this.defaultPercentage();

    if (!when || !src || pct == null) return null;

    const overrides = this.overrides();

    return {
      effectiveFrom: toDateOnly(when),
      sourceEffectiveFrom: src,
      defaultPercentage: pct,
      scaleOverrides: this.singleSpine()
        ? []
        : Object.entries(overrides)
            .filter(([, v]) => v != null)
            .map(([payScaleId, v]) => ({ payScaleId, percentage: v as number })),
    };
  }

  protected async runPreview(): Promise<void> {
    const payload = this.buildRequest();
    if (!payload) return;

    this.busy.set(true);

    try {
      const result = await firstValueFrom(this.data.previewAward(this.serviceTermId(), payload));
      this.preview.set(this.toRows(result.rates));
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('staff-setup.payScales.previewError'));
    } finally {
      this.busy.set(false);
    }
  }

  protected async apply(): Promise<void> {
    const payload = this.buildRequest();
    if (!payload) return;

    this.busy.set(true);

    try {
      await firstValueFrom(this.data.applyAward(this.serviceTermId(), payload));
      this.notify.success(this.transloco.translate('staff-setup.payScales.awardAppliedToast'));
      this.applied.emit();
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('staff-setup.payScales.awardError'));
    } finally {
      this.busy.set(false);
    }
  }

  protected onClose(): void {
    this.closed.emit();
  }

  private toRows(rates: PayAwardPreviewItem[]): PreviewRow[] {
    const zoneById = new Map(this.zones().map(z => [z.id, z.description]));
    const scaleById = new Map(this.scales().map(s => [s.id, s.description]));
    const spineLabel = this.transloco.translate('staff-setup.payScales.theSpine');

    return rates.map(r => ({
      key: `${r.payScalePointId}|${r.payZoneId}`,
      scale: r.payScaleId ? (scaleById.get(r.payScaleId) ?? '') : spineLabel,
      point: formatPoint(r.pointValue),
      zone: zoneById.get(r.payZoneId) ?? '',
      before: r.previousAnnualSalary,
      after: r.annualSalary,
    }));
  }
}
