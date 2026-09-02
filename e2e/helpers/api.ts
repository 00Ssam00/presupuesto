const API_BASE = 'http://localhost:3000/api'

async function json<T>(res: Response): Promise<T> {
  if (!res.ok) {
    const body = await res.text()
    throw new Error(`API ${res.status}: ${body}`)
  }
  return res.json() as Promise<T>
}

export interface TestPeriod {
  id: string
  label: string
  start_date: string
  end_date: string
  base_income: number
}

export async function createTestPeriod(overrides: Partial<{ label: string; start_date: string; end_date: string; base_income: number }> = {}): Promise<TestPeriod> {
  const body = {
    label: 'E2E Quincena de prueba',
    start_date: '2031-01-01',
    end_date: '2031-01-15',
    base_income: 1000000,
    ...overrides,
  }
  const res = await fetch(`${API_BASE}/periods`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  })
  return json<TestPeriod>(res)
}

export async function deletePeriod(id: string): Promise<void> {
  await fetch(`${API_BASE}/periods/${id}`, { method: 'DELETE' })
}

export async function getPeriod(id: string): Promise<TestPeriod & { rollover: number }> {
  const res = await fetch(`${API_BASE}/periods/${id}`)
  return json<TestPeriod & { rollover: number }>(res)
}

export async function deleteTransaction(id: string): Promise<void> {
  await fetch(`${API_BASE}/transactions/${id}`, { method: 'DELETE' })
}

export async function getFirstCategory(): Promise<{ id: string; name: string }> {
  const res = await fetch(`${API_BASE}/categories`)
  const cats = await json<Array<{ id: string; name: string }>>(res)
  if (cats.length === 0) throw new Error('No hay categorías en la base de datos para el test')
  return cats[0]
}

export async function getCashPaymentMethod(): Promise<{ id: string; name: string }> {
  const res = await fetch(`${API_BASE}/payment-methods`)
  const methods = await json<Array<{ id: string; name: string; requires_card: boolean }>>(res)
  const cash = methods.find(m => !m.requires_card) || methods[0]
  if (!cash) throw new Error('No hay métodos de pago en la base de datos para el test')
  return cash
}
