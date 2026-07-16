import { Pipe, PipeTransform, inject } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';

/**
 * Renders a stored gender code (M/F/U) as a friendly label, e.g. `M` → "Male".
 * Falls back to the raw code for values not in `common.gender.*` (legacy /
 * unknown) and an em dash when empty. Pair with {@link GenderSelect} for editing.
 */
@Pipe({ name: 'genderLabel', standalone: true })
export class GenderLabelPipe implements PipeTransform {
  private readonly transloco = inject(TranslocoService);

  transform(code: string | null | undefined): string {
    if (!code) return '—';
    const key = `common.gender.${code}`;
    const label = this.transloco.translate(key);
    return label === key ? code : label;
  }
}
