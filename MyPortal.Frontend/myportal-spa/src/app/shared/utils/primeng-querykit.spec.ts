import { type MpTableLazyLoadEvent as TableLazyLoadEvent } from '@myportal/ui';
import { SortOptions, toQueryKitParams } from './primeng-querykit';

// The encoder is base64url(JSON(SortOptions)). Round-trip back through the same
// transform so tests assert on the decoded shape rather than the opaque string.
// encodeBase64Url strips trailing `=` padding (the API decoder tolerates that),
// so re-pad to the next multiple of 4 before atob, which otherwise throws.
function decodeSort(encoded: string | undefined): SortOptions | null {
  if (!encoded) return null;
  const b64 = encoded.replace(/-/g, '+').replace(/_/g, '/');
  const padded = b64 + '='.repeat((4 - (b64.length % 4)) % 4);
  const binary = atob(padded);
  const bytes = new Uint8Array(binary.length);
  for (let i = 0; i < binary.length; i++) bytes[i] = binary.charCodeAt(i);
  return JSON.parse(new TextDecoder().decode(bytes)) as SortOptions;
}

describe('primeng-querykit toQueryKitParams sort handling', () => {
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
    // Regression: PrimeNG emits sortOrder=0 when a sortable column is clicked into
    // the "no sort" state. The previous `>= 0` mapping treated this as Ascending
    // and sent an unintended sort to the API.
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
