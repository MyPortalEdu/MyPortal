import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router, convertToParamMap } from '@angular/router';
import { Observable, of, throwError } from 'rxjs';

import { PageResult } from '../types/bulletin';
import { injectGridList } from './grid-list';
import { MpTableLazyLoadEvent } from '@myportal/ui';

interface Row {
  id: string;
}

function setup(
  queryParams: Record<string, string> = {},
  listImpl?: () => Observable<PageResult<Row>>,
) {
  const navigate = jasmine.createSpy('navigate').and.resolveTo(true);
  const filterGlobal = jasmine.createSpy('filterGlobal');
  const table = signal<{ filterGlobal: typeof filterGlobal } | undefined>({ filterGlobal });
  const list = jasmine
    .createSpy('list')
    .and.callFake(listImpl ?? (() => of<PageResult<Row>>({ items: [{ id: '1' }], totalItems: 1 })));
  const onError = jasmine.createSpy('onError');

  TestBed.configureTestingModule({
    providers: [
      { provide: Router, useValue: { navigate } },
      { provide: ActivatedRoute, useValue: { snapshot: { queryParamMap: convertToParamMap(queryParams) } } },
    ],
  });

  const grid = TestBed.runInInjectionContext(() =>
    injectGridList<Row>({
      list,
      searchFields: ['name'],
      defaults: { first: 0, rows: 25 },
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      table: table as any,
      onError,
    }),
  );

  return { grid, navigate, filterGlobal, list, onError };
}

const EVENT: MpTableLazyLoadEvent = { first: 0, rows: 25 };

describe('injectGridList', () => {
  it('load() fetches, populates rows/total, clears loading, and syncs the URL', () => {
    const { grid, navigate, list } = setup();
    grid.load(EVENT);

    expect(list).toHaveBeenCalledWith({ page: 1, pageSize: 25 });
    expect(grid.rows()).toEqual([{ id: '1' }]);
    expect(grid.totalRecords()).toBe(1);
    expect(grid.loading()).toBeFalse();
    expect(navigate).toHaveBeenCalled();
  });

  it('onSearch() mirrors the term and drives the table filter', () => {
    const { grid, filterGlobal } = setup();
    grid.onSearch('smith');
    expect(grid.searchTerm()).toBe('smith');
    expect(grid.hasFilter()).toBeTrue();
    expect(filterGlobal).toHaveBeenCalledWith('smith', 'contains');
  });

  it('reload() re-runs the last load event', () => {
    const { grid, list } = setup();
    grid.load(EVENT);
    grid.reload();
    expect(list).toHaveBeenCalledTimes(2);
  });

  it('reload() is a no-op before any load', () => {
    const { grid, list } = setup();
    grid.reload();
    expect(list).not.toHaveBeenCalled();
  });

  it('seeds searchTerm + initialFilters from the URL `q` param', () => {
    const { grid } = setup({ q: 'smith' });
    expect(grid.searchTerm()).toBe('smith');
    expect(grid.hasFilter()).toBeTrue();
    expect(grid.initialFilters).toEqual({ global: { value: 'smith', matchMode: 'contains' } });
  });

  it('routes fetch failures to onError and clears loading', () => {
    const { grid, onError } = setup({}, () => throwError(() => new Error('boom')));
    grid.load(EVENT);
    expect(onError).toHaveBeenCalled();
    expect(grid.loading()).toBeFalse();
    expect(grid.rows()).toEqual([]);
  });
});
