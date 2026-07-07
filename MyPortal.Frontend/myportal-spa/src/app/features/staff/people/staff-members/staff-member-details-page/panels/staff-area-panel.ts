import { Signal } from '@angular/core';

import { LookupResponse } from '../../../../../../shared/types/lookup';

/**
 * Contract the staff-details shell drives. The shell renders exactly one area panel at a time
 * (via the template `@switch`) and delegates the Edit / Save / Cancel header actions and the
 * dirty guard to whichever panel is mounted. Each panel component `extends StaffAreaPanel` and
 * aliases itself with `{ provide: StaffAreaPanel, useExisting: forwardRef(() => XPanel) }` so the
 * shell can pick it up with `viewChild(StaffAreaPanel)`.
 *
 * The class is abstract but ships the small formatting/serialisation helpers every panel shares,
 * so a concrete panel only has to implement its own load/save/dirty logic.
 */
export abstract class StaffAreaPanel {
  /** True while the panel is in edit mode (shell shows Save/Cancel instead of Edit). */
  abstract readonly editing: Signal<boolean>;
  /** True when the edit form differs from the last-loaded baseline. */
  abstract readonly dirty: Signal<boolean>;
  /** True when the edit form is safe to save. */
  abstract readonly valid: Signal<boolean>;
  /** True when the current viewer may edit this area (drives whether Edit renders at all). */
  abstract readonly canEdit: Signal<boolean>;
  /** True while a save is in flight (drives the header Save button's loading/disabled state). */
  abstract readonly saving: Signal<boolean>;

  abstract startEdit(): void;
  abstract cancel(): void;
  abstract save(): Promise<void>;

  // ─── Shared helpers ──────────────────────────────────────────────────────────

  // p-datepicker binds Date; models carry ISO strings. Cached so the same string always yields the
  // same Date instance — a stable reference the datepicker won't treat as a change (see NG note in
  // the project memory on datepicker state).
  private readonly dateCache = new Map<string, Date>();

  protected toDate(value: string | null | undefined): Date | null {
    if (!value) return null;
    let date = this.dateCache.get(value);
    if (!date) {
      date = new Date(value);
      this.dateCache.set(value, date);
    }
    return date;
  }

  // Trim to null: empty/whitespace-only strings become null at the wire boundary.
  protected normalise(value: string | null | undefined): string | null {
    if (value == null) return null;
    const trimmed = value.trim();
    return trimmed.length === 0 ? null : trimmed;
  }

  // Resolve a lookup id to its description for read-only display; em dash if unset.
  protected lookupLabel(list: LookupResponse[], id: string | null | undefined): string {
    if (!id) return '—';
    return list.find(x => x.id === id)?.description ?? '—';
  }

  // Comma-joined descriptions for a set of lookup ids (read-only multi-select view).
  protected selectedLabels(list: LookupResponse[], ids: string[]): string {
    if (!ids.length) return '—';
    const byId = new Map(list.map(x => [x.id, x.description]));
    return ids.map(id => byId.get(id)).filter(Boolean).join(', ') || '—';
  }
}
