import { describe, it, expect, vi, afterEach } from 'vitest'
import { formatCOP, formatDate, formatShortDate, todayISO, pct, clamp, clampDateToPeriod, parseCsvLine, csvEscapeField } from './utils'

describe('formatCOP', () => {
  it('formatea como pesos colombianos sin decimales', () => {
    const out = formatCOP(1022000)
    expect(out.replace(/\s/g, '')).toBe('$1.022.000')
  })

  it('redondea a entero (sin decimales)', () => {
    const out = formatCOP(1500.75)
    expect(out).not.toContain(',75')
    expect(out.replace(/\s/g, '')).toBe('$1.501')
  })

  it('formatea cero', () => {
    expect(formatCOP(0).replace(/\s/g, '')).toBe('$0')
  })

  it('formatea negativos con signo', () => {
    expect(formatCOP(-5000)).toContain('-')
  })
})

describe('formatDate', () => {
  it('formatea una fecha plana YYYY-MM-DD', () => {
    const out = formatDate('2026-04-03')
    expect(out).toContain('2026')
    expect(out).toMatch(/03/)
  })

  it('no se corre de día por husos horarios (regresión del bug de fechas)', () => {
    // Si se parseara con `new Date('2026-01-01')` a secas (UTC medianoche) en un
    // huso horario negativo, mostraría 31 de diciembre en vez de 1 de enero.
    const out = formatDate('2026-01-01')
    expect(out).toContain('01')
    expect(out).not.toContain('31')
  })

  it('tolera timestamps ISO completos igual que fechas planas', () => {
    const plain = formatDate('2026-06-16')
    const withTime = formatDate('2026-06-16T05:00:00.000Z')
    expect(withTime).toBe(plain)
  })
})

describe('formatShortDate', () => {
  it('no incluye el año', () => {
    const out = formatShortDate('2026-04-03')
    expect(out).not.toContain('2026')
  })
})

describe('todayISO', () => {
  it('devuelve formato YYYY-MM-DD', () => {
    expect(todayISO()).toMatch(/^\d{4}-\d{2}-\d{2}$/)
  })
})

describe('pct', () => {
  it('calcula el porcentaje redondeado', () => {
    expect(pct(50, 200)).toBe(25)
    expect(pct(1, 3)).toBe(33)
  })

  it('devuelve 0 cuando el total es 0 (evita división por cero)', () => {
    expect(pct(100, 0)).toBe(0)
  })

  it('puede superar 100 cuando el valor excede el total', () => {
    expect(pct(150, 100)).toBe(150)
  })
})

describe('clampDateToPeriod', () => {
  afterEach(() => {
    vi.useRealTimers()
  })

  it('devuelve hoy si cae dentro del rango de la quincena', () => {
    vi.useFakeTimers()
    vi.setSystemTime(new Date('2026-06-20T12:00:00.000Z'))
    const period = { fecha_inicio: '2026-06-16', fecha_fin: '2026-06-30' }
    expect(clampDateToPeriod(period)).toBe('2026-06-20')
  })

  it('ajusta al inicio de la quincena si hoy es anterior a su rango', () => {
    vi.useFakeTimers()
    vi.setSystemTime(new Date('2026-06-01T12:00:00.000Z'))
    const period = { fecha_inicio: '2026-06-16', fecha_fin: '2026-06-30' }
    expect(clampDateToPeriod(period)).toBe('2026-06-16')
  })

  it('ajusta al final de la quincena si hoy es posterior a su rango', () => {
    vi.useFakeTimers()
    vi.setSystemTime(new Date('2026-07-15T12:00:00.000Z'))
    const period = { fecha_inicio: '2026-06-16', fecha_fin: '2026-06-30' }
    expect(clampDateToPeriod(period)).toBe('2026-06-30')
  })

  it('devuelve hoy si no hay quincena seleccionada', () => {
    expect(clampDateToPeriod(null)).toBe(todayISO())
  })

  it('tolera timestamps ISO completos en fecha_inicio/fecha_fin', () => {
    vi.useFakeTimers()
    vi.setSystemTime(new Date('2026-06-01T12:00:00.000Z'))
    const period = { fecha_inicio: '2026-06-16T00:00:00.000Z', fecha_fin: '2026-06-30T00:00:00.000Z' }
    expect(clampDateToPeriod(period)).toBe('2026-06-16')
  })
})

describe('parseCsvLine', () => {
  it('divide una línea simple por comas', () => {
    expect(parseCsvLine('85000,expense,2026-04-03')).toEqual(['85000', 'expense', '2026-04-03'])
  })

  it('respeta comas dentro de un campo entre comillas (regresión: split(",") ingenuo lo rompía)', () => {
    expect(parseCsvLine('85000,expense,"Mercado, verduras y carne"')).toEqual(['85000', 'expense', 'Mercado, verduras y carne'])
  })

  it('des-escapa comillas dobles dentro de un campo entre comillas', () => {
    expect(parseCsvLine('1,"dijo ""hola"" al llegar"')).toEqual(['1', 'dijo "hola" al llegar'])
  })

  it('recorta espacios alrededor de cada valor', () => {
    expect(parseCsvLine(' 85000 , expense ')).toEqual(['85000', 'expense'])
  })
})

describe('csvEscapeField', () => {
  it('deja igual un valor sin caracteres especiales', () => {
    expect(csvEscapeField('Mercado')).toBe('Mercado')
  })

  it('envuelve en comillas un valor con coma', () => {
    expect(csvEscapeField('Mercado, verduras')).toBe('"Mercado, verduras"')
  })

  it('escapa comillas internas duplicándolas', () => {
    expect(csvEscapeField('dijo "hola"')).toBe('"dijo ""hola"""')
  })

  it('produce un valor que parseCsvLine puede leer de vuelta sin pérdida', () => {
    const original = 'Mercado, "verduras" y carne'
    const line = [csvEscapeField(original), csvEscapeField('expense')].join(',')
    expect(parseCsvLine(line)).toEqual([original, 'expense'])
  })
})

describe('clamp', () => {
  it('mantiene el valor si está dentro del rango', () => {
    expect(clamp(50, 0, 100)).toBe(50)
  })

  it('recorta al mínimo', () => {
    expect(clamp(-10, 0, 100)).toBe(0)
  })

  it('recorta al máximo', () => {
    expect(clamp(150, 0, 100)).toBe(100)
  })
})
