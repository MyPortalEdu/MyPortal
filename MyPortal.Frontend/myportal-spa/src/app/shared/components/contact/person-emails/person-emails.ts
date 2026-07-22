import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { FieldTree, FormField } from '@angular/forms/signals';
import { MpButton, MpInput, MpSelect } from '@myportal/ui';
import { TranslocoDirective } from '@jsverse/transloco';

import { LookupResponse } from '../../../types/lookup';
import { PersonEmailFormRow } from '../../../types/person-contact-details';
import { CopyButton } from '../../copy-button/copy-button';
import { Field } from '../../field/field';

@Component({
  selector: 'mp-person-emails',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormField, Field, MpButton, MpInput, MpSelect, CopyButton, TranslocoDirective],
  templateUrl: './person-emails.html',
})
export class PersonEmails {
  readonly field = input.required<FieldTree<PersonEmailFormRow[]>>();
  readonly types = input<LookupResponse[]>([]);
  readonly editing = input<boolean>(false);

  readonly add = output<void>();
  readonly remove = output<number>();
  readonly setMain = output<number>();

  protected readonly items = computed(() => this.field()().value());

  protected typeName(typeId: string): string {
    return this.types().find(t => t.id === typeId)?.description ?? '';
  }
}
