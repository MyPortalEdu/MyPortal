import { DestroyRef, Injectable, computed, effect, inject, signal } from '@angular/core';

export type Theme = 'light' | 'dark' | 'system';

// Keep in sync with the inline pre-bootstrap script in index.html. That script
// applies the dark class before Angular boots to avoid a light → dark flash and
// can't import from here because it runs before the bundle loads.
const STORAGE_KEY = 'mp:theme';
const DARK_CLASS = 'mp-dark';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly _theme = signal<Theme>(this.readStored());
  readonly theme = this._theme.asReadonly();

  private readonly darkQuery = window.matchMedia('(prefers-color-scheme: dark)');
  private readonly osDark = signal(this.darkQuery.matches);

  readonly isDark = computed(() => {
    const t = this._theme();
    return t === 'dark' || (t === 'system' && this.osDark());
  });

  constructor() {
    const onChange = (e: MediaQueryListEvent) => this.osDark.set(e.matches);
    this.darkQuery.addEventListener('change', onChange);
    inject(DestroyRef).onDestroy(() => this.darkQuery.removeEventListener('change', onChange));

    effect(() => {
      document.documentElement.classList.toggle(DARK_CLASS, this.isDark());
    });
  }

  setTheme(next: Theme): void {
    this._theme.set(next);
    try {
      localStorage.setItem(STORAGE_KEY, next);
    } catch { /* private mode / disabled storage — preference applies for the session only */ }
  }

  private readStored(): Theme {
    try {
      const v = localStorage.getItem(STORAGE_KEY);
      return v === 'light' || v === 'dark' || v === 'system' ? v : 'system';
    } catch {
      return 'system';
    }
  }
}
