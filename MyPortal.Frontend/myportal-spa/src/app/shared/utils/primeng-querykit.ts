import { FilterMetadata, SortMeta } from 'primeng/api';
import { TableLazyLoadEvent } from 'primeng/table';

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
 */
export function toQueryKitParams(event: TableLazyLoadEvent): QueryKitParams {
  const pageSize = event.rows ?? DEFAULT_PAGE_SIZE;
  // PrimeNG's `first` is the row offset; QueryKit's `page` is 1-based.
  const page = Math.floor((event.first ?? 0) / pageSize) + 1;

  const params: QueryKitParams = { page, pageSize };

  const sort = buildSort(event);
  if (sort) params.sort = encodeBase64Url(JSON.stringify(sort));

  const filter = buildFilter(event);
  if (filter) params.filter = encodeBase64Url(JSON.stringify(filter));

  return params;
}

function buildSort(event: TableLazyLoadEvent): SortOptions | null {
  const criteria: SortCriterion[] = [];

  if (event.multiSortMeta?.length) {
    for (const m of event.multiSortMeta as SortMeta[]) {
      if (!m.field) continue;
      criteria.push({
        columnName: m.field,
        direction: m.order >= 0 ? 'Ascending' : 'Descending',
      });
    }
  } else if (event.sortField) {
    const field = Array.isArray(event.sortField) ? event.sortField[0] : event.sortField;
    if (field) {
      criteria.push({
        columnName: field,
        direction: (event.sortOrder ?? 1) >= 0 ? 'Ascending' : 'Descending',
      });
    }
  }

  return criteria.length ? { criteria } : null;
}

function buildFilter(event: TableLazyLoadEvent): FilterOptions | null {
  const fieldFilters = event.filters;
  if (!fieldFilters) return null;

  const groups: FilterGroup[] = [];

  for (const [field, raw] of Object.entries(fieldFilters)) {
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
