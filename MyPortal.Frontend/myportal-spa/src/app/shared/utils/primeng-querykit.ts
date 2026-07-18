import { ParamMap, Params } from '@angular/router';
import {
  MpTableLazyLoadEvent as TableLazyLoadEvent,
  MpFilterMetadata as FilterMetadata,
  MpSortMeta as SortMeta,
} from '@myportal/ui';

// QueryKit's wire JSON shapes. Mirrors the C# types in Rowan.QueryKit.Repositories
// (FilterOptions / FilterGroup / FilterCriterion / SortOptions / SortCriterion).
// Property names match the camel-cased deserialiser config on the API.
export type BoolJoin = 'And' | 'Or';
export type FilterOperator =
  | 'Equals' | 'NotEquals'
  | 'Contains' | 'NotContains'
  | 'StartsWith' | 'EndsWith'
  | 'LessThan' | 'LessThanOrEqual'
  | 'GreaterThan' | 'GreaterThanOrEqual'
  | 'In' | 'Between'
  | 'IsNull' | 'IsNotNull';
export type SortDirection = 'Ascending' | 'Descending';

export interface FilterCriterion {
  columnName: string;
  operator: FilterOperator;
  value?: unknown;
  value2?: unknown;
  values?: unknown[];
}

export interface FilterGroup {
  join: BoolJoin;
  criteria: FilterCriterion[];
}

export interface FilterOptions {
  join: BoolJoin;
  groups: FilterGroup[];
}

export interface SortCriterion {
  columnName: string;
  direction: SortDirection;
}

export interface SortOptions {
  criteria: SortCriterion[];
}

export interface QueryKitParams {
  page: number;
  pageSize: number;
  filter?: string;  // base64url(JSON(FilterOptions))
  sort?: string;    // base64url(JSON(SortOptions))
}

const DEFAULT_PAGE_SIZE = 25;

// PrimeNG → QueryKit operator mapping. PrimeNG uses lower-cased shortcodes;
// QueryKit uses the enum names. Anything PrimeNG can express that QueryKit
// can't is mapped to its nearest equivalent (e.g. PrimeNG's `is/isNot` for
// non-string equality is just Equals/NotEquals).
const OPERATOR_MAP: Record<string, FilterOperator> = {
  startsWith: 'StartsWith',
  contains: 'Contains',
  notContains: 'NotContains',
  endsWith: 'EndsWith',
  equals: 'Equals',
  notEquals: 'NotEquals',
  in: 'In',
  lt: 'LessThan',
  lte: 'LessThanOrEqual',
  gt: 'GreaterThan',
  gte: 'GreaterThanOrEqual',
  between: 'Between',
  is: 'Equals',
  isNot: 'NotEquals',
  dateIs: 'Equals',
  dateIsNot: 'NotEquals',
  dateBefore: 'LessThan',
  dateAfter: 'GreaterThan',
  before: 'LessThan',
  after: 'GreaterThan',
};

/**
 * Convert a PrimeNG table lazy-load event into the query-string params the
 * QueryKit listing endpoints expect ({ page, pageSize, filter?, sort? }).
 *
 * - first/rows → page/pageSize (page is 1-based on the API side)
 * - sortField/sortOrder OR multiSortMeta → SortOptions
 * - filters (per-field FilterMetadata) → FilterOptions; each field becomes its
 *   own FilterGroup so PrimeNG's per-column AND/OR composition is preserved.
 *
 * Returns plain primitives, not an HttpParams instance, so callers can stitch
 * extra params (e.g. academicYearId) in without round-tripping through HttpParams.
 *
 * `opts.globalFields` supports a single search box (PrimeNG's `filterGlobal`):
 * the one search term is expanded into an OR group of `Contains` predicates
 * across the listed columns.
 */
export function toQueryKitParams(
  event: TableLazyLoadEvent,
  opts: { globalFields?: string[] } = {},
): QueryKitParams {
  const pageSize = event.rows ?? DEFAULT_PAGE_SIZE;
  // PrimeNG's `first` is the row offset; QueryKit's `page` is 1-based.
  const page = Math.floor((event.first ?? 0) / pageSize) + 1;

  const params: QueryKitParams = { page, pageSize };

  const sort = buildSort(event);
  if (sort) params.sort = encodeBase64Url(JSON.stringify(sort));

  const filter = buildFilter(event, opts.globalFields ?? []);
  if (filter) params.filter = encodeBase64Url(JSON.stringify(filter));

  return params;
}

