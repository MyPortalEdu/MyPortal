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
import { FormsModule } from '@angular/forms';
import { ProgressSpinner } from 'primeng/progressspinner';
import { MpButton, MpDialog, MpInput, MpSelect } from '@myportal/ui';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { of } from 'rxjs';
import { catchError, debounceTime, distinctUntilChanged, map, switchMap } from 'rxjs/operators';
import { firstValueFrom } from 'rxjs';
import { TranslocoDirective, TranslocoService } from '@jsverse/transloco';

import { StaffMembersDataService } from '../../../services/staff-members-data.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { ConfirmationDialog } from '../../../../core/services/confirmation.service';
import { LookupResponse } from '../../../types/lookup';
import {
  AddressEditMode,
  AddressMatchResponse,
  PersonAddressResponse,
  PersonAddressUpdateRequest,
  PersonAddressUpsertRequest,
} from '../../../types/staff-address';

interface AddressForm {
  buildingNumber: string;
  buildingName: string;
  apartment: string;
  street: string;
  district: string;
  town: string;
  county: string;
  postcode: string;
  country: string;
  typeId: string;
  isMain: boolean;
}

const EMPTY_FORM: AddressForm = {
  buildingNumber: '',
  buildingName: '',
  apartment: '',
  street: '',
  district: '',
  town: '',
  county: '',
  postcode: '',
  country: '',
  typeId: '',
  isMain: false,
};

/**
 * Add/edit dialog for a person's address. Add mode is a search-before-add flow (find an existing
 * shared address to link, or enter a new one). Edit mode pre-fills the form and, when the address
 * is shared, asks whether the change applies to everyone (FixInPlace) or forks a new address
 * (Moved). Self-contained: commits via the staff data service and emits `saved` on success.
 */
@Component({
  selector: 'mp-address-form-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule, MpButton, MpDialog, MpInput, ProgressSpinner, MpSelect, TranslocoDirective],
  templateUrl: './address-form-dialog.html',
})
export class AddressFormDialog {
  private readonly data = inject(StaffMembersDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);
  private readonly confirmDialog = inject(ConfirmationDialog);

  readonly open = input.required<boolean>();
  readonly staffMemberId = input.required<string>();
  readonly addressTypes = input<LookupResponse[]>([]);
  readonly editTarget = input<PersonAddressResponse | null>(null);

  readonly closed = output<void>();
  readonly saved = output<void>();

  protected readonly step = signal<'search' | 'link' | 'form'>('search');
  protected readonly searchTerm = signal('');
  protected readonly results = signal<AddressMatchResponse[]>([]);
  protected readonly searching = signal(false);
  protected readonly picked = signal<AddressMatchResponse | null>(null);
  protected readonly form = signal<AddressForm>({ ...EMPTY_FORM });
  protected readonly saving = signal(false);
  protected readonly confirmShared = signal(false);

  protected readonly isEdit = computed(() => !!this.editTarget());

  protected readonly canSave = computed(() => {
    const f = this.form();
    if (!f.typeId) return false;
    if (this.step() === 'link') return !!this.picked();
    return (
      f.street.trim().length > 0 &&
      f.town.trim().length > 0 &&
      f.county.trim().length > 0 &&
      f.postcode.trim().length > 0 &&
      f.country.trim().length > 0
    );
  });

  // Re-baselined whenever a step begins, so the search term itself never reads as dirty —
  // only the address the user has actually composed (or the match they picked) does.
  private readonly snapshot = signal<string | null>(null);

  private readonly currentForm = computed(() =>
    JSON.stringify({ ...this.form(), pickedId: this.picked()?.addressId ?? null }),
  );

  protected readonly isDirty = computed(() => {
    const s = this.snapshot();
    return s !== null && s !== this.currentForm();
  });

