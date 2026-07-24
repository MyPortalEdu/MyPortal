// Shared formatting for report pages, so every report renders money, FTE and dates the same way.
const GBP = new Intl.NumberFormat('en-GB', {
  style: 'currency',
  currency: 'GBP',
  maximumFractionDigits: 0,
});

export function money(value: number | null | undefined): string {
  return value == null ? '—' : GBP.format(value);
}

/** FTE trimmed of trailing zeros (1.0000 → "1", 0.5000 → "0.5"). */
export function fteLabel(value: number | null | undefined): string {
  return value == null ? '—' : value.toFixed(4).replace(/\.?0+$/, '');
}

export function dateLabel(value: string | null | undefined): string {
  if (!value) return '—';
  return new Date(value).toLocaleDateString('en-GB', {
    day: 'numeric',
    month: 'short',
    year: 'numeric',
  });
}

/** Local YYYY-MM-DD for the wire — no time component, no UTC shift. */
export function toDateOnly(date: Date): string {
  const y = date.getFullYear();
  const m = String(date.getMonth() + 1).padStart(2, '0');
  const d = String(date.getDate()).padStart(2, '0');
  return `${y}-${m}-${d}`;
}
