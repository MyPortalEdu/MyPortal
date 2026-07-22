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
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import {
  catchError,
  debounceTime,
  distinctUntilChanged,
  firstValueFrom,
  map,
  of,
  switchMap,
} from 'rxjs';
import { MpButton, MpInput, MpSelect, MpSpinner, MpTextarea } from '@myportal/ui';
import {
  TranslocoDirective,
  TranslocoService,
  provideTranslocoScope,
} from '@jsverse/transloco';

import { NotificationService } from '../../../../../../core/services/notification.service';
import { Permissions } from '../../../../../../core/constants/permissions';
import { StaffMembersDataService } from '../../../../../../shared/services/staff-members-data.service';
import { Loading } from '../../../../../../shared/components/loading/loading';
import { EmptyState } from '../../../../../../shared/components/empty-state/empty-state';
import { SectionHeader } from '../../../../../../shared/components/section-header/section-header';
import { Field } from '../../../../../../shared/components/field/field';
import { Callout } from '../../../../../../shared/components/callout/callout';
import { PersonEmails } from '../../../../../../shared/components/contact/person-emails/person-emails';
import { PersonPhones } from '../../../../../../shared/components/contact/person-phones/person-phones';
import {
  PersonEmailUpsertItem,
  PersonPhoneUpsertItem,
} from '../../../../../../shared/types/staff-contact-details';
import { PersonMatchResponse } from '../../../../../../shared/types/person-match';
import {
  StaffNextOfKinAreaResponse,
  StaffNextOfKinAreaUpsertRequest,
} from '../../../../../../shared/types/staff-next-of-kin';
import { StaffAreaPanel } from './staff-area-panel';

interface ContactRow {
  id: string | null;
  personId: string | null;
  title: string;
  firstName: string;
  middleName: string;
  lastName: string;
  gender: string;
  relationshipTypeId: string | null;
  contactOrder: number;
  notes: string;
  phones: PersonPhoneUpsertItem[];
  emails: PersonEmailUpsertItem[];
}

@Component({
  selector: 'mp-staff-next-of-kin-panel',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    FormsModule,
    MpButton,
    MpInput,
    MpSelect,
    MpSpinner,
    MpTextarea,
    Loading,
    EmptyState,
    SectionHeader,
    Field,
    Callout,
    PersonEmails,
    PersonPhones,
    TranslocoDirective,
  ],
  providers: [
    provideTranslocoScope('staff-members'),
    { provide: StaffAreaPanel, useExisting: forwardRef(() => StaffNextOfKinPanel) },
  ],
  templateUrl: './staff-next-of-kin-panel.html',
})
export class StaffNextOfKinPanel extends StaffAreaPanel implements OnInit {
  private readonly data = inject(StaffMembersDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);

  readonly staffMemberId = input.required<string>();
  readonly permissions = input.required<Set<string>>();

  protected readonly loading = signal(false);
  override readonly saving = signal(false);
  override readonly editing = signal(false);

  protected readonly area = signal<StaffNextOfKinAreaResponse | null>(null);
  protected readonly contacts = signal<ContactRow[]>([]);
  private readonly snapshot = signal<string>('');

  protected readonly relationshipTypes = computed(() => this.area()?.relationshipTypes ?? []);
  protected readonly phoneTypes = computed(() => this.area()?.phoneTypes ?? []);
  protected readonly emailTypes = computed(() => this.area()?.emailTypes ?? []);

  protected readonly adding = signal(false);
  protected readonly searchTerm = signal('');
  protected readonly results = signal<PersonMatchResponse[]>([]);
  protected readonly searching = signal(false);

  override readonly canEdit = computed(() =>
    this.permissions().has(Permissions.Staff.EditAllStaffEmergencyContacts),
  );

  override readonly valid = computed(() =>
    this.contacts().every(
      c =>
        c.firstName.trim().length > 0 &&
        c.lastName.trim().length > 0 &&
        c.phones.every(p => !!p.typeId && p.number.trim().length > 0) &&
        c.emails.every(e => !!e.typeId && e.address.trim().length > 0),
    ),
  );

  private readonly form = computed(() => JSON.stringify(this.contacts()));

  override readonly dirty = computed(() => this.area() != null && this.snapshot() !== this.form());

