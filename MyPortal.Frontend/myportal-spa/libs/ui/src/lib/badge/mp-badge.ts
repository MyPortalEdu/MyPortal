import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { cva, type VariantProps } from 'class-variance-authority';
import { type ClassValue } from 'clsx';
import { cn } from '../utils/cn';

/** Small pill/label — the design-system equivalent of p-tag. */
export const mpBadgeVariants = cva(
  'inline-flex items-center gap-1 rounded-full border px-2 py-0.5 text-xs font-medium whitespace-nowrap',
  {
    variants: {
      variant: {
        default: 'border-transparent bg-primary/10 text-primary',
        secondary: 'border-transparent bg-muted text-muted-foreground',
        outline: 'border-border text-foreground',
        destructive: 'border-transparent bg-destructive/10 text-destructive',
      },
    },
    defaultVariants: { variant: 'default' },
  },
);

export type MpBadgeVariant = NonNullable<VariantProps<typeof mpBadgeVariants>['variant']>;

@Component({
  selector: 'mp-badge',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { '[class]': 'computedClass()' },
  templateUrl: './mp-badge.html',
})
export class MpBadge {
  readonly value = input<string>('');
  readonly variant = input<MpBadgeVariant>('default');
  readonly userClass = input<ClassValue>('', { alias: 'class' });

  protected readonly computedClass = computed(() =>
    cn(mpBadgeVariants({ variant: this.variant() }), this.userClass()),
  );
}
