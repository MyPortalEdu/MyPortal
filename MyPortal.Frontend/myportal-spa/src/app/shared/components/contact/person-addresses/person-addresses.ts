import { ChangeDetectionStrategy, Component, OnInit, inject, input, signal } from '@angular/core';
import { Button } from 'primeng/button';
import { ProgressSpinner } from 'primeng/progressspinner';
import { Tag } from 'primeng/tag';
import { firstValueFrom } from 'rxjs';
import { TranslocoDirective, TranslocoService } from '@jsverse/transloco';

import { StaffMembersDataService } from '../../../services/staff-members-data.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { ConfirmationDialog } from '../../../../core/services/confirmation.service';
import { LookupResponse } from '../../../types/lookup';
import { PersonAddressResponse } from '../../../types/staff-address';
import { AddressFormDialog } from '../address-form-dialog/address-form-dialog';
import { CopyButton } from '../../copy-button/copy-button';

/**
 * Addresses section of the Contact Details panel. Self-contained: loads the staff member's
 * addresses and commits each add/edit/remove immediately (shared-address semantics don't stage
 * cleanly behind a panel-level Save). Hosts the search-before-add / warn-and-choose dialog.
 *
 * Staff-specific today (uses StaffMembersDataService). Generalise via a data-access input when
 * the student/parent portals need it.
 */
@Component({
  selector: 'mp-person-addresses',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Button, ProgressSpinner, Tag, TranslocoDirective, AddressFormDialog, CopyButton],
  templateUrl: './person-addresses.html',
})
export class PersonAddresses implements OnInit {
  private readonly data = inject(StaffMembersDataService);
  private readonly notify = inject(NotificationService);
  private readonly confirm = inject(ConfirmationDialog);
  private readonly transloco = inject(TranslocoService);

  readonly staffMemberId = input.required<string>();
  readonly canEdit = input<boolean>(false);

  protected readonly addresses = signal<PersonAddressResponse[]>([]);
  protected readonly addressTypes = signal<LookupResponse[]>([]);
  protected readonly loading = signal(false);
  protected readonly dialogOpen = signal(false);
  protected readonly editTarget = signal<PersonAddressResponse | null>(null);

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.data.getAddresses(this.staffMemberId()).subscribe({
      next: row => {
        this.addresses.set(row.addresses);
        this.addressTypes.set(row.addressTypes);
        this.loading.set(false);
      },
      error: err => {
        this.loading.set(false);
        this.notify.apiError(err, this.transloco.translate('common.contact.address.loadError'));
      },
    });
  }

  protected typeName(typeId: string): string {
    return this.addressTypes().find(t => t.id === typeId)?.description ?? '';
  }

  protected formatAddress(a: PersonAddressResponse): string {
    return [
      a.buildingName,
      [a.buildingNumber, a.street].filter(Boolean).join(' '),
      a.town,
      a.postcode,
    ]
      .filter(p => p && p.trim().length > 0)
      .join(', ');
  }

  // Google Maps search link for the formatted address (opens in a new tab).
  protected mapHref(a: PersonAddressResponse): string {
    return `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(this.formatAddress(a))}`;
  }

  protected openAdd(): void {
    this.editTarget.set(null);
    this.dialogOpen.set(true);
  }

  protected openEdit(address: PersonAddressResponse): void {
    this.editTarget.set(address);
    this.dialogOpen.set(true);
  }

  protected onDialogClosed(): void {
    this.dialogOpen.set(false);
  }

  protected onSaved(): void {
    this.dialogOpen.set(false);
    this.load();
  }

  protected async remove(address: PersonAddressResponse): Promise<void> {
    const ok = await this.confirm.confirm({
      header: this.transloco.translate('common.contact.address.removeHeader'),
      message: this.transloco.translate('common.contact.address.removeConfirm'),
      acceptLabel: this.transloco.translate('common.delete'),
      acceptSeverity: 'danger',
    });

    if (!ok) return;

    try {
      await firstValueFrom(this.data.removeAddress(this.staffMemberId(), address.addressPersonId));
      this.notify.success(this.transloco.translate('common.contact.address.removedToast'));
      this.load();
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('common.contact.address.removeError'));
    }
  }
}
