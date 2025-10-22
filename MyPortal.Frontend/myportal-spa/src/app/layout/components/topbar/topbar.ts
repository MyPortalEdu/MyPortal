import {Component, EventEmitter, OnInit, Output} from '@angular/core';
import {MenuItem} from 'primeng/api';
import {Menu} from 'primeng/menu';
import {Avatar} from 'primeng/avatar';
import {Observable} from 'rxjs';
import {MeService} from '../../../core/services/me-service';
import {HttpClient} from '@angular/common/http';
import {Me} from '../../../core/interfaces/me';
import {AsyncPipe, NgIf, UpperCasePipe} from '@angular/common';

@Component({
  selector: 'mp-topbar',
  standalone: true,
  imports: [Menu, Avatar, UpperCasePipe, AsyncPipe, NgIf],
  templateUrl: './topbar.html',
  styleUrl: './topbar.scss'
})
export class Topbar implements OnInit {
  @Output() menuToggle = new EventEmitter<void>();
  me$!: Observable<Me>;
  userMenu: MenuItem[] = [];

  constructor(private http: HttpClient, private me: MeService) {}

  ngOnInit(): void {
    // restore preference
    if (localStorage.getItem('mp:dark') === '1') {
      document.documentElement.classList.add('mp-dark');
    }
    this.me$ = this.me.me();
    this.updateUserMenu();
  }

  private get isDark(): boolean {
    return document.documentElement.classList.contains('mp-dark');
  }

  toggleDarkMode(): void {
    const root = document.documentElement;
    root.classList.toggle('mp-dark');
    localStorage.setItem('mp:dark', this.isDark ? '1' : '0');
    this.updateUserMenu();
  }

  private updateUserMenu(): void {
    const darkToggle: MenuItem = this.isDark
      ? { label: 'Light mode', icon: 'pi pi-sun',  command: () => this.toggleDarkMode() }
      : { label: 'Dark mode',  icon: 'pi pi-moon', command: () => this.toggleDarkMode() };

    this.userMenu = [
      { label: 'Profile',  icon: 'pi pi-user',  routerLink: ['/portal'] },
      { label: 'Settings', icon: 'pi pi-cog',   routerLink: ['/portal'] },
      { separator: true },
      darkToggle,
      { separator: true },
      { label: 'Sign out', icon: 'pi pi-sign-out', command: () => this.logout() },
    ];
  }

  logout() {
    this.me.clearCache?.();
    location.href = '/account/logout';
  }
}
