import { Directive, TemplateRef, inject } from '@angular/core';

/**
 * Template-slot directives — the design-system equivalent of p-table's `pTemplate="…"`. Each just
 * captures its `<ng-template>`'s TemplateRef; MpTable queries them by type and renders them in the
 * right place (caption toolbar, thead, per-row tbody, empty state).
 *
 *   <ng-template mpTableCaption> … </ng-template>
 *   <ng-template mpTableHeader> <tr><th>…</th></tr> </ng-template>
 *   <ng-template mpTableBody let-row let-i="index"> <tr>…</tr> </ng-template>
 *   <ng-template mpTableEmpty> <tr><td [attr.colspan]>…</td></tr> </ng-template>
 */
@Directive({ selector: '[mpTableCaption]', standalone: true })
export class MpTableCaption {
  readonly template = inject<TemplateRef<unknown>>(TemplateRef);
}

@Directive({ selector: '[mpTableHeader]', standalone: true })
export class MpTableHeader {
  readonly template = inject<TemplateRef<unknown>>(TemplateRef);
}

@Directive({ selector: '[mpTableBody]', standalone: true })
export class MpTableBody {
  readonly template = inject<TemplateRef<{ $implicit: unknown; index: number }>>(TemplateRef);
}

@Directive({ selector: '[mpTableEmpty]', standalone: true })
export class MpTableEmpty {
  readonly template = inject<TemplateRef<unknown>>(TemplateRef);
}
