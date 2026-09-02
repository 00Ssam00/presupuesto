import { test, expect } from '@playwright/test'
import { createTestPeriod, deletePeriod, getFirstCategory, type TestPeriod } from './helpers/api'

let period: TestPeriod

test.beforeAll(async () => {
  period = await createTestPeriod({ label: 'E2E Presupuesto' })
})

test.afterAll(async () => {
  await deletePeriod(period.id)
})

test.describe('Presupuesto', () => {
  test('permite asignar presupuesto a una categoría sin fila existente', async ({ page }) => {
    const category = await getFirstCategory()

    await page.goto('/budget')
    await page.locator('select').first().selectOption({ label: period.label })

    // Sin ningún presupuesto asignado todavía, la categoría igual debe listarse
    // (con $0) en vez de estar ausente de la pantalla.
    const card = page.locator(`text=${category.name}`).locator('..').locator('..')
    await expect(card).toBeVisible()
    const input = card.getByRole('spinbutton')
    await expect(input).toHaveValue('0')

    // Asignar presupuesto a esa categoría (que no tenía fila en `budgets`).
    await input.fill('120000')
    await input.blur()

    // Persistido: al recargar sigue mostrando el valor asignado.
    await page.reload()
    await page.locator('select').first().selectOption({ label: period.label })
    const cardAfterReload = page.locator(`text=${category.name}`).locator('..').locator('..')
    await expect(cardAfterReload.getByRole('spinbutton')).toHaveValue('120000')
  })
})
