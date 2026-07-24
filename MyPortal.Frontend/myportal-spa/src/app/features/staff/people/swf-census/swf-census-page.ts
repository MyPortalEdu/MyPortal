import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { NgClass } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  MpBadge,
  MpButton,
  MpCard,
  MpDatePicker,
  MpSkeleton,
  MpTable,
  MpTableBody,
  MpTableEmpty,
  MpTableHeader,
} from '@myportal/ui';
import { TranslocoDirective, TranslocoService, provideTranslocoScope } from '@jsverse/transloco';

import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { NotificationService } from '../../../../core/services/notification.service';
import { SwfCensusDataService } from '../../../../shared/services/swf-census-data.service';
import { SwfCensusPreview } from '../../../../shared/types/swf-census';

function toDateOnly(date: Date): string {
  const p = (n: number) => String(n).padStart(2, '0');
  return `${date.getFullYear()}-${p(date.getMonth() + 1)}-${p(date.getDate())}`;
}

@Component({
  selector: 'mp-swf-census-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    NgClass,
    FormsModule,
    MpBadge,
    MpButton,
    MpCard,
    MpDatePicker,
    MpSkeleton,
    MpTable,
    MpTableBody,
    MpTableEmpty,
    MpTableHeader,
    PageHeader,
    EmptyState,
    TranslocoDirective,
  ],
  providers: [provideTranslocoScope('swf-census')],
  templateUrl: './swf-census-page.html',
})
export class SwfCensusPage implements OnInit {
  private readonly data = inject(SwfCensusDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);

  protected readonly referenceDate = signal<Date>(new Date(2026, 10, 5));
  protected readonly preview = signal<SwfCensusPreview | null>(null);
  protected readonly loading = signal(false);
  protected readonly downloading = signal(false);

  protected readonly issuesByField = computed(() => {
    const p = this.preview();
    if (!p) return [];
    const counts = new Map<string, number>();
    for (const i of p.issues) counts.set(i.field, (counts.get(i.field) ?? 0) + 1);
    return [...counts.entries()].map(([field, count]) => ({ field, count })).sort((a, b) => b.count - a.count);
  });

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.data.getPreview(toDateOnly(this.referenceDate())).subscribe({
      next: p => {
        this.preview.set(p);
        this.loading.set(false);
      },
      error: err => {
        this.loading.set(false);
        this.notify.apiError(err, this.transloco.translate('swf-census.loadError'));
      },
    });
  }

  protected download(): void {
    this.downloading.set(true);
    this.data.downloadXml(toDateOnly(this.referenceDate())).subscribe({
      next: blob => {
        this.downloading.set(false);
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = 'school-workforce-census-2026.xml';
        link.click();
        URL.revokeObjectURL(url);
      },
      error: err => {
        this.downloading.set(false);
        this.notify.apiError(err, this.transloco.translate('swf-census.downloadError'));
      },
    });
  }
}
