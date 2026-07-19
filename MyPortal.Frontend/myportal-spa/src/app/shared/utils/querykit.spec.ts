import { type MpTableLazyLoadEvent as TableLazyLoadEvent } from '@myportal/ui';
import { SortOptions, toQueryKitParams } from './querykit';

function decodeSort(encoded: string | undefined): SortOptions | null {
  if (!encoded) return null;
  const b64 = encoded.replace(/-/g, '+').replace(/_/g, '/');
  const padded = b64 + '='.repeat((4 - (b64.length % 4)) % 4);
  const binary = atob(padded);
  const bytes = new Uint8Array(binary.length);
  for (let i = 0; i < binary.length; i++) bytes[i] = binary.charCodeAt(i);
  return JSON.parse(new TextDecoder().decode(bytes)) as SortOptions;
}

describe('querykit toQueryKitParams sort handling', () => {
  it('maps single-sort order 1 to Ascending', () => {
    const event: TableLazyLoadEvent = { first: 0, rows: 25, sortField: 'title', sortOrder: 1 };
    const params = toQueryKitParams(event);
    expect(decodeSort(params.sort)).toEqual({
      criteria: [{ columnName: 'title', direction: 'Ascending' }],
    });
  });

  it('maps single-sort order -1 to Descending', () => {
    const event: TableLazyLoadEvent = { first: 0, rows: 25, sortField: 'title', sortOrder: -1 };
    const params = toQueryKitParams(event);
    expect(decodeSort(params.sort)).toEqual({
      criteria: [{ columnName: 'title', direction: 'Descending' }],
    });
  });

  it('omits sort when single-sort order is 0 (user cleared the column)', () => {
    const event: TableLazyLoadEvent = { first: 0, rows: 25, sortField: 'title', sortOrder: 0 };
    const params = toQueryKitParams(event);
    expect(params.sort).toBeUndefined();
  });

  it('keeps multi-sort entries with order 1 and -1 and drops order 0', () => {
    const event: TableLazyLoadEvent = {
      first: 0,
      rows: 25,
      multiSortMeta: [
        { field: 'title', order: 1 },
        { field: 'createdAt', order: -1 },
        { field: 'category', order: 0 },
      ],
    };
    const params = toQueryKitParams(event);
    expect(decodeSort(params.sort)).toEqual({
      criteria: [
        { columnName: 'title', direction: 'Ascending' },
        { columnName: 'createdAt', direction: 'Descending' },
      ],
    });
  });

  it('omits sort entirely when every multi-sort entry has order 0', () => {
    const event: TableLazyLoadEvent = {
      first: 0,
      rows: 25,
      multiSortMeta: [
        { field: 'title', order: 0 },
        { field: 'createdAt', order: 0 },
      ],
    };
    const params = toQueryKitParams(event);
    expect(params.sort).toBeUndefined();
  });
});
