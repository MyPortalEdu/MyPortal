import { ChangeDetectionStrategy, Component, input } from '@angular/core';

/**
 * Placeholder for an empty list or result set: a dashed-bordered box with an icon, a message, and an
 * optional projected action. Standardises the several ad-hoc empty treatments (bare muted spans,
 * differing paddings) onto one look.
 */
@Component({
  selector: 'mp-empty-state',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="flex flex-col items-center justify-center text-center gap-2 rounded-md border border-dashed border-surface px-4 py-8">
      <i [class]="icon() + ' text-2xl text-muted-color'"></i>
      <p class="text-sm text-muted-color m-0">{{ message() }}</p>
      <ng-content></ng-content>
    </div>
  `,
})
export class EmptyState {
  readonly icon = input('fa-regular fa-folder-open');
  readonly message = input.required<string>();
}
