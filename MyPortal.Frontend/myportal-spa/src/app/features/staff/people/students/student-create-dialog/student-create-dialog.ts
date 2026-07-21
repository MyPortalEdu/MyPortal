import {
  ChangeDetectionStrategy,
  Component,
  effect,
  inject,
  input,
  output,
  signal,
  untracked,
} from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { FormField, applyWhen, form, required, submit, validate } from '@angular/forms/signals';
import { DatePipe } from '@angular/common';
import { MpDatePicker, MpDialog, MpDialogFooter, MpButton, MpInput, MpSpinner } from '@myportal/ui';
import {
  catchError,
  debounceTime,
  distinctUntilChanged,
  firstValueFrom,
  map,
  of,
  switchMap,
} from 'rxjs';
import {
  TranslocoDirective,
  TranslocoService,
  provideTranslocoScope,
} from '@jsverse/transloco';

import { GenderSelect } from '../../../../../shared/components/gender-select/gender-select';
import { NotificationService } from '../../../../../core/services/notification.service';
import { StudentsDataService } from '../../../../../shared/services/students-data.service';
import { StudentBasicDetailsUpsertRequest } from '../../../../../shared/types/student-basic-details';
import { StudentMatchResponse } from '../../../../../shared/types/student-match';

interface CreateModel {
  title: string;
  firstName: string;
  middleName: string;
  lastName: string;
  preferredFirstName: string;
  preferredLastName: string;
  gender: string;
  dob: Date | null;
}

@Component({
  selector: 'mp-student-create-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    FormsModule,
    FormField,
    DatePipe,
    MpButton,
    MpDatePicker,
    MpDialog,
    MpDialogFooter,
    MpInput,
    GenderSelect,
    MpSpinner,
    TranslocoDirective,
  ],
  providers: [provideTranslocoScope('students')],
  templateUrl: './student-create-dialog.html',
})
export class StudentCreateDialog {
  private readonly data = inject(StudentsDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);

  readonly open = input.required<boolean>();
  readonly closed = output<void>();
  readonly created = output<string>();
  readonly openExisting = output<string>();

  protected readonly step = signal<'search' | 'attach'>('search');

  protected readonly searchTerm = signal('');
  protected readonly results = signal<StudentMatchResponse[]>([]);
  protected readonly searching = signal(false);

  protected readonly selectedPerson = signal<StudentMatchResponse | null>(null);

  protected readonly model = signal<CreateModel>({
    title: '',
    firstName: '',
    middleName: '',
    lastName: '',
    preferredFirstName: '',
    preferredLastName: '',
    gender: '',
    dob: null,
  });
  protected readonly f = form(this.model, path => {
    applyWhen(path, () => this.step() === 'search', p => {
      for (const field of [p.firstName, p.lastName, p.gender]) {
        required(field);
        validate(field, ({ value }) =>
          value().trim().length ? undefined : { kind: 'blank', message: 'common.validation.required' },
        );
      }
    });
  });

  constructor() {
    effect(() => {
      if (this.open()) {
        untracked(() => this.reset());
      }
    });

    toObservable(this.searchTerm)
      .pipe(
        map(term => term.trim()),
        debounceTime(300),
        distinctUntilChanged(),
        switchMap(term => {
          if (term.length < 2) {
            this.searching.set(false);
            return of<StudentMatchResponse[]>([]);
          }
          this.searching.set(true);
          return this.data.searchPeople(term).pipe(
            catchError(err => {
              this.notify.apiError(err, this.transloco.translate('students.create.searchError'));
              return of<StudentMatchResponse[]>([]);
            }),
          );
        }),
        takeUntilDestroyed(),
      )
      .subscribe(res => {
        this.results.set(res);
        this.searching.set(false);
      });
  }

  protected fullName(p: StudentMatchResponse): string {
    return [p.title, p.firstName, p.lastName].filter(Boolean).join(' ');
  }

  protected onClose(): void {
    if (this.f().submitting()) return;
    this.closed.emit();
  }

  protected pickPerson(person: StudentMatchResponse): void {
    if (person.isStudent) {
      if (person.existingStudentId) {
        this.openExisting.emit(person.existingStudentId);
      }
      return;
    }
    this.selectedPerson.set(person);
    this.step.set('attach');
  }

  protected backToSearch(): void {
    this.selectedPerson.set(null);
    this.step.set('search');
  }

  protected async attach(): Promise<void> {
    const person = this.selectedPerson();
    if (!person) return;
    await submit(this.f, async () => {
      try {
        const { id } = await firstValueFrom(
          this.data.createForPerson({ personId: person.personId }),
        );
        this.notify.success(this.transloco.translate('students.createdToast'));
        this.created.emit(id);
      } catch (err) {
        this.notify.apiError(err, this.transloco.translate('students.createError'));
      }
    });
  }

  protected async save(): Promise<void> {
    await submit(this.f, async () => {
      const m = this.model();
      const payload: StudentBasicDetailsUpsertRequest = {
        title: this.normalise(m.title),
        firstName: m.firstName.trim(),
        middleName: this.normalise(m.middleName),
        lastName: m.lastName.trim(),
        preferredFirstName: this.normalise(m.preferredFirstName),
        preferredLastName: this.normalise(m.preferredLastName),
        photoId: null,
        gender: m.gender.trim(),
        dob: m.dob?.toISOString() ?? null,
        deceased: null,
      };
      try {
        const { id } = await firstValueFrom(this.data.create(payload));
        this.notify.success(this.transloco.translate('students.createdToast'));
        this.created.emit(id);
      } catch (err) {
        this.notify.apiError(err, this.transloco.translate('students.createError'));
      }
    });
  }

  private reset(): void {
    this.step.set('search');
    this.searchTerm.set('');
    this.results.set([]);
    this.searching.set(false);
    this.selectedPerson.set(null);

    this.model.set({
      title: '',
      firstName: '',
      middleName: '',
      lastName: '',
      preferredFirstName: '',
      preferredLastName: '',
      gender: '',
      dob: null,
    });
    this.f().reset();
  }

  private normalise(value: string | null | undefined): string | null {
    if (value == null) return null;
    const trimmed = value.trim();
    return trimmed.length === 0 ? null : trimmed;
  }
}
