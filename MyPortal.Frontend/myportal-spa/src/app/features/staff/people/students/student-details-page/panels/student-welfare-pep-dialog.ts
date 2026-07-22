import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  input,
  output,
  signal,
  untracked,
} from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { FormField, form, submit, validate } from '@angular/forms/signals';
import { DatePipe } from '@angular/common';
import {
  MpButton,
  MpDatePicker,
  MpDialog,
  MpDialogFooter,
  MpInput,
  MpSpinner,
  MpTextarea,
} from '@myportal/ui';
import {
  catchError,
  debounceTime,
  distinctUntilChanged,
  map,
  of,
  switchMap,
} from 'rxjs';
import {
  TranslocoDirective,
  TranslocoService,
  provideTranslocoScope,
} from '@jsverse/transloco';

import { NotificationService } from '../../../../../../core/services/notification.service';
import { Field } from '../../../../../../shared/components/field/field';
import { ContactsDataService } from '../../../../../../shared/services/contacts-data.service';
import { ContactMatchResponse } from '../../../../../../shared/types/contact-match';
import { PepResponse, PepUpsertRequest } from '../../../../../../shared/types/student-welfare';

interface PepModel {
  startDate: Date | null;
  endDate: Date | null;
  comment: string;
}

interface Contributor {
  personId: string;
  personName: string;
}

@Component({
  selector: 'mp-student-welfare-pep-dialog',
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
    MpSpinner,
    MpTextarea,
    Field,
    TranslocoDirective,
  ],
  providers: [provideTranslocoScope('students')],
  templateUrl: './student-welfare-pep-dialog.html',
})
export class StudentWelfarePepDialog {
  private readonly contacts = inject(ContactsDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);

  readonly open = input.required<boolean>();
  readonly editTarget = input<PepResponse | null>(null);
  readonly saving = input(false);

  readonly closed = output<void>();
  readonly save = output<PepUpsertRequest>();

  protected readonly isEdit = computed(() => !!this.editTarget());

  protected readonly searchTerm = signal('');
  protected readonly results = signal<ContactMatchResponse[]>([]);
  protected readonly searching = signal(false);
  protected readonly contributors = signal<Contributor[]>([]);

  protected readonly model = signal<PepModel>(this.empty());
  protected readonly f = form(this.model, path => {
    validate(path.startDate, ({ value }) => (value() ? undefined : { kind: 'required' }));
    validate(path.endDate, ({ value, valueOf }) => {
      const start = valueOf(path.startDate);
      const end = value();
      return start && end && end.getTime() < start.getTime()
        ? { kind: 'range', message: 'students.welfare.peps.endBeforeStart' }
        : undefined;
    });
  });

  constructor() {
    effect(() => {
      if (this.open()) untracked(() => this.reset());
    });

    toObservable(this.searchTerm)
      .pipe(
        map(term => term.trim()),
        debounceTime(300),
        distinctUntilChanged(),
        switchMap(term => {
          if (term.length < 2) {
            this.searching.set(false);
            return of<ContactMatchResponse[]>([]);
          }
          this.searching.set(true);
          return this.contacts.searchPeople(term).pipe(
            catchError(err => {
              this.notify.apiError(err, this.transloco.translate('students.welfare.peps.searchError'));
              return of<ContactMatchResponse[]>([]);
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

  protected fullName(p: ContactMatchResponse): string {
    return [p.title, p.firstName, p.lastName].filter(Boolean).join(' ');
  }

  protected addContributor(p: ContactMatchResponse): void {
    if (this.contributors().some(c => c.personId === p.personId)) return;
    this.contributors.update(list => [...list, { personId: p.personId, personName: this.fullName(p) }]);
    this.searchTerm.set('');
    this.results.set([]);
  }

  protected removeContributor(personId: string): void {
    this.contributors.update(list => list.filter(c => c.personId !== personId));
  }

  protected onClose(): void {
    if (this.saving()) return;
    this.closed.emit();
  }

  protected async submit(): Promise<void> {
    await submit(this.f, async () => {
      const m = this.model();
      this.save.emit({
        id: this.editTarget()?.id ?? null,
        startDate: (m.startDate as Date).toISOString(),
        endDate: m.endDate?.toISOString() ?? null,
        comment: m.comment.trim() || null,
        contributorPersonIds: this.contributors().map(c => c.personId),
      });
    });
  }

  private reset(): void {
    this.searchTerm.set('');
    this.results.set([]);
    this.searching.set(false);
    const t = this.editTarget();
    this.contributors.set(
      t ? t.contributors.map(c => ({ personId: c.personId, personName: c.personName })) : [],
    );
    this.model.set(
      t
        ? {
            startDate: new Date(t.startDate),
            endDate: t.endDate ? new Date(t.endDate) : null,
            comment: t.comment ?? '',
          }
        : this.empty(),
    );
    this.f().reset();
  }

  private empty(): PepModel {
    return { startDate: null, endDate: null, comment: '' };
  }
}
