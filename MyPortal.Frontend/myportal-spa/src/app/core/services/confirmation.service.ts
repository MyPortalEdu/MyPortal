import { Injectable, inject } from '@angular/core';
import { MpConfirmStore } from '@myportal/ui';
import { TranslocoService } from '@jsverse/transloco';

export interface ConfirmOptions {
  message: string;
  header?: string;
  icon?: string;
  acceptLabel?: string;
  rejectLabel?: string;
  acceptSeverity?: 'primary' | 'secondary' | 'success' | 'info' | 'warn' | 'danger' | 'help' | 'contrast';
}

@Injectable({ providedIn: 'root' })
export class ConfirmationDialog {
  private readonly store = inject(MpConfirmStore);
  private readonly transloco = inject(TranslocoService);

  confirm(opts: ConfirmOptions): Promise<boolean> {
    return this.store.confirm({
      message: opts.message,
      header: opts.header ?? this.transloco.translate('common.confirmHeader'),
      icon: opts.icon ?? 'fa-solid fa-circle-exclamation',
      acceptLabel: opts.acceptLabel ?? this.transloco.translate('common.confirm'),
      rejectLabel: opts.rejectLabel ?? this.transloco.translate('common.cancel'),
      acceptVariant:
        opts.acceptSeverity === 'danger'
          ? 'destructive'
          : opts.acceptSeverity === 'secondary'
            ? 'secondary'
            : 'default',
    });
  }

  danger(opts: Omit<ConfirmOptions, 'acceptSeverity' | 'icon'> & { icon?: string }): Promise<boolean> {
    return this.confirm({
      ...opts,
      icon: opts.icon ?? 'fa-solid fa-triangle-exclamation',
      acceptLabel: opts.acceptLabel ?? this.transloco.translate('common.delete'),
      acceptSeverity: 'danger',
    });
  }
}
