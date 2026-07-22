import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { FieldTree } from '@angular/forms/signals';
import { TranslocoService } from '@jsverse/transloco';

import { validationMessageKey } from '../../validation/validation-messages';

@Component({
  selector: 'mp-field',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="flex flex-col gap-1">
      <label class="text-sm font-medium" [attr.for]="for()">
        {{ label() }}
        @if (required()) {
          <span class="text-red-500" aria-hidden="true">*</span>
        }
      </label>
      <ng-content></ng-content>
      @if (message(); as e) {
        <small class="text-xs text-red-600 dark:text-red-400">{{ e }}</small>
      } @else if (hint(); as h) {
        <small class="text-xs text-muted-foreground">{{ h }}</small>
      }
    </div>
  `,
})
export class Field {
  private readonly transloco = inject(TranslocoService);

  readonly label = input.required<string>();
  readonly for = input<string>();
  readonly hint = input<string>();
  readonly required = input(false);

  /** Explicit error message; wins over the field-derived one. Use for messages the field can't
   *  express, or to keep manual control. */
  readonly error = input<string>();

  /** Optional Signal Forms field. When bound, the field's first validation error is shown once the
   *  field is touched (blur or submit): the error's own `message` key wins, else the built-in default
   *  for its `kind` (see DEFAULT_VALIDATION_MESSAGES) — so a plain required/maxLength field needs no
   *  message, while custom rules carry their own. A message-less unknown error shows nothing. */
  readonly field = input<FieldTree<unknown>>();

  protected readonly message = computed<string | undefined>(() => {
    const explicit = this.error();
    if (explicit) return explicit;

    const fieldTree = this.field();
    if (!fieldTree) return undefined;

    const state = fieldTree();
    if (!state.touched()) return undefined;

    const first = state.errors()[0] as { kind?: string; message?: string } | undefined;
    const key = validationMessageKey(first);
    if (!key) return undefined;

    // Pass the error as params so parameterised messages (e.g. {{ maxLength }}) interpolate.
    return this.transloco.translate(key, first);
  });
}
