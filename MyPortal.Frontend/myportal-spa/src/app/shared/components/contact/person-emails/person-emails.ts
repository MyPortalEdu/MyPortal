import { ChangeDetectionStrategy, Component, input, model } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Button } from 'primeng/button';
import { InputText } from 'primeng/inputtext';
import { MpSelect } from '@myportal/ui';
import { TranslocoDirective } from '@jsverse/transloco';

import { LookupResponse } from '../../../types/lookup';
import { PersonEmailUpsertItem } from '../../../types/staff-contact-details';
import { CopyButton } from '../../copy-button/copy-button';

/**
 * Presentational list-editor for a person's email addresses. Owner-agnostic — the host owns
 * the model and persistence; this just renders/edits the list and enforces one-main-per-list.
 */
@Component({
  selector: 'mp-person-emails',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule, Button, InputText, MpSelect, CopyButton, TranslocoDirective],
  templateUrl: './person-emails.html',
})
export class PersonEmails {
  readonly emails = model<PersonEmailUpsertItem[]>([]);
  readonly types = input<LookupResponse[]>([]);
  readonly editing = input<boolean>(false);

  protected typeName(typeId: string): string {
    return this.types().find(t => t.id === typeId)?.description ?? '';
  }

  protected addEmail(): void {
    const list = [...this.emails()];
    list.push({
      id: null,
      typeId: this.types()[0]?.id ?? '',
      address: '',
      isMain: list.length === 0, // first one in defaults to main
      notes: null,
    });
    this.emails.set(list);
  }

  protected removeEmail(index: number): void {
    const list = this.emails().filter((_, i) => i !== index);
    // Lost the main? Promote the first remaining so a non-empty list always has one.
    if (list.length && !list.some(e => e.isMain)) {
      list[0] = { ...list[0], isMain: true };
    }
    this.emails.set(list);
  }

  protected setMain(index: number): void {
    this.emails.set(this.emails().map((e, i) => ({ ...e, isMain: i === index })));
  }

  protected patch(index: number, changes: Partial<PersonEmailUpsertItem>): void {
    this.emails.set(this.emails().map((e, i) => (i === index ? { ...e, ...changes } : e)));
  }
}
