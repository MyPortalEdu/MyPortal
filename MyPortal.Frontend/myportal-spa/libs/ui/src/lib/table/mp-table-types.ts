

export interface MpFilterMetadata {
  value?: unknown;
  matchMode?: string;
  operator?: string;
}

export interface MpSortMeta {
  field?: string;
  order?: number;
}

export interface MpTableLazyLoadEvent {
  first?: number | null;
  rows?: number | null;
  sortField?: string | string[] | null;
  sortOrder?: number | null;
  multiSortMeta?: MpSortMeta[] | null;
  filters?: Record<string, MpFilterMetadata | MpFilterMetadata[] | undefined>;
  globalFilter?: string | string[] | null;
}
