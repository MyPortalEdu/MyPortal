import { ChangeDetectionStrategy, Component, OnInit, inject, output, signal } from '@angular/core';
import { Avatar } from 'primeng/avatar';
import { Observable, catchError, combineLatest, map, of } from 'rxjs';
import { MeService } from '../../../core/services/me-service';
import { SchoolService } from '../../../core/services/school-service';
import { AcademicYearService } from '../../../core/services/academic-year-service';
import { AsyncPipe } from '@angular/common';
import { ButtonDirective, ButtonIcon } from 'primeng/button';
import { RouterLink } from '@angular/router';
import { Popover } from 'primeng/popover';
import { TranslocoDirective, TranslocoService } from '@jsverse/transloco';
import { UserType } from '../../../core/enums/user-type';
import { Me } from '../../../core/interfaces/me';

interface SiteLabel {
  school: string | null;
  year: string | null;
}

type Theme = 'light' | 'dark' | 'system';

@Component({
  selector: 'mp-topbar',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Avatar, AsyncPipe, ButtonDirective, ButtonIcon, RouterLink, Popover, TranslocoDirective],
  templateUrl: './topbar.html',
  styleUrl: './topbar.scss',
})
export class Topbar implements OnInit {
  private readonly me = inject(MeService);
  private readonly schools = inject(SchoolService);
  private readonly academicYears = inject(AcademicYearService);
  private readonly transloco = inject(TranslocoService);

  readonly menuToggle = output<void>();

  me$!: Observable<Me>;
  siteLabel$!: Observable<SiteLabel>;
  readonly theme = signal<Theme>('system');

  ngOnInit(): void {
    this.me$ = this.me.me();

    // Either request can 403 for users without view permissions (e.g. students for
    // academic years); fall back to null so the label just hides that segment.
    this.siteLabel$ = combineLatest([
      this.schools.getLocalName().pipe(catchError(() => of<string | null>(null))),
      this.academicYears.getCurrent().pipe(catchError(() => of(null))),
    ]).pipe(
      map(([school, year]) => ({ school, year: year?.name ?? null })),
    );

    this.theme.set((localStorage.getItem('mp:theme') as Theme | null) ?? 'system');
    this.applyTheme();

    // Re-evaluate the resolved theme when the OS preference changes — only matters
    // when the user has picked 'system'. Modern browsers fire `change` on the media
    // query when the OS toggles between light/dark.
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', () => {
      if (this.theme() === 'system') this.applyTheme();
    });
  }

  setTheme(next: Theme): void {
    this.theme.set(next);
    localStorage.setItem('mp:theme', next);
    this.applyTheme();
  }

  private applyTheme(): void {
    const dark =
      this.theme() === 'dark' ||
      (this.theme() === 'system' && window.matchMedia('(prefers-color-scheme: dark)').matches);
    document.documentElement.classList.toggle('mp-dark', dark);
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

  logout(): void {
    this.me.clearCache();
    location.href = '/account/logout';
  }
}
