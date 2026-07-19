import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { cva, type VariantProps } from 'class-variance-authority';
import { type ClassValue } from 'clsx';
import { cn } from '../utils/cn';

/**
 * Small pill/label — the design-system equivalent of `p-tag`.
 *
 * Two ways to colour it: the design-system `variant` (default/secondary/outline/destructive), or
 * p-tag's `severity` (success/info/warn/danger/secondary/contrast) for status pills — `severity`
 * wins when set, so `[severity]="fn()"` migrations from p-tag are mechanical. `icon` + `value`
 * mirror p-tag; extra content can also be projected.
 */
const BADGE_BASE =
  'inline-flex items-center gap-1 rounded-full border px-2 py-0.5 text-xs font-medium whitespace-nowrap';

export const mpBadgeVariants = cva(BADGE_BASE, {
  variants: {
    variant: {
      default: 'border-transparent bg-primary/10 text-primary',
      secondary: 'border-transparent bg-muted text-muted-foreground',
      outline: 'border-border text-foreground',
      destructive: 'border-transparent bg-destructive/10 text-destructive',
    },
  },
  defaultVariants: { variant: 'default' },
});

export type MpBadgeVariant = NonNullable<VariantProps<typeof mpBadgeVariants>['variant']>;
export type MpBadgeSeverity = 'success' | 'info' | 'warn' | 'danger' | 'secondary' | 'contrast';

// p-tag severities → status colours. Semantic status hues (not the theme primary); `dark:` maps to
// the app's `.mp-dark` custom variant.
const SEVERITY_CLASSES: Record<MpBadgeSeverity, string> = {
  success: 'border-transparent bg-green-100 text-green-700 dark:bg-green-500/15 dark:text-green-400',
  info: 'border-transparent bg-blue-100 text-blue-700 dark:bg-blue-500/15 dark:text-blue-400',
  warn: 'border-transparent bg-amber-100 text-amber-700 dark:bg-amber-500/15 dark:text-amber-400',
  danger: 'border-transparent bg-red-100 text-red-700 dark:bg-red-500/15 dark:text-red-400',
  secondary: 'border-transparent bg-muted text-muted-foreground',
  contrast: 'border-transparent bg-foreground text-background',
};

@Component({
  selector: 'mp-badge',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { '[class]': 'computedClass()' },
  templateUrl: './mp-badge.html',
})
export class MpBadge {
  readonly value = input<string>('');
  readonly icon = input<string | undefined>(undefined);
  readonly variant = input<MpBadgeVariant>('default');
  readonly severity = input<MpBadgeSeverity | null | undefined>(undefined);
  readonly userClass = input<ClassValue>('', { alias: 'class' });

  protected readonly computedClass = computed(() => {
    const severity = this.severity();
    const base = severity ? cn(BADGE_BASE, SEVERITY_CLASSES[severity]) : mpBadgeVariants({ variant: this.variant() });
    return cn(base, this.userClass());
  });
}
