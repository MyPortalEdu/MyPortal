// Pretty file size — 1.2 KB / 3.4 MB. Locale-agnostic; the unit suffixes are
// short enough that translation isn't worth the i18n weight.
export function formatFileSize(bytes?: number | null): string {
  if (bytes == null) return '';
  const units = ['B', 'KB', 'MB', 'GB'];
  let value = bytes;
  let unit = 0;
  while (value >= 1024 && unit < units.length - 1) {
    value /= 1024;
    unit++;
  }
  return `${value.toFixed(unit === 0 ? 0 : 1)} ${units[unit]}`;
}

// Map a MIME type to a Font Awesome icon. Falls back to a generic file icon.
export function fileIcon(contentType: string): string {
  if (!contentType) return 'fa-solid fa-file';
  if (contentType.startsWith('image/')) return 'fa-solid fa-file-image';
  if (contentType.startsWith('video/')) return 'fa-solid fa-file-video';
  if (contentType.startsWith('audio/')) return 'fa-solid fa-file-audio';
  if (contentType === 'application/pdf') return 'fa-solid fa-file-pdf';
  if (contentType.includes('word')) return 'fa-solid fa-file-word';
  if (contentType.includes('excel') || contentType.includes('spreadsheet')) return 'fa-solid fa-file-excel';
  if (contentType.includes('zip') || contentType.includes('compressed')) return 'fa-solid fa-file-zipper';
  return 'fa-solid fa-file';
}
