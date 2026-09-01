import { test, expect } from '@playwright/test'
import { createTestPeriod, deletePeriod, type TestPeriod } from './helpers/api'

let period: TestPeriod

test.beforeAll(async () => {
  period = await createTestPeriod({ label: 'E2E Transacciones' })
})

test.afterAll(async () => {
  await deletePeriod(period.id)
})

test.describe('Transacciones', () => {
  test('crear, editar y eliminar una transacción desde la UI', async ({ page }) => {
    await page.goto('/transactions')

    await page.locator('select').first().selectOption({ label: period.label })
    await expect(page.getByText('Sin transacciones')).toBeVisible()

    // Crear
    await page.getByRole('button', { name: '+ Nueva transacción' }).click()
    await page.getByLabel('Quincena').selectOption({ label: period.label })
    await page.getByLabel('Tipo').selectOption('expense')
    await page.getByLabel('Monto').fill('75000')
    await page.getByLabel('Descripción').fill('Compra E2E')
    await page.getByLabel('Categoría').selectOption({ index: 1 })
    // "Efectivo" no requiere tarjeta asociada, a diferencia de "Débito" y "Tarjeta Crédito".
    await page.getByLabel('Método de pago').selectOption({ label: 'Efectivo' })
    await page.getByRole('button', { name: 'Crear' }).click()

    await expect(page.getByText('Compra E2E')).toBeVisible()
    await expect(page.getByText('-$ 75.000').or(page.getByText('-$75.000'))).toBeVisible()

    // Editar
    await page.getByRole('row', { name: /Compra E2E/ }).getByRole('button', { name: 'Editar' }).click()
    await page.getByLabel('Monto').fill('90000')
    await page.getByRole('button', { name: 'Guardar' }).click()
    await expect(page.getByText('-$ 90.000').or(page.getByText('-$90.000'))).toBeVisible()

    // Eliminar
    page.once('dialog', dialog => dialog.accept())
    await page.getByRole('row', { name: /Compra E2E/ }).getByRole('button', { name: '✕' }).click()
    await expect(page.getByText('Compra E2E')).toHaveCount(0)
  })
})
