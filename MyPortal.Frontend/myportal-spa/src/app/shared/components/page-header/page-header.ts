import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { HeaderAction } from '../../types/header-action.type';
import { Button } from 'primeng/button';

@Component({
  selector: 'mp-page-header',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Button],
  templateUrl: './page-header.html',
  styleUrl: './page-header.scss'
})
export class PageHeader {
  readonly title = input.required<string>();
  readonly subtitle = input<string | undefined>(undefined);
  readonly icon = input<string | undefined>(undefined);
  readonly actions = input<HeaderAction[]>([]);
}
