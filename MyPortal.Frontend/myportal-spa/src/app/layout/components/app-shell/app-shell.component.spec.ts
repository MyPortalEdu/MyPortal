import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { TranslocoTestingModule } from '@jsverse/transloco';

import { AppShell } from './app-shell.component';

describe('Shell', () => {
  let component: AppShell;
  let fixture: ComponentFixture<AppShell>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      // Topbar transitively depends on MeService → HttpClient, and the template
      // hosts a <router-outlet>. Both are application-level concerns; the test
      // only cares that the component renders without crashing. Transloco is
      // pulled in by TranslocoDirective in the template — TranslocoTestingModule
      // wires a no-op transpiler/loader so the directive resolves.
      imports: [AppShell, TranslocoTestingModule.forRoot({ langs: { en: {} } })],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    })
    .compileComponents();

    fixture = TestBed.createComponent(AppShell);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
