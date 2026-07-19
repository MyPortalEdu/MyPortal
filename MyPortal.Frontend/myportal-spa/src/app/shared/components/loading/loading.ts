import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { MpSpinner } from '@myportal/ui';

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
