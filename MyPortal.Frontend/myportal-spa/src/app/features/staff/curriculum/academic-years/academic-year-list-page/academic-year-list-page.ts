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
import { TranslocoDirective, TranslocoService, provideTranslocoScope } from '@jsverse/transloco';

import { PageHeader } from '../../../../../shared/components/page-header/page-header';
import { HeaderAction } from '../../../../../shared/types/header-action.type';
import { AcademicYearsDataService } from '../../../../../shared/services/academic-years-data.service';
import { ConfirmationDialog } from '../../../../../core/services/confirmation.service';
import { NotificationService } from '../../../../../core/services/notification.service';
import { AcademicYearService } from '../../../../../core/services/academic-year-service';
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
  private readonly notify = inject(NotificationService);
  private readonly confirm = inject(ConfirmationDialog);
  private readonly transloco = inject(TranslocoService);

  readonly years = signal<AcademicYearSummary[]>([]);
  readonly loading = signal(false);
  readonly wizardOpen = signal(false);

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
    this.wizardOpen.set(true);
  }

  closeWizard(): void {
    this.wizardOpen.set(false);
  }

  onWizardSaved(): void {
    this.wizardOpen.set(false);
    this.refresh();
  }

  openEdit(_year: AcademicYearSummary): void {
    // TODO: launch the edit wizard once built. Edit reuses the same wizard
    // pre-filled via GetByIdAsync.
  }

  async deleteYear(year: AcademicYearSummary): Promise<void> {
    if (year.isLocked) return;

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
        this.refresh();
      },
      error: err => this.notify.apiError(err,
        this.transloco.translate('academic-years.deleteError')),
    });
  }
}
