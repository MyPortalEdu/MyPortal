import { ChangeDetectionStrategy, Component, input, model } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Button } from 'primeng/button';
import { InputText } from 'primeng/inputtext';
import { Select } from 'primeng/select';
import { TranslocoDirective } from '@jsverse/transloco';

import { LookupResponse } from '../../../types/lookup';
import { PersonPhoneUpsertItem } from '../../../types/staff-contact-details';
import { CopyButton } from '../../copy-button/copy-button';

/**
 * Presentational list-editor for a person's phone numbers. Owner-agnostic — the host owns the
 * model and persistence; this just renders/edits the list and enforces one-main-per-list.
 */
@Component({
  selector: 'mp-person-phones',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule, Button, InputText, Select, CopyButton, TranslocoDirective],
  templateUrl: './person-phones.html',
})
export class PersonPhones {
  readonly phones = model<PersonPhoneUpsertItem[]>([]);
  readonly types = input<LookupResponse[]>([]);
  readonly editing = input<boolean>(false);

  protected typeName(typeId: string): string {
    return this.types().find(t => t.id === typeId)?.description ?? '';
  }

  // tel: URIs shouldn't contain spaces; keep digits and a leading +/() dashes.
  protected telHref(number: string): string {
    return 'tel:' + number.replace(/[^\d+]/g, '');
  }

  protected addPhone(): void {
    const list = [...this.phones()];
    list.push({
      id: null,
      typeId: this.types()[0]?.id ?? '',
      number: '',
      isMain: list.length === 0,
    });
    this.phones.set(list);
  }

  protected removePhone(index: number): void {
    const list = this.phones().filter((_, i) => i !== index);
    if (list.length && !list.some(p => p.isMain)) {
      list[0] = { ...list[0], isMain: true };
    }
    this.phones.set(list);
  }

  protected setMain(index: number): void {
    this.phones.set(this.phones().map((p, i) => ({ ...p, isMain: i === index })));
  }

  protected patch(index: number, changes: Partial<PersonPhoneUpsertItem>): void {
    this.phones.set(this.phones().map((p, i) => (i === index ? { ...p, ...changes } : p)));
  }
}
