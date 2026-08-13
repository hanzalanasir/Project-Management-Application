// The API expects "YYYY-MM-DD" (DateOnly), never a timestamp. Going through
// Date.toISOString()/`new Date(dateString)` converts via UTC and can shift the day by one for
// anyone west of UTC. These stay in local-calendar terms, matching what the user clicked.

export function toDateOnlyString(date: Date | null): string {
  if (!date) return '';
  const y = date.getFullYear();
  const m = String(date.getMonth() + 1).padStart(2, '0');
  const d = String(date.getDate()).padStart(2, '0');
  return `${y}-${m}-${d}`;
}

export function fromDateOnlyString(value: string | null | undefined): Date | null {
  if (!value) return null;
  const [y, m, d] = value.split('-').map(Number);
  return new Date(y, m - 1, d);
}
