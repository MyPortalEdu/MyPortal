import { ChangeDetectionStrategy, Component, inject, input, output, signal, viewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  MpButton,
  MpDialog,
  MpSelect,
  MpTable,
  MpTableHeader,
  MpTableBody,
  MpTableEmpty,
  MpSortable,
  MpSortIcon,
  MpSelectableRow,
  MpColumnFilter,
  MpTableCheckbox,
  MpTableHeaderCheckbox,
  type MpTableLazyLoadEvent,
} from '@myportal/ui';
import { TranslocoDirective, TranslocoPipe, TranslocoService } from '@jsverse/transloco';

import { StudentGroupsDataService } from '../../../services/student-groups-data.service';
import { StudentGroupKind, StudentGroupSummaryResponse } from '../../../types/student-group';
import { toQueryKitParams } from '../../../utils/querykit';

interface KindOption {
  label: string;
  value: StudentGroupKind;
}

@Component({
  selector: 'mp-student-group-picker',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    FormsModule,
    MpButton,
    MpDialog,
    MpSelect,
    MpTable,
    MpTableHeader,
    MpTableBody,
    MpTableEmpty,
    MpSortable,
    MpSortIcon,
    MpSelectableRow,
    MpColumnFilter,
    MpTableCheckbox,
    MpTableHeaderCheckbox,
    TranslocoDirective,
    TranslocoPipe,
  ],
  templateUrl: './student-group-picker.html',
})
export class StudentGroupPicker {
  private readonly data = inject(StudentGroupsDataService);
  private readonly transloco = inject(TranslocoService);

  readonly academicYearId = input.required<string>();
  readonly allowMultiple = input(false);
  readonly buttonLabel = input<string | undefined>(undefined);
  readonly excludeIds = input<readonly string[]>([]);

  readonly picked = output<StudentGroupSummaryResponse[]>();

  protected readonly visible = signal(false);
  protected readonly rows = signal<StudentGroupSummaryResponse[]>([]);
  protected readonly totalRecords = signal(0);
  protected readonly loading = signal(false);
  protected selection: StudentGroupSummaryResponse[] = [];

  private readonly table = viewChild(MpTable);
  protected readonly kindFilter = signal<StudentGroupKind | null>(null);

  protected readonly rowDisabledFn = (row: unknown): boolean =>
    this.isAlreadyPicked(row as StudentGroupSummaryResponse);

  protected kindOptions: KindOption[] = [];

  open(): void {
    this.refreshKindOptions();
    this.visible.set(true);
  }

  close(): void {
    this.visible.set(false);
  }

  onKindFilter(value: StudentGroupKind | null): void {
    this.kindFilter.set(value ?? null);
    this.table()?.filter(value ?? null, 'kind', 'equals');
  }

  load(event: MpTableLazyLoadEvent): void {
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
