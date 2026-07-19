import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { MpSpinner } from '@myportal/ui';

/**
 * Centred loading spinner at one canonical size, with an optional label. Replaces the several bespoke
 * spinner sizes (`2rem`, `2.5rem`, `!w-12`, `!w-5`…) scattered across pages, panels, and dialogs.
 */
@Component({
  selector: 'mp-loading',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MpSpinner],
  template: `
    <div class="flex flex-col items-center justify-center gap-3 py-10">
      <mp-spinner [size]="dim()" ariaLabel="Loading"></mp-spinner>
      @if (label(); as l) {
        <span class="text-sm text-muted-color">{{ l }}</span>
      }
    </div>
  `,
})
export class Loading {
  readonly label = input<string>();
  readonly size = input<'sm' | 'md'>('md');

  protected readonly dim = computed(() => (this.size() === 'sm' ? '1.5rem' : '2rem'));
}
