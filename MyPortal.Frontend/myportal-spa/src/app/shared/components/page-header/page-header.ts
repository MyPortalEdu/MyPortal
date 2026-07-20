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

  protected variantFor(action: HeaderAction): MpButtonVariant {
    if (action.text) return 'ghost';
    if (action.outlined) return 'outline';
    if (action.severity === 'error' || action.severity === 'danger') return 'destructive';
    if (action.severity === 'secondary') return 'secondary';
    return 'default';
  }
}
