import { test, expect } from '@playwright/test'
import { createTestPeriod, deletePeriod, getPeriod, type TestPeriod } from './helpers/api'

let periodA: TestPeriod
let periodB: TestPeriod

test.beforeAll(async () => {
  periodA = await createTestPeriod({
    label: 'E2E Rollover A',
    start_date: '2099-01-01',
    end_date: '2099-01-15',
    base_income: 1000000,
  })
  periodB = await createTestPeriod({
    label: 'E2E Rollover B',
    start_date: '2099-01-16',
    end_date: '2099-01-31',
    base_income: 1000000,
  })
})

test.afterAll(async () => {
  await deletePeriod(periodA.id)
  await deletePeriod(periodB.id)
})

test.describe('Rollover automático', () => {
  test('el gasto de una quincena reduce el rollover automático de la siguiente', async ({ page }) => {
    const initialA = await getPeriod(periodA.id)
    const initialB = await getPeriod(periodB.id)
    // Sin transacciones en A, el rollover de B = base_income_A + rollover_A (todo el sobrante de A).
    const expectedInitialBRollover = Number(initialA.base_income) + Number(initialA.rollover)
    expect(Math.round(Number(initialB.rollover))).toBe(Math.round(expectedInitialBRollover))

    // Agregar un gasto en A vía el "gasto rápido" del Dashboard.
    await page.goto('/dashboard')
    await page.locator('select').first().selectOption({ label: periodA.label })
    await page.getByRole('button', { name: '+ Agregar gasto' }).click()
    await page.getByLabel('Monto').fill('300000')
    await page.getByLabel('Descripción').fill('Gasto Rollover E2E')
    await page.getByLabel('Categoría').selectOption({ index: 1 })
    // "Efectivo" no requiere tarjeta asociada, a diferencia de "Débito" y "Tarjeta Crédito".
    await page.getByLabel('Método').selectOption({ label: 'Efectivo' })
    await page.getByRole('button', { name: 'Guardar' }).click()
    await expect(page.locator('[class*="kpiValue"]').first()).toHaveText(/300\.000/)

    // El rollover de B en la base de datos debe reflejar el sobrante reducido de A.
    const afterB = await getPeriod(periodB.id)
    const expectedAfterBRollover = expectedInitialBRollover - 300000
    expect(Math.round(Number(afterB.rollover))).toBe(Math.round(expectedAfterBRollover))

    // Y el saldo disponible de B mostrado en el Dashboard debe incluir ese rollover.
    const expectedAvailableB = Number(periodB.base_income) + expectedAfterBRollover
    const formatted = Math.round(expectedAvailableB).toLocaleString('es-CO')
    await page.locator('select').first().selectOption({ label: periodB.label })
    await expect(page.locator('[class*="heroAmount"]')).toContainText(formatted)
  })
})
