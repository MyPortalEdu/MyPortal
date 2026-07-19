import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { type ClassValue } from 'clsx';
import { cn } from '../utils/cn';

@Component({
  selector: 'mp-avatar',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { '[class]': 'computedClass()' },
  templateUrl: './mp-avatar.html',
})
export class MpAvatar {
  readonly label = input('');
  readonly image = input<string | undefined>(undefined);
  readonly shape = input<'circle' | 'square'>('circle');
  readonly size = input<'normal' | 'large' | 'xlarge'>('normal');
  readonly userClass = input<ClassValue>('', { alias: 'class' });

  protected readonly computedClass = computed(() =>
    cn(
      'inline-flex shrink-0 items-center justify-center overflow-hidden bg-muted font-semibold ' +
        'text-muted-foreground select-none',
      this.shape() === 'circle' ? 'rounded-full' : 'rounded-surface',
      this.size() === 'large' ? 'h-12 w-12 text-base' : this.size() === 'xlarge' ? 'h-16 w-16 text-lg' : 'h-8 w-8 text-xs',
      this.userClass(),
    ),
  );
}
