import { Injectable, inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { MessageService } from 'primeng/api';

/**
 * Thin wrapper around PrimeNG's MessageService so consumers don't import the
 * PrimeNG API directly. Keeps the toast vendor a one-file swap if we ever
 * move off PrimeNG, and lets us centralise defaults (life, sticky behaviour
 * for errors, dedup) in one place.
 */
@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly messages = inject(MessageService);

  private static readonly DefaultLifeMs = 4000;
  private static readonly ErrorLifeMs = 6000;

  success(summary: string, detail?: string): void {
    this.messages.add({
      severity: 'success',
      summary,
      detail,
      life: NotificationService.DefaultLifeMs,
    });
  }

  info(summary: string, detail?: string): void {
    this.messages.add({
      severity: 'info',
      summary,
      detail,
      life: NotificationService.DefaultLifeMs,
    });
  }

  warn(summary: string, detail?: string): void {
    this.messages.add({
      severity: 'warn',
      summary,
      detail,
      life: NotificationService.DefaultLifeMs,
    });
  }

  /**
   * Surface an error. Errors get a longer life than informational toasts so
   * the user has time to read them before they auto-dismiss.
   */
  error(summary: string, detail?: string): void {
    this.messages.add({
      severity: 'error',
      summary,
      detail,
      life: NotificationService.ErrorLifeMs,
    });
  }

  /**
   * Surface an error from an HTTP call, using the server's ProblemDetails
   * body as the toast detail when available. The caller supplies an
   * action-shaped summary ("Couldn't delete bulletin") so the user sees
   * what action failed, not just the abstract title ("Forbidden.").
   */
  apiError(error: unknown, fallbackSummary: string): void {
    const detail = extractApiErrorDetail(error);
    this.error(fallbackSummary, detail);
  }

  clear(): void {
    this.messages.clear();
  }
}

/**
 * Pull a human-readable detail string out of whatever an HTTP failure put on
 * the wire. Order of precedence:
 *   1. ValidationProblemDetails — flatten `errors[field]` arrays so the user
 *      sees all field messages joined.
 *   2. ProblemDetails `detail` — the server's specific message.
 *   3. ProblemDetails `title` — the category ("Forbidden.", "Not found.")
 *      when no detail is set.
 *   4. Network / unknown / non-HTTP — a generic fallback so the toast still
 *      shows the action-context summary without an empty body.
 */
function extractApiErrorDetail(error: unknown): string | undefined {
  if (!(error instanceof HttpErrorResponse)) {
    return undefined;
  }

  // status 0 is the browser's "request never reached the server" signal —
  // offline, DNS failure, CORS preflight reject, etc.
  if (error.status === 0) {
    return 'Network error — please check your connection and try again.';
  }

  const body = error.error;

  // ValidationProblemDetails: { errors: { field: [msg, msg], ... }, title, ... }
  if (isRecord(body) && isRecord(body['errors'])) {
    const messages = Object.values(body['errors'] as Record<string, unknown>)
      .flatMap(v => (Array.isArray(v) ? v : [v]))
      .filter((m): m is string => typeof m === 'string' && m.length > 0);
    if (messages.length > 0) {
      return messages.join(' ');
    }
  }

  // ProblemDetails: { title, detail, status, ... }
  if (isRecord(body)) {
    const detail = typeof body['detail'] === 'string' ? body['detail'].trim() : '';
    if (detail) return detail;

    const title = typeof body['title'] === 'string' ? body['title'].trim() : '';
    if (title) return title;
  }

  // Plain-text bodies (e.g. some unhandled paths return a string).
  if (typeof body === 'string' && body.trim().length > 0) {
    return body.trim();
  }

  return undefined;
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value);
}
