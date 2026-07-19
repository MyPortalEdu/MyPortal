import { Directive, ElementRef, computed, inject, input } from '@angular/core';
import { cva, type VariantProps } from 'class-variance-authority';
import { type ClassValue } from 'clsx';
import { cn } from '../utils/cn';

export const mpButtonVariants = cva(
  'inline-flex items-center justify-center gap-2 whitespace-nowrap rounded-control text-sm font-medium ' +
    'transition-colors cursor-pointer focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring ' +
    'focus-visible:ring-offset-1 disabled:pointer-events-none disabled:opacity-50',
  {
    variants: {
      variant: {
        default: 'bg-primary text-primary-foreground hover:bg-primary/90',
        secondary: 'bg-secondary text-secondary-foreground hover:bg-secondary/80',
        outline: 'border border-input bg-background hover:bg-accent hover:text-accent-foreground',
        ghost: 'hover:bg-accent hover:text-accent-foreground',
        destructive: 'bg-destructive text-destructive-foreground hover:bg-destructive/90',
      },
      size: {
        default: 'h-8 px-3 py-1',
        sm: 'h-7 rounded-control px-2.5 text-xs',
        lg: 'h-10 px-6',
        icon: 'h-8 w-8',
      },
    },
    defaultVariants: { variant: 'default', size: 'default' },
  },
);

export type MpButtonVariant = NonNullable<VariantProps<typeof mpButtonVariants>['variant']>;
export type MpButtonSize = NonNullable<VariantProps<typeof mpButtonVariants>['size']>;

@Directive({
  selector: 'button[mpButton], a[mpButton]',
  standalone: true,
  host: {
    '[class]': 'computedClass()',
    '[attr.type]': 'resolvedType()',
  },
})
export class MpButton {
  private readonly el = inject<ElementRef<HTMLElement>>(ElementRef);

  readonly variant = input<MpButtonVariant>('default');
  readonly size = input<MpButtonSize>('default');
  readonly type = input<string | null>(null);
  readonly userClass = input<ClassValue>('', { alias: 'class' });

  protected readonly resolvedType = computed(() => {
    if (this.type()) return this.type();
    return this.el.nativeElement.tagName === 'BUTTON' ? 'button' : null;
  });

  protected readonly computedClass = computed(() =>
    cn(mpButtonVariants({ variant: this.variant(), size: this.size() }), this.userClass()),
  );
}
