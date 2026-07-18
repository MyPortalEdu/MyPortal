import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { type ClassValue } from 'clsx';
import { cn } from '../utils/cn';

/** Loading placeholder — equivalent of p-skeleton. `width`/`height` are CSS lengths (e.g. '2rem'). */
@Component({
  selector: 'mp-skeleton',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[class]': 'computedClass()',
    '[style.width]': 'width()',
    '[style.height]': 'height()',
  },
  templateUrl: './mp-skeleton.html',
})
export class MpSkeleton {
  readonly width = input<string>();
  readonly height = input<string>('1rem');
  readonly userClass = input<ClassValue>('', { alias: 'class' });

  protected readonly computedClass = computed(() =>
    cn('block animate-pulse rounded-control bg-muted', this.userClass()),
  );
}
