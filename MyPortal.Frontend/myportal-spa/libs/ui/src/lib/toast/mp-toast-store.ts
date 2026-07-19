import { Injectable, signal } from '@angular/core';

export type MpToastSeverity = 'success' | 'info' | 'warn' | 'error';

export interface MpToastItem {
  readonly id: number;
  readonly severity: MpToastSeverity;
  readonly summary: string;
  readonly detail?: string;
}

/**
 * Signal-backed toast queue — the design-system replacement for PrimeNG's MessageService. A single
 * `<mp-toast>` host (mounted once at the app root) renders `toasts()`. App code should go through
 * the app's NotificationService (which calls `add`), not this directly.
 */
@Injectable({ providedIn: 'root' })
export class MpToastStore {
  readonly toasts = signal<readonly MpToastItem[]>([]);
  private seq = 0;

  /** Queue a toast; auto-dismisses after `life` ms (0 = sticky). */
  add(toast: { severity: MpToastSeverity; summary: string; detail?: string; life?: number }): void {
    const id = ++this.seq;
    const life = toast.life ?? 4000;
    this.toasts.update((list) => [...list, { id, severity: toast.severity, summary: toast.summary, detail: toast.detail }]);
    if (life > 0) setTimeout(() => this.dismiss(id), life);
  }

  dismiss(id: number): void {
    this.toasts.update((list) => list.filter((t) => t.id !== id));
  }

  clear(): void {
    this.toasts.set([]);
  }
}
