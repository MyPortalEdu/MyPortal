import {Component, EventEmitter, OnInit, Output} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {MeService} from '../../../../core/services/me.service';
import {Observable} from 'rxjs';
import {Me} from '../../../../core/interfaces/me';

@Component({
  selector: 'app-topbar',
  templateUrl: './topbar.component.html',
  standalone: false
})
export class TopbarComponent implements OnInit {
  @Output() menuToggle = new EventEmitter<void>();
  me$: Observable<Me> | undefined;

  constructor(private http: HttpClient, private me: MeService) {
  }

  ngOnInit(): void {
    this.me$ = this.me.me();
  }

  logout() {
    this.me.clearCache();
    location.href = '/account/logout';
  }
}
