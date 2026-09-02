import { test, expect } from '@playwright/test'
import { createTestPeriod, deletePeriod, getPeriod, type TestPeriod } from './helpers/api'

let period: TestPeriod

test.beforeAll(async () => {
  period = await createTestPeriod({
    label: 'E2E Dashboard',
    base_income: 500000,
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
    const expectedAvailable = Number(current.base_income) + Number(current.rollover)
    const formatted = Math.round(expectedAvailable).toLocaleString('es-CO')

    await page.goto('/dashboard')
    await page.locator('select').first().selectOption({ label: period.label })

    await expect(page.locator('[class*="heroAmount"]')).toContainText(formatted)
    await expect(page.locator('[class*="kpiValue"]').first()).toHaveText(/\$\s?0/)
  })

  test('agregar un gasto rápido actualiza los gastos totales', async ({ page }) => {
    await page.goto('/dashboard')
    await page.locator('select').first().selectOption({ label: period.label })

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
