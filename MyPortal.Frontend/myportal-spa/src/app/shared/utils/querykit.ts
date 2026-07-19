import { ParamMap, Params } from '@angular/router';
import {
  MpTableLazyLoadEvent as TableLazyLoadEvent,
  MpFilterMetadata as FilterMetadata,
  MpSortMeta as SortMeta,
} from '@myportal/ui';

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
  filter?: string;
  sort?: string;
}

const DEFAULT_PAGE_SIZE = 25;

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

export function toQueryKitParams(
  event: TableLazyLoadEvent,
  opts: { globalFields?: string[] } = {},
): QueryKitParams {
  const pageSize = event.rows ?? DEFAULT_PAGE_SIZE;
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
    if (field === 'global') {
      const group = buildGlobalGroup(raw, globalFields);
      if (group) groups.push(group);
      continue;
    }

    const metas: FilterMetadata[] = Array.isArray(raw) ? raw : raw ? [raw] : [];
    const criteria: FilterCriterion[] = [];
    let join: BoolJoin = 'And';

    for (const m of metas) {
      const criterion = toCriterion(field, m);
      if (criterion) criteria.push(criterion);
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

function encodeBase64Url(input: string): string {
  const bytes = new TextEncoder().encode(input);
  let binary = '';
  for (const b of bytes) binary += String.fromCharCode(b);
  return btoa(binary).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');
}
