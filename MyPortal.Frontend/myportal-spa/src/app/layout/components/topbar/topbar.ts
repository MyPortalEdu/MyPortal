import { ChangeDetectionStrategy, Component, OnInit, computed, inject, output, signal } from '@angular/core';
import { MpButton, MpAvatar } from '@myportal/ui';
import { Observable, catchError, of } from 'rxjs';
import { toSignal } from '@angular/core/rxjs-interop';
import { MeService } from '../../../core/services/me-service';
import { SchoolService } from '../../../core/services/school-service';
import { SelectedAcademicYearService } from '../../../core/services/selected-academic-year-service';
import { ThemeService } from '../../../core/services/theme-service';
import { AsyncPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { BrnPopoverImports } from '@spartan-ng/brain/popover';
import { MpPopover } from '@myportal/ui';
import { TranslocoDirective, TranslocoService } from '@jsverse/transloco';
import { UserType } from '../../../core/types/user-type';
import { Me } from '../../../core/types/me';
import { AcademicYearSwitcherDialog } from './academic-year-switcher-dialog/academic-year-switcher-dialog';

@Component({
  selector: 'mp-topbar',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    MpAvatar, AsyncPipe, MpButton, RouterLink, BrnPopoverImports, MpPopover,
    TranslocoDirective, AcademicYearSwitcherDialog,
  ],
  templateUrl: './topbar.html',
})
export class Topbar implements OnInit {
  protected readonly menuRowClass =
    'flex items-center gap-3 w-full px-4 py-2 rounded-control text-sm text-inherit no-underline ' +
    'bg-transparent border-0 cursor-pointer text-left outline-none transition-colors ' +
    'hover:bg-accent hover:text-accent-foreground focus-visible:bg-accent';
  protected readonly menuRowDangerClass =
    'flex items-center gap-3 w-full px-4 py-2 rounded-control text-sm no-underline ' +
    'bg-transparent border-0 cursor-pointer text-left outline-none transition-colors ' +
    'text-destructive hover:bg-destructive/10 focus-visible:bg-destructive/10';
  private readonly me = inject(MeService);
  private readonly schools = inject(SchoolService);
  private readonly selectedYear = inject(SelectedAcademicYearService);
  private readonly transloco = inject(TranslocoService);
  protected readonly themeService = inject(ThemeService);

  readonly menuToggle = output<void>();

  me$!: Observable<Me>;

  readonly switcherOpen = signal(false);
  readonly canSwitchYear = this.selectedYear.hasAccess;
  readonly UserType = UserType;

  readonly topbarAvatarClass = computed(
    () =>
      (this.themeService.isDark()
        ? '!bg-primary-500 !text-white'
        : '!bg-white !text-primary-600') + ' !w-9 !h-9 !text-sm font-medium',
  );

  private readonly schoolName = toSignal(
    this.schools.getLocalName().pipe(catchError(() => of<string | null>(null))),
    { initialValue: null as string | null },
  );

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
    this.selectedYear.clear();
    location.href = '/account/logout';
  }
}
