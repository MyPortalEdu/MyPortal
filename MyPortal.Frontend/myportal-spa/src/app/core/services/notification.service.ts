import { Injectable, inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { MpToastStore } from '@myportal/ui';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly toasts = inject(MpToastStore);

  private static readonly DefaultLifeMs = 4000;
  private static readonly ErrorLifeMs = 6000;

  success(summary: string, detail?: string): void {
    this.toasts.add({
      severity: 'success',
      summary,
      detail,
      life: NotificationService.DefaultLifeMs,
    });
  }

  info(summary: string, detail?: string): void {
    this.toasts.add({
      severity: 'info',
      summary,
      detail,
      life: NotificationService.DefaultLifeMs,
    });
  }

  warn(summary: string, detail?: string): void {
    this.toasts.add({
      severity: 'warn',
      summary,
      detail,
      life: NotificationService.DefaultLifeMs,
    });
  }

  error(summary: string, detail?: string): void {
    this.toasts.add({
      severity: 'error',
      summary,
      detail,
      life: NotificationService.ErrorLifeMs,
    });
  }

  apiError(error: unknown, fallbackSummary: string): void {
    const detail = extractApiErrorDetail(error);
    this.error(fallbackSummary, detail);
  }

  clear(): void {
    this.toasts.clear();
  }
}

function extractApiErrorDetail(error: unknown): string | undefined {
  if (!(error instanceof HttpErrorResponse)) {
    return undefined;
  }

  if (error.status === 0) {
    return 'Network error — please check your connection and try again.';
  }

  const body = error.error;

  if (isRecord(body) && isRecord(body['errors'])) {
    const messages = Object.values(body['errors'] as Record<string, unknown>)
      .flatMap(v => (Array.isArray(v) ? v : [v]))
      .filter((m): m is string => typeof m === 'string' && m.length > 0);
    if (messages.length > 0) {
      return messages.join(' ');
    }
  }

  if (isRecord(body)) {
    const detail = typeof body['detail'] === 'string' ? body['detail'].trim() : '';
    if (detail) return detail;

    const title = typeof body['title'] === 'string' ? body['title'].trim() : '';
    if (title) return title;
  }

  if (typeof body === 'string' && body.trim().length > 0) {
    return body.trim();
  }

  return undefined;
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value);
}
