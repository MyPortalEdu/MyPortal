import { ChangeDetectionStrategy, Component, computed, forwardRef, input, signal } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { BrnSelectImports } from '@spartan-ng/brain/select';
import { BrnPopoverImports } from '@spartan-ng/brain/popover';
import { cn } from '../utils/cn';

@Component({
  selector: 'mp-select',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [BrnSelectImports, BrnPopoverImports],
  providers: [{ provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => MpSelect), multi: true }],
  host: { class: 'block' },
  templateUrl: './mp-select.html',
})
export class MpSelect implements ControlValueAccessor {
  readonly options = input<readonly unknown[]>([]);
  readonly optionLabel = input<string | undefined>(undefined);
  readonly optionValue = input<string | undefined>(undefined);
  readonly placeholder = input<string | undefined>(undefined);
  readonly disabledInput = input(false, { alias: 'disabled' });
  readonly invalid = input<boolean | null | undefined>(false);
  readonly inputId = input<string | undefined>(undefined);
  readonly showClear = input(false);
  readonly filter = input(false);
  readonly filterBy = input<string | undefined>(undefined);
  readonly filterPlaceholder = input('Search');
  readonly emptyMessage = input('No results found');

  protected readonly value = signal<unknown>(null);
  protected readonly filterQuery = signal('');
  private readonly cvaDisabled = signal(false);
  protected readonly disabled = computed(() => this.disabledInput() || this.cvaDisabled());
  protected readonly hasValue = computed(() => {
    const v = this.value();
    return v !== null && v !== undefined && v !== '';
  });

  protected onChange: (value: unknown) => void = () => {};
  protected onTouched: () => void = () => {};

  protected readonly labelOf = (opt: unknown): string => {
    const key = this.optionLabel();
    return key ? String((opt as Record<string, unknown>)?.[key] ?? '') : String(opt ?? '');
  };

  protected readonly valueOf = (opt: unknown): unknown => {
    const key = this.optionValue();
    return key ? (opt as Record<string, unknown>)?.[key] : opt;
  };

  protected readonly visibleOptions = computed(() => {
    const opts = this.options();
    if (!this.filter()) return opts;
    const query = this.filterQuery().trim().toLowerCase();
    if (!query) return opts;
    const key = this.filterBy() ?? this.optionLabel();
    return opts.filter((o) => {
      const text = key ? String((o as Record<string, unknown>)?.[key] ?? '') : String(o ?? '');
      return text.toLowerCase().includes(query);
    });
  });

  protected readonly itemToString = (val: unknown): string => {
    const match = this.options().find((o) => this.valueOf(o) === val);
    return match ? this.labelOf(match) : '';
  };

  protected onValueChange(value: unknown): void {
    this.value.set(value);
    this.filterQuery.set('');
    this.onChange(value);
    this.onTouched();
  }

  protected onFilterInput(event: Event): void {
    this.filterQuery.set((event.target as HTMLInputElement).value);
  }

  protected clear(event: Event): void {
    event.stopPropagation();
    event.preventDefault();
    this.value.set(null);
    this.onChange(null);
    this.onTouched();
  }

  protected readonly triggerClass = computed(() =>
    cn(
      'flex h-8 w-full items-center justify-between rounded-control border border-input bg-background ' +
        'px-2.5 py-1 text-sm outline-none transition-colors hover:border-[var(--p-form-field-hover-border-color)] ' +
        'focus:border-ring focus:ring-2 focus:ring-ring/40 ' +
        'aria-expanded:border-ring aria-expanded:ring-2 aria-expanded:ring-ring/40',
      this.invalid() ? 'border-destructive' : '',
      this.disabled() ? 'cursor-not-allowed opacity-50' : 'cursor-pointer',
    ),
  );

  protected readonly contentClass =
    'z-50 max-h-72 w-[var(--brn-select-width)] min-w-[8rem] overflow-y-auto rounded-surface border ' +
    'border-border bg-background p-1 shadow-overlay';

  protected readonly filterClass =
    'flex h-8 w-full rounded-control border border-input bg-background px-2.5 py-1 text-sm ' +
    'outline-none transition-colors placeholder:text-muted-foreground focus:border-ring focus:ring-2 focus:ring-ring/40';

  protected readonly itemClass =
    'flex w-full items-center justify-between gap-2 rounded-control px-2 py-1.5 text-sm text-left ' +
    'cursor-pointer select-none outline-none hover:bg-accent hover:text-accent-foreground ' +
    'data-[highlighted]:bg-accent data-[highlighted]:text-accent-foreground ' +
    'data-[disabled]:pointer-events-none data-[disabled]:opacity-50';

  writeValue(value: unknown): void {
    this.value.set(value ?? null);
  }
  registerOnChange(fn: (value: unknown) => void): void {
    this.onChange = fn;
  }
  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }
  setDisabledState(isDisabled: boolean): void {
    this.cvaDisabled.set(isDisabled);
  }
}
