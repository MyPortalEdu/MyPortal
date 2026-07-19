import { Directive, computed, input } from '@angular/core';
import { type ClassValue } from 'clsx';
import { cn } from '../utils/cn';

@Directive({
  selector: 'input[mpInput]',
  standalone: true,
  host: {
    '[class]': 'computedClass()',
    '[attr.aria-invalid]': 'invalid() || null',
  },
})
export class MpInput {
  readonly invalid = input<boolean | null | undefined>(false);
  readonly userClass = input<ClassValue>('', { alias: 'class' });

  protected readonly computedClass = computed(() =>
    cn(
      'flex h-8 w-full rounded-control border bg-background px-2.5 py-1 text-sm outline-none transition-colors ' +
        'placeholder:text-muted-foreground hover:border-[var(--p-form-field-hover-border-color)] ' +
        'focus:border-ring focus:ring-2 focus:ring-ring/40 disabled:cursor-not-allowed disabled:opacity-50',
      this.invalid() ? 'border-destructive' : 'border-input',
      this.userClass(),
    ),
  );
}
