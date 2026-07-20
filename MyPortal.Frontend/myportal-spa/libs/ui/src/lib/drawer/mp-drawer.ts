import { DOCUMENT } from '@angular/common';
import { A11yModule } from '@angular/cdk/a11y';
import { ChangeDetectionStrategy, Component, computed, effect, inject, input, model, output } from '@angular/core';

@Component({
  selector: 'mp-drawer',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [A11yModule],
  templateUrl: './mp-drawer.html',
})
export class MpDrawer {
  private readonly doc = inject(DOCUMENT);

  readonly visible = model<boolean>(false);
  readonly position = input<'left' | 'right' | 'top' | 'bottom'>('left');
  readonly modal = input(true);
  readonly dismissable = input(true);
  readonly closed = output<void>();

  protected readonly panelClasses = computed(() => {
    const base = 'absolute flex flex-col bg-background shadow-overlay outline-none';
    switch (this.position()) {
      case 'right':
        return `${base} inset-y-0 right-0 w-80 max-w-[85vw] border-l border-border`;
      case 'top':
        return `${base} inset-x-0 top-0 max-h-[85vh] border-b border-border`;
      case 'bottom':
        return `${base} inset-x-0 bottom-0 max-h-[85vh] border-t border-border`;
      default:
        return `${base} inset-y-0 left-0 w-80 max-w-[85vw] border-r border-border`;
    }
  });

  constructor() {
    effect(() => (this.doc.body.style.overflow = this.visible() ? 'hidden' : ''));
  }

  private dismiss(): void {
    this.visible.set(false);
    this.closed.emit();
  }
  protected onBackdrop(): void {
    if (this.dismissable()) this.dismiss();
  }
  protected onEscape(): void {
    this.dismiss();
  }
}
