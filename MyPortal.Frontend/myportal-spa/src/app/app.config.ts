import { ApplicationConfig, ErrorHandler, isDevMode, provideBrowserGlobalErrorListeners, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import Aura from '@primeuix/themes/aura';

import { routes } from './app.routes';
import {provideAnimationsAsync} from '@angular/platform-browser/animations/async';
import {providePrimeNG} from 'primeng/config';
import {ConfirmationService, MessageService} from 'primeng/api';
import {provideHttpClient, withInterceptors, withXsrfConfiguration} from '@angular/common/http';
import {provideTransloco} from '@jsverse/transloco';
import {MENU_CONTRIBUTORS} from './layout/menu/menu-token';
import {StaffMenuContributor} from './features/staff/staff-menu-contributor';
import {definePreset} from '@primeuix/themes';
import {apiBaseInterceptor} from './core/interceptors/api-base-interceptor';
import {authErrorInterceptor} from './core/interceptors/auth-error-interceptor';
import {AppErrorHandler} from './core/error/app-error-handler';
import {TranslocoHttpLoader} from './core/i18n/transloco-loader';

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
    // MIS-grade density. Aura's defaults are consumer/marketing-scale (~36px
    // input height, rounded corners); for a school information system where a
    // single page can carry 30+ fields and users live in forms all day, the
    // baseline sits closer to Linear / GitHub admin density (~30px input,
    // sharper corners). The `sm` and `lg` variants still work — they're now
    // relative to this tighter baseline. Source of token defaults:
    // @primeuix/themes/aura/base.
    //
    // Font-size is set to 0.875rem (14px) via `.p-component` in styles.css —
    // padding alone doesn't shrink the visible mass enough, and PrimeNG's
    // typings only expose fontSize on the sm/lg variants so we can't set it
    // here. Card body and table cell padding tightening live in styles.css
    // (token coverage is fragmented across those components).
    formField: {
      paddingX: '0.5rem',       // was 0.625rem (Aura default 0.75rem)
      paddingY: '0.25rem',      // was 0.375rem (Aura default 0.5rem) — input height ~30px
      borderRadius: '4px'       // was Aura default ~6px — sharper, reads as data entry not CTA
    },
    // Containers (cards / dialogs / panels) keep slightly more rounded corners
    // than inputs so they still feel like containers — just a touch less round
    // than the Aura default.
    content: {
      borderRadius: '6px'
    }
    // Button density is pinned via CSS variables in styles.css — the v20
    // ButtonDesignTokens shape doesn't expose paddingX/Y/iconOnlyWidth at the
    // typed root, so override the underlying --p-button-* vars directly.
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
      },
      // Lift PrimeNG overlays (e.g. p-columnFilter menus) above MpDialog (z-1101) and the CDK
      // overlay layer (z-1200) so any PrimeNG popup opened inside an mp-dialog isn't hidden behind it.
      zIndex: {
        modal: 1100,
        overlay: 1300,
        menu: 1300,
        tooltip: 1300,
      }
    }),
    // Single MessageService instance shared by everything that toasts via
    // NotificationService. PrimeNG ties toast messages to whichever
    // MessageService provided the <p-toast> in the tree — root-scoped is what
    // we want, because <p-toast> lives in the root App component.
    MessageService,
    // Same root-scoping rationale as MessageService: <p-confirmDialog> lives
    // in the root template, so callers anywhere in the tree can invoke our
    // ConfirmationDialog wrapper (core/services/confirmation.service.ts).
    ConfirmationService,
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
