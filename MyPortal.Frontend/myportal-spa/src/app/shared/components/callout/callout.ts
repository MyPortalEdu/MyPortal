import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

type CalloutSeverity = 'info' | 'warn' | 'success' | 'danger';

@Component({
  selector: 'mp-callout',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="flex items-start gap-3 p-4 rounded-md border" [class]="boxClass()">
      <i [class]="(icon() || defaultIcon()) + ' text-xl mt-0.5 ' + iconClass()"></i>
      <div class="flex-1 min-w-0 flex flex-col gap-0.5">
        @if (title(); as t) {
          <span class="font-semibold">{{ t }}</span>
        }
        <div class="text-sm text-muted-foreground">
          <ng-content></ng-content>
        </div>
      </div>
      <div class="shrink-0">
        <ng-content select="[calloutAction]"></ng-content>
      </div>
    </div>
  `,
})
export class Callout {
  readonly severity = input<CalloutSeverity>('info');
  readonly title = input<string>();
  readonly icon = input<string>();

  protected readonly boxClass = computed(() => BOX_CLASS[this.severity()]);
  protected readonly iconClass = computed(() => ICON_CLASS[this.severity()]);
  protected readonly defaultIcon = computed(() => DEFAULT_ICON[this.severity()]);
}

const BOX_CLASS: Record<CalloutSeverity, string> = {
  info: 'border-primary/40 bg-primary/10',
  warn: 'border-amber-500/40 bg-amber-500/10',
  success: 'border-emerald-500/40 bg-emerald-500/10',
  danger: 'border-red-500/40 bg-red-500/10',
};

const ICON_CLASS: Record<CalloutSeverity, string> = {
  info: 'text-primary',
  warn: 'text-amber-600 dark:text-amber-400',
  success: 'text-emerald-600 dark:text-emerald-400',
  danger: 'text-red-600 dark:text-red-400',
};

const DEFAULT_ICON: Record<CalloutSeverity, string> = {
  info: 'fa-solid fa-circle-info',
  warn: 'fa-solid fa-triangle-exclamation',
  success: 'fa-solid fa-circle-check',
  danger: 'fa-solid fa-circle-exclamation',
};
