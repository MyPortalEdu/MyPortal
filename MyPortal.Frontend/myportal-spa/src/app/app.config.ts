import { ApplicationConfig, ErrorHandler, isDevMode, provideBrowserGlobalErrorListeners, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import {provideAnimationsAsync} from '@angular/platform-browser/animations/async';
import {provideHttpClient, withInterceptors, withXsrfConfiguration} from '@angular/common/http';
import {provideTransloco} from '@jsverse/transloco';
import {MENU_CONTRIBUTORS} from './layout/menu/menu-token';
import {StaffMenuContributor} from './features/staff/staff-menu-contributor';
import {apiBaseInterceptor} from './core/interceptors/api-base-interceptor';
import {authErrorInterceptor} from './core/interceptors/auth-error-interceptor';
import {AppErrorHandler} from './core/error/app-error-handler';
import {TranslocoHttpLoader} from './core/i18n/transloco-loader';

export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(
      withInterceptors([apiBaseInterceptor, authErrorInterceptor]),
      withXsrfConfiguration({ cookieName: 'XSRF-TOKEN', headerName: 'X-XSRF-TOKEN' })
    ),
    provideBrowserGlobalErrorListeners(),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideAnimationsAsync(),
    provideTransloco({
      config: {
        availableLangs: ['en'],
        defaultLang: 'en',
        fallbackLang: 'en',
        reRenderOnLangChange: true,
        prodMode: !isDevMode(),
        scopes: { keepCasing: true },
      },
      loader: TranslocoHttpLoader,
    }),
    { provide: MENU_CONTRIBUTORS, useClass: StaffMenuContributor, multi: true },
    { provide: ErrorHandler, useClass: AppErrorHandler }
  ]
};
