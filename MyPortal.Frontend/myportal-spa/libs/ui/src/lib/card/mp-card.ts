import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { type ClassValue } from 'clsx';
import { cn } from '../utils/cn';

@Component({
  selector: 'mp-card',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { '[class]': 'computedClass()' },
  templateUrl: './mp-card.html',
})
export class MpCard {
  readonly userClass = input<ClassValue>('', { alias: 'class' });

  protected readonly computedClass = computed(() =>
    cn('block rounded-surface border border-border bg-background p-4', this.userClass()),
  );
}
