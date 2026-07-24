// Generic client-side CSV export. Columns carry their own header + value accessor so a report can
// export exactly what it shows, in order. Prepends a BOM so Excel opens UTF-8 cleanly.
export interface CsvColumn<T> {
  header: string;
  value: (row: T) => string | number | null | undefined;
}

function escapeCell(value: string | number | null | undefined): string {
  const text = value == null ? '' : String(value);
  return /[",\n\r]/.test(text) ? `"${text.replace(/"/g, '""')}"` : text;
}

export function exportToCsv<T>(filename: string, columns: CsvColumn<T>[], rows: readonly T[]): void {
  const header = columns.map(c => escapeCell(c.header)).join(',');
  const body = rows.map(row => columns.map(c => escapeCell(c.value(row))).join(',')).join('\r\n');
  const csv = `﻿${header}\r\n${body}`;

  const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = filename.endsWith('.csv') ? filename : `${filename}.csv`;
  link.style.display = 'none';
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  URL.revokeObjectURL(url);
}
