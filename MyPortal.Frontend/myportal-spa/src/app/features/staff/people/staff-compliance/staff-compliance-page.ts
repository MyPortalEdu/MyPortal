import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import {
  MpBadge,
  MpCard,
  MpInput,
  MpSelect,
  MpSkeleton,
  MpSortIcon,
  MpSortable,
  MpFilterMetadata,
  MpTable,
  MpTableBody,
  MpTableCaption,
  MpTableEmpty,
  MpTableHeader,
  MpTableLazyLoadEvent,
} from '@myportal/ui';
import { TranslocoDirective, TranslocoService, provideTranslocoScope } from '@jsverse/transloco';

import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { NotificationService } from '../../../../core/services/notification.service';
import { StaffComplianceDataService } from '../../../../shared/services/staff-compliance-data.service';
import {
  ComplianceItem,
  ComplianceKind,
  StaffComplianceDashboard,
} from '../../../../shared/types/staff-compliance';

const ALL = 'All';
const PAGE_SIZE = 20;

const CATEGORIES = ['Dbs', 'RightToWork', 'Training', 'Contract', 'PreEmployment'] as const;

const CATEGORY_ICON: Record<string, string> = {
  Dbs: 'fa-solid fa-fingerprint',
  RightToWork: 'fa-solid fa-passport',
  Training: 'fa-solid fa-certificate',
  Contract: 'fa-solid fa-file-signature',
  PreEmployment: 'fa-solid fa-clipboard-check',
};

function raw(item: ComplianceItem, field: string): string | null {
  switch (field) {
    case 'staffName':
      return item.staffName;
    case 'detail':
      return item.detail;
    case 'dueDate':
      return item.dueDate ?? null;
    case 'kind':
      return item.kind;
    default:
      return null;
  }
}

@Component({
  selector: 'mp-staff-compliance-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    FormsModule,
    RouterLink,
    MpBadge,
    MpCard,
    MpInput,
    MpSelect,
    MpSkeleton,
    MpSortIcon,
    MpSortable,
    MpTable,
    MpTableBody,
    MpTableCaption,
    MpTableEmpty,
    MpTableHeader,
    PageHeader,
    EmptyState,
    TranslocoDirective,
  ],
  providers: [provideTranslocoScope('staff-compliance')],
  templateUrl: './staff-compliance-page.html',
})
export class StaffCompliancePage implements OnInit {
  private readonly data = inject(StaffComplianceDataService);
  private readonly notify = inject(NotificationService);
  private readonly transloco = inject(TranslocoService);

  private readonly table = viewChild(MpTable);

  protected readonly pageSize = PAGE_SIZE;

  protected readonly loading = signal(false);
  protected readonly dashboard = signal<StaffComplianceDashboard | null>(null);
  protected readonly horizonDays = signal(90);
  protected readonly search = signal('');

  // Table state, captured from the (in-memory) lazy-load events so the grid stays the single
  // source of truth for page, sort and filters — no stale paginator when a filter changes.
  private readonly first = signal(0);
  private readonly rows = signal(PAGE_SIZE);
  private readonly sortField = signal<string | null>(null);
  private readonly sortOrder = signal(1);
  private readonly filters = signal<
    Record<string, MpFilterMetadata | MpFilterMetadata[] | undefined>
  >({});

  private filterValue(field: string): unknown {
    const meta = this.filters()[field];
    return Array.isArray(meta) ? meta[0]?.value : meta?.value;
  }

  protected readonly horizonOptions = computed(() =>
    [30, 60, 90, 180, 365].map(d => ({
      value: d,
      label: this.transloco.translate('staff-compliance.horizon.days', { days: d }),
    })),
  );

  protected readonly categoryOptions = computed(() => [
    { value: ALL, label: this.transloco.translate('staff-compliance.filter.allCategories') },
    ...CATEGORIES.map(c => ({
      value: c,
      label: this.transloco.translate(`staff-compliance.category.${c}`),
    })),
  ]);

  protected readonly items = computed(() => this.dashboard()?.items ?? []);

  private readonly filtered = computed(() => {
    const global = String(this.filterValue('global') ?? '').trim().toLowerCase();
    const category = this.filterValue('category') as string | undefined;
    const kind = this.filterValue('kind') as string | undefined;

    return this.items().filter(
      i =>
        (!category || i.category === category) &&
        (!kind || i.kind === kind) &&
        (!global ||
          `${i.staffName} ${i.staffCode} ${i.detail}`.toLowerCase().includes(global)),
    );
  });

  private readonly sorted = computed(() => {
    const field = this.sortField();
    if (!field) return this.filtered();

    const order = this.sortOrder();
    return [...this.filtered()].sort((a, b) => {
      const av = raw(a, field);
      const bv = raw(b, field);
      const aNull = av == null || av === '';
      const bNull = bv == null || bv === '';
      if (aNull && bNull) return 0;
      if (aNull) return 1; // nulls last regardless of direction
      if (bNull) return -1;
      return order * (av < bv ? -1 : av > bv ? 1 : 0);
    });
  });

  protected readonly totalRecords = computed(() => this.filtered().length);

  protected readonly pagedRows = computed(() => {
    const size = this.rows();
    const total = this.sorted().length;
    const maxFirst = total ? Math.floor((total - 1) / size) * size : 0;
    const first = Math.min(this.first(), maxFirst);
    return this.sorted().slice(first, first + size);
  });

  protected readonly activeCategory = computed(
    () => (this.filterValue('category') as string | undefined) ?? ALL,
  );

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.data.getDashboard(this.horizonDays()).subscribe({
      next: row => {
        this.dashboard.set(row);
        this.loading.set(false);
      },
      error: err => {
        this.loading.set(false);
        this.notify.apiError(err, this.transloco.translate('staff-compliance.loadError'));
      },
    });
  }

  protected onHorizonChange(days: number): void {
    this.horizonDays.set(days);
    this.load();
  }

  protected onLazy(event: MpTableLazyLoadEvent): void {
    this.first.set(event.first ?? 0);
    this.rows.set(event.rows ?? PAGE_SIZE);
    this.sortField.set((event.sortField as string | null) ?? null);
    this.sortOrder.set(event.sortOrder ?? 1);
    this.filters.set(event.filters ?? {});
  }

  /** Clicking a summary tile filters to that kind (clicking the active one clears it). */
  protected toggleKind(kind: ComplianceKind): void {
    const active = this.isKindActive(kind);
    this.table()?.filter(active ? null : kind, 'kind', 'equals');
  }

  protected isKindActive(kind: ComplianceKind): boolean {
    return this.filterValue('kind') === kind;
  }

  protected onCategoryChange(value: string): void {
    this.table()?.filter(value === ALL ? null : value, 'category', 'equals');
  }

  protected onSearch(value: string): void {
    this.search.set(value);
    this.table()?.filterGlobal(value, 'contains');
  }

  protected clearSearch(): void {
    this.onSearch('');
  }

  protected categoryIcon(category: string): string {
    return CATEGORY_ICON[category] ?? 'fa-solid fa-circle-exclamation';
  }

  protected kindSeverity(kind: ComplianceKind): 'danger' | 'warn' | 'secondary' {
    switch (kind) {
      case 'Expired':
        return 'danger';
      case 'ExpiringSoon':
        return 'warn';
      default:
        return 'secondary';
    }
  }

  protected dueLabel(item: ComplianceItem): string {
    if (!item.dueDate) return '—';
    return new Date(item.dueDate).toLocaleDateString('en-GB', {
      day: 'numeric',
      month: 'short',
      year: 'numeric',
    });
  }
}
