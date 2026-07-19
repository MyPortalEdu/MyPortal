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
import { StaffMembersDataService } from '../../../../../shared/services/staff-members-data.service';
import { StaffBasicDetailsUpsertRequest } from '../../../../../shared/types/staff-basic-details';
import { PersonMatchResponse } from '../../../../../shared/types/person-match';

@Component({
  selector: 'mp-staff-member-create-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    FormsModule,
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
  protected readonly saving = signal(false);

  protected readonly searchTerm = signal('');
  protected readonly results = signal<PersonMatchResponse[]>([]);
  protected readonly searching = signal(false);

  protected readonly selectedPerson = signal<PersonMatchResponse | null>(null);

  protected readonly title = signal<string | null>(null);
  protected readonly firstName = signal('');
  protected readonly middleName = signal<string | null>(null);
  protected readonly lastName = signal('');
  protected readonly preferredFirstName = signal<string | null>(null);
  protected readonly preferredLastName = signal<string | null>(null);
  protected readonly gender = signal('');
  protected readonly dob = signal<Date | null>(null);

  protected readonly code = signal('');

  protected readonly newPersonValid = computed(
    () =>
      this.firstName().trim().length > 0 &&
      this.lastName().trim().length > 0 &&
      this.gender().trim().length > 0 &&
      this.code().trim().length > 0,
  );

  protected readonly attachValid = computed(() => this.code().trim().length > 0);

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
    if (this.saving()) return;
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
    this.code.set('');
    this.step.set('attach');
  }

  protected backToSearch(): void {
    this.selectedPerson.set(null);
    this.code.set('');
    this.step.set('search');
  }

  protected async attach(): Promise<void> {
    const person = this.selectedPerson();
    if (!person || !this.attachValid() || this.saving()) return;
    this.saving.set(true);

    try {
      const { id } = await firstValueFrom(
        this.data.createForPerson({ personId: person.personId, code: this.code().trim() }),
      );
      this.notify.success(this.transloco.translate('staff-members.createdToast'));
      this.created.emit(id);
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('staff-members.createError'));
    } finally {
      this.saving.set(false);
    }
  }

  protected async save(): Promise<void> {
    if (!this.newPersonValid() || this.saving()) return;
    this.saving.set(true);

    const payload: StaffBasicDetailsUpsertRequest = {
      title: this.normalise(this.title()),
      firstName: this.firstName().trim(),
      middleName: this.normalise(this.middleName()),
      lastName: this.lastName().trim(),
      preferredFirstName: this.normalise(this.preferredFirstName()),
      preferredLastName: this.normalise(this.preferredLastName()),
      photoId: null,
      gender: this.gender().trim(),
      dob: this.dob()?.toISOString() ?? null,
      deceased: null,
      code: this.code().trim(),
    };

    try {
      const { id } = await firstValueFrom(this.data.create(payload));
      this.notify.success(this.transloco.translate('staff-members.createdToast'));
      this.created.emit(id);
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('staff-members.createError'));
    } finally {
      this.saving.set(false);
    }
  }

  private reset(): void {
    this.step.set('search');
    this.searchTerm.set('');
    this.results.set([]);
    this.searching.set(false);
    this.selectedPerson.set(null);

    this.title.set(null);
    this.firstName.set('');
    this.middleName.set(null);
    this.lastName.set('');
    this.preferredFirstName.set(null);
    this.preferredLastName.set(null);
    this.gender.set('');
    this.dob.set(null);
    this.code.set('');
  }

  private normalise(value: string | null | undefined): string | null {
    if (value == null) return null;
    const trimmed = value.trim();
    return trimmed.length === 0 ? null : trimmed;
  }
}
