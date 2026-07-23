export const MAX_POINTS_PER_SCALE = 250;

/**
 * Date-only wire format taken from the local calendar date. `toISOString()` would shift a local
 * midnight back a day through most of the British summer, landing the generation on 31 August.
 */
export function toDateOnly(value: Date): string {
  const month = String(value.getMonth() + 1).padStart(2, '0');
  const day = String(value.getDate()).padStart(2, '0');
  return `${value.getFullYear()}-${month}-${day}`;
}

/** 1 September of the academic year in progress — when pay awards conventionally land. */
export function defaultGenerationDate(today: Date): Date {
  const year = today.getMonth() >= 8 ? today.getFullYear() : today.getFullYear() - 1;
  return new Date(year, 8, 1);
}

export function formatPoint(value: number): string {
  return Number.isInteger(value) ? String(value) : String(Number(value.toFixed(2)));
}

/**
 * Mirrors the server's point generation so the grid can show a row the moment a range is typed,
 * before anything is saved. Multiplies rather than accumulates, since repeated addition of a
 * fractional interval drifts where the server's decimal arithmetic does not.
 */
export function generatePointValues(
  minimum: number | null | undefined,
  maximum: number | null | undefined,
  interval: number | null | undefined,
): number[] {
  if (minimum == null || maximum == null || interval == null) return [];
  if (interval <= 0 || maximum < minimum) return [];

  const values: number[] = [];

  for (let i = 0; i <= MAX_POINTS_PER_SCALE; i++) {
    const value = Number((minimum + i * interval).toFixed(2));
    if (value > maximum) break;
    values.push(value);
  }

  return values;
}
