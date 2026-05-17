import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { DatePipe } from '@angular/common';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { Skeleton } from 'primeng/skeleton';
import { Tag } from 'primeng/tag';
import { Tooltip } from 'primeng/tooltip';
import { TranslocoDirective, TranslocoService, provideTranslocoScope } from '@jsverse/transloco';

import { PageHeader } from '../../../../../shared/components/page-header/page-header';
import { HeaderAction } from '../../../../../shared/types/header-action.type';
import { AcademicYearsDataService } from '../../../../../shared/services/academic-years-data.service';
import { ConfirmationDialog } from '../../../../../core/services/confirmation.service';
import { NotificationService } from '../../../../../core/services/notification.service';
import { AcademicYearService } from '../../../../../core/services/academic-year-service';
import { SelectedAcademicYearService } from '../../../../../core/services/selected-academic-year-service';
import { AcademicYearSummary } from '../../../../../core/types/academic-year-summary';
import { AcademicYearWizardDialog } from '../academic-year-wizard-dialog/academic-year-wizard-dialog';

@Component({
  selector: 'mp-academic-year-list-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    DatePipe,
    Button,
    Card,
    TableModule,
    Skeleton,
    Tag,
    Tooltip,
    PageHeader,
    AcademicYearWizardDialog,
    TranslocoDirective,
  ],
  providers: [provideTranslocoScope('academic-years')],
  templateUrl: './academic-year-list-page.html',
})
export class AcademicYearListPage implements OnInit {
  private readonly data = inject(AcademicYearsDataService);
  private readonly currentYearCache = inject(AcademicYearService);
  private readonly selectedYear = inject(SelectedAcademicYearService);
  private readonly notify = inject(NotificationService);
  private readonly confirm = inject(ConfirmationDialog);
  private readonly transloco = inject(TranslocoService);

  readonly years = signal<AcademicYearSummary[]>([]);
  readonly loading = signal(false);
  readonly wizardOpen = signal(false);
  // Non-null when the wizard is open for editing an existing year. The wizard
  // reads this on open to decide whether to fetch+prefill (edit) or start
  // blank (create).
  readonly editYearId = signal<string | null>(null);

  readonly headerActions = computed<HeaderAction[]>(() => [
    {
      label: this.transloco.translate('common.new'),
      icon: 'fa-solid fa-plus',
      severity: 'primary',
      command: () => this.openCreateWizard(),
    },
  ]);

  ngOnInit(): void {
    this.refresh();
  }

  refresh(): void {
    this.loading.set(true);
    this.data.list().subscribe({
      next: rows => {
        this.years.set(rows ?? []);
        this.loading.set(false);
      },
      error: err => {
        this.loading.set(false);
        this.notify.apiError(err, this.transloco.translate('academic-years.loadError'));
      },
    });
  }

  openCreateWizard(): void {
    this.editYearId.set(null);
    this.wizardOpen.set(true);
  }

  closeWizard(): void {
    this.wizardOpen.set(false);
    this.editYearId.set(null);
  }

  onWizardSaved(): void {
    this.wizardOpen.set(false);
    this.editYearId.set(null);
    this.refresh();
    // Rename / date change on the selected year wouldn't otherwise reach the
    // topbar — its label is bound to the cached summary. A newly-created year
    // that happens to cover today should also surface there.
    this.selectedYear.revalidate();
  }

  openEdit(year: AcademicYearSummary): void {
    if (!this.canMutate(year)) return;
    this.editYearId.set(year.id);
    this.wizardOpen.set(true);
  }

  // Mirror of the BE rule in AcademicYearService.UpdateAcademicYear / Delete:
  // a locked year is permanently frozen, and a year that has already started
  // (earliest term start ≤ today) is also frozen. Surface both upfront so the
  // user doesn't click through to a guaranteed 400.
  canMutate(year: AcademicYearSummary): boolean {
    if (year.isLocked) return false;
    if (!year.startDate) return true;
    const start = new Date(year.startDate);
    const today = new Date();
    start.setHours(0, 0, 0, 0);
    today.setHours(0, 0, 0, 0);
    return start.getTime() > today.getTime();
  }

  mutateBlockReason(year: AcademicYearSummary): string {
    if (year.isLocked) return this.transloco.translate('academic-years.lockedHint');
    if (!this.canMutate(year)) return this.transloco.translate('academic-years.startedHint');
    return '';
  }

  async deleteYear(year: AcademicYearSummary): Promise<void> {
    if (!this.canMutate(year)) return;

    const ok = await this.confirm.danger({
      message: this.transloco.translate('academic-years.deleteConfirm', { name: year.name }),
    });
    if (!ok) return;

    this.data.delete(year.id).subscribe({
      next: () => {
        this.notify.success(this.transloco.translate('academic-years.deletedToast'));
        // Bust the cached current-AY lookup — deleting the current year
        // changes what /api/academicyears/current returns.
        this.currentYearCache.clearCache();
        // If the deleted year is the topbar's selected year, fall back to the
        // calendar-current year instead of leaving a dangling label.
        this.selectedYear.revalidate();
        this.refresh();
      },
      error: err => this.notify.apiError(err,
        this.transloco.translate('academic-years.deleteError')),
    });
  }
}
