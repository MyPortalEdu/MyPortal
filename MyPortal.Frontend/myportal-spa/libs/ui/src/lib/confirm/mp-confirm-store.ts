import { Injectable, signal } from '@angular/core';
import { MpButtonVariant } from '../button/mp-button';

export interface MpConfirmRequest {
  readonly message: string;
  readonly header: string;
  readonly icon?: string;
  readonly acceptLabel: string;
  readonly rejectLabel: string;
  readonly acceptVariant: MpButtonVariant;
}

/**
 * Signal-backed confirm-dialog state — the design-system replacement for PrimeNG's
 * ConfirmationService. A single `<mp-confirm-dialog>` host (app root) renders `request()`. App code
 * goes through the app's ConfirmationDialog wrapper (promise-based), which calls `confirm`.
 */
@Injectable({ providedIn: 'root' })
export class MpConfirmStore {
  readonly request = signal<MpConfirmRequest | null>(null);
  private resolver: ((accepted: boolean) => void) | null = null;

  /** Show a prompt; resolves true on accept, false on reject/dismiss. */
  confirm(request: MpConfirmRequest): Promise<boolean> {
    this.resolver?.(false); // a new prompt supersedes any open one
    this.request.set(request);
    return new Promise<boolean>((resolve) => (this.resolver = resolve));
  }

  accept(): void {
    this.settle(true);
  }

  reject(): void {
    this.settle(false);
  }

  private settle(accepted: boolean): void {
    this.request.set(null);
    const resolve = this.resolver;
    this.resolver = null;
    resolve?.(accepted);
  }
}
