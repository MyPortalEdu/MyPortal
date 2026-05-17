import { Injectable, computed, inject, signal } from '@angular/core';
import { combineLatest, of, take } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { AcademicYearService } from './academic-year-service';
import { MeService } from './me-service';
import { AcademicYearsDataService } from '../../shared/services/academic-years-data.service';
import { AcademicYearSummary } from '../types/academic-year-summary';

const STORAGE_PREFIX = 'mp.selectedAcademicYearId:';

// Holds the academic year the staff user is currently "viewing" — distinct from
// the calendar-current year (AcademicYearService.getCurrent), which is the
// system fact "what year covers today". The selection is per-user, persisted to
// localStorage so it survives a refresh, cleared on logout so the next sign-in
// starts fresh. Init validates the persisted id against the latest list — if
// the year was deleted, we silently fall back to the calendar-current year.
//
// Staff-only by design; the service still works for student/parent sessions
// but no UI surface exposes it to them.
@Injectable({ providedIn: 'root' })
export class SelectedAcademicYearService {
  private readonly me = inject(MeService);
  private readonly years = inject(AcademicYearsDataService);
  private readonly currentYear = inject(AcademicYearService);

  private readonly _selected = signal<AcademicYearSummary | null>(null);
  private readonly _initialized = signal<boolean>(false);
  private _currentUserId: string | null = null;

  readonly selected = this._selected.asReadonly();
  readonly selectedId = computed(() => this._selected()?.id ?? null);
  readonly initialized = this._initialized.asReadonly();

  // Resolve the active selection. Called once after auth completes (from the
  // app shell). Subsequent calls are no-ops — repeated mounts of the shell
  // shouldn't re-seed and clobber a deliberate user choice.
  init(): void {
    if (this._initialized()) return;

    this.me.me().pipe(take(1)).subscribe(me => {
      this._currentUserId = me.id;

      const storedId = readStored(me.id);
      combineLatest([
        this.years.list().pipe(catchError(() => of<AcademicYearSummary[]>([]))),
        this.currentYear.getCurrent().pipe(catchError(() => of<AcademicYearSummary | null>(null))),
      ]).pipe(take(1)).subscribe(([list, current]) => {
        const persisted = storedId ? list.find(y => y.id === storedId) ?? null : null;
        const chosen = persisted ?? current;

        this._selected.set(chosen);
        // Stale storage gets cleared when we couldn't find the persisted id —
        // otherwise it'd keep looking ghosted on the next refresh.
        if (storedId && !persisted) {
          writeStored(me.id, null);
        }
        this._initialized.set(true);
      });
    });
  }

  select(year: AcademicYearSummary | null): void {
    this._selected.set(year);
    if (this._currentUserId) {
      writeStored(this._currentUserId, year?.id ?? null);
    }
  }

  // Wipe the persisted selection — called on logout so the next sign-in (even
  // as the same user) starts on the calendar-current year. Token refresh /
  // silent re-auth doesn't go through this path, so a long-running session
  // keeps the user's choice through background refreshes.
  clear(): void {
    if (this._currentUserId) {
      writeStored(this._currentUserId, null);
    }
    this._selected.set(null);
    this._initialized.set(false);
    this._currentUserId = null;
  }
}

function readStored(userId: string): string | null {
  try {
    return localStorage.getItem(STORAGE_PREFIX + userId);
  } catch {
    return null;
  }
}

function writeStored(userId: string, id: string | null): void {
  try {
    if (id) {
      localStorage.setItem(STORAGE_PREFIX + userId, id);
    } else {
      localStorage.removeItem(STORAGE_PREFIX + userId);
    }
  } catch {
    // localStorage can throw in private-browsing modes / when quota-exceeded;
    // failing silently is fine because the in-memory signal still works for
    // the current session.
  }
}
