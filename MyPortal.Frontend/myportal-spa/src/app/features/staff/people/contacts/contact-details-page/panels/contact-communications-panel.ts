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
import { applyEach, form, submit, validate } from '@angular/forms/signals';
import { firstValueFrom } from 'rxjs';
import {
  TranslocoDirective,
  TranslocoService,
  provideTranslocoScope,
} from '@jsverse/transloco';

import { NotificationService } from '../../../../../../core/services/notification.service';
import { Permissions } from '../../../../../../core/constants/permissions';
import { ContactsDataService } from '../../../../../../shared/services/contacts-data.service';
import { Loading } from '../../../../../../shared/components/loading/loading';
import { SectionHeader } from '../../../../../../shared/components/section-header/section-header';
import { PersonEmails } from '../../../../../../shared/components/contact/person-emails/person-emails';
import { PersonPhones } from '../../../../../../shared/components/contact/person-phones/person-phones';
import { PersonAddresses } from '../../../../../../shared/components/contact/person-addresses/person-addresses';
import { PersonAddressDataSource } from '../../../../../../shared/components/contact/person-address-data-source';
import {
  PersonEmailFormRow,
  PersonPhoneFormRow,
  PersonContactDetailsResponse,
  PersonContactDetailsUpsertRequest,
} from '../../../../../../shared/types/person-contact-details';
import { ContactAreaPanel } from './contact-area-panel';

@Component({
  selector: 'mp-contact-communications-panel',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Loading, SectionHeader, PersonEmails, PersonPhones, PersonAddresses, TranslocoDirective],
  providers: [
    provideTranslocoScope('contacts'),
    { provide: ContactAreaPanel, useExisting: forwardRef(() => ContactCommunicationsPanel) },
    { provide: PersonAddressDataSource, useExisting: ContactsDataService },
  ],
  templateUrl: './contact-communications-panel.html',
})
export class ContactCommunicationsPanel extends ContactAreaPanel implements OnInit {
  private readonly data = inject(ContactsDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);

  readonly contactId = input.required<string>();
  readonly permissions = input.required<Set<string>>();

  protected readonly loading = signal(false);
  override readonly editing = signal(false);

  protected readonly contact = signal<PersonContactDetailsResponse | null>(null);
  protected readonly emailTypes = computed(() => this.contact()?.emailTypes ?? []);
  protected readonly phoneTypes = computed(() => this.contact()?.phoneTypes ?? []);

  protected readonly model = signal<{ emails: PersonEmailFormRow[]; phones: PersonPhoneFormRow[] }>({
    emails: [],
    phones: [],
  });
  protected readonly f = form(this.model, path => {
    applyEach(path.emails, item => {
      validate(item.typeId, ({ value }) => (value() ? undefined : { kind: 'required' }));
      validate(item.address, ({ value }) => (value().trim().length ? undefined : { kind: 'required' }));
    });
    applyEach(path.phones, item => {
      validate(item.typeId, ({ value }) => (value() ? undefined : { kind: 'required' }));
      validate(item.number, ({ value }) => (value().trim().length ? undefined : { kind: 'required' }));
    });
  });
  private readonly snapshot = signal<string>('');

  override readonly canEdit = computed(() =>
    this.permissions().has(Permissions.Contact.EditContactDetails),
  );

  override readonly saving = computed(() => this.f().submitting());
  override readonly valid = computed(() => this.f().valid());

  private readonly formState = computed(() => JSON.stringify(this.model()));

  override readonly dirty = computed(
    () => this.contact() != null && this.snapshot() !== this.formState(),
  );

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.data.getContactDetails(this.contactId()).subscribe({
      next: row => {
        this.contact.set(row);
        this.apply(row);
        this.loading.set(false);
      },
      error: err => {
        this.loading.set(false);
        this.notify.apiError(err, this.transloco.translate('contacts.communications.loadError'));
      },
    });
  }

  override startEdit(): void {
    this.editing.set(true);
  }

  override cancel(): void {
    this.apply(this.contact());
    this.editing.set(false);
  }

  override async save(): Promise<void> {
    if (!this.canEdit()) return;
    await submit(this.f, async () => {
      const payload: PersonContactDetailsUpsertRequest = {
        emails: this.model().emails.map(e => ({
          id: e.id,
          typeId: e.typeId,
          address: e.address.trim(),
          isMain: e.isMain,
          notes: e.notes.trim() ? e.notes.trim() : null,
        })),
        phones: this.model().phones.map(p => ({
          id: p.id,
          typeId: p.typeId,
          number: p.number.trim(),
          isMain: p.isMain,
        })),
      };
      try {
        await firstValueFrom(this.data.updateContactDetails(this.contactId(), payload));
      } catch (err) {
        this.notify.apiError(err, this.transloco.translate('contacts.communications.saveError'));
        return;
      }
      this.notify.success(this.transloco.translate('contacts.communications.savedToast'));
      this.editing.set(false);
      this.load();
    });
  }

  protected addEmail(): void {
    this.model.update(m => ({
      ...m,
      emails: [
        ...m.emails,
        { id: null, typeId: this.emailTypes()[0]?.id ?? '', address: '', isMain: m.emails.length === 0, notes: '' },
      ],
    }));
  }

  protected removeEmail(index: number): void {
    this.model.update(m => {
      const emails = m.emails.filter((_, i) => i !== index);
      if (emails.length && !emails.some(e => e.isMain)) emails[0] = { ...emails[0], isMain: true };
      return { ...m, emails };
    });
  }

  protected setMainEmail(index: number): void {
    this.model.update(m => ({
      ...m,
      emails: m.emails.map((e, i) => ({ ...e, isMain: i === index })),
    }));
  }

  protected addPhone(): void {
    this.model.update(m => ({
      ...m,
      phones: [
        ...m.phones,
        { id: null, typeId: this.phoneTypes()[0]?.id ?? '', number: '', isMain: m.phones.length === 0 },
      ],
    }));
  }

  protected removePhone(index: number): void {
    this.model.update(m => {
      const phones = m.phones.filter((_, i) => i !== index);
      if (phones.length && !phones.some(p => p.isMain)) phones[0] = { ...phones[0], isMain: true };
      return { ...m, phones };
    });
  }

  protected setMainPhone(index: number): void {
    this.model.update(m => ({
      ...m,
      phones: m.phones.map((p, i) => ({ ...p, isMain: i === index })),
    }));
  }

  private apply(row: PersonContactDetailsResponse | null): void {
    this.model.set({
      emails: (row?.emails ?? []).map(e => ({
        id: e.id,
        typeId: e.typeId,
        address: e.address,
        isMain: e.isMain,
        notes: e.notes ?? '',
      })),
      phones: (row?.phones ?? []).map(p => ({
        id: p.id,
        typeId: p.typeId,
        number: p.number,
        isMain: p.isMain,
      })),
    });
    this.f().reset();
    this.snapshot.set(this.formState());
  }
}
