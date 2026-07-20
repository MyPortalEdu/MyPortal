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
import { FormField, applyWhen, form, required, submit, validate, validateHttp } from '@angular/forms/signals';
import { DatePipe } from '@angular/common';
import { MpDatePicker, MpDialog, MpDialogFooter, MpButton, MpFormField, MpInput, MpSpinner } from '@myportal/ui';
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
import { StaffMembersDataService } from '../../../../../shared/services/staff-members-data.service';
import { StaffBasicDetailsUpsertRequest } from '../../../../../shared/types/staff-basic-details';
import { PersonMatchResponse } from '../../../../../shared/types/person-match';

interface CreateModel {
  code: string;
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
  selector: 'mp-staff-member-create-dialog',
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
    MpFormField,
    MpInput,
    GenderSelect,
    MpSpinner,
    TranslocoDirective,
  ],
  providers: [provideTranslocoScope('staff-members')],
  templateUrl: './staff-member-create-dialog.html',
})
export class StaffMemberCreateDialog {
  private readonly data = inject(StaffMembersDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);

  readonly open = input.required<boolean>();
  readonly closed = output<void>();
  readonly created = output<string>();
  readonly openExisting = output<string>();

  protected readonly step = signal<'search' | 'attach'>('search');

  protected readonly searchTerm = signal('');
  protected readonly results = signal<PersonMatchResponse[]>([]);
  protected readonly searching = signal(false);

  protected readonly selectedPerson = signal<PersonMatchResponse | null>(null);

  protected readonly model = signal<CreateModel>({
    code: '',
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
    required(path.code);
    validate(path.code, ({ value }) =>
      value().trim().length ? undefined : { kind: 'blank', message: 'common.validation.required' },
    );
    validateHttp(path.code, {
      request: ({ value }) =>
        `/api/v1/staffmembers/code-available?code=${encodeURIComponent(value().trim())}`,
      onSuccess: (result: unknown) =>
        (result as { available: boolean }).available
          ? undefined
          : { kind: 'taken', message: 'staff-members.create.codeTaken' },
      onError: () => undefined,
      when: ({ value }) => value().trim().length > 0,
      debounce: 400,
    });
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
            return of<PersonMatchResponse[]>([]);
          }
          this.searching.set(true);
          return this.data.searchPeople(term).pipe(
            catchError(err => {
              this.notify.apiError(err, this.transloco.translate('staff-members.create.searchError'));
              return of<PersonMatchResponse[]>([]);
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

  protected fullName(p: PersonMatchResponse): string {
    return [p.title, p.firstName, p.lastName].filter(Boolean).join(' ');
  }

  protected onClose(): void {
    if (this.f().submitting()) return;
    this.closed.emit();
  }

  protected pickPerson(person: PersonMatchResponse): void {
    if (person.isStaffMember) {
      if (person.existingStaffMemberId) {
        this.openExisting.emit(person.existingStaffMemberId);
      }
      return;
    }
    this.selectedPerson.set(person);
    this.model.update(m => ({ ...m, code: '' }));
    this.step.set('attach');
  }

  protected backToSearch(): void {
    this.selectedPerson.set(null);
    this.model.update(m => ({ ...m, code: '' }));
    this.step.set('search');
  }

  protected async attach(): Promise<void> {
    const person = this.selectedPerson();
    if (!person) return;
    await submit(this.f, async () => {
      try {
        const { id } = await firstValueFrom(
          this.data.createForPerson({ personId: person.personId, code: this.model().code.trim() }),
        );
        this.notify.success(this.transloco.translate('staff-members.createdToast'));
        this.created.emit(id);
      } catch (err) {
        this.notify.apiError(err, this.transloco.translate('staff-members.createError'));
      }
    });
  }

  protected async save(): Promise<void> {
    await submit(this.f, async () => {
      const m = this.model();
      const payload: StaffBasicDetailsUpsertRequest = {
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
        code: m.code.trim(),
      };
      try {
        const { id } = await firstValueFrom(this.data.create(payload));
        this.notify.success(this.transloco.translate('staff-members.createdToast'));
        this.created.emit(id);
      } catch (err) {
        this.notify.apiError(err, this.transloco.translate('staff-members.createError'));
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
      code: '',
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
