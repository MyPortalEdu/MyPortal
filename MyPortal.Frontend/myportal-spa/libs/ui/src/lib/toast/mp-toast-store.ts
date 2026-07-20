import { Injectable, signal } from '@angular/core';

export type MpToastSeverity = 'success' | 'info' | 'warn' | 'error';

export interface MpToastItem {
  readonly id: number;
  readonly severity: MpToastSeverity;
  readonly summary: string;
  readonly detail?: string;
}

interface ToastTimer {
  handle: ReturnType<typeof setTimeout>;
  startedAt: number;
  remaining: number;
}

@Injectable({ providedIn: 'root' })
export class MpToastStore {
  readonly toasts = signal<readonly MpToastItem[]>([]);
  private seq = 0;
  private readonly timers = new Map<number, ToastTimer>();

  add(toast: { severity: MpToastSeverity; summary: string; detail?: string; life?: number }): void {
    const id = ++this.seq;
    const life = toast.life ?? 4000;
    this.toasts.update((list) => [...list, { id, severity: toast.severity, summary: toast.summary, detail: toast.detail }]);
    if (life > 0) {
      this.timers.set(id, { handle: setTimeout(() => this.dismiss(id), life), startedAt: Date.now(), remaining: life });
    }
  }

  pause(id: number): void {
    const timer = this.timers.get(id);
    if (!timer) return;
    clearTimeout(timer.handle);
    timer.remaining = Math.max(0, timer.remaining - (Date.now() - timer.startedAt));
  }

  resume(id: number): void {
    const timer = this.timers.get(id);
    if (!timer) return;
    timer.startedAt = Date.now();
    timer.handle = setTimeout(() => this.dismiss(id), timer.remaining);
  }

  dismiss(id: number): void {
    const timer = this.timers.get(id);
    if (timer) {
      clearTimeout(timer.handle);
      this.timers.delete(id);
    }
    this.toasts.update((list) => list.filter((t) => t.id !== id));
  }

  clear(): void {
    this.timers.forEach((t) => clearTimeout(t.handle));
    this.timers.clear();
    this.toasts.set([]);
  }
}
