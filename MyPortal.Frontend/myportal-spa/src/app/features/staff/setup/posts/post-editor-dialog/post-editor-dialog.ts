import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import {
  FormField,
  applyEach,
  form,
  maxLength,
  required,
  submit,
  validate,
} from '@angular/forms/signals';
import {
  MpButton,
  MpCheckbox,
  MpDatePicker,
  MpDialog,
  MpDialogFooter,
  MpFormField,
  MpInput,
  MpInputNumber,
} from '@myportal/ui';
import { firstValueFrom } from 'rxjs';
import { TranslocoDirective, TranslocoService, provideTranslocoScope } from '@jsverse/transloco';

import { NotificationService } from '../../../../../core/services/notification.service';
import { LookupSelect } from '../../../../../shared/components/lookup-select/lookup-select';
import { PostsDataService } from '../../../../../shared/services/posts-data.service';
import { LookupResponse } from '../../../../../shared/types/lookup';
import { PostResponse, PostUpsertRequest } from '../../../../../shared/types/staff-setup';

interface VacancyFormRow {
  id: string | null;
  startDate: Date | null;
  endDate: Date | null;
  isAdvertised: boolean;
  isTemporarilyFilled: boolean;
  subjectId: string | null;
  notes: string;
}

interface PostForm {
  reference: string;
  description: string;
  postCategoryId: string | null;
  serviceTermId: string | null;
  swrPostCode: string;
  establishedFte: number | null;
  vacancies: VacancyFormRow[];
}

function blank(): PostForm {
  return {
    reference: '',
    description: '',
    postCategoryId: null,
    serviceTermId: null,
    swrPostCode: '',
    establishedFte: null,
    vacancies: [],
  };
}

@Component({
  selector: 'mp-post-editor-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    FormField,
    MpButton,
    MpCheckbox,
    MpDatePicker,
    MpDialog,
    MpDialogFooter,
    MpFormField,
    MpInput,
    MpInputNumber,
    LookupSelect,
    TranslocoDirective,
  ],
  providers: [provideTranslocoScope('staff-setup')],
  templateUrl: './post-editor-dialog.html',
})
export class PostEditorDialog {
  private readonly data = inject(PostsDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);

  readonly open = input(false);
  readonly post = input<PostResponse | null>(null);
  readonly categories = input<LookupResponse[]>([]);
  readonly serviceTerms = input<LookupResponse[]>([]);
  readonly subjects = input<LookupResponse[]>([]);

  readonly closed = output<void>();
  readonly saved = output<void>();

  protected readonly isEdit = computed(() => this.post() != null);

  protected readonly model = signal<PostForm>(blank());
  protected readonly f = form(this.model, path => {
    required(path.reference);
    maxLength(path.reference, 32);
    required(path.description);
    maxLength(path.description, 256);
    maxLength(path.swrPostCode, 10);

    applyEach(path.vacancies, item => {
      required(item.startDate);
      maxLength(item.notes, 256);
      validate(item.endDate, ({ value, valueOf }) => {
        const end = value();
        const start = valueOf(item.startDate);
        return !end || !start || end.getTime() >= start.getTime()
          ? undefined
          : { kind: 'endBeforeStart', message: 'staff-setup.posts.endBeforeStart' };
      });
    });
  });

  constructor() {
    effect(() => {
      if (!this.open()) return;
      const row = this.post();
      this.model.set(
        row
          ? {
              reference: row.reference,
              description: row.description,
              postCategoryId: row.postCategoryId ?? null,
              serviceTermId: row.serviceTermId ?? null,
              swrPostCode: row.swrPostCode ?? '',
              establishedFte: row.establishedFte ?? null,
              vacancies: row.vacancies.map(v => ({
                id: v.id,
                startDate: v.startDate ? new Date(v.startDate) : null,
                endDate: v.endDate ? new Date(v.endDate) : null,
                isAdvertised: v.isAdvertised,
                isTemporarilyFilled: v.isTemporarilyFilled,
                subjectId: v.subjectId ?? null,
                notes: v.notes ?? '',
              })),
            }
          : blank(),
      );
      this.f().reset();
    });
  }

  protected addVacancy(): void {
    this.model.update(m => ({
      ...m,
      vacancies: [
        ...m.vacancies,
        {
          id: null,
          startDate: null,
          endDate: null,
          isAdvertised: false,
          isTemporarilyFilled: false,
          subjectId: null,
          notes: '',
        },
      ],
    }));
  }

  protected removeVacancy(index: number): void {
    this.model.update(m => ({ ...m, vacancies: m.vacancies.filter((_, i) => i !== index) }));
  }

  protected onClose(): void {
    this.closed.emit();
  }

  protected save(): Promise<boolean> {
    return submit(this.f, async () => {
      const m = this.model();
      const payload: PostUpsertRequest = {
        reference: m.reference.trim(),
        description: m.description.trim(),
        postCategoryId: m.postCategoryId,
        serviceTermId: m.serviceTermId,
        swrPostCode: m.swrPostCode.trim() || null,
        establishedFte: m.establishedFte,
        vacancies: m.vacancies.map(v => ({
          id: v.id,
          startDate: v.startDate?.toISOString() ?? null,
          endDate: v.endDate?.toISOString() ?? null,
          isAdvertised: v.isAdvertised,
          isTemporarilyFilled: v.isTemporarilyFilled,
          subjectId: v.subjectId,
          notes: v.notes.trim() || null,
        })),
      };

      const existing = this.post();

      try {
        if (existing) {
          await firstValueFrom(this.data.update(existing.id, payload));
        } else {
          await firstValueFrom(this.data.create(payload));
        }
      } catch (err) {
        this.notify.apiError(err, this.transloco.translate('staff-setup.posts.saveError'));
        return;
      }

      this.notify.success(this.transloco.translate('staff-setup.posts.savedToast'));
      this.saved.emit();
    });
  }
}