  constructor() {
    // Reset whenever the dialog opens (track only `open`, not the form inputs).
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
            return of<AddressMatchResponse[]>([]);
          }
          this.searching.set(true);
          return this.data.searchAddressMatches(this.staffMemberId(), term).pipe(
            catchError(err => {
              this.notify.apiError(err, this.transloco.translate('common.contact.address.searchError'));
              return of<AddressMatchResponse[]>([]);
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

  protected patch(changes: Partial<AddressForm>): void {
    this.form.update(f => ({ ...f, ...changes }));
  }

  protected pickExisting(match: AddressMatchResponse): void {
    this.picked.set(match);
    this.step.set('link');
    this.snapshot.set(this.currentForm());
  }

  protected enterManually(): void {
    this.picked.set(null);
    this.form.set({
      ...EMPTY_FORM,
      typeId: this.form().typeId || this.addressTypes()[0]?.id || '',
      country: 'United Kingdom',
    });
    this.step.set('form');
    this.snapshot.set(this.currentForm());
  }

  protected backToSearch(): void {
    this.picked.set(null);
    this.step.set('search');
    this.snapshot.set(this.currentForm());
  }

  protected formatMatch(m: AddressMatchResponse): string {
    return [
      m.buildingName,
      [m.buildingNumber, m.street].filter(Boolean).join(' '),
      m.town,
      m.postcode,
    ]
      .filter(p => p && p.trim().length > 0)
      .join(', ');
  }

  protected save(): void {
    if (!this.canSave() || this.saving()) return;

    if (this.isEdit()) {
      const t = this.editTarget()!;
      if (t.sharedCount > 1) {
        // Shared — let the user choose how the edit applies.
        this.confirmShared.set(true);
        return;
      }
      void this.doUpdate('FixInPlace');
      return;
    }

    void this.doAdd();
  }

  protected async doUpdate(mode: AddressEditMode): Promise<void> {
    const t = this.editTarget();
    if (!t || this.saving()) return;
    this.saving.set(true);

    const f = this.form();
    const payload: PersonAddressUpdateRequest = {
      typeId: f.typeId,
      isMain: f.isMain,
      mode,
      buildingNumber: this.nz(f.buildingNumber),
      buildingName: this.nz(f.buildingName),
      apartment: this.nz(f.apartment),
      street: f.street.trim(),
      district: this.nz(f.district),
      town: f.town.trim(),
      county: f.county.trim(),
      postcode: f.postcode.trim(),
      country: f.country.trim(),
    };

    try {
      await firstValueFrom(this.data.updateAddress(this.staffMemberId(), t.addressPersonId, payload));
      // Re-baseline before emitting so the parent-driven close doesn't prompt.
      this.snapshot.set(this.currentForm());
      this.notify.success(this.transloco.translate('common.contact.address.savedToast'));
      this.saved.emit();
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('common.contact.address.saveError'));
    } finally {
      this.saving.set(false);
    }
  }

  private async doAdd(): Promise<void> {
    this.saving.set(true);

    const f = this.form();
    const existing = this.picked();
    const payload: PersonAddressUpsertRequest = existing
      ? { existingAddressId: existing.addressId, typeId: f.typeId, isMain: f.isMain }
      : {
          typeId: f.typeId,
          isMain: f.isMain,
          buildingNumber: this.nz(f.buildingNumber),
          buildingName: this.nz(f.buildingName),
          apartment: this.nz(f.apartment),
          street: f.street.trim(),
          district: this.nz(f.district),
          town: f.town.trim(),
          county: f.county.trim(),
          postcode: f.postcode.trim(),
          country: f.country.trim(),
        };

    try {
      await firstValueFrom(this.data.addAddress(this.staffMemberId(), payload));
      this.snapshot.set(this.currentForm());
      this.notify.success(this.transloco.translate('common.contact.address.savedToast'));
      this.saved.emit();
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('common.contact.address.saveError'));
    } finally {
      this.saving.set(false);
    }
  }

  protected async onCancel(): Promise<void> {
    if (this.saving()) return;
    if (this.isDirty()) {
      const ok = await this.confirmDialog.confirm({
        header: this.transloco.translate('common.discardChanges'),
        message: this.transloco.translate('common.discardConfirm'),
        acceptLabel: this.transloco.translate('common.discard'),
        acceptSeverity: 'danger',
      });
      if (!ok) return;
    }
    this.closed.emit();
  }

  // Escape is gated by closeOnEscape, so this only fires on a clean form or on a
  // parent-driven close — just keep the parent in sync.
  protected onHide(): void {
    this.closed.emit();
  }

  private reset(): void {
    this.saving.set(false);
    this.confirmShared.set(false);
    this.picked.set(null);
    this.searchTerm.set('');
    this.results.set([]);

    const target = this.editTarget();
    if (target) {
      this.step.set('form');
      this.form.set({
        buildingNumber: target.buildingNumber ?? '',
        buildingName: target.buildingName ?? '',
        apartment: target.apartment ?? '',
        street: target.street,
        district: target.district ?? '',
        town: target.town,
        county: target.county,
        postcode: target.postcode,
        country: target.country,
        typeId: target.typeId,
        isMain: target.isMain,
      });
    } else {
      this.step.set('search');
      this.form.set({
        ...EMPTY_FORM,
        typeId: this.addressTypes()[0]?.id ?? '',
        country: 'United Kingdom',
      });
    }

    this.snapshot.set(this.currentForm());
  }

  private nz(value: string): string | null {
    const trimmed = value.trim();
    return trimmed.length === 0 ? null : trimmed;
  }
}
