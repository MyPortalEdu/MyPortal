import { ChangeDetectionStrategy, Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { MeService } from '../../services/me-service';
import { firstValueFrom } from 'rxjs';
import { UserType } from '../../types/user-type';

import { TranslocoPipe } from '@jsverse/transloco';

@Component({
  selector: 'mp-portal-redirect',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [TranslocoPipe],
  templateUrl: './portal-redirect.html'
})
export class PortalRedirect implements OnInit {
  private readonly me = inject(MeService);
  private readonly router = inject(Router);

  async ngOnInit() {
    let userType: UserType;
    try {
      const me = await firstValueFrom(this.me.me());
      userType = me.userType;
    } catch (err) {
      // 401 is already redirected by auth-error-interceptor; anything else is
      // a transient / server problem — sign out so the user lands somewhere
      // sensible instead of an infinite "Redirecting…" page.
      console.error('PortalRedirect: failed to resolve current user', err);
      window.location.href = '/account/logout';
      return;
    }

    const url =
      userType === UserType.Staff   ? '/staff'   :
        userType === UserType.Student ? '/student' :
          userType === UserType.Parent  ? '/parent'  :
            // Unknown / unmapped user type — sign out instead of bouncing back to '/' which
            // re-enters this component and creates an infinite redirect loop.
            '/account/logout';
    if (url.startsWith('/account/')) {
      window.location.href = url;
      return;
    }
    this.router.navigateByUrl(url);
  }
}
