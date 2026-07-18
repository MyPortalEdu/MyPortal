const CONTROL_SELECTOR = 'input, textarea, select';

/**
 * Scrolls to and focuses the first invalid control inside `root`, so a Save that can't proceed
 * points at what's missing instead of dead-ending. Skips the `<form>` itself (Angular flags it
 * `ng-invalid` too) and reaches inside component wrappers whose host carries the class.
 */
export function focusFirstInvalid(root: HTMLElement): void {
  const flagged = root.querySelector<HTMLElement>('.ng-invalid:not(form)');
  if (!flagged) return;

  const control = flagged.matches(CONTROL_SELECTOR)
    ? flagged
    : flagged.querySelector<HTMLElement>(CONTROL_SELECTOR);

  (control ?? flagged).scrollIntoView({ block: 'center', behavior: 'smooth' });
  control?.focus({ preventScroll: true });
}
