import { DestroyRef, Signal, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { MpFilterMetadata, MpTable, MpTableLazyLoadEvent } from '@myportal/ui';

import { PageResult } from '../types/bulletin';
import {
  GridState,
  QueryKitParams,
  gridStateFromLazyLoadEvent,
  gridStateFromQueryParams,
  gridStateToQueryParams,
  toQueryKitParams,
} from './querykit';

export interface GridListConfig<T> {
  list: (params: QueryKitParams) => Observable<PageResult<T>>;
  searchFields: string[];
  defaults: GridState;
  table: Signal<MpTable | undefined>;
  onError?: (err: unknown) => void;
  filters?: () => Record<string, string> | undefined;
}

export interface GridListController<T> {
  readonly initialState: GridState;
  readonly initialFilters: Record<string, MpFilterMetadata>;
  readonly rows: Signal<T[]>;
  readonly totalRecords: Signal<number>;
  readonly loading: Signal<boolean>;
  readonly searchTerm: Signal<string>;
  readonly hasFilter: Signal<boolean>;
  load(event: MpTableLazyLoadEvent): void;
  reload(): void;
  onSearch(value: string): void;
  clearSearch(): void;
}

export function injectGridList<T>(config: GridListConfig<T>): GridListController<T> {
  const router = inject(Router);
  const route = inject(ActivatedRoute);
  const destroyRef = inject(DestroyRef);

  const rows = signal<T[]>([]);
  const totalRecords = signal(0);
  const loading = signal(false);

  const initialState = gridStateFromQueryParams(route.snapshot.queryParamMap, config.defaults);

  const searchTerm = signal(initialState.global ?? '');
  const hasFilter = computed(() => searchTerm().trim().length > 0);

  const initialFilters: Record<string, MpFilterMetadata> = initialState.global
    ? { global: { value: initialState.global, matchMode: 'contains' } }
    : {};

  let lastEvent: MpTableLazyLoadEvent | null = null;

  function syncUrl(event: MpTableLazyLoadEvent): void {
    const state = gridStateFromLazyLoadEvent(event, {
      defaultRows: config.defaults.rows,
      filters: config.filters?.(),
    });
    void router.navigate([], {
      relativeTo: route,
      queryParams: gridStateToQueryParams(state, config.defaults),
      replaceUrl: true,
    });
  }

  function load(event: MpTableLazyLoadEvent): void {
    lastEvent = event;
    syncUrl(event);
    loading.set(true);
    config
      .list(toQueryKitParams(event, { globalFields: config.searchFields }))
      .pipe(takeUntilDestroyed(destroyRef))
      .subscribe({
        next: page => {
          rows.set(page.items ?? []);
          totalRecords.set(page.totalItems ?? 0);
          loading.set(false);
        },
        error: err => {
          loading.set(false);
          config.onError?.(err);
        },
      });
  }

  function reload(): void {
    if (lastEvent) load(lastEvent);
  }

  function onSearch(value: string): void {
    searchTerm.set(value);
    config.table()?.filterGlobal(value, 'contains');
  }

  function clearSearch(): void {
    onSearch('');
  }

  return {
    initialState,
    initialFilters,
    rows: rows.asReadonly(),
    totalRecords: totalRecords.asReadonly(),
    loading: loading.asReadonly(),
    searchTerm: searchTerm.asReadonly(),
    hasFilter,
    load,
    reload,
    onSearch,
    clearSearch,
  };
}
