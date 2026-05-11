import { Component, OnInit, inject } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { PageHeader } from '../../../../../shared/components/page-header/page-header';
import { HeaderAction } from '../../../../../shared/types/header-action.type';
import { Card } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { TranslocoDirective } from '@jsverse/transloco';

@Component({
  selector: 'app-user-list-page',
  imports: [
    PageHeader,
    Card,
    TableModule,
    TranslocoDirective,
  ],
  templateUrl: './user-list-page.html',
})
export class UserListPage implements OnInit {
  private readonly transloco = inject(TranslocoService);
  headerActions: HeaderAction[] = [];

  ngOnInit(): void {
    this.headerActions = [
      { icon: 'pi pi-plus', label: this.transloco.translate('common.new'), severity: 'primary' }
    ];
  }
}
