import { Component, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterOutlet } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { Toast } from 'primeng/toast';
import { SchoolService } from './core/services/school-service';

@Component({
  selector: 'mp-root',
  imports: [RouterOutlet, Toast, ConfirmDialog],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  private readonly titleService = inject(Title);
  private readonly schools = inject(SchoolService);

  constructor() {
    this.schools.getLocalName()
      .pipe(takeUntilDestroyed())
      .subscribe(name => {
        this.titleService.setTitle(name ? `MyPortal - ${name}` : 'MyPortal');
      });
  }
}
