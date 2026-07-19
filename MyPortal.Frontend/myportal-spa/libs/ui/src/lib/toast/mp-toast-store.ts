import { Injectable, signal } from '@angular/core';

export type MpToastSeverity = 'success' | 'info' | 'warn' | 'error';

export interface MpToastItem {
  readonly id: number;
  readonly severity: MpToastSeverity;
  readonly summary: string;
  readonly detail?: string;
}

@Injectable({ providedIn: 'root' })
export class MpToastStore {
  readonly toasts = signal<readonly MpToastItem[]>([]);
  private seq = 0;

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
