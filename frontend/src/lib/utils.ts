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
// para que la transacción no quede fuera del periodo_id que se está creando.
export function clampDateToPeriod(period: { fecha_inicio: string; fecha_fin: string } | null | undefined): string {
  const today = todayISO()
  if (!period) return today
  const start = period.fecha_inicio.slice(0, 10)
  const end = period.fecha_fin.slice(0, 10)
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

// Parser de una línea CSV (RFC 4180): respeta campos entre comillas que
// contienen comas o comillas escapadas (""), a diferencia de un `.split(',')`
// ingenuo que rompe en descripciones como `"Mercado, verduras y carne"`.
export function parseCsvLine(line: string): string[] {
  const result: string[] = []
  let cur = ''
  let inQuotes = false
  for (let i = 0; i < line.length; i++) {
    const ch = line[i]
    if (inQuotes) {
      if (ch === '"') {
        if (line[i + 1] === '"') { cur += '"'; i++ }
        else { inQuotes = false }
      } else {
        cur += ch
      }
    } else if (ch === '"') {
      inQuotes = true
    } else if (ch === ',') {
      result.push(cur)
      cur = ''
    } else {
      cur += ch
    }
  }
  result.push(cur)
  return result.map(v => v.trim())
}

// Envuelve en comillas (y escapa las comillas internas) solo los campos que
// lo necesitan, para producir un CSV válido al exportar.
export function csvEscapeField(value: string): string {
  if (/[",\n]/.test(value)) {
    return `"${value.replace(/"/g, '""')}"`
  }
  return value
}