function buildSort(event: TableLazyLoadEvent): SortOptions | null {
  const criteria: SortCriterion[] = [];

  if (event.multiSortMeta?.length) {
    for (const m of event.multiSortMeta as SortMeta[]) {
      if (!m.field) continue;
      // PrimeNG emits order ∈ {1, -1, 0}; 0 means the column has been cleared.
      // Skip 0 — emitting Ascending would send an unintended sort.
      const direction = toDirection(m.order);
      if (!direction) continue;
      criteria.push({ columnName: m.field, direction });
    }
  } else if (event.sortField) {
    const field = Array.isArray(event.sortField) ? event.sortField[0] : event.sortField;
    if (field) {
      const direction = toDirection(event.sortOrder);
      if (direction) {
        criteria.push({ columnName: field, direction });
      }
    }
  }

  return criteria.length ? { criteria } : null;
}

function toDirection(order: number | null | undefined): SortDirection | null {
  if (order === 1) return 'Ascending';
  if (order === -1) return 'Descending';
  return null;
}

function buildFilter(event: TableLazyLoadEvent, globalFields: string[]): FilterOptions | null {
  const fieldFilters = event.filters;
  if (!fieldFilters) return null;

  const groups: FilterGroup[] = [];

  for (const [field, raw] of Object.entries(fieldFilters)) {
    // PrimeNG's single search box arrives under the `global` key. Fan it out
    // into one OR group across the configured columns.
    if (field === 'global') {
      const group = buildGlobalGroup(raw, globalFields);
      if (group) groups.push(group);
      continue;
    }

    // PrimeNG passes either a single FilterMetadata (basic mode) or an array
    // (advanced mode, with per-clause and/or operator). Normalise to array.
    const metas: FilterMetadata[] = Array.isArray(raw) ? raw : raw ? [raw] : [];
    const criteria: FilterCriterion[] = [];
    let join: BoolJoin = 'And';

    for (const m of metas) {
      const criterion = toCriterion(field, m);
      if (criterion) criteria.push(criterion);
      // PrimeNG's `operator` is per-FilterMetadata but conceptually applies to
      // the whole group. Last one wins — that matches PrimeNG's own table behaviour.
      if (m.operator === 'or') join = 'Or';
    }

    if (criteria.length) groups.push({ join, criteria });
  }

  return groups.length ? { join: 'And', groups } : null;
}

function buildGlobalGroup(
  raw: FilterMetadata | FilterMetadata[] | undefined,
  globalFields: string[],
): FilterGroup | null {
  const meta = Array.isArray(raw) ? raw[0] : raw;
  const value = meta?.value;
  if (value === undefined || value === null || value === '' || !globalFields.length) {
    return null;
  }
  return {
    join: 'Or',
    criteria: globalFields.map(columnName => ({ columnName, operator: 'Contains', value })),
  };
}

function toCriterion(field: string, m: FilterMetadata): FilterCriterion | null {
  if (m.value === undefined || m.value === null || m.value === '') {
    // PrimeNG emits FilterMetadata with empty value when a column filter is
    // cleared. Skip — sending it would push an empty IsNull/Equals predicate.
    return null;
  }

  const matchMode = m.matchMode ?? 'contains';
  const operator = OPERATOR_MAP[matchMode] ?? 'Equals';

  if (operator === 'In' && Array.isArray(m.value)) {
    return { columnName: field, operator, values: m.value };
  }
  if (operator === 'Between' && Array.isArray(m.value) && m.value.length === 2) {
    return { columnName: field, operator, value: m.value[0], value2: m.value[1] };
  }
  return { columnName: field, operator, value: m.value };
}

/**
 * A list grid's user-visible state, in PrimeNG's terms (`first` is a row offset,
 * `sortOrder` is 1/-1). `filters` carries extra per-grid single-value filters
 * keyed by column (e.g. staff's `status`); the search box lives in `global`.
 */
export interface GridState {
  first: number;
  rows: number;
  sortField?: string;
  sortOrder?: number;
  global?: string;
  filters?: Record<string, string>;
}

const PARAM_SEARCH = 'q';
const PARAM_PAGE = 'page';
const PARAM_SIZE = 'size';
const PARAM_SORT = 'sort';
const PARAM_DIR = 'dir';

/**
 * Read a grid's state out of the URL, e.g. `?q=smith&page=3&sort=lastName&dir=asc&status=Active`.
 *
 * Anything absent falls back to `defaults`, so a pristine URL yields exactly the
 * grid's own defaults. The keys of `defaults.filters` declare which extra params
 * this grid understands — no other query param is read.
 */
