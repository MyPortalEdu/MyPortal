import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { Router } from '@angular/router';
import { MpBadge, MpButton, MpCard, MpSkeleton, MpTable, MpTableBody, MpTableHeader } from '@myportal/ui';
import { firstValueFrom } from 'rxjs';
import { TranslocoDirective, TranslocoService, provideTranslocoScope } from '@jsverse/transloco';

import { PageHeader } from '../../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../../shared/components/empty-state/empty-state';
import { HeaderAction } from '../../../../../shared/types/header-action.type';
import { ConfirmationDialog } from '../../../../../core/services/confirmation.service';
import { NotificationService } from '../../../../../core/services/notification.service';
import { ServiceTermsDataService } from '../../../../../shared/services/service-terms-data.service';
import { ServiceTermResponse, ServiceTermsResponse } from '../../../../../shared/types/staff-setup';
import { ServiceTermEditorDialog } from '../service-term-editor-dialog/service-term-editor-dialog';

const MONTHS = [
  'january', 'february', 'march', 'april', 'may', 'june',
  'july', 'august', 'september', 'october', 'november', 'december',
];

@Component({
  selector: 'mp-service-term-list-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    MpBadge,
    MpButton,
    MpCard,
    MpSkeleton,
    MpTable,
    MpTableHeader,
    MpTableBody,
    PageHeader,
    EmptyState,
    ServiceTermEditorDialog,
    TranslocoDirective,
  ],
  providers: [provideTranslocoScope('staff-setup')],
  templateUrl: './service-term-list-page.html',
})
export class ServiceTermListPage implements OnInit {
  private readonly router = inject(Router);
  private readonly data = inject(ServiceTermsDataService);
  private readonly notify = inject(NotificationService);
  private readonly confirm = inject(ConfirmationDialog);
  private readonly transloco = inject(TranslocoService);

  protected readonly loading = signal(false);
  protected readonly area = signal<ServiceTermsResponse | null>(null);
  protected readonly editorOpen = signal(false);
  protected readonly editing = signal<ServiceTermResponse | null>(null);

  protected readonly serviceTerms = computed(() => this.area()?.serviceTerms ?? []);
  protected readonly canEdit = computed(() => this.area()?.canEdit ?? false);
  protected readonly schemes = computed(() => this.area()?.superannuationSchemes ?? []);

  protected readonly headerActions = computed<HeaderAction[]>(() =>
    this.canEdit()
      ? [
          {
            label: this.transloco.translate('staff-setup.serviceTerms.add'),
            icon: 'fa-solid fa-plus',
            severity: 'primary',
            command: () => this.openNew(),
          },
        ]
      : [],
  );

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.data.getServiceTerms().subscribe({
      next: row => {
        this.area.set(row);
        this.loading.set(false);
      },
      error: err => {
        this.loading.set(false);
        this.notify.apiError(err, this.transloco.translate('staff-setup.serviceTerms.loadError'));
      },
    });
  }

  protected openNew(): void {
    this.editing.set(null);
    this.editorOpen.set(true);
  }

  protected openDetails(term: ServiceTermResponse): void {
    void this.router.navigate(['/staff/setup/service-terms', term.id]);
  }

  protected onSaved(): void {
    this.editorOpen.set(false);
    this.load();
  }

  protected incrementLabel(term: ServiceTermResponse): string {
    if (!term.spinalProgression || !term.incrementMonth) return '—';
    const month = this.transloco.translate(`staff-setup.months.${MONTHS[term.incrementMonth - 1]}`);
    return term.incrementDay ? `${term.incrementDay} ${month}` : month;
  }

  protected async confirmDelete(term: ServiceTermResponse): Promise<void> {
    const ok = await this.confirm.confirm({
      header: this.transloco.translate('staff-setup.serviceTerms.deleteHeader'),
      message: this.transloco.translate('staff-setup.serviceTerms.deleteConfirm'),
      acceptLabel: this.transloco.translate('common.delete'),
      acceptSeverity: 'danger',
    });

    if (!ok) return;

    try {
      await firstValueFrom(this.data.delete(term.id));
      this.notify.success(this.transloco.translate('staff-setup.serviceTerms.deletedToast'));
      this.load();
    } catch (err) {
      this.notify.apiError(err, this.transloco.translate('staff-setup.serviceTerms.deleteError'));
    }
  }
}
