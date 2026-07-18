import { NgTemplateOutlet } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  computed,
  contentChild,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { MpButton } from '../button/mp-button';
import { MpSortHost } from './mp-sortable';
import { MpSelectionHost } from './mp-selectable-row';
import { MpTableBody, MpTableCaption, MpTableEmpty, MpTableHeader } from './mp-table-slots';
import { MpFilterMetadata, MpTableLazyLoadEvent } from './mp-table-types';

/**
 * Data table — the design-system equivalent of `p-table` for the app's server-driven grids. A
 * projection wrapper: consumers supply `<ng-template mpTableCaption|mpTableHeader|mpTableBody|
 * mpTableEmpty>` slots (mirroring pTemplate), sortable headers via `mpSortable`/`mp-sort-icon`, and
 * selectable rows via `mpSelectableRow` — exactly the p-table markup shape, minus PrimeNG.
 *
 * In `lazy` mode it owns page/sort/filter state and emits `lazyLoad` (init, paging, sorting,
 * filtering); the parent fetches and feeds `value`/`totalRecords`/`loading` back. `filterGlobal()`
 * (call via `viewChild`) drives the single search box, debounced by `filterDelay`.
 *
 * Provides itself as MpSortHost + MpSelectionHost so the header/row directives — which render inside
 * the consumer's projected templates — can reach it by injection.
 */
@Component({
  selector: 'mp-table',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [NgTemplateOutlet, MpButton],
  providers: [
    { provide: MpSortHost, useExisting: MpTable },
    { provide: MpSelectionHost, useExisting: MpTable },
  ],
  host: { class: 'block' },
  templateUrl: './mp-table.html',
})
export class MpTable implements OnInit, MpSortHost, MpSelectionHost {
  private readonly destroyRef = inject(DestroyRef);

  readonly value = input<readonly unknown[]>([]);
  readonly lazy = input(false);
  readonly paginator = input(false);
  readonly rows = input(25); // page size (initial)
  readonly first = input(0); // row offset (initial)
  readonly totalRecords = input(0);
  readonly loading = input(false);
  readonly sortField = input<string | null>(null); // initial
  readonly sortOrder = input(1); // initial (1 asc, -1 desc)
  readonly filters = input<Record<string, MpFilterMetadata | MpFilterMetadata[]>>({}); // initial
  readonly filterDelay = input(300);
  readonly dataKey = input<string | undefined>(undefined);
  readonly selectionMode = input<'single' | 'multiple' | null>(null);
  readonly scrollable = input(false);
  readonly scrollHeight = input<string | undefined>(undefined);
  readonly globalFilterFields = input<string[]>([]); // accepted for parity; used by the parent's adapter

  readonly lazyLoad = output<MpTableLazyLoadEvent>();
  readonly rowSelect = output<unknown>();

  protected readonly captionSlot = contentChild(MpTableCaption);
  protected readonly headerSlot = contentChild(MpTableHeader);
  protected readonly bodySlot = contentChild(MpTableBody);
  protected readonly emptySlot = contentChild(MpTableEmpty);

  // Live grid state (seeded from the initial inputs in ngOnInit, then owned internally).
  protected readonly _first = signal(0);
  protected readonly _rows = signal(25);
  private readonly _sortField = signal<string | null>(null);
  private readonly _sortOrder = signal(1);
  private readonly _filters = signal<Record<string, MpFilterMetadata | MpFilterMetadata[]>>({});
  private readonly _selectedKey = signal<unknown>(null);
  private filterTimer: ReturnType<typeof setTimeout> | null = null;

  // --- MpSortHost ---
  readonly activeSortField = this._sortField.asReadonly();
  readonly activeSortOrder = this._sortOrder.asReadonly();

  // --- MpSelectionHost ---
  readonly selectionEnabled = computed(() => this.selectionMode() != null);