  constructor() {
    super();
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
              this.notify.apiError(err, this.transloco.translate('staff-members.nextOfKin.searchError'));
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

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.data.getNextOfKin(this.staffMemberId()).subscribe({
      next: row => {
        this.area.set(row);
        this.apply(row);
        this.loading.set(false);
      },
      error: err => {
        this.loading.set(false);
        this.notify.apiError(err, this.transloco.translate('staff-members.loadNextOfKinError'));
      },
    });
  }

  override startEdit(): void {
    this.editing.set(true);
  }

  override cancel(): void {
    this.apply(this.area());
    this.closeSearch();
    this.editing.set(false);
  }

  override async save(): Promise<void> {
    if (!this.canEdit() || !this.valid() || this.saving() || !this.dirty()) return;
    this.saving.set(true);

    const payload: StaffNextOfKinAreaUpsertRequest = {
      contacts: this.contacts().map(c => ({
        id: c.id,
        personId: c.personId,
        title: this.normalise(c.title),
        firstName: c.firstName.trim(),
        middleName: this.normalise(c.middleName),
        lastName: c.lastName.trim(),
        gender: this.normalise(c.gender),
        relationshipTypeId: c.relationshipTypeId,
        contactOrder: c.contactOrder,
        notes: this.normalise(c.notes),
        phones: c.phones,
        emails: c.emails,
      })),
    };

    try {
      await firstValueFrom(this.data.updateNextOfKin(this.staffMemberId(), payload));
      this.notify.success(this.transloco.translate('staff-members.savedNextOfKinToast'));
      this.closeSearch();
      this.editing.set(false);
      this.load();
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('staff-members.saveNextOfKinError'));
    } finally {
      this.saving.set(false);
    }
  }

  private apply(row: StaffNextOfKinAreaResponse | null): void {
    this.contacts.set(
      [...(row?.contacts ?? [])]
        .sort((a, b) => a.contactOrder - b.contactOrder)
        .map(c => ({
          id: c.id,
          personId: c.personId,
          title: c.title ?? '',
          firstName: c.firstName,
          middleName: c.middleName ?? '',
          lastName: c.lastName,
          gender: '',
          relationshipTypeId: c.relationshipTypeId ?? null,
          contactOrder: c.contactOrder,
          notes: c.notes ?? '',
          phones: c.phones.map(p => ({
            id: p.id,
            typeId: p.typeId,
            number: p.number,
            isMain: p.isMain,
          })),
          emails: c.emails.map(e => ({
            id: e.id,
            typeId: e.typeId,
            address: e.address,
            isMain: e.isMain,
            notes: e.notes ?? null,
          })),
        })),
    );
    this.snapshot.set(this.form());
  }

  protected fullName(p: PersonMatchResponse): string {
    return [p.title, p.firstName, p.lastName].filter(Boolean).join(' ');
  }

  protected relationshipLabel(id: string | null): string {
    return this.lookupLabel(this.relationshipTypes(), id);
  }

  protected startAdd(): void {
    this.searchTerm.set('');
    this.results.set([]);
    this.searching.set(false);
    this.adding.set(true);
  }

  protected closeSearch(): void {
    this.adding.set(false);
    this.searchTerm.set('');
    this.results.set([]);
    this.searching.set(false);
  }

  private nextOrder(): number {
    const orders = this.contacts().map(c => c.contactOrder);
    return orders.length ? Math.max(...orders) + 1 : 1;
  }

  protected createFresh(): void {
    this.contacts.update(list => [...list, this.blankContact(this.nextOrder())]);
    this.closeSearch();
  }

  protected pickMatch(person: PersonMatchResponse): void {
    const row = this.blankContact(this.nextOrder());
    row.personId = person.personId;
    row.title = person.title ?? '';
    row.firstName = person.firstName;
    row.middleName = person.middleName ?? '';
    row.lastName = person.lastName;
    this.contacts.update(list => [...list, row]);
    this.closeSearch();
  }

  private blankContact(order: number): ContactRow {
    return {
      id: null,
      personId: null,
      title: '',
      firstName: '',
      middleName: '',
      lastName: '',
      gender: '',
      relationshipTypeId: null,
      contactOrder: order,
      notes: '',
      phones: [],
      emails: [],
    };
  }

  protected removeContact(index: number): void {
    this.contacts.update(list => list.filter((_, i) => i !== index));
  }

  protected patchContact(index: number, changes: Partial<ContactRow>): void {
    this.contacts.update(list => list.map((c, i) => (i === index ? { ...c, ...changes } : c)));
  }
}
