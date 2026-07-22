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
import { MpButton, MpCheckbox, MpDialog, MpDialogFooter, MpInput, MpSelect, MpSpinner } from '@myportal/ui';
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

import { NotificationService } from '../../../../../../core/services/notification.service';
import { Field } from '../../../../../../shared/components/field/field';
import { StudentsDataService } from '../../../../../../shared/services/students-data.service';
import { ContactsDataService } from '../../../../../../shared/services/contacts-data.service';
import { LookupResponse } from '../../../../../../shared/types/lookup';
import { ContactMatchResponse } from '../../../../../../shared/types/contact-match';
import {
  StudentContactRelationshipResponse,
  StudentContactRelationshipUpsertRequest,
} from '../../../../../../shared/types/student-family';
import { ContactCreateDialog } from '../../../contacts/contact-create-dialog/contact-create-dialog';

interface RelationshipModel {
  relationshipTypeId: string | null;
  hasCorrespondence: boolean;
  hasParentalResponsibility: boolean;
  hasPupilReport: boolean;
  hasCourtOrder: boolean;
  contactOrder: number | null;
}

@Component({
  selector: 'mp-student-family-relationship-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    FormsModule,
    FormField,
    DatePipe,
    MpButton,
    MpCheckbox,
    MpDialog,
    MpDialogFooter,
    MpInput,
    MpSelect,
    MpSpinner,
    Field,
    ContactCreateDialog,
    TranslocoDirective,
  ],
  providers: [provideTranslocoScope('students')],
  templateUrl: './student-family-relationship-dialog.html',
})
export class StudentFamilyRelationshipDialog {
  private readonly students = inject(StudentsDataService);
  private readonly contacts = inject(ContactsDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);

  readonly open = input.required<boolean>();
  readonly studentId = input.required<string>();
  readonly relationshipTypes = input<LookupResponse[]>([]);
  readonly editTarget = input<StudentContactRelationshipResponse | null>(null);

  readonly closed = output<void>();
  readonly saved = output<void>();

  protected readonly isEdit = computed(() => !!this.editTarget());

  protected readonly step = signal<'contact' | 'relationship'>('contact');

  protected readonly searchTerm = signal('');
  protected readonly results = signal<ContactMatchResponse[]>([]);
  protected readonly searching = signal(false);
  protected readonly resolving = signal(false);
  protected readonly childCreateOpen = signal(false);

  protected readonly selectedContactId = signal<string | null>(null);
  protected readonly selectedContactName = signal('');

  protected readonly model = signal<RelationshipModel>({
    relationshipTypeId: null,
    hasCorrespondence: false,
    hasParentalResponsibility: false,
    hasPupilReport: false,
    hasCourtOrder: false,
    contactOrder: 0,
  });
  protected readonly f = form(this.model, path => {
    validate(path.relationshipTypeId, ({ value }) =>
      value() ? undefined : { kind: 'required' },
    );
    validate(path.contactOrder, ({ value }) => {
      const v = value();
      return v == null || v >= 0 ? undefined : { kind: 'min', min: 0 };
    });
  });

  protected readonly submitting = computed(() => this.f().submitting());

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
            return of<ContactMatchResponse[]>([]);
          }
          this.searching.set(true);
          return this.contacts.searchPeople(term).pipe(
            catchError(err => {
              this.notify.apiError(err, this.transloco.translate('students.family.searchError'));
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

  protected onClose(): void {
    if (this.submitting() || this.resolving()) return;
    this.closed.emit();
  }

  protected async pickPerson(person: ContactMatchResponse): Promise<void> {
    if (this.resolving()) return;
    if (person.isContact && person.existingContactId) {
      this.useContact(person.existingContactId, this.fullName(person));
      return;
    }
    this.resolving.set(true);
    try {
      const { id } = await firstValueFrom(
        this.contacts.createForPerson({ personId: person.personId }),
      );
      this.useContact(id, this.fullName(person));
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('students.family.contactResolveError'));
    } finally {
      this.resolving.set(false);
    }
  }

  protected openCreateContact(): void {
    this.childCreateOpen.set(true);
  }

  protected onChildClosed(): void {
    this.childCreateOpen.set(false);
  }

  protected onChildCreated(contactId: string): void {
    this.childCreateOpen.set(false);
    this.adoptContact(contactId);
  }

  protected onChildOpenExisting(contactId: string): void {
    this.childCreateOpen.set(false);
    this.adoptContact(contactId);
  }

  private adoptContact(contactId: string): void {
    this.resolving.set(true);
    this.contacts.getHeader(contactId).subscribe({
      next: header => {
        this.resolving.set(false);
        this.useContact(contactId, header.displayName);
      },
      error: () => {
        this.resolving.set(false);
        this.useContact(contactId, '');
      },
    });
  }

  private useContact(contactId: string, name: string): void {
    this.selectedContactId.set(contactId);
    this.selectedContactName.set(name);
    this.step.set('relationship');
  }

  protected changeContact(): void {
    if (this.isEdit()) return;
    this.selectedContactId.set(null);
    this.selectedContactName.set('');
    this.searchTerm.set('');
    this.results.set([]);
    this.step.set('contact');
  }

  protected async save(): Promise<void> {
    const contactId = this.selectedContactId();
    if (!contactId) return;
    await submit(this.f, async () => {
      const m = this.model();
      const payload: StudentContactRelationshipUpsertRequest = {
        contactId,
        relationshipTypeId: m.relationshipTypeId as string,
        hasCorrespondence: m.hasCorrespondence,
        hasParentalResponsibility: m.hasParentalResponsibility,
        hasPupilReport: m.hasPupilReport,
        hasCourtOrder: m.hasCourtOrder,
        contactOrder: m.contactOrder ?? 0,
      };
      const target = this.editTarget();
      const request = target
        ? this.students.updateContactRelationship(this.studentId(), target.id, payload)
        : this.students.addContactRelationship(this.studentId(), payload);
      try {
        await firstValueFrom(request);
      } catch (err) {
        this.notify.apiError(err, this.transloco.translate('students.family.saveError'));
        return;
      }
      this.notify.success(this.transloco.translate('students.family.savedToast'));
      this.saved.emit();
    });
  }

  private reset(): void {
    this.searchTerm.set('');
    this.results.set([]);
    this.searching.set(false);
    this.resolving.set(false);
    this.childCreateOpen.set(false);

    const target = this.editTarget();
    if (target) {
      this.selectedContactId.set(target.contactId);
      this.selectedContactName.set(target.contactName);
      this.model.set({
        relationshipTypeId: target.relationshipTypeId,
        hasCorrespondence: target.hasCorrespondence,
        hasParentalResponsibility: target.hasParentalResponsibility,
        hasPupilReport: target.hasPupilReport,
        hasCourtOrder: target.hasCourtOrder,
        contactOrder: target.contactOrder,
      });
      this.step.set('relationship');
    } else {
      this.selectedContactId.set(null);
      this.selectedContactName.set('');
      this.model.set({
        relationshipTypeId: null,
        hasCorrespondence: false,
        hasParentalResponsibility: false,
        hasPupilReport: false,
        hasCourtOrder: false,
        contactOrder: 0,
      });
      this.step.set('contact');
    }
    this.f().reset();
  }
}
