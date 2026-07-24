import { ChangeDetectionStrategy, Component, computed, inject, model, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MpButton, MpCheckbox, MpDialog, MpInput, MpTextarea } from '@myportal/ui';
import { TranslocoDirective, TranslocoService, provideTranslocoScope } from '@jsverse/transloco';

import { NotificationService } from '../../../../core/services/notification.service';
import { TrainingCoursesDataService } from '../../../../shared/services/training-courses-data.service';
import { TrainingCourse, TrainingCourseUpsert } from '../../../../shared/types/training-course';

interface FormModel {
  code: string;
  name: string;
  description: string;
  active: boolean;
}

@Component({
  selector: 'mp-training-course-form-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule, MpButton, MpCheckbox, MpDialog, MpInput, MpTextarea, TranslocoDirective],
  providers: [provideTranslocoScope('training-courses')],
  templateUrl: './training-course-form-dialog.html',
})
export class TrainingCourseFormDialog {
  private readonly data = inject(TrainingCoursesDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);

  readonly visible = model(false);
  readonly saved = output<void>();

  private readonly current = signal<TrainingCourse | null>(null);
  protected readonly model = signal<FormModel>({ code: '', name: '', description: '', active: true });
  protected readonly saving = signal(false);
  protected readonly isEdit = computed(() => !!this.current());

  open(course: TrainingCourse | null = null): void {
    this.current.set(course);
    const c = course;
    this.model.set(
      c
        ? { code: c.code, name: c.name, description: c.description ?? '', active: c.active }
        : { code: '', name: '', description: '', active: true },
    );
    this.visible.set(true);
  }

  protected patch<K extends keyof FormModel>(key: K, value: FormModel[K]): void {
    this.model.update(m => ({ ...m, [key]: value }));
  }

  protected save(): void {
    const m = this.model();
    if (!m.code.trim() || !m.name.trim()) {
      this.notify.error(this.transloco.translate('training-courses.form.incomplete'));
      return;
    }

    const payload: TrainingCourseUpsert = {
      code: m.code.trim(),
      name: m.name.trim(),
      description: m.description.trim() || null,
      active: m.active,
    };

    this.saving.set(true);
    const existing = this.current();
    const req$ = existing ? this.data.update(existing.id, payload) : this.data.create(payload);
    req$.subscribe({
      next: () => {
        this.saving.set(false);
        this.visible.set(false);
        this.notify.success(this.transloco.translate('training-courses.form.saved'));
        this.saved.emit();
      },
      error: err => {
        this.saving.set(false);
        this.notify.apiError(err, this.transloco.translate('training-courses.form.saveError'));
      },
    });
  }
}
