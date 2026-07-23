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
  MpBadge,
  MpButton,
  MpCheckbox,
  MpDatePicker,
  MpDialog,
  MpDialogFooter,
} from '@myportal/ui';
import { firstValueFrom } from 'rxjs';
import { TranslocoDirective, TranslocoService, provideTranslocoScope } from '@jsverse/transloco';

import { NotificationService } from '../../../../../core/services/notification.service';
import { ConfirmationDialog } from '../../../../../core/services/confirmation.service';
import { ServiceTermsDataService } from '../../../../../shared/services/service-terms-data.service';
import {
  IncrementItem,
  IncrementPreviewResponse,
  ScheduledIncrement,
} from '../../../../../shared/types/staff-setup';
import { defaultGenerationDate, formatPoint, toDateOnly } from '../pay-spine';

@Component({
  selector: 'mp-increment-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    FormsModule,
    CurrencyPipe,
    MpBadge,
    MpButton,
    MpCheckbox,
    MpDatePicker,
    MpDialog,
    MpDialogFooter,
    TranslocoDirective,
  ],
  providers: [provideTranslocoScope('staff-setup')],
  templateUrl: './increment-dialog.html',
})
export class IncrementDialog {
  private readonly data = inject(ServiceTermsDataService);
  private readonly notify = inject(NotificationService);
  private readonly confirm = inject(ConfirmationDialog);
  private readonly transloco = inject(TranslocoService);

  readonly open = input(false);
  readonly serviceTermId = input.required<string>();
  readonly incrementMonth = input<number | null>(null);
  readonly incrementDay = input<number | null>(null);

  readonly closed = output<void>();
  /** Emitted when salaries actually changed (apply now / run scheduled) so the parent reloads. */
  readonly applied = output<void>();
  /** Emitted when the scheduled list changed (schedule / cancel) so the parent's banner refreshes. */
  readonly scheduleChanged = output<void>();

  protected readonly effectiveFrom = signal<Date | null>(null);
  protected readonly preview = signal<IncrementPreviewResponse | null>(null);
  protected readonly selected = signal<Set<string>>(new Set());
  protected readonly scheduled = signal<ScheduledIncrement[]>([]);
  protected readonly busy = signal(false);

  protected readonly items = computed(() => this.preview()?.items ?? []);
  protected readonly eligibleItems = computed(() => this.items().filter(i => this.isEligible(i)));

  protected readonly allSelected = computed(() => {
    const eligible = this.eligibleItems();
    return eligible.length > 0 && eligible.every(i => this.selected().has(i.contractId));
  });

  // A future date must be scheduled (the change would otherwise go live before its own date); a
  // today/back-dated one is applied now. The single date field drives which button is live.
  protected readonly isFuture = computed(() => {
    const when = this.effectiveFrom();
    if (!when) return false;
    const now = new Date();
    const today = new Date(now.getFullYear(), now.getMonth(), now.getDate());
    return new Date(when.getFullYear(), when.getMonth(), when.getDate()) > today;
  });

  protected readonly canApply = computed(
    () => !this.busy() && this.selected().size > 0 && !this.isFuture(),
  );
  protected readonly canSchedule = computed(
    () => !this.busy() && !!this.effectiveFrom() && this.isFuture(),
  );

  protected readonly pendingScheduled = computed(() =>
    this.scheduled().filter(s => s.status === 'Scheduled'),
  );

  constructor() {
    effect(() => {
      if (!this.open()) return;
      this.effectiveFrom.set(this.defaultDate());
      this.preview.set(null);
      this.selected.set(new Set());
      this.busy.set(false);
      void this.loadScheduled();
    });
  }

  protected formatPoint(value: number | null | undefined): string {
    return value == null ? '' : formatPoint(value);
  }

  protected formatDate(iso: string): string {
    return new Date(iso).toLocaleDateString('en-GB', { day: 'numeric', month: 'short', year: 'numeric' });
  }

  protected isEligible(item: IncrementItem): boolean {
    return !item.atMaximum && !item.missingRate && !item.alreadyIncremented && !!item.nextPointId;
  }

  protected isSelected(item: IncrementItem): boolean {
    return this.selected().has(item.contractId);
  }

