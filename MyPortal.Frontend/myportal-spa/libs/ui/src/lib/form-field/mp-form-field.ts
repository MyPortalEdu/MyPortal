import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  Renderer2,
  computed,
  contentChild,
  effect,
  inject,
  input,
} from '@angular/core';
import { FormField } from '@angular/forms/signals';
import { TranslocoPipe } from '@jsverse/transloco';

const KIND_KEYS: Record<string, string> = {
  required: 'common.validation.required',
  minLength: 'common.validation.minLength',
  maxLength: 'common.validation.maxLength',
  min: 'common.validation.min',
  max: 'common.validation.max',
  email: 'common.validation.email',
  pattern: 'common.validation.pattern',
  minDate: 'common.validation.minDate',
  maxDate: 'common.validation.maxDate',
};

let uid = 0;

@Component({
  selector: 'mp-form-field',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [TranslocoPipe],
  host: { class: 'flex flex-col gap-1' },
  templateUrl: './mp-form-field.html',
})
export class MpFormField {
  private readonly renderer = inject(Renderer2);

  readonly label = input<string>();
  readonly hint = input<string>();
  readonly for = input<string>();

  private readonly base = `mp-field-${++uid}`;
  protected readonly hintId = `${this.base}-hint`;
  protected readonly errorId = `${this.base}-error`;

  private readonly control = contentChild(FormField);
  private readonly controlEl = contentChild(FormField, { read: ElementRef });
  private readonly state = computed(() => this.control()?.field()());

  protected readonly required = computed(() => !!this.state()?.required());

  protected readonly showError = computed(() => {
    const s = this.state();
    return !!s && s.touched() && s.invalid();
  });

  private readonly firstError = computed(() => this.state()?.errors()[0]);

  protected readonly messageKey = computed(() => {
    const e = this.firstError();
    if (!e) return '';
    if (typeof e.message === 'string') return e.message;
    return KIND_KEYS[e.kind] ?? 'common.validation.invalid';
  });

  protected readonly messageParams = computed<Record<string, unknown>>(() => {
    const e = this.firstError();
    if (!e) return {};
    const { kind, message, field, ...rest } = e as unknown as Record<string, unknown>;
    return rest;
  });

  private readonly describedBy = computed(() =>
    this.showError() ? this.errorId : this.hint() ? this.hintId : null,
  );

  constructor() {
    effect(() => {
      const el = this.controlEl()?.nativeElement as HTMLElement | undefined;
      if (!el) return;
      const id = this.describedBy();
      if (id) {
        this.renderer.setAttribute(el, 'aria-describedby', id);
      } else {
        this.renderer.removeAttribute(el, 'aria-describedby');
      }
    });
  }
}
