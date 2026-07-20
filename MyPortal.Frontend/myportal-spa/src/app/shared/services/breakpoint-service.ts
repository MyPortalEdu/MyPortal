import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class BreakpointService {
  private readonly mobileQuery = window.matchMedia('(max-width: 639.98px)');

  readonly isMobile = signal(this.mobileQuery.matches);

  constructor() {
    this.mobileQuery.addEventListener('change', e => this.isMobile.set(e.matches));
  }
}
