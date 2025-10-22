import {Component, Input} from '@angular/core';
import {HeaderAction} from '../../types/header-action.type';
import {Button} from 'primeng/button';
import {NgForOf, NgIf} from '@angular/common';

@Component({
  selector: 'mp-page-header',
  imports: [
    Button,
    NgIf,
    NgForOf
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
