import { ChangeDetectionStrategy, Component, input } from '@angular/core';

/**
 * Inline error panel for a failed load: a centred icon + message with an optional projected retry
 * action. Pages should render this on load failure instead of relying only on a transient toast —
 * several pages currently show nothing at all when their initial GET fails.
 */
@Component({
  selector: 'mp-error-state',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="flex flex-col items-center justify-center text-center gap-3 px-4 py-12">
      <i [class]="icon() + ' text-3xl text-muted-color'"></i>
      <p class="text-sm text-muted-color m-0">{{ message() }}</p>
      <ng-content></ng-content>
    </div>
  `,
})
export class ErrorState {
  readonly icon = input('fa-solid fa-triangle-exclamation');
  readonly message = input.required<string>();
}
