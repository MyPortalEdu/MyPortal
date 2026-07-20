import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'mp-error-state',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="flex flex-col items-center justify-center text-center gap-3 px-4 py-12">
      <i [class]="icon() + ' text-3xl text-muted-foreground'"></i>
      <p class="text-sm text-muted-foreground m-0">{{ message() }}</p>
      <ng-content></ng-content>
    </div>
  `,
})
export class ErrorState {
  readonly icon = input('fa-solid fa-triangle-exclamation');
  readonly message = input.required<string>();
}
