import { Overlay, OverlayPositionBuilder, OverlayRef } from '@angular/cdk/overlay';
import { TemplatePortal } from '@angular/cdk/portal';
import { ChangeDetectionStrategy, Component, TemplateRef, ViewContainerRef, inject, input, viewChild } from '@angular/core';
import { cn } from '../utils/cn';

/** A menu entry — structurally compatible with the subset of PrimeNG's MenuItem the app uses. */
export interface MpMenuItem {
  label?: string;
  icon?: string;
  command?: (event?: unknown) => void;
  disabled?: boolean;
  separator?: boolean;
  styleClass?: string;
}

/**
 * Popup menu — the design-system equivalent of `<p-menu [popup]="true">`. Give it `[model]` and
 * call `toggle($event)` from a trigger button (via a template ref). Renders in a CDK overlay
 * anchored to the trigger; a transparent backdrop closes it on outside click.
 */
@Component({
  selector: 'mp-menu',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './mp-menu.html',
})
export class MpMenu {
  private readonly overlay = inject(Overlay);
  private readonly positionBuilder = inject(OverlayPositionBuilder);
  private readonly vcr = inject(ViewContainerRef);

  readonly model = input<readonly MpMenuItem[]>([]);
  protected readonly menuTpl = viewChild.required<TemplateRef<unknown>>('menu');

  private overlayRef: OverlayRef | null = null;

  toggle(event: Event): void {
    if (this.overlayRef) {
      this.hide();
      return;
    }
    this.show(event);
  }

  show(event: Event): void {
    const origin = (event.currentTarget ?? event.target) as HTMLElement;
    this.overlayRef = this.overlay.create({
      positionStrategy: this.positionBuilder
        .flexibleConnectedTo(origin)
        .withPositions([
          { originX: 'end', originY: 'bottom', overlayX: 'end', overlayY: 'top', offsetY: 4 },
          { originX: 'end', originY: 'top', overlayX: 'end', overlayY: 'bottom', offsetY: -4 },
        ])
        .withPush(true),
      scrollStrategy: this.overlay.scrollStrategies.reposition(),
      hasBackdrop: true,
      backdropClass: 'cdk-overlay-transparent-backdrop',
    });
    this.overlayRef.backdropClick().subscribe(() => this.hide());
    this.overlayRef.attach(new TemplatePortal(this.menuTpl(), this.vcr));
  }

  hide(): void {
    this.overlayRef?.dispose();
    this.overlayRef = null;
  }

  protected run(item: MpMenuItem): void {
    if (item.disabled) return;
    item.command?.();
    this.hide();
  }

  protected itemClass(item: MpMenuItem): string {
    return cn(
      'flex w-full items-center gap-2 rounded-control px-2 py-1.5 text-sm text-left outline-none ' +
        'hover:bg-accent hover:text-accent-foreground disabled:pointer-events-none disabled:opacity-50',
      item.styleClass,
    );
  }
}
