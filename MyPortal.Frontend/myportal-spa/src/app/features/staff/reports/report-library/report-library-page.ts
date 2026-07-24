import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { NgClass } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { MpInput } from '@myportal/ui';
import { TranslocoDirective, TranslocoService, provideTranslocoScope } from '@jsverse/transloco';

import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { MeService } from '../../../../core/services/me-service';
import { Permissions } from '../../../../core/constants/permissions';

interface ReportEntry {
  key: string;
  icon: string;
  route: string | null; // null → not yet built
}

interface ReportGroup {
  key: string;
  eyebrowIcon: string;
  tint: string; // eyebrow text colour
  tile: string; // icon-tile background + colour
  reports: ReportEntry[];
}

interface ReportArea {
  key: string;
  icon: string;
  permission: string; // area shows only if the user holds this
  groups: ReportGroup[];
}

// Catalog: area → group → report. Every report declares its home here; the library derives the tab
// strip, the groups and the search index from it. Unbuilt reports still appear (route: null) so the
// roadmap stays visible.
const AREAS: ReportArea[] = [
  {
    key: 'personnel',
    icon: 'fa-solid fa-id-badge',
    permission: Permissions.Staff.ViewAllStaffEmploymentDetails,
    groups: [
      {
        key: 'payContracts',
        eyebrowIcon: 'fa-solid fa-file-invoice-dollar',
        tint: 'text-emerald-600 dark:text-emerald-400',
        tile: 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400',
        reports: [
          { key: 'salaryInformation', icon: 'fa-solid fa-sterling-sign', route: 'salary-information' },
          { key: 'contractInformation', icon: 'fa-solid fa-file-signature', route: 'contract-information' },
          { key: 'contractAnalysis', icon: 'fa-solid fa-chart-pie', route: 'contract-analysis' },
          { key: 'terminatingContracts', icon: 'fa-solid fa-file-circle-xmark', route: 'terminating-contracts' },
        ],
      },
      {
        key: 'absence',
        eyebrowIcon: 'fa-solid fa-user-clock',
        tint: 'text-amber-600 dark:text-amber-400',
        tile: 'bg-amber-500/10 text-amber-600 dark:text-amber-400',
        reports: [
          { key: 'staffAbsenceAnalysis', icon: 'fa-solid fa-chart-column', route: 'staff-absence-analysis' },
          { key: 'individualAbsence', icon: 'fa-solid fa-user-large', route: 'individual-absence' },
          { key: 'longTermAbsence', icon: 'fa-solid fa-calendar-days', route: 'long-term-absence' },
        ],
      },
      {
        key: 'training',
        eyebrowIcon: 'fa-solid fa-graduation-cap',
        tint: 'text-violet-600 dark:text-violet-400',
        tile: 'bg-violet-500/10 text-violet-600 dark:text-violet-400',
        reports: [
          { key: 'staffTraining', icon: 'fa-solid fa-certificate', route: 'staff-training' },
          { key: 'trainingCourse', icon: 'fa-solid fa-chalkboard-user', route: 'training-course' },
        ],
      },
    ],
  },
];

@Component({
  selector: 'mp-report-library-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [NgClass, FormsModule, MpInput, RouterLink, PageHeader, EmptyState, TranslocoDirective],
  providers: [provideTranslocoScope('staff-reports')],
  templateUrl: './report-library-page.html',
})
export class ReportLibraryPage implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly me = inject(MeService);
  private readonly transloco = inject(TranslocoService);

  private readonly permissions = signal<string[]>([]);
  private readonly queryParams = toSignal(this.route.queryParamMap, {
    initialValue: this.route.snapshot.queryParamMap,
  });

  protected readonly search = signal('');

  protected readonly accessibleAreas = computed(() =>
    AREAS.filter(area => this.permissions().includes(area.permission)),
  );

  protected readonly showTabs = computed(() => this.accessibleAreas().length > 1);

  protected readonly activeArea = computed(() => {
    const areas = this.accessibleAreas();
    const requested = this.queryParams().get('area');
    return areas.find(a => a.key === requested) ?? areas[0] ?? null;
  });

  // Groups of the active area, filtered by the search term (matches report title or description).
  protected readonly visibleGroups = computed<ReportGroup[]>(() => {
    const area = this.activeArea();
    if (!area) return [];

    const term = this.search().trim().toLowerCase();
    if (!term) return area.groups;

    return area.groups
      .map(group => ({ ...group, reports: group.reports.filter(r => this.matches(r, term)) }))
      .filter(group => group.reports.length > 0);
  });

  ngOnInit(): void {
    this.me.me().subscribe(me => this.permissions.set(me.permissions ?? []));
  }

  protected clearSearch(): void {
    this.search.set('');
  }

  private matches(report: ReportEntry, term: string): boolean {
    const title = this.transloco.translate(`staff-reports.reports.${report.key}.title`).toLowerCase();
    const description = this.transloco
      .translate(`staff-reports.reports.${report.key}.description`)
      .toLowerCase();
    return title.includes(term) || description.includes(term);
  }
}
