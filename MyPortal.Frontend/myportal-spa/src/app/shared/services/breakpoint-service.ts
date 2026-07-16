import { Injectable, signal } from '@angular/core';

/**
 * Reactive viewport breakpoint flags backed by `matchMedia`. Use to adapt
 * layout/markup that CSS alone can't reach — e.g. dropping a button label so
 * PrimeNG renders a proper icon-only button on phones. Breakpoints mirror
 * Tailwind's defaults so template `sm:`/`lg:` classes and these signals agree.
 */
@Injectable({ providedIn: 'root' })
export class BreakpointService {
  // Below Tailwind's `sm` (640px): treat as a phone-width layout.
  private readonly mobileQuery = window.matchMedia('(max-width: 639.98px)');

  /** True when the viewport is narrower than the `sm` breakpoint. */
  readonly isMobile = signal(this.mobileQuery.matches);

  constructor() {
    this.mobileQuery.addEventListener('change', e => this.isMobile.set(e.matches));
  }
}
