import { Component } from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {MeService} from './core/services/me.service';

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styleUrl: './app.component.scss',
    standalone: false
})
export class AppComponent {
  title = 'MyPortal';

  constructor(private http: HttpClient, private me: MeService) {

  }

  logout(): void {
    this.http.post('/account/logout', {}).subscribe(() => {
      this.me.clearCache();
      location.href = "/";
    });
  }
}
