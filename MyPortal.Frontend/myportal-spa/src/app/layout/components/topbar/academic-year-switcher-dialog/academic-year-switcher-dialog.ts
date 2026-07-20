import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  input,
  output,
  signal,
  untracked,
} from '@angular/core';
import { MpDialog, MpSpinner, MpBadge } from '@myportal/ui';
import { TranslocoDirective, TranslocoPipe, TranslocoService } from '@jsverse/transloco';

import { SelectedAcademicYearService } from '../../../../core/services/selected-academic-year-service';
import { AcademicYearService } from '../../../../core/services/academic-year-service';
import { AcademicYearsDataService } from '../../../../shared/services/academic-years-data.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { AcademicYearSummary } from '../../../../core/types/academic-year-summary';

@Component({
  selector: 'mp-academic-year-switcher-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MpDialog, MpSpinner, MpBadge, TranslocoDirective, TranslocoPipe],
  templateUrl: './academic-year-switcher-dialog.html',
})
export class AcademicYearSwitcherDialog {
  private readonly years = inject(AcademicYearsDataService);
  private readonly currentService = inject(AcademicYearService);
  private readonly selectedService = inject(SelectedAcademicYearService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);

  readonly visible = input.required<boolean>();
  readonly closed = output<void>();

  readonly loading = signal<boolean>(false);
  readonly rows = signal<AcademicYearSummary[]>([]);
  readonly currentId = signal<string | null>(null);

  readonly selectedId = computed(() => this.selectedService.selectedId());

  constructor() {
    effect(() => {
      if (this.visible()) {
        untracked(() => this.refresh());
      }
    });
  }

  refresh(): void {
    this.loading.set(true);
    this.years.list().subscribe({
      next: rows => {
        const sorted = [...(rows ?? [])].sort((a, b) => {
          const aStart = a.startDate ? new Date(a.startDate).getTime() : 0;
          const bStart = b.startDate ? new Date(b.startDate).getTime() : 0;
          return bStart - aStart;
        });
        this.rows.set(sorted);
        this.loading.set(false);
      },
      error: err => {
        this.loading.set(false);
        this.notify.apiError(err, this.transloco.translate('topbar.yearSwitcher.loadError'));
      },
    });

    this.currentService.getCurrent().subscribe({
      next: c => this.currentId.set(c?.id ?? null),
      error: () => this.currentId.set(null),
    });
  }

  pick(year: AcademicYearSummary): void {
    this.selectedService.select(year);
    this.closed.emit();
  }

  onHide(): void {
    this.closed.emit();
  }
}
