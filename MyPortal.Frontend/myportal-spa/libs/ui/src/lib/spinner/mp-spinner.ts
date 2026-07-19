import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { type ClassValue } from 'clsx';
import { cn } from '../utils/cn';

/**
 * Indeterminate loading spinner — the design-system equivalent of `p-progressSpinner`. A simple
 * CSS ring spun with `animate-spin`, coloured by `currentColor` (defaults to the primary colour;
 * override with a text-colour utility via `class`). Size with the `size` input (any CSS length).
 *
 * Migrating from p-progressSpinner: `[style]="{ width: 'W', height: 'W' }"` / `styleClass` sizing
 * → `size="W"`; `ariaLabel` → `ariaLabel`; `strokeWidth` has no equivalent (fixed 2px ring).
 */
@Component({
  selector: 'mp-spinner',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'inline-block text-primary',
    role: 'status',
    '[attr.aria-label]': 'ariaLabel()',
  },
  templateUrl: './mp-spinner.html',
})
export class MpSpinner {
  readonly size = input('2rem');
  readonly ariaLabel = input('Loading');
  readonly userClass = input<ClassValue>('', { alias: 'class' });

  protected readonly ringClass = computed(() =>
    cn('inline-block animate-spin rounded-full border-2 border-current border-t-transparent align-middle', this.userClass()),
  );
}
