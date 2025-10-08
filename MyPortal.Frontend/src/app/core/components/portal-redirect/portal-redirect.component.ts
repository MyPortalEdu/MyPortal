import {Component, OnInit} from '@angular/core';
import {Router} from '@angular/router';
import {MeService} from '../../services/me.service';
import {Me} from '../../interfaces/me';
import {UserType} from '../../enums/user-type';

@Component({
  selector: 'app-portal-redirect',
  template: `<p class="text-center text-gray-500 mt-10">Redirecting...</p>`,
  standalone: false
})
export class PortalRedirectComponent implements OnInit {
  constructor(private me: MeService, private router: Router) {}

  ngOnInit(): void {
    this.me.me().subscribe({
      next: (user: Me) => {
        const target = this.mapUserType(user.userType);
        this.router.navigateByUrl(`/${target}`);
      },
      error: () => {
        const returnUrl = encodeURIComponent(location.pathname + location.search + location.hash);
        location.href = `/account/login?returnUrl=${returnUrl}`;
      }
    });
  }

  private mapUserType(userType: UserType): string {
    switch (userType) {
      case UserType.Student:
        return 'student';
      case UserType.Parent:
        return 'parent';
      case UserType.Staff:
      default:
        return 'staff';
    }
  }
}
