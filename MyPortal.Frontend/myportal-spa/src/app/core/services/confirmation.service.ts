import { Injectable, inject } from '@angular/core';
import { ConfirmationService } from 'primeng/api';
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
  private readonly primeng = inject(ConfirmationService);
  private readonly transloco = inject(TranslocoService);

  /**
   * Generic confirm. Resolves true if the user accepts, false if they reject
   * or dismiss (backdrop / escape).
   */
  confirm(opts: ConfirmOptions): Promise<boolean> {
    return new Promise<boolean>(resolve => {
      const accept = opts.acceptSeverity ?? 'primary';
      this.primeng.confirm({
        message: opts.message,
        header: opts.header ?? this.transloco.translate('common.confirmHeader'),
        icon: opts.icon ?? 'fa-solid fa-circle-exclamation',
        acceptLabel: opts.acceptLabel ?? this.transloco.translate('common.confirm'),
        rejectLabel: opts.rejectLabel ?? this.transloco.translate('common.cancel'),
        // p-button severity flows through via styleClass — `p-button-danger`
        // gets the red treatment, `p-button-secondary` etc. work too.
        acceptButtonStyleClass: `p-button-${accept}`,
        rejectButtonStyleClass: 'p-button-text p-button-secondary',
        accept: () => resolve(true),
        reject: () => resolve(false),
      });
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
