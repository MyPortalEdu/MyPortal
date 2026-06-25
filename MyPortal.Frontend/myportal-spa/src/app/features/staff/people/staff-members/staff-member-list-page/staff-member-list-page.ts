import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Card } from 'primeng/card';
import { InputText } from 'primeng/inputtext';
import { TableLazyLoadEvent, TableModule } from 'primeng/table';
import { TranslocoDirective, TranslocoService, provideTranslocoScope } from '@jsverse/transloco';

import { PageHeader } from '../../../../../shared/components/page-header/page-header';
import { HeaderAction } from '../../../../../shared/types/header-action.type';
import { StaffMembersDataService } from '../../../../../shared/services/staff-members-data.service';
import { StaffMemberSummaryResponse } from '../../../../../shared/types/staff-member';
import { NotificationService } from '../../../../../core/services/notification.service';
import { MeService } from '../../../../../core/services/me-service';
import { Permissions } from '../../../../../core/constants/permissions';
import { toQueryKitParams } from '../../../../../shared/utils/primeng-querykit';
import { StaffMemberCreateDialog } from '../staff-member-create-dialog/staff-member-create-dialog';

// Columns the single search box matches against. Includes preferred names so a
// search hits whatever the grid actually displays, not just the legal name.
const SEARCH_FIELDS = ['firstName', 'lastName', 'preferredFirstName', 'preferredLastName', 'code'];

@Component({
  selector: 'mp-staff-member-list-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Card, InputText, TableModule, PageHeader, StaffMemberCreateDialog, TranslocoDirective],
  providers: [provideTranslocoScope('staff-members')],
  templateUrl: './staff-member-list-page.html',
})
export class StaffMemberListPage implements OnInit {
  private readonly data = inject(StaffMembersDataService);
  private readonly router = inject(Router);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);
  private readonly me = inject(MeService);

  protected readonly rows = signal<StaffMemberSummaryResponse[]>([]);
  protected readonly totalRecords = signal(0);
  protected readonly loading = signal(false);
  protected readonly createOpen = signal(false);

  private readonly canCreate = signal(false);

  protected readonly headerActions = computed<HeaderAction[]>(() =>
    this.canCreate()
      ? [
          {
            label: this.transloco.translate('staff-members.new'),
            icon: 'fa-solid fa-plus',
            severity: 'primary',
            command: () => this.createOpen.set(true),
          },
        ]
      : [],
  );

  ngOnInit(): void {
    this.me.me().subscribe(me => {
      this.canCreate.set(me.permissions?.includes(Permissions.Staff.EditAllStaffBasicDetails) ?? false);
    });
  }

  load(event: TableLazyLoadEvent): void {
    this.loading.set(true);
    this.data.list(toQueryKitParams(event, { globalFields: SEARCH_FIELDS })).subscribe({
      next: page => {
        this.rows.set(page.items ?? []);
        this.totalRecords.set(page.totalItems ?? 0);
        this.loading.set(false);
      },
      error: err => {
        this.loading.set(false);
        this.notify.apiError(err, this.transloco.translate('staff-members.loadError'));
      },
    });
  }

  openDetails(row: StaffMemberSummaryResponse): void {
    this.router.navigate(['/staff/people/staff-members', row.id]);
  }

  // Name shown in the grid: preferred name wins over legal where set.
  protected displayName(row: StaffMemberSummaryResponse): string {
    const first = row.preferredFirstName?.trim() || row.firstName;
    const last = row.preferredLastName?.trim() || row.lastName;
    return `${first} ${last}`.trim();
  }

  // Legal name, shown as a muted second line only when it differs from the
  // preferred name above — so a row never repeats the same name twice.
  protected legalName(row: StaffMemberSummaryResponse): string | null {
    const legal = `${row.firstName} ${row.lastName}`.trim();
    return legal === this.displayName(row) ? null : legal;
  }

  protected initials(row: StaffMemberSummaryResponse): string {
    const first = (row.preferredFirstName?.trim() || row.firstName).charAt(0);
    const last = (row.preferredLastName?.trim() || row.lastName).charAt(0);
    return (first + last).toUpperCase();
  }

  protected onCreateClosed(): void {
    this.createOpen.set(false);
  }

  protected onCreated(staffMemberId: string): void {
    this.createOpen.set(false);
    // Land straight on the new record so HR can fill in the other areas
    // (equality, employment, professional, etc.) via their PUTs.
    this.router.navigate(['/staff/people/staff-members', staffMemberId]);
  }

  protected onOpenExisting(staffMemberId: string): void {
    // HR picked someone already on staff — jump to their existing profile rather
    // than creating a duplicate.
    this.createOpen.set(false);
    this.router.navigate(['/staff/people/staff-members', staffMemberId]);
  }
}
