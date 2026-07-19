import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'mp-empty-state',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="flex flex-col items-center justify-center text-center gap-2 rounded-md border border-dashed border-border px-4 py-8">
      <i [class]="icon() + ' text-2xl text-muted-foreground'"></i>
      <p class="text-sm text-muted-foreground m-0">{{ message() }}</p>
      <ng-content></ng-content>
    </div>
  `,
})
export class EmptyState {
  readonly icon = input('fa-regular fa-folder-open');
  readonly message = input.required<string>();
}
