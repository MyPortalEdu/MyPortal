import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Translation, TranslocoLoader } from '@jsverse/transloco';

// Loads JSON translation bundles from /i18n. Transloco passes either the bare
// language code ("en") for the root scope, or "<scope>/<lang>" (e.g.
// "bulletins/en") for feature scopes — so the URL pattern below covers both.
//
// The dev server proxies /api, /connect and /account to the API; /i18n is
// served straight from public/ by ng serve, so no proxy entry is needed.
@Injectable({ providedIn: 'root' })
export class TranslocoHttpLoader implements TranslocoLoader {
  private readonly http = inject(HttpClient);

  getTranslation(lang: string) {
    return this.http.get<Translation>(`/i18n/${lang}.json`);
  }
}
