const REDIRECT_MARKER = 'mp-auth-redirect-at';
const LOOP_WINDOW_MS = 10_000;

/**
 * Hard-redirects to the server login page for a 401, but only once within a short window.
 *
 * The server login page bounces a still-cookied user back to `returnUrl`, so a 401 that
 * cannot be recovered (e.g. an invalidated security stamp) would otherwise cause an infinite
 * hard-refresh loop. The marker (kept in sessionStorage so it survives the reload) means a
 * repeated 401 lands the user on the failed page instead of reloading forever.
 *
 * @returns true if a redirect was issued, false if it was suppressed as a likely loop.
 */
export function redirectToLogin(returnUrl: string): boolean {
  if (typeof window === 'undefined') return false;
  if (window.location.pathname.startsWith('/account/')) return false;

  if (recentlyRedirected()) return false;

  markRedirect();
  window.location.href = `/account/login?returnUrl=${encodeURIComponent(returnUrl)}`;
  return true;
}

function recentlyRedirected(): boolean {
  try {
    const last = Number(sessionStorage.getItem(REDIRECT_MARKER) ?? 0);
    return Date.now() - last < LOOP_WINDOW_MS;
  } catch {
    return false;
  }
}

function markRedirect(): void {
  try {
    sessionStorage.setItem(REDIRECT_MARKER, String(Date.now()));
  } catch {
    /* sessionStorage unavailable — best effort */
  }
}
