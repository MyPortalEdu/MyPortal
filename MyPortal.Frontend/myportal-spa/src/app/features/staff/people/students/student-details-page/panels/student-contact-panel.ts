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
import { firstValueFrom } from 'rxjs';
import {
  TranslocoDirective,
  TranslocoService,
  provideTranslocoScope,
} from '@jsverse/transloco';

import { NotificationService } from '../../../../../../core/services/notification.service';
import { Permissions } from '../../../../../../core/constants/permissions';
import { StudentsDataService } from '../../../../../../shared/services/students-data.service';
import { Loading } from '../../../../../../shared/components/loading/loading';
import { SectionHeader } from '../../../../../../shared/components/section-header/section-header';
import { PersonEmails } from '../../../../../../shared/components/contact/person-emails/person-emails';
import { PersonPhones } from '../../../../../../shared/components/contact/person-phones/person-phones';
import { PersonAddresses } from '../../../../../../shared/components/contact/person-addresses/person-addresses';
import { PersonAddressDataSource } from '../../../../../../shared/components/contact/person-address-data-source';
import {
  PersonEmailUpsertItem,
  PersonPhoneUpsertItem,
  PersonContactDetailsResponse,
  PersonContactDetailsUpsertRequest,
} from '../../../../../../shared/types/person-contact-details';
import { StudentAreaPanel } from './student-area-panel';

@Component({
  selector: 'mp-student-contact-panel',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Loading, SectionHeader, PersonEmails, PersonPhones, PersonAddresses, TranslocoDirective],
  providers: [
    provideTranslocoScope('students'),
    { provide: StudentAreaPanel, useExisting: forwardRef(() => StudentContactPanel) },
    { provide: PersonAddressDataSource, useExisting: StudentsDataService },
  ],
  templateUrl: './student-contact-panel.html',
})
export class StudentContactPanel extends StudentAreaPanel implements OnInit {
  private readonly data = inject(StudentsDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);

  readonly studentId = input.required<string>();
  readonly permissions = input.required<Set<string>>();

  protected readonly loading = signal(false);
  override readonly saving = signal(false);
  override readonly editing = signal(false);

  protected readonly contact = signal<PersonContactDetailsResponse | null>(null);
  protected readonly emails = signal<PersonEmailUpsertItem[]>([]);
  protected readonly phones = signal<PersonPhoneUpsertItem[]>([]);
  protected readonly emailTypes = computed(() => this.contact()?.emailTypes ?? []);
  protected readonly phoneTypes = computed(() => this.contact()?.phoneTypes ?? []);
  private readonly snapshot = signal<string>('');

  override readonly canEdit = computed(() =>
    this.permissions().has(Permissions.Student.EditStudentBasicDetails),
  );

  override readonly valid = computed(
    () =>
      this.emails().every(e => !!e.typeId && e.address.trim().length > 0) &&
      this.phones().every(p => !!p.typeId && p.number.trim().length > 0),
  );

  private readonly form = computed(() =>
    JSON.stringify({ emails: this.emails(), phones: this.phones() }),
  );

  override readonly dirty = computed(
    () => this.contact() != null && this.snapshot() !== this.form(),
  );

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.data.getContactDetails(this.studentId()).subscribe({
      next: row => {
        this.contact.set(row);
        this.apply(row);
        this.loading.set(false);
      },
      error: err => {
        this.loading.set(false);
        this.notify.apiError(err, this.transloco.translate('students.contact.loadError'));
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
    if (!this.canEdit() || !this.valid() || this.saving()) return;
    this.saving.set(true);

    const payload: PersonContactDetailsUpsertRequest = {
      emails: this.emails(),
      phones: this.phones(),
    };

    try {
      await firstValueFrom(this.data.updateContactDetails(this.studentId(), payload));
      this.notify.success(this.transloco.translate('students.contact.savedToast'));
      this.editing.set(false);
      this.load();
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('students.contact.saveError'));
    } finally {
      this.saving.set(false);
    }
  }

  private apply(row: PersonContactDetailsResponse | null): void {
    this.emails.set(
      (row?.emails ?? []).map(e => ({
        id: e.id,
        typeId: e.typeId,
        address: e.address,
        isMain: e.isMain,
        notes: e.notes ?? null,
      })),
    );
    this.phones.set(
      (row?.phones ?? []).map(p => ({
        id: p.id,
        typeId: p.typeId,
        number: p.number,
        isMain: p.isMain,
      })),
    );
    this.snapshot.set(this.form());
  }
}