export function gridStateFromQueryParams(params: ParamMap, defaults: GridState): GridState {
  const rows = toPositiveInt(params.get(PARAM_SIZE)) ?? defaults.rows;
  const page = toPositiveInt(params.get(PARAM_PAGE)) ?? 1;

  const sortParam = params.get(PARAM_SORT);
  const dirParam = params.get(PARAM_DIR);
  const sortField = sortParam ?? defaults.sortField;
  const sortOrder = sortParam
    ? dirParam === 'desc'
      ? -1
      : 1
    : defaults.sortOrder;

  const state: GridState = {
    // The URL carries a 1-based page because users read it; the table wants an offset.
    first: (page - 1) * rows,
    rows,
    sortField: sortField || undefined,
    sortOrder: sortField ? (sortOrder ?? 1) : undefined,
    global: params.get(PARAM_SEARCH)?.trim() || defaults.global,
  };

  const defaultFilters = defaults.filters;
  if (defaultFilters) {
    state.filters = {};
    for (const [key, fallback] of Object.entries(defaultFilters)) {
      state.filters[key] = params.get(key) ?? fallback;
    }
  }

  return state;
}

/**
 * Serialise a grid's state back to query params. Values equal to `defaults` are
 * omitted, so an untouched list keeps a clean URL.
 */
export function gridStateToQueryParams(state: GridState, defaults: GridState): Params {
  const params: Params = {};
  const rows = state.rows || defaults.rows;
  const page = Math.floor((state.first || 0) / rows) + 1;

  if (page > 1) params[PARAM_PAGE] = page;
  if (rows !== defaults.rows) params[PARAM_SIZE] = rows;
  if (state.global) params[PARAM_SEARCH] = state.global;

  const sortOrder = state.sortOrder ?? 1;
  const isDefaultSort =
    state.sortField === defaults.sortField && sortOrder === (defaults.sortOrder ?? 1);
  if (state.sortField && !isDefaultSort) {
    params[PARAM_SORT] = state.sortField;
    params[PARAM_DIR] = sortOrder === -1 ? 'desc' : 'asc';
  }

  for (const [key, value] of Object.entries(state.filters ?? {})) {
    if (value && value !== defaults.filters?.[key]) params[key] = value;
  }

  return params;
}

/**
 * Pull a grid's state back out of the event the table emits, so the URL can be
 * rewritten from the same shape the request was built from.
 *
 * `opts.filters` lets a page supply the extra filters from its own models rather
 * than the event — a dropdown's "All" is absence-of-predicate in the event, but
 * still a value the URL must round-trip.
 */
export function gridStateFromLazyLoadEvent(
  event: TableLazyLoadEvent,
  opts: { defaultRows?: number; filters?: Record<string, string> } = {},
): GridState {
  const sortField = Array.isArray(event.sortField) ? event.sortField[0] : event.sortField;

  return {
    first: event.first ?? 0,
    rows: event.rows ?? opts.defaultRows ?? DEFAULT_PAGE_SIZE,
    sortField: sortField || undefined,
    sortOrder: sortField ? (event.sortOrder ?? 1) : undefined,
    global: globalFilterValue(event.filters?.['global']),
    filters: opts.filters,
  };
}

function globalFilterValue(raw: FilterMetadata | FilterMetadata[] | undefined): string | undefined {
  const meta = Array.isArray(raw) ? raw[0] : raw;
  const value = meta?.value;
  return typeof value === 'string' && value.trim() ? value : undefined;
}

function toPositiveInt(raw: string | null): number | undefined {
  if (!raw) return undefined;
  const parsed = Number(raw);
  return Number.isInteger(parsed) && parsed > 0 ? parsed : undefined;
}

/**
 * Base64-URL encode a UTF-8 string. The API decodes with `Base64UrlEncoder`
 * (Microsoft.IdentityModel), which expects the URL-safe alphabet (`-`/`_`)
 * and tolerates missing `=` padding.
 */
function encodeBase64Url(input: string): string {
  // btoa expects Latin-1; round-trip through encodeURIComponent so multi-byte
  // characters (Welsh diacritics, etc.) survive.
  const bytes = new TextEncoder().encode(input);
  let binary = '';
  for (const b of bytes) binary += String.fromCharCode(b);
  return btoa(binary).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');
}
