import { Directive, forwardRef } from '@angular/core';
import { ConnectedPosition } from '@angular/cdk/overlay';
import { BRN_POPOVER_OVERLAY_DEFAULT_OPTIONS, BrnPopover } from '@spartan-ng/brain/popover';
import { BrnOverlay, provideBrnOverlayDefaultOptions } from '@spartan-ng/brain/overlay';

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
