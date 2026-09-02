export function formatCOP(amount: number): string {
  return new Intl.NumberFormat('es-CO', {
    style: 'currency',
    currency: 'COP',
    minimumFractionDigits: 0,
    maximumFractionDigits: 0,
  }).format(amount)
}

export function formatDate(dateStr: string): string {
  return new Date(dateStr.slice(0, 10) + 'T00:00:00').toLocaleDateString('es-CO', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
  })
}

export function formatShortDate(dateStr: string): string {
  return new Date(dateStr.slice(0, 10) + 'T00:00:00').toLocaleDateString('es-CO', {
    day: '2-digit',
    month: 'short',
  })
}

export function todayISO(): string {
  return new Date().toISOString().split('T')[0]
}

// La fecha de hoy solo tiene sentido como default si cae dentro de la
// quincena seleccionada; si no, se ajusta al borde más cercano del rango
// para que la transacción no quede fuera del period_id que se está creando.
export function clampDateToPeriod(period: { start_date: string; end_date: string } | null | undefined): string {
  const today = todayISO()
  if (!period) return today
  const start = period.start_date.slice(0, 10)
  const end = period.end_date.slice(0, 10)
  if (today < start) return start
  if (today > end) return end
  return today
}

export function pct(value: number, total: number): number {
  if (total === 0) return 0
  return Math.round((value / total) * 100)
}

export function clamp(value: number, min: number, max: number): number {
  return Math.min(Math.max(value, min), max)
}
