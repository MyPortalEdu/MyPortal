import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal, viewChild } from '@angular/core';
import { Router } from '@angular/router';
import {
  MpButton,
  MpCard,
  MpSkeleton,
  MpTable,
  MpTableBody,
  MpTableEmpty,
  MpTableHeader,
} from '@myportal/ui';
import { TranslocoDirective, TranslocoService, provideTranslocoScope } from '@jsverse/transloco';

import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { NotificationService } from '../../../../core/services/notification.service';
import { MeService } from '../../../../core/services/me-service';
import { Permissions } from '../../../../core/constants/permissions';
import { TrainingEventsDataService } from '../../../../shared/services/training-events-data.service';
import { TrainingEventSummary } from '../../../../shared/types/training-events';
import { HeaderAction } from '../../../../shared/types/header-action.type';
import { TrainingEventFormDialog } from './training-event-form-dialog';

@Component({
  selector: 'mp-training-event-list-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    MpButton,
    MpCard,
    MpSkeleton,
    MpTable,
    MpTableBody,
    MpTableEmpty,
    MpTableHeader,
    PageHeader,
    EmptyState,
    TrainingEventFormDialog,
    TranslocoDirective,
  ],
  providers: [provideTranslocoScope('training-events')],
  templateUrl: './training-event-list-page.html',
})
export class TrainingEventListPage implements OnInit {
  private readonly data = inject(TrainingEventsDataService);
  private readonly router = inject(Router);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);
  private readonly me = inject(MeService);

  protected readonly events = signal<TrainingEventSummary[]>([]);
  protected readonly loading = signal(false);
  protected readonly canEdit = signal(false);

  private readonly formDialog = viewChild(TrainingEventFormDialog);

  protected readonly headerActions = computed<HeaderAction[]>(() =>
    this.canEdit()
      ? [
          {
            label: this.transloco.translate('training-events.new'),
            icon: 'fa-solid fa-plus',
            severity: 'primary',
            command: () => this.formDialog()?.open(),
          },
        ]
      : [],
  );

  ngOnInit(): void {
    this.me.me().subscribe(me =>
      this.canEdit.set(me.permissions?.includes(Permissions.Staff.EditAllStaffProfessionalDetails) ?? false),
    );
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.data.list().subscribe({
      next: rows => {
        this.events.set(rows);
        this.loading.set(false);
      },
      error: err => {
        this.loading.set(false);
        this.notify.apiError(err, this.transloco.translate('training-events.loadError'));
      },
    });
  }

  protected openDetails(row: TrainingEventSummary): void {
    this.router.navigate(['/staff/people/training-events', row.id]);
  }

  protected whenLabel(row: TrainingEventSummary): string {
    const start = new Date(row.startTime);
    return start.toLocaleString('en-GB', {
      day: 'numeric',
      month: 'short',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  }

  protected attendeesLabel(row: TrainingEventSummary): string {
    return row.capacity != null ? `${row.attendeeCount} / ${row.capacity}` : `${row.attendeeCount}`;
  }
}
