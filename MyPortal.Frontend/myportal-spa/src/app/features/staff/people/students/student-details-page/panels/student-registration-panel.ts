import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  forwardRef,
  inject,
  input,
  signal,
} from '@angular/core';
import { FormField, form, submit } from '@angular/forms/signals';
import { DatePipe } from '@angular/common';
import { MpButton, MpCheckbox, MpDatePicker, MpInput } from '@myportal/ui';
import { firstValueFrom } from 'rxjs';
import {
  TranslocoDirective,
  TranslocoService,
  provideTranslocoScope,
} from '@jsverse/transloco';

import { NotificationService } from '../../../../../../core/services/notification.service';
import { Permissions } from '../../../../../../core/constants/permissions';
import { StudentsDataService } from '../../../../../../shared/services/students-data.service';
import { LookupSelect } from '../../../../../../shared/components/lookup-select/lookup-select';
import { Loading } from '../../../../../../shared/components/loading/loading';
import { SectionHeader } from '../../../../../../shared/components/section-header/section-header';
import { Field } from '../../../../../../shared/components/field/field';
import {
  StudentRegistrationDetailsResponse,
  StudentRegistrationDetailsUpsertRequest,
} from '../../../../../../shared/types/student-registration';
import { StudentAreaPanel } from './student-area-panel';

interface RegistrationModel {
  enrolmentStatusId: string | null;
  boarderStatusId: string | null;
  dateStarting: Date | null;
  upn: string;
  formerUpn: string;
  upnUnknownReasonId: string | null;
  uln: string;
  laChildId: string;
  isPartTime: boolean;
}

@Component({
  selector: 'mp-student-registration-panel',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormField, DatePipe, MpButton, MpCheckbox, MpDatePicker, MpInput, LookupSelect, Loading, SectionHeader, Field, TranslocoDirective],
  providers: [
    provideTranslocoScope('students'),
    { provide: StudentAreaPanel, useExisting: forwardRef(() => StudentRegistrationPanel) },
  ],
  templateUrl: './student-registration-panel.html',
})
export class StudentRegistrationPanel extends StudentAreaPanel implements OnInit {
  private readonly data = inject(StudentsDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);

  readonly studentId = input.required<string>();
  readonly permissions = input.required<Set<string>>();

  protected readonly loading = signal(false);
  protected readonly generatingUpn = signal(false);
  override readonly editing = signal(false);

  protected readonly registration = signal<StudentRegistrationDetailsResponse | null>(null);

  protected readonly model = signal<RegistrationModel>({
    enrolmentStatusId: null,
    boarderStatusId: null,
    dateStarting: null,
    upn: '',
    formerUpn: '',
    upnUnknownReasonId: null,
    uln: '',
    laChildId: '',
    isPartTime: false,
  });
  protected readonly f = form(this.model);
  private readonly snapshot = signal<string>('');

  protected readonly admissionNumber = computed(() => this.registration()?.admissionNumber ?? null);
  protected readonly enrolmentStatuses = computed(() => this.registration()?.enrolmentStatuses ?? []);
  protected readonly boarderStatuses = computed(() => this.registration()?.boarderStatuses ?? []);
  protected readonly upnUnknownReasons = computed(() => this.registration()?.upnUnknownReasons ?? []);

  override readonly canEdit = computed(() =>
    this.permissions().has(Permissions.Student.EditStudentRegistration),
  );

  override readonly saving = computed(() => this.f().submitting());
  override readonly valid = computed(() => this.f().valid());

  private readonly formState = computed(() => {
    const m = this.model();
    return JSON.stringify({
      enrolmentStatusId: m.enrolmentStatusId,
      boarderStatusId: m.boarderStatusId,
      dateStarting: m.dateStarting?.toISOString() ?? null,
      upn: m.upn,
      formerUpn: m.formerUpn,
      upnUnknownReasonId: m.upnUnknownReasonId,
      uln: m.uln,
      laChildId: m.laChildId,
      isPartTime: m.isPartTime,
    });
  });

  override readonly dirty = computed(
    () => this.registration() != null && this.snapshot() !== this.formState(),
  );

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.data.getRegistrationDetails(this.studentId()).subscribe({
      next: row => {
        this.registration.set(row);
        this.apply(row);
        this.loading.set(false);
      },
      error: err => {
        this.loading.set(false);
        this.notify.apiError(err, this.transloco.translate('students.registration.loadError'));
      },
    });
  }

  protected async generateUpn(): Promise<void> {
    if (!this.canEdit() || this.generatingUpn()) return;
    this.generatingUpn.set(true);
    try {
      const res = await firstValueFrom(this.data.generateUpn());
      this.model.update(m => ({ ...m, upn: res.upn }));
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('students.registration.upnGenerateError'));
    } finally {
      this.generatingUpn.set(false);
    }
  }

  override startEdit(): void {
    this.editing.set(true);
  }

  override cancel(): void {
    this.apply(this.registration());
    this.editing.set(false);
  }

  override async save(): Promise<void> {
    if (!this.canEdit() || !this.dirty()) return;
    await submit(this.f, async () => {
      const m = this.model();
      const payload: StudentRegistrationDetailsUpsertRequest = {
        enrolmentStatusId: m.enrolmentStatusId,
        boarderStatusId: m.boarderStatusId,
        dateStarting: m.dateStarting?.toISOString() ?? null,
        upn: this.normalise(m.upn),
        formerUpn: this.normalise(m.formerUpn),
        upnUnknownReasonId: m.upnUnknownReasonId,
        uln: this.normalise(m.uln),
        laChildId: this.normalise(m.laChildId),
        isPartTime: m.isPartTime,
      };
      try {
        await firstValueFrom(this.data.updateRegistrationDetails(this.studentId(), payload));
      } catch (err) {
        this.notify.apiError(err, this.transloco.translate('students.registration.saveError'));
        return;
      }
      this.notify.success(this.transloco.translate('students.registration.savedToast'));
      this.editing.set(false);
      this.load();
    });
  }

  private apply(row: StudentRegistrationDetailsResponse | null): void {
    this.model.set({
      enrolmentStatusId: row?.enrolmentStatusId ?? null,
      boarderStatusId: row?.boarderStatusId ?? null,
      dateStarting: row?.dateStarting ? new Date(row.dateStarting) : null,
      upn: row?.upn ?? '',
      formerUpn: row?.formerUpn ?? '',
      upnUnknownReasonId: row?.upnUnknownReasonId ?? null,
      uln: row?.uln ?? '',
      laChildId: row?.laChildId ?? '',
      isPartTime: row?.isPartTime ?? false,
    });
    this.f().reset();
    this.snapshot.set(this.formState());
  }
}
