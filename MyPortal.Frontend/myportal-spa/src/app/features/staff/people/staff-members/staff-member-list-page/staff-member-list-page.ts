import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Card } from 'primeng/card';
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

@Component({
  selector: 'mp-staff-member-list-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Card, TableModule, PageHeader, StaffMemberCreateDialog, TranslocoDirective],
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
    this.data.list(toQueryKitParams(event)).subscribe({
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
