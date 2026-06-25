import { ChangeDetectionStrategy, Component, computed, inject, input, signal } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';

/**
 * Small icon button that copies a string to the clipboard and briefly flips to a check mark for
 * confirmation. Reusable wherever a value is worth copying (emails, phones, addresses…). Styled as
 * an {@link .mp-icon-button} so it sits alongside the other row actions.
 */
@Component({
  selector: 'mp-copy-button',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <button
      type="button"
      class="mp-icon-button shrink-0"
      [title]="label()"
      [attr.aria-label]="label()"
      (click)="copy()">
      <i [class]="copied() ? 'fa-solid fa-check text-green-500 text-sm' : 'fa-solid fa-copy text-sm'"></i>
    </button>
  `,
})
export class CopyButton {
  private readonly transloco = inject(TranslocoService);

  readonly value = input.required<string>();

  protected readonly copied = signal(false);

  protected readonly label = computed(() =>
    this.transloco.translate(this.copied() ? 'common.copied' : 'common.copy'),
  );

  protected async copy(): Promise<void> {
    try {
      await navigator.clipboard.writeText(this.value());
      this.copied.set(true);
      setTimeout(() => this.copied.set(false), 1500);
    } catch {
      // Clipboard API unavailable (insecure context / permission denied) — silently no-op.
    }
  }
}
