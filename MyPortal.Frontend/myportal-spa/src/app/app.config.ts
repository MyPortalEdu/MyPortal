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

// PrimeNG was removed at the end of Phase 3. The Aura/indigo palette it used to inject at runtime
// (the `--p-*` vars) is now baked into src/styles.css as light/dark literals; the tightened
// form-field/button/table density it configured lives there too. The app renders entirely on the
// @myportal/ui design system now.
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
    // Toasts + confirm prompts are now design-system (MpToastStore / MpConfirmStore, both
    // providedIn:'root'), rendered by <mp-toast>/<mp-confirm-dialog> in the root App template —
    // no PrimeNG MessageService/ConfirmationService providers needed.
    // Transloco. `en` is the only locale today; the config is structured so
    // adding `cy` (or anything else) is a one-line change to availableLangs
    // plus dropping the new <lang>.json files into public/i18n/. Feature
    // scopes (e.g. "bulletins") are loaded lazily via provideTranslocoScope
    // on the consuming components.
    provideTransloco({
      config: {
        availableLangs: ['en'],
        defaultLang: 'en',
        fallbackLang: 'en',
        reRenderOnLangChange: true,
        prodMode: !isDevMode(),
        // Keep scope names verbatim. Without this Transloco auto-camelCases
        // every scope name, so a hyphenated scope like "bulletin-settings"
        // gets stored as "bulletinSettings" and lookups via the hyphenated
        // prefix silently miss.
        scopes: { keepCasing: true },
      },
      loader: TranslocoHttpLoader,
    }),
    { provide: MENU_CONTRIBUTORS, useClass: StaffMenuContributor, multi: true },
    { provide: ErrorHandler, useClass: AppErrorHandler }
  ]
};
