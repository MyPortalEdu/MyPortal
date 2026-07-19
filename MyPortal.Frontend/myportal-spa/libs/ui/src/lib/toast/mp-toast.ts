import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MpToastSeverity, MpToastStore } from './mp-toast-store';

@Component({
  selector: 'mp-toast',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './mp-toast.html',
})
export class MpToast {
  protected readonly store = inject(MpToastStore);

  protected panelClass(severity: MpToastSeverity): string {
    switch (severity) {
      case 'success':
        return 'border-green-200 bg-green-50 text-green-800 dark:border-green-500/30 dark:bg-green-500/10 dark:text-green-300';
      case 'info':
        return 'border-blue-200 bg-blue-50 text-blue-800 dark:border-blue-500/30 dark:bg-blue-500/10 dark:text-blue-300';
      case 'warn':
        return 'border-amber-200 bg-amber-50 text-amber-800 dark:border-amber-500/30 dark:bg-amber-500/10 dark:text-amber-300';
      case 'error':
        return 'border-red-200 bg-red-50 text-red-800 dark:border-red-500/30 dark:bg-red-500/10 dark:text-red-300';
    }
  }

  protected icon(severity: MpToastSeverity): string {
    switch (severity) {
      case 'success':
        return 'fa-solid fa-circle-check';
      case 'info':
        return 'fa-solid fa-circle-info';
      case 'warn':
        return 'fa-solid fa-triangle-exclamation';
      case 'error':
        return 'fa-solid fa-circle-exclamation';
    }
  }
}
