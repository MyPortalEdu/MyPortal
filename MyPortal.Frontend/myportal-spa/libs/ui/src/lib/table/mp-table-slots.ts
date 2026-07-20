import { Directive, TemplateRef, inject } from '@angular/core';

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
