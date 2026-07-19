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

@Injectable({ providedIn: 'root' })
export class MpConfirmStore {
  readonly request = signal<MpConfirmRequest | null>(null);
  private resolver: ((accepted: boolean) => void) | null = null;

  confirm(request: MpConfirmRequest): Promise<boolean> {
    this.resolver?.(false);
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
