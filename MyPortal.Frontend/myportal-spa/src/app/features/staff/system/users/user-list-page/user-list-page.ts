import {Component, OnInit} from '@angular/core';
import {PageHeader} from '../../../../../shared/components/page-header/page-header';
import {HeaderAction} from '../../../../../shared/types/header-action.type';
import {Card} from 'primeng/card';
import {TableModule} from 'primeng/table';

@Component({
  selector: 'app-user-list-page',
  imports: [
    PageHeader,
    Card,
    TableModule
  ],
  templateUrl: './user-list-page.html',
  styleUrl: './user-list-page.scss'
})
export class UserListPage implements OnInit {
  headerActions: HeaderAction[] = [];

  ngOnInit(): void {
    this.headerActions = [
      {icon: 'pi pi-plus', label: 'New'},
    ];
  }
}
