# MyPortal SPA — guidance

Angular 22 SPA. Design system under `libs/ui` (alias `@myportal/ui`).

## Commands
Angular 22 needs Node ≥24.15 (installed v24.18.0 via nvm). Prefix `ng`/`npm`:
```
export PATH="$HOME/.nvm/versions/node/v24.18.0/bin:$PATH"
```
Build: `npx ng build` · Test (Vitest): `npx ng test --no-watch`

## Frontend conventions
- **No essay comments** in TS/HTML — code self-documents (behaviour-affecting directives like `eslint-disable`/`@ts-*` excepted).
- **External templates only**: components use `templateUrl: './name.html'`, never inline `template`.

## Forms — Signal Forms (`@angular/forms/signals`)
Stable in Angular 22. **All new/edited forms use Signal Forms**, not reactive or `ngModel` forms. The template directive is `[formField]` (class `FormField`) — NOT `[control]`.

### Recipe
1. `model = signal<T>({...})`
2. `f = form(this.model, path => { required(path.name); ... })`
3. Template: import `FormField`; `<input mpInput [formField]="f.name">`, `<mp-select [formField]="f.x">`
4. Read state: `f().valid()`/`.invalid()`/`.dirty()`/`.touched()`/`.submitting()`; per-field `f.name().value()`/`.errors()`

### Validation lives in the schema (single source)
Built-ins: `required`, `minLength`/`maxLength`, `min`/`max`, `pattern`, `email`, `minDate`/`maxDate`.
- These forward to the native attr too — **never set `maxlength`/`required`/`disabled`/`invalid` on a `[formField]` element (NG8022)**; put them in the schema.
- Custom: return `{ kind, message }` where `message` is an i18n key. Blank guard alongside `required`:
  `validate(path.name, ({value}) => value().trim().length ? undefined : {kind:'blank', message:'common.validation.required'})`
- Cross-field: `validate(path.confirm, ({value, valueOf}) => value()===valueOf(path.password) ? undefined : {kind:'mismatch', message:'...'})`
- Whole-form/field disable: `disabled(path, () => this.readOnly())` (root cascades to descendants).
- Conditional/step rules: `applyWhen(path, () => this.step()==='form', p => { required(p.street); })`. Schema logic is **reactive** — NEVER branch on a signal directly in the schema body (it runs once at init, before `ngOnInit`); always wrap signal-conditioned rules in `applyWhen`/logic fns.

### Arrays — `applyEach`
- `model = signal<{items: Row[]}>`; schema `applyEach(path.items, item => { required(item.title); maxLength(item.notes, 256); })`
- Template iterates the field tree: `@for (field of f.items; track $index; let i = $index) { @let row = field().value(); <input [formField]="field.title" /> }`
- Add/remove by mutating the model: `model.update(m => ({...m, items:[...m.items, blank]}))` / `.filter`.
- Model dates as `Date` (controls bind `Date`), convert `?.toISOString()` in the payload.

### Submission — `submit()`
```ts
save(): Promise<boolean> {
  return submit(this.f, async () => {
    try { await firstValueFrom(this.data.create(payload)); }
    catch (err) { this.notify.apiError(err, '...'); return; }
    this.notify.success('...'); this.saved.emit();
  });
}
```
- `submit()` validates, **marks all fields touched** (reveals errors), sets `f().submitting()`, and guards concurrency. Use `f().submitting()` for spinners — no manual `saving` signal.
- Bridge RxJS with `firstValueFrom`. A ternary over two differently-typed observables breaks inference (TS2345) → split into if/else.
- Need focus-on-invalid: `submit(f, { action, onInvalid: () => focusFirstInvalid(host) })`.
- Success returns nothing (`ValidationSuccess = null|undefined|void`).

### Button rule (app-wide)
`[disabled]="!isDirty() || f().submitting()"` — **validity drops out** of the disable; clicking an incomplete form runs `submit()`, which reveals the errors. (Exception: multi-step dialogs with a step gate + no error display keep their `canSave` gate.)

### `reset()` on reopen — REQUIRED
After `model.set(...)`, call `this.f().reset()` to clear touched/dirty (it does not change the value). `submit()` marks fields touched, so without this a reopened dialog flashes every error and dirty-gated logic breaks.

### Dirty tracking
`f().dirty()` only tracks control interaction. For forms with button-driven collections (pickers, chips, arrays edited outside inputs), keep a **snapshot-based `isDirty`** (`JSON.stringify(model)` vs a saved snapshot; `JSON.stringify` serialises `Date`→ISO consistently).

### Side-effects on a field value
Can't use `(ngModelChange)` with `[formField]`. React with an `effect`, guarding against no-op writes:
```ts
effect(() => { if (!this.model().flag) untracked(() =>
  this.model.update(m => alreadyCleared(m) ? m : { ...m, dependent: null })); });
```

### Design system / wrapper
- `<mp-form-field [label] [hint]><control [formField]="f.x"/></mp-form-field>` → label + schema-driven required asterisk + touched-gated error + aria. Field is inferred from the projected control; don't pass it.
- Form-ready controls: `mp-input`, `mp-select`, `mp-input-number`, `mp-multi-select`, `mp-date-picker`, `mp-checkbox`, `mp-textarea`, plus `lookup-select` & `gender-select` (FormValueControl via a `value = model()`), and native `<input type="checkbox"/type="number" [formField]>`.
- Dynamic control loops (flag grid, summary-date list): store the child FieldTree in the loop config `{ key, field: FieldTree<T> }` and bind `[formField]="cfg.field"` — not get/set closures. `typeof this.f.x` is invalid in a type position → use the `FieldTree<T>` type.

### When NOT to use Signal Forms
Stay imperative when a form has aggregate cross-array validation, per-user-action cascades that would fight reactive effects (e.g. auto-calc that must not loop), nested arrays threaded through child components, or is a collection editor delegating to child components. Precedents left imperative: staff **employment** panel, staff **contact** panel, **academic-year-wizard**.

### Specs
Mock services + `TestBed.overrideComponent(C, {set:{template:''}})`; drive `(component as Internals).model.update(...)`; assert `f().valid()/invalid()/submitting()` and `save()` payloads. Effects need `fixture.detectChanges()` to flush. Fire-and-forget saves (`void this.doX()`) need `await flush()` (`new Promise(r=>setTimeout(r,0))`) before asserting the API call.
