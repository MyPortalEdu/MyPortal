import { ChangeDetectionStrategy, Component, OnInit, inject, signal, viewChild } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import {
  MpButton,
  MpCard,
  MpCheckbox,
  MpSkeleton,
  MpTable,
  MpTableBody,
  MpTableEmpty,
  MpTableHeader,
} from '@myportal/ui';
import { FormsModule } from '@angular/forms';
import { TranslocoDirective, TranslocoService, provideTranslocoScope } from '@jsverse/transloco';

import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { StaffMemberPicker } from '../../../../shared/components/pickers/staff-member-picker/staff-member-picker';
import { NotificationService } from '../../../../core/services/notification.service';
import { MeService } from '../../../../core/services/me-service';
import { Permissions } from '../../../../core/constants/permissions';
import { TrainingEventsDataService } from '../../../../shared/services/training-events-data.service';
import { StaffMemberSummaryResponse } from '../../../../shared/types/staff-member';
import { TrainingEventAttendee, TrainingEventDetails } from '../../../../shared/types/training-events';
import { TrainingEventFormDialog } from './training-event-form-dialog';

@Component({
  selector: 'mp-training-event-details-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    FormsModule,
    RouterLink,
    MpButton,
    MpCard,
    MpCheckbox,
    MpSkeleton,
    MpTable,
    MpTableBody,
    MpTableEmpty,
    MpTableHeader,
    StaffMemberPicker,
    PageHeader,
    EmptyState,
    TrainingEventFormDialog,
    TranslocoDirective,
  ],
  providers: [provideTranslocoScope('training-events')],
  templateUrl: './training-event-details-page.html',
})
export class TrainingEventDetailsPage implements OnInit {
  private readonly data = inject(TrainingEventsDataService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);
  private readonly me = inject(MeService);

  protected readonly event = signal<TrainingEventDetails | null>(null);
  protected readonly loading = signal(false);
  protected readonly canEdit = signal(false);

  private readonly formDialog = viewChild(TrainingEventFormDialog);
  private id = '';

  ngOnInit(): void {
    this.id = this.route.snapshot.paramMap.get('id') ?? '';
    this.me.me().subscribe(me =>
      this.canEdit.set(me.permissions?.includes(Permissions.Staff.EditAllStaffProfessionalDetails) ?? false),
    );
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.data.get(this.id).subscribe({
      next: e => {
        this.event.set(e);
        this.loading.set(false);
      },
      error: err => {
        this.loading.set(false);
        this.notify.apiError(err, this.transloco.translate('training-events.loadError'));
      },
    });
  }

  protected edit(): void {
    this.formDialog()?.open();
  }

  protected remove(): void {
    if (!confirm(this.transloco.translate('training-events.confirmDelete'))) return;
    this.data.delete(this.id).subscribe({
      next: () => {
        this.notify.success(this.transloco.translate('training-events.deleted'));
        this.router.navigate(['/staff/people/training-events']);
      },
      error: err => this.notify.apiError(err, this.transloco.translate('training-events.deleteError')),
    });
  }

  protected onStaffPicked(staff: StaffMemberSummaryResponse): void {
    this.data.bookAttendees(this.id, [staff.id]).subscribe({
      next: () => this.load(),
      error: err => this.notify.apiError(err, this.transloco.translate('training-events.bookError')),
    });
  }

  protected removeAttendee(attendee: TrainingEventAttendee): void {
    this.data.removeAttendee(this.id, attendee.staffMemberId).subscribe({
      next: () => this.load(),
      error: err => this.notify.apiError(err, this.transloco.translate('training-events.bookError')),
    });
  }

  protected setAttended(attendee: TrainingEventAttendee, attended: boolean): void {
    this.data.setAttendance(this.id, attendee.staffMemberId, attended).subscribe({
      next: () => this.load(),
      error: err => this.notify.apiError(err, this.transloco.translate('training-events.attendError')),
    });
  }

  protected whenLabel(e: TrainingEventDetails): string {
    const start = new Date(e.startTime).toLocaleString('en-GB', {
      day: 'numeric',
      month: 'short',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
    if (!e.endTime) return start;
    const end = new Date(e.endTime).toLocaleTimeString('en-GB', { hour: '2-digit', minute: '2-digit' });
    return `${start} – ${end}`;
  }
}
