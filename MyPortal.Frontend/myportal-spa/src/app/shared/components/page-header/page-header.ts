import {Component, Input} from '@angular/core';
import {HeaderAction} from '../../types/header-action.type';
import {Button, ButtonDirective} from 'primeng/button';

@Component({
  selector: 'mp-page-header',
  imports: [
    Button,
    ButtonDirective
  ],
  templateUrl: './page-header.html',
  styleUrl: './page-header.scss'
})
export class PageHeader {
  @Input() title!: string;
  @Input() subtitle?: string;
  @Input() icon?: string;
  @Input() actions: HeaderAction[] = [];
}
