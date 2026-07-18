import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { HeaderAction } from '../../types/header-action.type';
import { MpButton, type MpButtonVariant } from '@myportal/ui';

@Component({
  selector: 'mp-page-header',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MpButton],
  templateUrl: './page-header.html',
  styleUrl: './page-header.scss'
})
export class PageHeader {
  readonly title = input.required<string>();
  readonly subtitle = input<string | undefined>(undefined);
  readonly icon = input<string | undefined>(undefined);
  readonly actions = input<HeaderAction[]>([]);

  // Map PrimeNG's orthogonal severity/outlined/text model onto a single design-system variant.
  // text → ghost, outlined → outline, error/danger → destructive, secondary → secondary,
  // else primary (default). warn/success/info collapse to default for now (rare; no semantic
  // button colours in the token set yet).
  protected variantFor(action: HeaderAction): MpButtonVariant {
    if (action.text) return 'ghost';
    if (action.outlined) return 'outline';
    if (action.severity === 'error' || action.severity === 'danger') return 'destructive';
    if (action.severity === 'secondary') return 'secondary';
    return 'default';
  }
}
