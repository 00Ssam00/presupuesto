import { test, expect } from '@playwright/test'

// Regresión: editar una categoría/tarjeta/meta desde la UI no enviaba
// is_active en el body, y el backend lo pisaba a NULL — el registro
// desaparecía de todos los listados (que filtran is_active = TRUE) aunque
// la edición "se guardara" sin error visible.

test.describe('Configuración — editar no borra el registro', () => {
  test('editar una categoría la mantiene visible en el listado', async ({ page }) => {
    await page.goto('/settings')

    await page.getByRole('button', { name: '+ Agregar' }).first().click()
    await page.getByLabel('Nombre').fill('E2E Categoria Temp')
    await page.getByRole('button', { name: 'Guardar' }).click()

    const row = page.locator('[class*="listItem"]').filter({ hasText: 'E2E Categoria Temp' })
    await expect(row).toBeVisible()

    await row.getByRole('button', { name: 'Editar' }).click()
    await page.getByLabel('Nombre').fill('E2E Categoria Editada')
    await page.getByRole('button', { name: 'Guardar' }).click()

    // Antes del fix, la categoría editada desaparecía por completo del listado.
    const editedRow = page.locator('[class*="listItem"]').filter({ hasText: 'E2E Categoria Editada' })
    await expect(editedRow).toBeVisible()

    page.once('dialog', dialog => dialog.accept())
    await editedRow.getByRole('button', { name: '✕' }).click()
    await expect(page.locator('[class*="listItem"]').filter({ hasText: 'E2E Categoria Editada' })).toHaveCount(0)
  })

  test('editar una tarjeta la mantiene visible en el listado', async ({ page }) => {
    await page.goto('/settings')

    await page.getByRole('button', { name: '+ Agregar' }).nth(1).click()
    await page.getByLabel('Nombre de la tarjeta').fill('E2E Tarjeta Temp')
    await page.getByRole('button', { name: 'Guardar' }).click()

    const row = page.locator('[class*="listItem"]').filter({ hasText: 'E2E Tarjeta Temp' })
    await expect(row).toBeVisible()

    await row.getByRole('button', { name: 'Editar' }).click()
    await page.getByLabel('Nombre de la tarjeta').fill('E2E Tarjeta Editada')
    await page.getByRole('button', { name: 'Guardar' }).click()

    const editedRow = page.locator('[class*="listItem"]').filter({ hasText: 'E2E Tarjeta Editada' })
    await expect(editedRow).toBeVisible()

    page.once('dialog', dialog => dialog.accept())
    await editedRow.getByRole('button', { name: '✕' }).click()
    await expect(page.locator('[class*="listItem"]').filter({ hasText: 'E2E Tarjeta Editada' })).toHaveCount(0)
  })

  test('editar una meta la mantiene visible en el listado', async ({ page }) => {
    await page.goto('/goals')

    await page.getByRole('button', { name: '+ Nueva meta' }).click()
    await page.getByLabel('Nombre').fill('E2E Meta Temp')
    await page.getByLabel('Meta').fill('1000000')
    await page.getByRole('button', { name: 'Crear' }).click()

    await expect(page.getByText('E2E Meta Temp')).toBeVisible()

    await page.locator('[class*="goalCard"]').filter({ hasText: 'E2E Meta Temp' })
      .getByRole('button', { name: 'Editar' }).click()
    await page.getByLabel('Nombre').fill('E2E Meta Editada')
    await page.getByRole('button', { name: 'Guardar' }).click()

    await expect(page.getByText('E2E Meta Editada')).toBeVisible()

    page.once('dialog', dialog => dialog.accept())
    await page.locator('[class*="goalCard"]').filter({ hasText: 'E2E Meta Editada' })
      .getByRole('button', { name: '✕' }).click()
    await expect(page.getByText('E2E Meta Editada')).toHaveCount(0)
  })
})
