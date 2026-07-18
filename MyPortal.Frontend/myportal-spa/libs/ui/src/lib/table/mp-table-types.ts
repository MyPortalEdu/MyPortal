/**
 * Design-system table event/filter contract. Structurally the fields the app's QueryKit adapter
 * consumes (page offset, page size, sort, per-column + global filters) — but owned here, not
 * imported from PrimeNG, so tables carry no PrimeNG type dependency.
 */

// Fields are optional so this is a structural superset of the event PrimeNG p-table still emits
// (during coexistence, unmigrated tables feed the same QueryKit adapter). MpTable itself always
// emits `first`/`rows` as concrete numbers.

/** A single column/global filter clause. `matchMode` maps to a QueryKit operator (contains, equals…). */
export interface MpFilterMetadata {
  value?: unknown;
  matchMode?: string;
  operator?: string;
}

/** One column's sort direction: 1 = ascending, -1 = descending. */
export interface MpSortMeta {
  field?: string;
  order?: number;
}

/** Emitted by MpTable whenever the server should refetch (init, page, sort, filter). */
export interface MpTableLazyLoadEvent {
  /** Row offset of the first row on the current page (page = first/rows + 1). */
  first?: number | null;
  /** Page size. */
  rows?: number | null;
  sortField?: string | string[] | null;
  sortOrder?: number | null;
  multiSortMeta?: MpSortMeta[] | null;
  /** Per-column metadata; the single search box lives under the `global` key. */
  filters?: Record<string, MpFilterMetadata | MpFilterMetadata[] | undefined>;
  globalFilter?: string | string[] | null;
}
