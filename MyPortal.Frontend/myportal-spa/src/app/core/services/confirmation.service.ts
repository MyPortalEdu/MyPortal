import { Injectable, inject } from '@angular/core';
import { MpConfirmStore } from '@myportal/ui';
import { TranslocoService } from '@jsverse/transloco';

export interface ConfirmOptions {
  /** The body of the dialog. Already-translated string. */
  message: string;
  /** Header text. Defaults to a generic "Confirm" from the root scope. */
  header?: string;
  /** Icon class for the icon next to the message (e.g. `fa-solid fa-...`). */
  icon?: string;
  /** Accept-button label. Defaults to "Confirm". */
  acceptLabel?: string;
  /** Reject-button label. Defaults to "Cancel". */
  rejectLabel?: string;
  /** PrimeNG severity for the accept button. Defaults to primary. */
  acceptSeverity?: 'primary' | 'secondary' | 'success' | 'info' | 'warn' | 'danger' | 'help' | 'contrast';
}

/**
 * Promise-based wrapper around PrimeNG's ConfirmationService. Call sites do
 * `const ok = await this.confirm.danger({ message: '…' })` instead of
 * splitting their delete logic across `accept` and `reject` callbacks.
 *
 * Wraps a single root-mounted <p-confirmDialog> (see app.html) so callers
 * don't have to drop their own into every screen that needs a prompt.
 */
@Injectable({ providedIn: 'root' })
export class ConfirmationDialog {
  private readonly store = inject(MpConfirmStore);
  private readonly transloco = inject(TranslocoService);

  /**
   * Generic confirm. Resolves true if the user accepts, false if they reject
   * or dismiss (backdrop / escape).
   */
  confirm(opts: ConfirmOptions): Promise<boolean> {
    return this.store.confirm({
      message: opts.message,
      header: opts.header ?? this.transloco.translate('common.confirmHeader'),
      icon: opts.icon ?? 'fa-solid fa-circle-exclamation',
      acceptLabel: opts.acceptLabel ?? this.transloco.translate('common.confirm'),
      rejectLabel: opts.rejectLabel ?? this.transloco.translate('common.cancel'),
      // Map the accept button's intent to an MpButton variant.
      acceptVariant:
        opts.acceptSeverity === 'danger'
          ? 'destructive'
          : opts.acceptSeverity === 'secondary'
            ? 'secondary'
            : 'default',
    });
  }

  /**
   * Destructive-action confirm. Pre-styled with a red Delete button and a
   * warning icon — the standard shape of an "are you sure?" prompt before
   * an irreversible action.
   */
  danger(opts: Omit<ConfirmOptions, 'acceptSeverity' | 'icon'> & { icon?: string }): Promise<boolean> {
    return this.confirm({
      ...opts,
      icon: opts.icon ?? 'fa-solid fa-triangle-exclamation',
      acceptLabel: opts.acceptLabel ?? this.transloco.translate('common.delete'),
      acceptSeverity: 'danger',
    });
  }
}
