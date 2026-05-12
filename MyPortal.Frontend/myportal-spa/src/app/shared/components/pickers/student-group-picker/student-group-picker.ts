import { ChangeDetectionStrategy, Component, inject, input, output, signal, viewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Button } from 'primeng/button';
import { Popover } from 'primeng/popover';
import { Select } from 'primeng/select';
import { TableLazyLoadEvent, TableModule } from 'primeng/table';
import { TranslocoDirective, TranslocoPipe, TranslocoService } from '@jsverse/transloco';

import { StudentGroupsDataService } from '../../../services/student-groups-data.service';
import { StudentGroupKind, StudentGroupSummaryResponse } from '../../../types/student-group';
import { toQueryKitParams } from '../../../utils/primeng-querykit';

interface KindOption {
  label: string;
  value: StudentGroupKind;
}

/**
 * Generic student-group browser. Renders a trigger button; clicking opens a
 * popover with a lazy-loaded p-table backed by /api/studentgroups, with column
 * filters on Code/Name and a Kind dropdown filter.
 *
 * Single-select mode (default): clicking a row emits and closes the popover
 * immediately. Multi-select (`allowMultiple` true): checkboxes appear and a
 * Confirm button emits the array; pre-existing selections in the parent are
 * not surfaced — this is an "add new" picker, not a stateful "manage" widget.
 *
 * Translation keys live under `common.studentGroupPicker.*` in the root scope.
 */
@Component({
  selector: 'mp-student-group-picker',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule, Button, Popover, Select, TableModule, TranslocoDirective, TranslocoPipe],
  templateUrl: './student-group-picker.html',
})
export class StudentGroupPicker {
  private readonly data = inject(StudentGroupsDataService);
  private readonly transloco = inject(TranslocoService);

  readonly academicYearId = input.required<string>();
  readonly allowMultiple = input(false);
  /** Optional override for the trigger button label. Defaults to "Add student group…". */
  readonly buttonLabel = input<string | undefined>(undefined);
  /**
   * Group ids the caller has already picked. The picker doesn't *filter* on
   * these (filtering client-side breaks pagination and totalRecords) — it
   * just dims the row and disables selection. Dedup of the final picked set
   * is the caller's responsibility.
   */
  readonly excludeIds = input<readonly string[]>([]);

  readonly picked = output<StudentGroupSummaryResponse[]>();

  private readonly op = viewChild<Popover>('op');

  protected readonly rows = signal<StudentGroupSummaryResponse[]>([]);
  protected readonly totalRecords = signal(0);
  protected readonly loading = signal(false);
  // Multi-select state. PrimeNG's p-table mutates this array reference, so we
  // hold a plain array bound via [(selection)] and reassign on confirm/cancel.
  protected selection: StudentGroupSummaryResponse[] = [];

  // Cached per-render: building these in the template would re-translate every
  // change-detection pass. Refreshed when the user opens the popover so a
  // language change between opens still gets picked up.
  protected kindOptions: KindOption[] = [];

  open(event: Event): void {
    this.refreshKindOptions();
    this.op()?.toggle(event);
  }

  close(): void {
    this.op()?.hide();
  }

  load(event: TableLazyLoadEvent): void {
    this.loading.set(true);
    const params = toQueryKitParams(event);
    this.data.list(this.academicYearId(), params).subscribe({
      next: page => {
        this.rows.set(page.items ?? []);
        this.totalRecords.set(page.totalItems ?? 0);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  isAlreadyPicked(row: StudentGroupSummaryResponse): boolean {
    return this.excludeIds().includes(row.id);
  }

  // Single-select path. PrimeNG fires (onRowSelect) when selectionMode="single".
  onRowSelect(row: StudentGroupSummaryResponse): void {
    if (this.allowMultiple()) return;
    if (this.isAlreadyPicked(row)) return;
    this.picked.emit([row]);
    this.close();
  }

  confirm(): void {
    if (!this.allowMultiple() || this.selection.length === 0) return;
    const picked = [...this.selection];
    this.selection = [];
    this.picked.emit(picked);
    this.close();
  }

  cancel(): void {
    this.selection = [];
    this.close();
  }

  kindLabel(kind: StudentGroupKind): string {
    return this.transloco.translate(`common.studentGroupPicker.kind.${StudentGroupKind[kind]}`);
  }

  private refreshKindOptions(): void {
    // Order: House, YearGroup, RegGroup, CurriculumGroup, Other. Order matters
    // for the dropdown — bulletins-relevant kinds (Houses/years/reg groups)
    // bubble up first.
    const kinds: StudentGroupKind[] = [
      StudentGroupKind.House,
      StudentGroupKind.YearGroup,
      StudentGroupKind.RegGroup,
      StudentGroupKind.CurriculumGroup,
      StudentGroupKind.Other,
    ];
    this.kindOptions = kinds.map(value => ({ value, label: this.kindLabel(value) }));
  }
}
