import { Directive, TemplateRef, inject, input } from '@angular/core';

export type MpColumnAlign = 'left' | 'right' | 'center';
export type MpColumnBreakpoint = 'sm' | 'md' | 'lg' | 'xl';

/** A dropdown filter for a column (e.g. status, gender). */
export interface MpSelectColumnFilter {
  type: 'select';
  options: readonly unknown[];
  /** The option value meaning "no filter" (e.g. 'All'). */
  clearValue?: unknown;
  optionLabel?: string;
  optionValue?: string;
}

/** How a column filters: an input by data type, or a select. Omit for no filter. */
export type MpColumnFilterSpec = 'text' | 'number' | 'date' | MpSelectColumnFilter;

/**
 * A declarative column for `mp-table`'s `[columns]` mode: header, sorting and filtering are described
 * here rather than hand-written in the template. Body cells render `row[field]` by default; supply a
 * `<ng-template mpCell="field">` for custom rendering (links, badges, avatars).
 */
export interface MpColumn {
  /** Row key for default rendering, and the default sort/filter field. */
  field: string;
  header?: string;
  sortable?: boolean;
  /** Sort a different field than `field` (e.g. a Name column sorting by surname). */
  sortField?: string;
  filter?: MpColumnFilterSpec;
  /** Filter a different field than `field` (e.g. a Name column filtering a search column). */
  filterField?: string;
  align?: MpColumnAlign;
  /** CSS width, e.g. '10rem'. */
  width?: string;
  /** Hide the column below this breakpoint. */
  hideBelow?: MpColumnBreakpoint;
  ariaLabel?: string;
  headerClass?: string;
  cellClass?: string;
}

/**
 * A custom body-cell template for a `[columns]`-mode table, matched to a column by field:
 * `<ng-template mpCell="status" let-row>…</ng-template>`.
 */
@Directive({ selector: '[mpCell]', standalone: true })
export class MpCellDef {
  readonly field = input.required<string>({ alias: 'mpCell' });
  readonly template = inject<TemplateRef<unknown>>(TemplateRef);
}
