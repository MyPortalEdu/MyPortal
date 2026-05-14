import { ChangeDetectionStrategy, Component, inject, input, output, signal, viewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Button } from 'primeng/button';
import { Popover } from 'primeng/popover';
import { TableLazyLoadEvent, TableModule } from 'primeng/table';
import { TranslocoDirective, TranslocoPipe } from '@jsverse/transloco';

import { LocalAuthoritiesDataService } from '../../../services/local-authorities-data.service';
import { LocalAuthoritySummaryResponse } from '../../../types/school';
import { toQueryKitParams } from '../../../utils/primeng-querykit';

/**
 * Single-select local-authority browser. Trigger button opens a popover with
 * a lazy-loaded p-table backed by /api/localauthorities, with column filters
 * on LeaCode and Name. Modelled on StudentGroupPicker but always single-pick:
 * a school has at most one LA so the multi-select branch is unnecessary.
 *
 * Translation keys live under `common.localAuthorityPicker.*` in the root scope.
 */
@Component({
  selector: 'mp-local-authority-picker',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule, Button, Popover, TableModule, TranslocoDirective, TranslocoPipe],
  templateUrl: './local-authority-picker.html',
})
export class LocalAuthorityPicker {
  private readonly data = inject(LocalAuthoritiesDataService);

  /** Optional override for the trigger button label. Defaults to "Pick a local authority…". */
  readonly buttonLabel = input<string | undefined>(undefined);

  readonly picked = output<LocalAuthoritySummaryResponse>();

  private readonly op = viewChild<Popover>('op');

  protected readonly rows = signal<LocalAuthoritySummaryResponse[]>([]);
  protected readonly totalRecords = signal(0);
  protected readonly loading = signal(false);

  open(event: Event): void {
    this.op()?.toggle(event);
  }

  close(): void {
    this.op()?.hide();
  }

  load(event: TableLazyLoadEvent): void {
    this.loading.set(true);
    const params = toQueryKitParams(event);
    this.data.list(params).subscribe({
      next: page => {
        this.rows.set(page.items ?? []);
        this.totalRecords.set(page.totalItems ?? 0);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  onRowSelect(row: LocalAuthoritySummaryResponse): void {
    this.picked.emit(row);
    this.close();
  }
}
