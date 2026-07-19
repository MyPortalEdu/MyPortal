const CONTROL_SELECTOR = 'input, textarea, select';

export function focusFirstInvalid(root: HTMLElement): void {
  const flagged = root.querySelector<HTMLElement>('.ng-invalid:not(form)');
  if (!flagged) return;

  const control = flagged.matches(CONTROL_SELECTOR)
    ? flagged
    : flagged.querySelector<HTMLElement>(CONTROL_SELECTOR);

  (control ?? flagged).scrollIntoView({ block: 'center', behavior: 'smooth' });
  control?.focus({ preventScroll: true });
}
