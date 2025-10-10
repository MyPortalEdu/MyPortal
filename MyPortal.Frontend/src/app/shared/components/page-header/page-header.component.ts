import {Component, Input} from '@angular/core';
import {HeaderAction} from '../../types/header-action.type';

@Component({
  selector: 'mp-page-header',
  standalone: false,
  templateUrl: './page-header.component.html',
  styleUrl: './page-header.component.scss'
})
export class PageHeaderComponent {
  @Input() title!: string;
  @Input() subtitle?: string;
  @Input() icon?: string;
  @Input() actions: HeaderAction[] = [];
}
