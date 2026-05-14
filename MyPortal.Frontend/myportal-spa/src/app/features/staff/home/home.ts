import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Button } from 'primeng/button';
import { TranslocoDirective } from '@jsverse/transloco';
import { PageHeader } from '../../../shared/components/page-header/page-header';
import { BulletinsFeed } from '../../../shared/components/bulletins/bulletins-feed/bulletins-feed';
import { MeService } from '../../../core/services/me-service';
import { SchoolService } from '../../../core/services/school-service';
import { Permissions } from '../../../core/constants/permissions';

@Component({
  selector: 'mp-home',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    PageHeader,
    BulletinsFeed,
    Button,
    RouterLink,
    TranslocoDirective,
  ],
  templateUrl: './home.html',
  styleUrl: './home.scss',
})
export class Home implements OnInit {
  private readonly me = inject(MeService);
  private readonly schools = inject(SchoolService);

  readonly canEditAgencies = signal(false);
  // null while loading — keeps the banner hidden until we know the answer,
  // so it doesn't flash on every home visit before /api/schools/local/name
  // resolves.
  readonly schoolExists = signal<boolean | null>(null);

  readonly showSchoolSetup = computed(() =>
    this.canEditAgencies() && this.schoolExists() === false,
  );

  ngOnInit(): void {
    this.me.me().subscribe(me => {
      this.canEditAgencies.set(me.permissions?.includes(Permissions.Agencies.EditAgencies) ?? false);
    });
    this.schools.getLocalName().subscribe(name => {
      this.schoolExists.set(name !== null);
    });
  }
}
