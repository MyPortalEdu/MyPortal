import { Pipe, PipeTransform, inject } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';

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
