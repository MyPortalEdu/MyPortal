import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'mp-section-header',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="flex items-center justify-between gap-3 mb-3 pb-2 border-b border-surface">
      <h4 class="mp-section-title flex items-center gap-2 m-0 min-w-0">
        @if (icon(); as i) {
          <i [class]="i + ' text-primary'"></i>
        }
        <span class="truncate">{{ title() }}</span>
      </h4>
      <div class="shrink-0">
        <ng-content></ng-content>
      </div>
    </div>
  `,
})
export class SectionHeader {
  readonly title = input.required<string>();
  readonly icon = input<string>();
}
