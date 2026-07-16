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
import { ProgressSpinner } from 'primeng/progressspinner';
import { firstValueFrom } from 'rxjs';
import {
  TranslocoDirective,
  TranslocoService,
  provideTranslocoScope,
} from '@jsverse/transloco';

import { NotificationService } from '../../../../../../core/services/notification.service';
import { Permissions } from '../../../../../../core/constants/permissions';
import { StaffMembersDataService } from '../../../../../../shared/services/staff-members-data.service';
import { StaffRelationship } from '../../../../../../shared/types/staff-member-header';
import { PersonEmails } from '../../../../../../shared/components/contact/person-emails/person-emails';
import { PersonPhones } from '../../../../../../shared/components/contact/person-phones/person-phones';
import { PersonAddresses } from '../../../../../../shared/components/contact/person-addresses/person-addresses';
import {
  PersonEmailUpsertItem,
  PersonPhoneUpsertItem,
  StaffContactDetailsResponse,
  StaffContactDetailsUpsertRequest,
} from '../../../../../../shared/types/staff-contact-details';
import { StaffAreaPanel } from './staff-area-panel';

/**
 * Contact Details area: emails, phones and addresses. Contact methods live under the BasicDetails
 * permission domain, so the edit gate mirrors basic details (HR All or line-manager Managed — no
 * self-edit). Addresses are managed by their own component (search-before-add, warn-and-choose).
 * Self-loads on mount.
 */
@Component({
  selector: 'mp-staff-contact-panel',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ProgressSpinner, PersonEmails, PersonPhones, PersonAddresses, TranslocoDirective],
  providers: [
    provideTranslocoScope('staff-members'),
    { provide: StaffAreaPanel, useExisting: forwardRef(() => StaffContactPanel) },
  ],
  templateUrl: './staff-contact-panel.html',
})
export class StaffContactPanel extends StaffAreaPanel implements OnInit {
  private readonly data = inject(StaffMembersDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);

  readonly staffMemberId = input.required<string>();
  readonly permissions = input.required<Set<string>>();
  readonly relationship = input<StaffRelationship>();

  protected readonly loading = signal(false);
  override readonly saving = signal(false);
  override readonly editing = signal(false);

  protected readonly contact = signal<StaffContactDetailsResponse | null>(null);
  protected readonly emails = signal<PersonEmailUpsertItem[]>([]);
  protected readonly phones = signal<PersonPhoneUpsertItem[]>([]);
  protected readonly emailTypes = computed(() => this.contact()?.emailTypes ?? []);
  protected readonly phoneTypes = computed(() => this.contact()?.phoneTypes ?? []);
  private readonly snapshot = signal<string>('');

  // Contact methods live under the BasicDetails permission domain: HR (All) or the line manager
  // (Managed) — no self-edit. Also drives the addresses component's edit gate.
  override readonly canEdit = computed(() => {
    const perms = this.permissions();
    const rel = this.relationship();
    if (perms.has(Permissions.Staff.EditAllStaffBasicDetails)) return true;
    if (rel === 'LineManaged' && perms.has(Permissions.Staff.EditManagedStaffBasicDetails)) return true;
    return false;
  });

  // Every email/phone row needs a type and a non-empty value.
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
    this.data.getContactDetails(this.staffMemberId()).subscribe({
      next: row => {
        this.contact.set(row);
        this.apply(row);
        this.loading.set(false);
      },
      error: err => {
        this.loading.set(false);
        this.notify.apiError(err, this.transloco.translate('staff-members.loadContactError'));
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

    const payload: StaffContactDetailsUpsertRequest = {
      emails: this.emails(),
      phones: this.phones(),
    };

    try {
      await firstValueFrom(this.data.updateContactDetails(this.staffMemberId(), payload));
      this.notify.success(this.transloco.translate('staff-members.savedContactToast'));
      this.editing.set(false);
      this.load();
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('staff-members.saveContactError'));
    } finally {
      this.saving.set(false);
    }
  }

  private apply(row: StaffContactDetailsResponse | null): void {
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
