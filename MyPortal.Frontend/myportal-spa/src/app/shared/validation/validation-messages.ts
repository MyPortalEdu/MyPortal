/**
 * Default i18n message keys for the built-in Signal Forms validators, keyed by the error `kind`.
 * `mp-field` falls back to these when a validation error carries no explicit `message`, so a plain
 * `required(...)` / `maxLength(...)` needs no per-control message. The strings live under
 * `common.validation.*` and interpolate fields off the error object (e.g. `{{ maxLength }}`), so the
 * whole error is passed as translation params. Add an entry here (plus the `common.validation.*`
 * string) for any new built-in default; custom rules keep supplying their own `message` key.
 */
export const DEFAULT_VALIDATION_MESSAGES: Readonly<Record<string, string>> = {
  required: 'common.validation.required',
  invalid: 'common.validation.invalid',
  min: 'common.validation.min',
  max: 'common.validation.max',
  minLength: 'common.validation.minLength',
  maxLength: 'common.validation.maxLength',
  email: 'common.validation.email',
  pattern: 'common.validation.pattern',
  minDate: 'common.validation.minDate',
  maxDate: 'common.validation.maxDate',
};

/** The message key for a validation error: its own `message`, else the built-in default for its
 *  `kind`, else undefined (an unknown/message-less error shows nothing rather than a wrong default). */
export function validationMessageKey(
  error: { kind?: string; message?: string } | undefined,
): string | undefined {
  if (!error) return undefined;
  return error.message ?? DEFAULT_VALIDATION_MESSAGES[error.kind ?? ''];
}
