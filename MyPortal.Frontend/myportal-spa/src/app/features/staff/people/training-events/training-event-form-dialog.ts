import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  input,
  model,
  output,
  signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  MpButton,
  MpDialog,
  MpInput,
  MpInputNumber,
  MpDatePicker,
  MpSelect,
  MpTextarea,
} from '@myportal/ui';
import { TranslocoDirective, TranslocoService, provideTranslocoScope } from '@jsverse/transloco';

import { NotificationService } from '../../../../core/services/notification.service';
import { TrainingEventsDataService } from '../../../../shared/services/training-events-data.service';
import { StaffReportsDataService } from '../../../../shared/services/staff-reports-data.service';
import { ReportOption } from '../../../../shared/types/staff-reports';
import { TrainingEventDetails, TrainingEventUpsert } from '../../../../shared/types/training-events';

interface FormModel {
  trainingCourseId: string;
  title: string;
  start: Date;
  end: Date | null;
  location: string;
  trainer: string;
  provider: string;
  hours: number | null;
  capacity: number | null;
  notes: string;
}

function blank(): FormModel {
  const start = new Date();
  start.setMinutes(0, 0, 0);
  return {
    trainingCourseId: '',
    title: '',
    start,
    end: null,
    location: '',
    trainer: '',
    provider: '',
    hours: null,
    capacity: null,
    notes: '',
  };
}

function toLocalIso(date: Date): string {
  const p = (n: number) => String(n).padStart(2, '0');
  return `${date.getFullYear()}-${p(date.getMonth() + 1)}-${p(date.getDate())}T${p(date.getHours())}:${p(date.getMinutes())}:00`;
}

@Component({
  selector: 'mp-training-event-form-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    FormsModule,
    MpButton,
    MpDialog,
    MpInput,
    MpInputNumber,
    MpDatePicker,
    MpSelect,
    MpTextarea,
    TranslocoDirective,
  ],
  providers: [provideTranslocoScope('training-events')],
  templateUrl: './training-event-form-dialog.html',
})
export class TrainingEventFormDialog implements OnInit {
  private readonly data = inject(TrainingEventsDataService);
  private readonly reports = inject(StaffReportsDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);

  readonly visible = model(false);
  readonly event = input<TrainingEventDetails | null>(null);
  readonly saved = output<void>();

  protected readonly model = signal<FormModel>(blank());
  protected readonly courses = signal<ReportOption[]>([]);
  protected readonly saving = signal(false);

  protected readonly isEdit = computed(() => !!this.event());

  ngOnInit(): void {
    this.reports.getTrainingCourses().subscribe({ next: c => this.courses.set(c), error: () => {} });
  }

  open(): void {
    const e = this.event();
    this.model.set(
      e
        ? {
            trainingCourseId: e.trainingCourseId,
            title: e.title,
            start: new Date(e.startTime),
            end: e.endTime ? new Date(e.endTime) : null,
            location: e.location ?? '',
            trainer: e.trainer ?? '',
            provider: e.provider ?? '',
            hours: e.hours,
            capacity: e.capacity,
            notes: e.notes ?? '',
          }
        : blank(),
    );
    this.visible.set(true);
  }

  protected patch<K extends keyof FormModel>(key: K, value: FormModel[K]): void {
    this.model.update(m => ({ ...m, [key]: value }));
  }

  protected onCourseChange(id: string): void {
    const name = this.courses().find(c => c.id === id)?.name ?? '';
    this.model.update(m => ({ ...m, trainingCourseId: id, title: m.title.trim() ? m.title : name }));
  }

  protected save(): void {
    const m = this.model();
    if (!m.trainingCourseId || !m.title.trim()) {
      this.notify.error(this.transloco.translate('training-events.form.incomplete'));
      return;
    }

    const payload: TrainingEventUpsert = {
      trainingCourseId: m.trainingCourseId,
      title: m.title.trim(),
      startTime: toLocalIso(m.start),
      endTime: m.end ? toLocalIso(m.end) : null,
      location: m.location.trim() || null,
      trainer: m.trainer.trim() || null,
      provider: m.provider.trim() || null,
      hours: m.hours,
      capacity: m.capacity,
      notes: m.notes.trim() || null,
    };

    this.saving.set(true);
    const existing = this.event();
    const req$ = existing
      ? this.data.update(existing.id, payload)
      : this.data.create(payload);

    req$.subscribe({
      next: () => {
        this.saving.set(false);
        this.visible.set(false);
        this.notify.success(this.transloco.translate('training-events.form.saved'));
        this.saved.emit();
      },
      error: err => {
        this.saving.set(false);
        this.notify.apiError(err, this.transloco.translate('training-events.form.saveError'));
      },
    });
  }
}
