import { Injectable, computed, inject, signal } from '@angular/core';
import { combineLatest, of, take } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { AcademicYearService } from './academic-year-service';
import { MeService } from './me-service';
import { AcademicYearsDataService } from '../../shared/services/academic-years-data.service';
import { AcademicYearSummary } from '../types/academic-year-summary';

const STORAGE_PREFIX = 'mp.selectedAcademicYearId:';

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

  revalidate(): void {
    if (!this._initialized() || !this._currentUserId) return;

    const userId = this._currentUserId;
    const wantedId = this.selectedId();
    combineLatest([
      this.years.list().pipe(catchError(() => of<AcademicYearSummary[]>([]))),
      this.currentYear.getCurrent().pipe(catchError(() => of<AcademicYearSummary | null>(null))),
    ]).pipe(take(1)).subscribe(([list, current]) => {
      const matched = wantedId ? list.find(y => y.id === wantedId) ?? null : null;
      const chosen = matched ?? current;
      this._selected.set(chosen);

      if (wantedId && !matched) {
        writeStored(userId, chosen?.id ?? null);
      }
    });
  }

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
  }
}
