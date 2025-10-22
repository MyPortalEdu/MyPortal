import { Component } from '@angular/core';
import {Card} from 'primeng/card';
import {PageHeader} from '../../../shared/components/page-header/page-header';
import {HeaderAction} from '../../../shared/types/header-action.type';

@Component({
  selector: 'app-home',
  imports: [
    Card,
    PageHeader
  ],
  templateUrl: './home.html',
  styleUrl: './home.scss'
})
export class Home {

}
