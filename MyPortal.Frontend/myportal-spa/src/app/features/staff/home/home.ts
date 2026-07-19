import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MpButton } from '@myportal/ui';
import { TranslocoDirective } from '@jsverse/transloco';
import { PageHeader } from '../../../shared/components/page-header/page-header';
import { Callout } from '../../../shared/components/callout/callout';
import { BulletinsFeed } from '../../../shared/components/bulletins/bulletins-feed/bulletins-feed';
import { StaffTimetable } from '../../../shared/components/staff-timetable/staff-timetable';
import { MeService } from '../../../core/services/me-service';
import { SchoolService } from '../../../core/services/school-service';
import { AcademicYearsDataService } from '../../../shared/services/academic-years-data.service';
import { Permissions } from '../../../core/constants/permissions';

@Component({
  selector: 'mp-home',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    PageHeader,
    Callout,
    BulletinsFeed,
    StaffTimetable,
    MpButton,
    RouterLink,
    TranslocoDirective,
  ],
  templateUrl: './home.html',
  styleUrl: './home.scss',
})
export class Home implements OnInit {
  private readonly me = inject(MeService);
  private readonly schools = inject(SchoolService);
  private readonly academicYears = inject(AcademicYearsDataService);

  readonly canEditAgencies = signal(false);
  readonly canEditAcademicYears = signal(false);
  readonly schoolExists = signal<boolean | null>(null);
  readonly anyAcademicYears = signal<boolean | null>(null);

  readonly showSchoolSetup = computed(() =>
    this.canEditAgencies() && this.schoolExists() === false,
  );

  readonly showAcademicYearSetup = computed(() =>
    this.canEditAcademicYears() && this.anyAcademicYears() === false,
  );

  ngOnInit(): void {
    this.me.me().subscribe(me => {
      this.canEditAgencies.set(me.permissions?.includes(Permissions.Agencies.EditAgencies) ?? false);
      this.canEditAcademicYears.set(
        me.permissions?.includes(Permissions.Curriculum.EditAcademicYears) ?? false,
      );
    });
    this.schools.getLocalName().subscribe(name => {
      this.schoolExists.set(name !== null);
    });
    this.academicYears.list().subscribe({
      next: rows => this.anyAcademicYears.set((rows ?? []).length > 0),
      error: () => this.anyAcademicYears.set(null),
    });
  }
}
