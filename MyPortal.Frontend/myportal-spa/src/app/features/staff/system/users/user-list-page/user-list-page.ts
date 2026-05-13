import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { PageHeader } from '../../../../../shared/components/page-header/page-header';
import { HeaderAction } from '../../../../../shared/types/header-action.type';
import { Card } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { TranslocoDirective } from '@jsverse/transloco';

@Component({
  selector: 'mp-user-list-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
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
  readonly headerActions = signal<HeaderAction[]>([]);

  ngOnInit(): void {
    this.headerActions.set([
      { icon: 'fa-solid fa-plus', label: this.transloco.translate('common.new'), severity: 'primary' }
    ]);
  }
}
