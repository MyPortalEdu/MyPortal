import { ChangeDetectionStrategy, Component, inject, input, output, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MpButton, MpDialog, MpInput } from '@myportal/ui';
import { Subject, debounceTime, distinctUntilChanged, of, switchMap } from 'rxjs';
import { TranslocoDirective, TranslocoPipe } from '@jsverse/transloco';

import { UsersDataService } from '../../../services/users-data.service';
import { PersonSearchResponse } from '../../../types/user';

/**
 * General Person search + picker for linking a Person to a user account. Trigger button opens a
 * popover with a debounced search box backed by /api/people/search (all person types). Emits the
 * chosen Person. Translation keys live under `common.personPicker.*` in the root scope.
 */
@Component({
  selector: 'mp-person-picker',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DatePipe, FormsModule, MpButton, MpDialog, MpInput, TranslocoDirective, TranslocoPipe],
  templateUrl: './person-picker.html',
})
export class PersonPicker {
  private readonly data = inject(UsersDataService);

  readonly buttonLabel = input<string | undefined>(undefined);

  readonly picked = output<PersonSearchResponse>();

  protected readonly visible = signal(false);
  protected readonly query = signal('');
  protected readonly results = signal<PersonSearchResponse[]>([]);
  protected readonly loading = signal(false);
  protected readonly searched = signal(false);

  private readonly search$ = new Subject<string>();

  constructor() {
    this.search$
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        switchMap(q => (q.trim().length < 2 ? of([] as PersonSearchResponse[]) : this.data.searchPeople(q.trim()))),
        takeUntilDestroyed(),
      )
      .subscribe(results => {
        this.results.set(results ?? []);
        this.loading.set(false);
        this.searched.set(true);
      });
  }

  onQueryChange(q: string): void {
    this.query.set(q);
    const active = q.trim().length >= 2;
    this.loading.set(active);
    if (!active) {
      this.results.set([]);
      this.searched.set(false);
    }
    this.search$.next(q);
  }

  displayName(p: PersonSearchResponse): string {
    const first = p.preferredFirstName ?? p.firstName;
    const last = p.preferredLastName ?? p.lastName;
    return `${first} ${last}`.trim();
  }

  select(p: PersonSearchResponse): void {
    this.picked.emit(p);
    this.visible.set(false);
  }
}