  protected toggle(item: IncrementItem, checked: boolean): void {
    this.selected.update(set => {
      const next = new Set(set);
      if (checked) next.add(item.contractId);
      else next.delete(item.contractId);
      return next;
    });
  }

  protected toggleAll(checked: boolean): void {
    this.selected.set(checked ? new Set(this.eligibleItems().map(i => i.contractId)) : new Set());
  }

  protected statusSeverity(status: string): 'success' | 'secondary' | 'info' {
    if (status === 'Completed') return 'success';
    if (status === 'Cancelled') return 'secondary';
    return 'info';
  }

  private async loadScheduled(): Promise<void> {
    try {
      this.scheduled.set(await firstValueFrom(this.data.getScheduledIncrements(this.serviceTermId())));
    } catch {
      // The list is supplementary; a load failure shouldn't block the apply flow.
      this.scheduled.set([]);
    }
  }

  protected async runPreview(): Promise<void> {
    const when = this.effectiveFrom();
    if (!when) return;

    this.busy.set(true);
    try {
      const result = await firstValueFrom(
        this.data.previewIncrement(this.serviceTermId(), { effectiveFrom: toDateOnly(when) }),
      );
      this.preview.set(result);
      this.selected.set(new Set(result.items.filter(i => this.isEligible(i)).map(i => i.contractId)));
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('staff-setup.increment.previewError'));
    } finally {
      this.busy.set(false);
    }
  }

  protected async apply(): Promise<void> {
    const when = this.effectiveFrom();
    if (!when || this.selected().size === 0) return;

    this.busy.set(true);
    try {
      await firstValueFrom(
        this.data.applyIncrement(this.serviceTermId(), {
          effectiveFrom: toDateOnly(when),
          contractIds: [...this.selected()],
        }),
      );
      this.notify.success(
        this.transloco.translate('staff-setup.increment.appliedToast', { count: this.selected().size }),
      );
      this.applied.emit();
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('staff-setup.increment.applyError'));
    } finally {
      this.busy.set(false);
    }
  }

  protected async schedule(): Promise<void> {
    const when = this.effectiveFrom();
    if (!when) return;

    this.busy.set(true);
    try {
      await firstValueFrom(
        this.data.scheduleIncrement(this.serviceTermId(), { effectiveFrom: toDateOnly(when) }),
      );
      this.notify.success(this.transloco.translate('staff-setup.increment.scheduledToast'));
      this.preview.set(null);
      await this.loadScheduled();
      this.scheduleChanged.emit();
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('staff-setup.increment.scheduleError'));
    } finally {
      this.busy.set(false);
    }
  }

  protected async runScheduled(row: ScheduledIncrement): Promise<void> {
    this.busy.set(true);
    try {
      await firstValueFrom(this.data.applyScheduledIncrement(row.id));
      this.notify.success(this.transloco.translate('staff-setup.increment.ranToast'));
      await this.loadScheduled();
      this.applied.emit();
      this.scheduleChanged.emit();
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('staff-setup.increment.applyError'));
    } finally {
      this.busy.set(false);
    }
  }

  protected async cancelScheduled(row: ScheduledIncrement): Promise<void> {
    const ok = await this.confirm.confirm({
      header: this.transloco.translate('staff-setup.increment.cancelHeader'),
      message: this.transloco.translate('staff-setup.increment.cancelConfirm', {
        date: this.formatDate(row.effectiveDate),
      }),
      acceptLabel: this.transloco.translate('staff-setup.increment.cancelAccept'),
      acceptSeverity: 'danger',
    });
    if (!ok) return;

    this.busy.set(true);
    try {
      await firstValueFrom(this.data.cancelScheduledIncrement(row.id));
      await this.loadScheduled();
      this.scheduleChanged.emit();
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('staff-setup.increment.cancelError'));
    } finally {
      this.busy.set(false);
    }
  }

  protected onClose(): void {
    this.closed.emit();
  }

  private defaultDate(): Date {
    const month = this.incrementMonth();
    if (!month) return defaultGenerationDate(new Date());

    const day = this.incrementDay() ?? 1;
    const now = new Date();
    const today = new Date(now.getFullYear(), now.getMonth(), now.getDate());
    let candidate = new Date(now.getFullYear(), month - 1, day);
    if (candidate < today) candidate = new Date(now.getFullYear() + 1, month - 1, day);
    return candidate;
  }
}
