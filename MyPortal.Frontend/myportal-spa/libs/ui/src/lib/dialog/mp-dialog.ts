import { DOCUMENT } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  input,
  model,
  output,
} from '@angular/core';
import { A11yModule } from '@angular/cdk/a11y';
import { type ClassValue } from 'clsx';
import { cn } from '../utils/cn';

/**
 * Modal dialog — the design-system equivalent of `p-dialog` for the app's controlled pattern
 * (`[(visible)]` two-way, own header/content/footer projected as children). A fixed backdrop +
 * centered panel, toggled by `visible`; CDK's `cdkTrapFocus` handles the focus trap and restores
 * focus to the trigger on close, and the body scroll is locked while open.
 *
 * Projected content IS the dialog chrome — the panel adds only the surface (border/rounded/shadow)
 * and clips it, so consumers keep full control of header/body/footer layout.
 *
 * Migrating from p-dialog: `[(visible)]` stays; `dismissableMask`→`dismissable`, `(onHide)`→`(closed)`,
 * width via `panelClass` (e.g. `panelClass="w-[min(32rem,94vw)]"`). `[modal]`/`[closeOnEscape]` map 1:1.
 */
@Component({
  selector: 'mp-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [A11yModule],
  templateUrl: './mp-dialog.html',
})
export class MpDialog {
  private readonly doc = inject(DOCUMENT);

  /** Two-way visibility. Use `[(visible)]` (or `[visible]` + `(visibleChange)`). */
  readonly visible = model<boolean>(false);
  /** Render the dimmed backdrop (p-dialog `modal`). */
  readonly modal = input(true);
  /** Clicking the backdrop closes the dialog (p-dialog `dismissableMask`). */
  readonly dismissable = input(true);
  readonly closeOnEscape = input(true);
  /** Extra classes for the panel — width and any per-dialog overrides. */
  readonly panelClass = input<ClassValue>('');
  readonly ariaLabel = input<string | undefined>(undefined);

  /** Fires when the user dismisses the dialog via the backdrop or Escape (p-dialog `onHide`). */
  readonly closed = output<void>();

  protected readonly panelClasses = computed(() =>
    cn(
      'relative z-[1101] flex max-h-[90vh] w-full max-w-lg flex-col overflow-hidden rounded-surface ' +
        'border border-border bg-background shadow-overlay outline-none',
      this.panelClass(),
    ),
  );

  constructor() {
    // Lock body scroll while a dialog is open (restored on close/destroy).
    effect(() => (this.doc.body.style.overflow = this.visible() ? 'hidden' : ''));
  }

  private dismiss(): void {
    // Set the model (two-way `[(visible)]` closes directly) AND emit closed so owners of a one-way
    // `[visible]` binding can flip their own source (mirrors p-dialog's internal-hide + onHide).
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
