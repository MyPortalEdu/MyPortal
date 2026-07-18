import { Directive, forwardRef } from '@angular/core';
import { ConnectedPosition } from '@angular/cdk/overlay';
import { BRN_POPOVER_OVERLAY_DEFAULT_OPTIONS, BrnPopover } from '@spartan-ng/brain/popover';
import { BrnOverlay, provideBrnOverlayDefaultOptions } from '@spartan-ng/brain/overlay';

/**
 * Edge-aware popover — a drop-in replacement for `<brn-popover>`.
 *
 * brn's stock popover only offers two fallback positions (below / above), BOTH anchored at the same
 * horizontal `align`, and runs `withPush(false)`. So a panel wider than its trigger near the right
 * (or left) edge just overflows off-screen — there's no horizontal flip and no push-back.
 *
 * This subclass adds start/end horizontal fallbacks and enables CDK's `withPush` + a viewport
 * margin, so the overlay shifts back on-screen instead of clipping. Everything else (open/close,
 * backdrop, scroll strategy, sideOffset/offsetX inputs) is inherited unchanged.
 *
 * Usage: `<div mpPopover #pop="mpPopover">` with `<button brnPopoverTrigger>` + `*brnPopoverContent`
 * inside — the trigger/content resolve this via the BrnOverlay/BrnPopover DI aliases below.
 */
@Directive({
  selector: '[mpPopover]',
  standalone: true,
  exportAs: 'mpPopover',
  providers: [
    { provide: BrnOverlay, useExisting: forwardRef(() => MpPopover) },
    { provide: BrnPopover, useExisting: forwardRef(() => MpPopover) },
    provideBrnOverlayDefaultOptions(BRN_POPOVER_OVERLAY_DEFAULT_OPTIONS),
  ],
})
export class MpPopover extends BrnPopover {
  // Prefer start-aligned below the trigger, then flip horizontally (end) before flipping above —
  // so a panel near the right edge slides left rather than off-screen.
  protected override getAttachPositions(): ConnectedPosition[] {
    const sideOffset = this.sideOffset();
    const offsetX = this.offsetX();
    return [
      { originX: 'start', originY: 'bottom', overlayX: 'start', overlayY: 'top', offsetX, offsetY: sideOffset },
      { originX: 'end', originY: 'bottom', overlayX: 'end', overlayY: 'top', offsetX: -offsetX, offsetY: sideOffset },
      { originX: 'start', originY: 'top', overlayX: 'start', overlayY: 'bottom', offsetX, offsetY: -sideOffset },
      { originX: 'end', originY: 'top', overlayX: 'end', overlayY: 'bottom', offsetX: -offsetX, offsetY: -sideOffset },
    ];
  }

  protected override getPositionStrategy() {
    const attachTo = this.getAttachTo();
    if (!attachTo) return super.getPositionStrategy();
    return this._positionBuilder
      .flexibleConnectedTo(attachTo)
      .withPositions(this.getAttachPositions())
      .withPush(true)
      .withViewportMargin(8);
  }
}
