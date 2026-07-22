import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import {
  MpButton,
  MpCard,
  MpInput,
  MpTable,
  MpTableCaption,
  MpTableHeader,
  MpTableBody,
  MpTableEmpty,
  MpSortable,
  MpSortIcon,
} from '@myportal/ui';
import { TranslocoDirective, TranslocoService, provideTranslocoScope } from '@jsverse/transloco';

import { PageHeader } from '../../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../../shared/components/empty-state/empty-state';
import { HeaderAction } from '../../../../../shared/types/header-action.type';
import { ContactsDataService } from '../../../../../shared/services/contacts-data.service';
import { ContactSummaryResponse } from '../../../../../shared/types/contact-summary';
import { NotificationService } from '../../../../../core/services/notification.service';
import { MeService } from '../../../../../core/services/me-service';
import { Permissions } from '../../../../../core/constants/permissions';
import { GridState } from '../../../../../shared/utils/querykit';
import { GridListController, injectGridList } from '../../../../../shared/utils/grid-list';
import { ContactCreateDialog } from '../contact-create-dialog/contact-create-dialog';

const SEARCH_FIELDS = ['firstName', 'lastName', 'preferredFirstName', 'preferredLastName'];

const GRID_DEFAULTS: GridState = { first: 0, rows: 25 };

@Component({
  selector: 'mp-contact-list-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    MpButton,
    MpCard,
    MpInput,
    MpTable,
    MpTableCaption,
    MpTableHeader,
    MpTableBody,
    MpTableEmpty,
    MpSortable,
    MpSortIcon,
    FormsModule,
    RouterLink,
    PageHeader,
    EmptyState,
    ContactCreateDialog,
    TranslocoDirective,
  ],
  providers: [provideTranslocoScope('contacts')],
  templateUrl: './contact-list-page.html',
})
export class ContactListPage implements OnInit {
  private readonly data = inject(ContactsDataService);
  private readonly router = inject(Router);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);
  private readonly me = inject(MeService);

  protected readonly createOpen = signal(false);
  protected readonly canCreate = signal(false);

  private readonly table = viewChild(MpTable);

  protected readonly grid: GridListController<ContactSummaryResponse> =
    injectGridList<ContactSummaryResponse>({
      list: params => this.data.list(params),
      searchFields: SEARCH_FIELDS,
      defaults: GRID_DEFAULTS,
      table: this.table,
      onError: err => this.notify.apiError(err, this.transloco.translate('contacts.loadError')),
    });

  protected readonly hasFilter = computed(() => this.grid.hasFilter());

  protected readonly headerActions = computed<HeaderAction[]>(() =>
    this.canCreate()
      ? [
          {
            label: this.transloco.translate('contacts.new'),
            icon: 'fa-solid fa-plus',
            severity: 'primary',
            command: () => this.createOpen.set(true),
          },
        ]
      : [],
  );

  ngOnInit(): void {
    this.me.me().subscribe(me => {
      this.canCreate.set(me.permissions?.includes(Permissions.Contact.EditContactDetails) ?? false);
    });
  }

  openDetails(row: ContactSummaryResponse): void {
    this.router.navigate(['/staff/people/contacts', row.id]);
  }

  protected onRowClick(event: MouseEvent, row: ContactSummaryResponse): void {
    if ((event.target as HTMLElement).closest('a,button')) return;
    this.openDetails(row);
  }

  protected clearFilters(): void {
    this.grid.clearSearch();
  }

  protected displayName(row: ContactSummaryResponse): string {
    const first = row.preferredFirstName?.trim() || row.firstName;
    const last = row.preferredLastName?.trim() || row.lastName;
    return `${first} ${last}`.trim();
  }

  protected legalName(row: ContactSummaryResponse): string | null {
    const legal = `${row.firstName} ${row.lastName}`.trim();
    return legal === this.displayName(row) ? null : legal;
  }

  protected initials(row: ContactSummaryResponse): string {
    const first = (row.preferredFirstName?.trim() || row.firstName).charAt(0);
    const last = (row.preferredLastName?.trim() || row.lastName).charAt(0);
    return (first + last).toUpperCase();
  }

  protected onCreateClosed(): void {
    this.createOpen.set(false);
  }

  protected onCreated(contactId: string): void {
    this.createOpen.set(false);
    this.router.navigate(['/staff/people/contacts', contactId]);
  }

  protected onOpenExisting(contactId: string): void {
    this.createOpen.set(false);
    this.router.navigate(['/staff/people/contacts', contactId]);
  }
}
