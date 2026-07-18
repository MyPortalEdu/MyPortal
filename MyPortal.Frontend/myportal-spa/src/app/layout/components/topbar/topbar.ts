import { ChangeDetectionStrategy, Component, OnInit, computed, inject, output, signal } from '@angular/core';
import { Avatar } from 'primeng/avatar';
import { Observable, catchError, of } from 'rxjs';
import { toSignal } from '@angular/core/rxjs-interop';
import { MeService } from '../../../core/services/me-service';
import { SchoolService } from '../../../core/services/school-service';
import { SelectedAcademicYearService } from '../../../core/services/selected-academic-year-service';
import { ThemeService } from '../../../core/services/theme-service';
import { AsyncPipe } from '@angular/common';
import { ButtonDirective, ButtonIcon } from 'primeng/button';
import { RouterLink } from '@angular/router';
import { Popover } from 'primeng/popover';
import { TranslocoDirective, TranslocoService } from '@jsverse/transloco';
import { UserType } from '../../../core/types/user-type';
import { Me } from '../../../core/types/me';
import { AcademicYearSwitcherDialog } from './academic-year-switcher-dialog/academic-year-switcher-dialog';

@Component({
  selector: 'mp-topbar',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    Avatar, AsyncPipe, ButtonDirective, ButtonIcon, RouterLink, Popover,
    TranslocoDirective, AcademicYearSwitcherDialog,
  ],
  templateUrl: './topbar.html',
  styleUrl: './topbar.scss',
})
export class Topbar implements OnInit {
  private readonly me = inject(MeService);
  private readonly schools = inject(SchoolService);
  private readonly selectedYear = inject(SelectedAcademicYearService);
  private readonly transloco = inject(TranslocoService);
  protected readonly themeService = inject(ThemeService);

  readonly menuToggle = output<void>();

  me$!: Observable<Me>;

  readonly switcherOpen = signal(false);
  readonly UserType = UserType;

  // Topbar avatar inverts with the theme so it pops against the bar: white disc /
  // indigo initials on the light (indigo) bar, indigo disc / white initials on
  // the dark bar. Driven by the theme signal rather than `dark:` utilities to
  // sidestep !important/variant ordering ambiguity for this one element.
  readonly topbarAvatarClass = computed(
    () =>
      (this.themeService.isDark()
        ? '!bg-primary-500 !text-white'
        : '!bg-white !text-primary-600') + ' !w-9 !h-9 !text-sm font-medium',
  );

  // School name as a signal — toSignal must be called in injection context, so
  // the bridge sits here as a field initializer rather than in ngOnInit.
  // School can 403 for users without view permission; fall back to null so the
  // label just hides that segment.
  private readonly schoolName = toSignal(
    this.schools.getLocalName().pipe(catchError(() => of<string | null>(null))),
    { initialValue: null as string | null },
  );

  // The AY segment reflects the user's *selected* AY (defaults to calendar-
  // current via SelectedAcademicYearService.init) rather than always showing
  // the calendar-current year — once a staff user switches, the topbar
  // reflects their choice.
  readonly siteLabel = computed(() => ({
    school: this.schoolName(),
    year: this.selectedYear.selected()?.name ?? null,
  }));

  ngOnInit(): void {
    this.me$ = this.me.me();
  }

  initials(displayName: string | undefined): string {
    if (!displayName) return 'U';
    const parts = displayName.trim().split(/\s+/);
    if (parts.length === 1) return parts[0]!.slice(0, 2).toUpperCase();
    return (parts[0]![0] + parts[parts.length - 1]![0]).toUpperCase();
  }

  userTypeLabel(t: UserType | undefined): string {
    const key = (() => {
      switch (t) {
        case UserType.Staff:   return 'staff';
        case UserType.Student: return 'student';
        case UserType.Parent:  return 'parent';
        default:               return null;
      }
    })();
    return key ? this.transloco.translate(`topbar.userType.${key}`) : '';
  }

  openYearSwitcher(): void {
    this.switcherOpen.set(true);
  }

  closeYearSwitcher(): void {
    this.switcherOpen.set(false);
  }

  logout(): void {
    this.me.clearCache();
    // Wipe the persisted AY selection — next sign-in (even same user) seeds
    // from calendar-current. Token refresh / silent re-auth doesn't pass
    // through here, so a long-running session keeps the user's choice.
    this.selectedYear.clear();
    location.href = '/account/logout';
  }
}
