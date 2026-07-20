import { Overlay, OverlayPositionBuilder, OverlayRef, type ConnectedPosition } from '@angular/cdk/overlay';
import { ComponentPortal } from '@angular/cdk/portal';
import { ChangeDetectionStrategy, Component, Directive, ElementRef, inject, input } from '@angular/core';

@Component({
  selector: 'mp-tooltip-bubble',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<div class="pointer-events-none max-w-xs rounded-control bg-foreground px-2 py-1 text-xs text-background shadow-overlay">{{ text() }}</div>`,
})
export class MpTooltipBubble {
  readonly text = input('');
}

type MpTooltipPosition = 'top' | 'bottom' | 'left' | 'right';

const POSITIONS: Record<MpTooltipPosition, ConnectedPosition> = {
  top: { originX: 'center', originY: 'top', overlayX: 'center', overlayY: 'bottom', offsetY: -6 },
  bottom: { originX: 'center', originY: 'bottom', overlayX: 'center', overlayY: 'top', offsetY: 6 },
  left: { originX: 'start', originY: 'center', overlayX: 'end', overlayY: 'center', offsetX: -6 },
  right: { originX: 'end', originY: 'center', overlayX: 'start', overlayY: 'center', offsetX: 6 },
};

@Directive({
  selector: '[mpTooltip]',
  standalone: true,
  host: {
    '(mouseenter)': 'show()',
    '(mouseleave)': 'hide()',
    '(focusin)': 'show()',
    '(focusout)': 'hide()',
    '(click)': 'hide()',
  },
})
export class MpTooltip {
  private readonly overlay = inject(Overlay);
  private readonly positionBuilder = inject(OverlayPositionBuilder);
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  readonly text = input.required<string>({ alias: 'mpTooltip' });
  readonly position = input<MpTooltipPosition>('top', { alias: 'tooltipPosition' });
  readonly tooltipDisabled = input(false);

  private overlayRef: OverlayRef | null = null;

  protected show(): void {
    const text = this.text()?.trim();
    if (!text || this.tooltipDisabled() || this.overlayRef) return;
    this.overlayRef = this.overlay.create({
      positionStrategy: this.positionBuilder
        .flexibleConnectedTo(this.host)
        .withPositions([POSITIONS[this.position()], POSITIONS.top, POSITIONS.bottom]),
      scrollStrategy: this.overlay.scrollStrategies.close(),
    });
    const ref = this.overlayRef.attach(new ComponentPortal(MpTooltipBubble));
    ref.setInput('text', text);
  }

  protected hide(): void {
    this.overlayRef?.dispose();
    this.overlayRef = null;
  }

  ngOnDestroy(): void {
    this.hide();
  }
}
