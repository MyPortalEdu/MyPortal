import { DOCUMENT } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  Directive,
  computed,
  contentChild,
  effect,
  inject,
  input,
  model,
  output,
} from '@angular/core';
import { A11yModule } from '@angular/cdk/a11y';
import { type ClassValue } from 'clsx';
import { cn } from '../utils/cn';

@Directive({ selector: '[mpDialogFooter]', standalone: true })
export class MpDialogFooter {}

@Component({
  selector: 'mp-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [A11yModule],
  templateUrl: './mp-dialog.html',
})
export class MpDialog {
  private readonly doc = inject(DOCUMENT);

  readonly visible = model<boolean>(false);
  readonly modal = input(true);
  readonly dismissable = input(true);
  readonly closeOnEscape = input(true);
  readonly panelClass = input<ClassValue>('');
  readonly ariaLabel = input<string | undefined>(undefined);

  readonly title = input<string | undefined>(undefined);
  readonly titleIcon = input<string | undefined>(undefined);
  readonly showClose = input(true);
  readonly closeDisabled = input(false);
  readonly closeAriaLabel = input('Close');
  readonly bodyClass = input<ClassValue>('');

  readonly closed = output<void>();

  protected readonly footerRef = contentChild(MpDialogFooter);

  protected readonly panelClasses = computed(() =>
    cn(
      'relative z-[1101] flex max-h-[90vh] w-full max-w-[95vw] flex-col overflow-hidden rounded-surface ' +
        'border border-border bg-background shadow-overlay outline-none',
      this.panelClass(),
    ),
  );

  protected readonly bodyClasses = computed(() =>
    cn('min-h-0 flex-1 overflow-auto p-4', this.bodyClass()),
  );

  constructor() {
    effect(() => (this.doc.body.style.overflow = this.visible() ? 'hidden' : ''));
  }

  protected dismiss(): void {
    this.visible.set(false);
    this.closed.emit();
  }

  protected onBackdrop(): void {
    if (this.dismissable()) this.dismiss();
  }

  protected onEscape(): void {
    if (this.closeOnEscape()) this.dismiss();
  }
}