  ngOnInit(): void {
    this._first.set(this.first());
    this._rows.set(this.rows());
    this._sortField.set(this.sortField());
    this._sortOrder.set(this.sortOrder());
    this._filters.set({ ...this.filters() });
    this.destroyRef.onDestroy(() => this.filterTimer && clearTimeout(this.filterTimer));
    // p-table fires one lazy-load on init so the parent makes its first fetch.
    if (this.lazy()) this.emit();
  }

  sort(field: string): void {
    if (this._sortField() === field) {
      // asc → desc → cleared
      if (this._sortOrder() === 1) this._sortOrder.set(-1);
      else {
        this._sortField.set(null);
        this._sortOrder.set(1);
      }
    } else {
      this._sortField.set(field);
      this._sortOrder.set(1);
    }
    this._first.set(0);
    this.emit();
  }

  selectRow(row: unknown): void {
    this._selectedKey.set(this.keyOf(row));
    this.rowSelect.emit(row);
  }

  isRowSelected(row: unknown): boolean {
    return this.dataKey() != null && this.keyOf(row) === this._selectedKey();
  }

  /** Set/clear a single column's filter, debounced by `filterDelay` (p-table's `filter`). */
  filter(value: unknown, field: string, matchMode = 'contains'): void {
    const next = { ...this._filters() };
    if (value == null || value === '') delete next[field];
    else next[field] = { value, matchMode };
    this._filters.set(next);
    this._first.set(0);
    if (this.filterTimer) clearTimeout(this.filterTimer);
    this.filterTimer = setTimeout(() => this.emit(), this.filterDelay());
  }

  /** Drive the single global search box (p-table's `filterGlobal`); debounced by `filterDelay`. */
  filterGlobal(value: string, matchMode = 'contains'): void {
    this.filter(value, 'global', matchMode);
  }

  protected keyOf(row: unknown): unknown {
    const key = this.dataKey();
    return key ? (row as Record<string, unknown>)?.[key] : row;
  }

  protected trackRow = (row: unknown): unknown => this.keyOf(row);

  private emit(): void {
    if (!this.lazy()) return;
    const filters = this._filters();
    const global = filters['global'];
    this.lazyLoad.emit({
      first: this._first(),
      rows: this._rows(),
      sortField: this._sortField(),
      sortOrder: this._sortField() ? this._sortOrder() : null,
      filters,
      globalFilter: (Array.isArray(global) ? global[0]?.value : global?.value) as string | null,
    });
  }

  // --- Paginator ---
  protected readonly pageCount = computed(() => Math.max(1, Math.ceil(this.totalRecords() / this._rows())));
  protected readonly currentPage = computed(() => Math.floor(this._first() / this._rows()) + 1);
  protected readonly rangeLabel = computed(() => {
    const total = this.totalRecords();
    if (total === 0) return '0';
    const start = this._first() + 1;
    const end = Math.min(this._first() + this._rows(), total);
    return `${start}–${end} of ${total}`;
  });

  protected toPage(first: number): void {
    this._first.set(Math.max(0, first));
    this.emit();
  }
  protected toFirst(): void {
    this.toPage(0);
  }
  protected toPrev(): void {
    this.toPage(this._first() - this._rows());
  }
  protected toNext(): void {
    this.toPage(this._first() + this._rows());
  }
  protected toLast(): void {
    this.toPage((this.pageCount() - 1) * this._rows());
  }

  protected readonly tableClass =
    'w-full border-collapse text-left text-sm ' +
    '[&_th]:whitespace-nowrap [&_th]:px-3 [&_th]:py-2 [&_th]:text-xs [&_th]:font-medium ' +
    '[&_th]:text-muted-foreground [&_th]:border-b [&_th]:border-border ' +
    '[&_td]:px-3 [&_td]:py-2 [&_td]:border-b [&_td]:border-border [&_td]:align-middle ' +
    '[&_tbody_tr:hover]:bg-muted/40 [&_tbody_tr[data-selected=true]]:bg-accent';
}
