import {Component, OnInit} from '@angular/core';
import {Router} from '@angular/router';
import {MeService} from '../../services/me-service';
import {firstValueFrom} from 'rxjs';
import {UserType} from '../../enums/user-type';

@Component({
  selector: 'mp-portal-redirect',
  imports: [],
  templateUrl: './portal-redirect.html'
})
export class PortalRedirectComponent implements OnInit {
  constructor(private me: MeService, private router: Router) {}
  async ngOnInit() {
    const me = await firstValueFrom(this.me.me());
    const url =
      me.userType === UserType.Staff   ? '/staff'   :
        me.userType === UserType.Student ? '/student' :
          me.userType === UserType.Parent  ? '/parent'  :
            '/';
    this.router.navigateByUrl(url);
  }
}
