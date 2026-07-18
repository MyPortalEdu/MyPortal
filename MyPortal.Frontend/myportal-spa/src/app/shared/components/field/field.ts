import { ChangeDetectionStrategy, Component, input } from '@angular/core';

/**
 * Labelled form-field wrapper: a canonical `text-sm font-medium` label above a projected control,
 * with an optional hint below. Replaces the hand-rolled `flex flex-col gap-1` + label blocks and the
 * three-way label-class drift (`text-sm font-medium` vs `text-xs … text-muted-color` vs weightless).
 *
 * `required` renders the marker only — put the `required` attribute on the projected control too, so
 * assistive tech gets it from the control rather than from a decorative asterisk. `error` replaces
 * the hint while it's set.
 */
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
      @if (error(); as e) {
        <small class="text-xs text-red-600 dark:text-red-400">{{ e }}</small>
      } @else if (hint(); as h) {
        <small class="text-xs text-muted-color">{{ h }}</small>
      }
    </div>
  `,
})
export class Field {
  readonly label = input.required<string>();
  readonly for = input<string>();
  readonly hint = input<string>();
  readonly required = input(false);
  readonly error = input<string>();
}
