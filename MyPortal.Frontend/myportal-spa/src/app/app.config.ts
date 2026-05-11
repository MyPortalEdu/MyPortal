import { ApplicationConfig, ErrorHandler, provideBrowserGlobalErrorListeners, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import Aura from '@primeuix/themes/aura';

import { routes } from './app.routes';
import {provideAnimationsAsync} from '@angular/platform-browser/animations/async';
import {providePrimeNG} from 'primeng/config';
import {MessageService} from 'primeng/api';
import {provideHttpClient, withInterceptors, withXsrfConfiguration} from '@angular/common/http';
import {MENU_CONTRIBUTORS} from './layout/menu/menu-token';
import {StaffMenuContributor} from './features/staff/staff-menu-contributor';
import {definePreset} from '@primeng/themes';
import {apiBaseInterceptor} from './core/interceptors/api-base-interceptor';
import {authErrorInterceptor} from './core/interceptors/auth-error-interceptor';
import {AppErrorHandler} from './core/error/app-error-handler';

const MyPortal = definePreset(Aura, {
  semantic: {
    primary: {
      50: '{indigo.50}',
      100: '{indigo.100}',
      200: '{indigo.200}',
      300: '{indigo.300}',
      400: '{indigo.400}',
      500: '{indigo.500}',
      600: '{indigo.600}',
      700: '{indigo.700}',
      800: '{indigo.800}',
      900: '{indigo.900}',
      950: '{indigo.950}'
    },
    // Promote Aura's "small" form-field density to the new default so we don't
    // litter every template with size="small" / pSize="small". Buttons inherit
    // padding via {form.field.padding.x|y} so they pick this up automatically.
    // The `sm` and `lg` variants still work — they're just now relative to this
    // tighter baseline. Source of token defaults: @primeuix/themes/aura/base.
    //
    // Font-size is dropped to 0.875rem via `.p-component` in styles.css —
    // padding alone doesn't shrink the visible mass enough, and PrimeNG's
    // typings only expose fontSize on the sm/lg variants so we can't set it
    // here.
    formField: {
      paddingX: '0.625rem',     // was 0.75rem
      paddingY: '0.375rem'      // was 0.5rem
    }
  }
})

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
    providePrimeNG({
      theme: {
        preset: MyPortal,
        options: {
          darkModeSelector: '.mp-dark',
          cssLayer: {
            name: 'primeng',
            order: 'tailwind-base, primeng, tailwind-utilities'
          }
        },
      }
    }),
    // Single MessageService instance shared by everything that toasts via
    // NotificationService. PrimeNG ties toast messages to whichever
    // MessageService provided the <p-toast> in the tree — root-scoped is what
    // we want, because <p-toast> lives in the root App component.
    MessageService,
    { provide: MENU_CONTRIBUTORS, useClass: StaffMenuContributor, multi: true },
    { provide: ErrorHandler, useClass: AppErrorHandler }
  ]
};
