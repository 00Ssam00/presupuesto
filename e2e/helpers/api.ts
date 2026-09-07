// Va contra el proxy de Vite (no directo a un backend) porque cada recurso se
// enruta a un backend distinto: los ya migrados van a .NET, el resto a Node
// (ver el split de proxy en frontend/vite.config.ts, que espeja vercel.json).
const API_BASE = 'http://localhost:5173/api'

async function json<T>(res: Response): Promise<T> {
  if (!res.ok) {
    const body = await res.text()
    throw new Error(`API ${res.status}: ${body}`)
  }
  return res.json() as Promise<T>
}

export interface TestPeriod {
  id: string
  etiqueta: string
  fecha_inicio: string
  fecha_fin: string
  ingreso_base: number
}

export async function createTestPeriod(overrides: Partial<{ etiqueta: string; fecha_inicio: string; fecha_fin: string; ingreso_base: number }> = {}): Promise<TestPeriod> {
  const body = {
    etiqueta: 'E2E Quincena de prueba',
    fecha_inicio: '2031-01-01',
    fecha_fin: '2031-01-15',
    ingreso_base: 1000000,
    ...overrides,
  }
  const res = await fetch(`${API_BASE}/periodos`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  })
  return json<TestPeriod>(res)
}

export async function deletePeriod(id: string): Promise<void> {
  await fetch(`${API_BASE}/periodos/${id}`, { method: 'DELETE' })
}

export async function getPeriod(id: string): Promise<TestPeriod & { arrastre: number }> {
  const res = await fetch(`${API_BASE}/periodos/${id}`)
  return json<TestPeriod & { arrastre: number }>(res)
}

export async function getCurrentPeriod(): Promise<TestPeriod & { arrastre: number }> {
  const res = await fetch(`${API_BASE}/periodos/actual`)
  return json<TestPeriod & { arrastre: number }>(res)
}

export async function deleteTransaction(id: string): Promise<void> {
  await fetch(`${API_BASE}/transacciones/${id}`, { method: 'DELETE' })
}

export async function getFirstCategory(): Promise<{ id: string; nombre: string }> {
  const res = await fetch(`${API_BASE}/categorias`)
  const cats = await json<Array<{ id: string; nombre: string }>>(res)
  if (cats.length === 0) throw new Error('No hay categorías en la base de datos para el test')
  return cats[0]
}

export async function getCashPaymentMethod(): Promise<{ id: string; nombre: string }> {
  const res = await fetch(`${API_BASE}/metodos-pago`)
  const methods = await json<Array<{ id: string; nombre: string; requiere_tarjeta: boolean }>>(res)
  const cash = methods.find(m => !m.requiere_tarjeta) || methods[0]
  if (!cash) throw new Error('No hay métodos de pago en la base de datos para el test')
  return cash
}
