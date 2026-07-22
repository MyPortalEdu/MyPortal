import { Signal } from '@angular/core';

import { LookupResponse } from '../../../../../../shared/types/lookup';

export abstract class ContactAreaPanel {
  abstract readonly editing: Signal<boolean>;
  abstract readonly dirty: Signal<boolean>;
  abstract readonly valid: Signal<boolean>;
  abstract readonly canEdit: Signal<boolean>;
  abstract readonly saving: Signal<boolean>;

  // The panel surfaces its own invalid state inline (Signal Forms field errors, revealed on
  // submit), so the container keeps Save enabled while invalid and a click invokes the validation
  // display rather than silently blocking.
  readonly explainsInvalid: boolean = true;

  abstract startEdit(): void;
  abstract cancel(): void;
  abstract save(): Promise<void>;

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

  protected normalise(value: string | null | undefined): string | null {
    if (value == null) return null;
    const trimmed = value.trim();
    return trimmed.length === 0 ? null : trimmed;
  }

  protected lookupLabel(list: LookupResponse[], id: string | null | undefined): string {
    if (!id) return '—';
    return list.find(x => x.id === id)?.description ?? '—';
  }

  protected selectedLabels(list: LookupResponse[], ids: string[]): string {
    if (!ids.length) return '—';
    const byId = new Map(list.map(x => [x.id, x.description]));
    return ids.map(id => byId.get(id)).filter(Boolean).join(', ') || '—';
  }
}
