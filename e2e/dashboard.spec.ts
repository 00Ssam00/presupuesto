import { test, expect } from '@playwright/test'
import { createTestPeriod, deletePeriod, getPeriod, getCurrentPeriod, type TestPeriod } from './helpers/api'

let period: TestPeriod

test.beforeAll(async () => {
  period = await createTestPeriod({
    etiqueta: 'E2E Dashboard',
    ingreso_base: 500000,
  })
})

test.afterAll(async () => {
  await deletePeriod(period.id)
})

test.describe('Dashboard', () => {
  test('el saldo disponible refleja el ingreso base más el rollover automático de la quincena seleccionada', async ({ page }) => {
    // El rollover se recalcula automáticamente a partir del sobrante/déficit de la
    // quincena anterior, así que el saldo disponible no es solo el ingreso base.
    const current = await getPeriod(period.id)
    const expectedAvailable = Number(current.ingreso_base) + Number(current.arrastre)
    const formatted = Math.round(expectedAvailable).toLocaleString('es-CO')

    await page.goto('/dashboard')
    await page.locator('select').first().selectOption({ label: period.etiqueta })

    await expect(page.locator('[class*="heroAmount"]')).toContainText(formatted)
    await expect(page.locator('[class*="kpiValue"]').first()).toHaveText(/\$\s?0/)
  })

  test('la fecha del gasto rápido respeta la quincena seleccionada, no la fecha real de hoy', async ({ page }) => {
    // `period` (helper createTestPeriod) usa por defecto 2031-01-01..2031-01-15,
    // un rango que no incluye la fecha real de hoy. El campo "Fecha" del modal
    // debía quedar fijo en la fecha de hoy sin importar la quincena elegida
    // (bug reportado); ahora debe ajustarse al rango de la quincena.
    await page.goto('/dashboard')
    await page.locator('select').first().selectOption({ label: period.etiqueta })

    await page.getByRole('button', { name: '+ Agregar gasto' }).click()
    const dateInput = page.getByLabel('Fecha')
    await expect(dateInput).toHaveValue(period.fecha_inicio.slice(0, 10))
  })

  test('la fecha del gasto rápido usa la fecha real de hoy cuando cae dentro de la quincena actual', async ({ page }) => {
    const current = await getCurrentPeriod()

    await page.goto('/dashboard')
    await page.locator('select').first().selectOption({ label: current.etiqueta })

    await page.getByRole('button', { name: '+ Agregar gasto' }).click()
    const today = new Date().toISOString().slice(0, 10)
    await expect(page.getByLabel('Fecha')).toHaveValue(today)
  })

  test('agregar un gasto rápido actualiza los gastos totales', async ({ page }) => {
    await page.goto('/dashboard')
    await page.locator('select').first().selectOption({ label: period.etiqueta })

    await page.getByRole('button', { name: '+ Agregar gasto' }).click()
    await page.getByLabel('Monto').fill('30000')
    await page.getByLabel('Descripción').fill('Gasto rápido E2E')
    await page.getByLabel('Categoría').selectOption({ index: 1 })
    // "Efectivo" no requiere tarjeta asociada, a diferencia de "Débito" y "Tarjeta Crédito".
    await page.getByLabel('Método').selectOption({ label: 'Efectivo' })
    await page.getByRole('button', { name: 'Guardar' }).click()

    await expect(page.getByText('Gasto rápido E2E')).toHaveCount(0) // el modal se cierra, no hay tabla en el dashboard
    await expect(page.locator('[class*="kpiValue"]').first()).toHaveText(/30\.000/)
  })
})
