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
import { FormField, applyWhen, form, required, submit, validate } from '@angular/forms/signals';
import { MpButton, MpDialog, MpInput, MpSelect, MpSpinner } from '@myportal/ui';
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

@Component({
  selector: 'mp-address-form-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule, FormField, MpButton, MpDialog, MpInput, MpSpinner, MpSelect, TranslocoDirective],
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
  protected readonly confirmShared = signal(false);

  protected readonly model = signal<AddressForm>({ ...EMPTY_FORM });
  protected readonly f = form(this.model, path => {
    required(path.typeId);
    applyWhen(path, () => this.step() === 'form', formPath => {
      for (const field of [
        formPath.street,
        formPath.town,
        formPath.county,
        formPath.postcode,
        formPath.country,
      ]) {
        required(field);
        validate(field, ({ value }) =>
          value().trim().length ? undefined : { kind: 'blank', message: 'common.validation.required' },
        );
      }
    });
  });

  protected readonly isEdit = computed(() => !!this.editTarget());

  protected readonly canSave = computed(() => {
    if (this.step() === 'search') return false;
    if (this.step() === 'link') return this.f().valid() && !!this.picked();
    return this.f().valid();
  });

  private readonly snapshot = signal<string | null>(null);

  private readonly currentForm = computed(() =>
    JSON.stringify({ ...this.model(), pickedId: this.picked()?.addressId ?? null }),
  );

  protected readonly isDirty = computed(() => {
    const s = this.snapshot();
    return s !== null && s !== this.currentForm();
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

  protected pickExisting(match: AddressMatchResponse): void {
    this.picked.set(match);
    this.step.set('link');
    this.snapshot.set(this.currentForm());
  }

  protected enterManually(): void {
    this.picked.set(null);
    this.model.set({
      ...EMPTY_FORM,
      typeId: this.model().typeId || this.addressTypes()[0]?.id || '',
      country: 'United Kingdom',
    });
    this.step.set('form');
    this.f().reset();
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
    if (!this.canSave()) return;

    if (this.isEdit()) {
      const t = this.editTarget()!;
      if (t.sharedCount > 1) {
        this.confirmShared.set(true);
        return;
      }
      void this.doUpdate('FixInPlace');
      return;
    }

    void this.doAdd();
  }

  protected doUpdate(mode: AddressEditMode): Promise<boolean> {
    return submit(this.f, async () => {
      const t = this.editTarget();
      if (!t) return;
      const m = this.model();
      const payload: PersonAddressUpdateRequest = {
        typeId: m.typeId,
        isMain: m.isMain,
        mode,
        buildingNumber: this.nz(m.buildingNumber),
        buildingName: this.nz(m.buildingName),
        apartment: this.nz(m.apartment),
        street: m.street.trim(),
        district: this.nz(m.district),
        town: m.town.trim(),
        county: m.county.trim(),
        postcode: m.postcode.trim(),
        country: m.country.trim(),
      };
      try {
        await firstValueFrom(this.data.updateAddress(this.staffMemberId(), t.addressPersonId, payload));
      } catch (err) {
        this.notify.apiError(err, this.transloco.translate('common.contact.address.saveError'));
        return;
      }
      this.snapshot.set(this.currentForm());
      this.notify.success(this.transloco.translate('common.contact.address.savedToast'));
      this.saved.emit();
    });
  }

  private doAdd(): Promise<boolean> {
    return submit(this.f, async () => {
      const m = this.model();
      const existing = this.picked();
      const payload: PersonAddressUpsertRequest = existing
        ? { existingAddressId: existing.addressId, typeId: m.typeId, isMain: m.isMain }
        : {
            typeId: m.typeId,
            isMain: m.isMain,
            buildingNumber: this.nz(m.buildingNumber),
            buildingName: this.nz(m.buildingName),
            apartment: this.nz(m.apartment),
            street: m.street.trim(),
            district: this.nz(m.district),
            town: m.town.trim(),
            county: m.county.trim(),
            postcode: m.postcode.trim(),
            country: m.country.trim(),
          };
      try {
        await firstValueFrom(this.data.addAddress(this.staffMemberId(), payload));
      } catch (err) {
        this.notify.apiError(err, this.transloco.translate('common.contact.address.saveError'));
        return;
      }
      this.snapshot.set(this.currentForm());
      this.notify.success(this.transloco.translate('common.contact.address.savedToast'));
      this.saved.emit();
    });
  }

  protected async onCancel(): Promise<void> {
    if (this.f().submitting()) return;
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

  protected onHide(): void {
    this.closed.emit();
  }

  private reset(): void {
    this.confirmShared.set(false);
    this.picked.set(null);
    this.searchTerm.set('');
    this.results.set([]);

    const target = this.editTarget();
    if (target) {
      this.step.set('form');
      this.model.set({
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
      this.model.set({
        ...EMPTY_FORM,
        typeId: this.addressTypes()[0]?.id ?? '',
        country: 'United Kingdom',
      });
    }

    this.f().reset();
    this.snapshot.set(this.currentForm());
  }

  private nz(value: string): string | null {
    const trimmed = value.trim();
    return trimmed.length === 0 ? null : trimmed;
  }
}
