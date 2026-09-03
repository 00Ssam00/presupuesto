import { test, expect } from '@playwright/test'
import { createTestPeriod, deletePeriod, getFirstCategory, getCashPaymentMethod, type TestPeriod } from './helpers/api'

let period: TestPeriod
let category: { id: string; name: string }
let method: { id: string; name: string }

test.beforeAll(async () => {
  period = await createTestPeriod({
    label: 'E2E Import Export',
    start_date: '2099-03-01',
    end_date: '2099-03-15',
  })
  category = await getFirstCategory()
  method = await getCashPaymentMethod()
})

test.afterAll(async () => {
  await deletePeriod(period.id)
})

test.describe('Importar / Exportar', () => {
  test('importa un CSV con filas válidas e inválidas (importación parcial) hacia la quincena elegida', async ({ page }) => {
    const csv = [
      'monto,tipo,fecha,categoria,metodo,descripcion',
      `50000,expense,2099-03-05,${category.name},${method.name},"Compra E2E, con coma en la descripción"`,
      `20000,expense,2099-03-06,Categoría Inexistente E2E,${method.name},Fila inválida`,
    ].join('\n')

    await page.goto('/import')

    await page.locator('input[type="file"]').setInputFiles({
      name: 'transacciones.csv',
      mimeType: 'text/csv',
      buffer: Buffer.from(csv, 'utf-8'),
    })

    await expect(page.getByText('2 filas detectadas')).toBeVisible()

    await page.getByLabel('Quincena de destino').selectOption({ label: period.label })
    await page.getByRole('button', { name: /Importar 2 filas/ }).click()

    await expect(page.getByText('1 transacción importada, 1 fila con errores.')).toBeVisible()
    await expect(page.getByText(/Fila 2: Categoría no encontrada/)).toBeVisible()
  })

  test('exporta las transacciones de la quincena y el CSV se puede volver a leer', async ({ page }) => {
    await page.goto('/import')

    const exportSelect = page.getByLabel('Quincena')
    await exportSelect.selectOption({ label: period.label })

    const [download] = await Promise.all([
      page.waitForEvent('download'),
      page.getByRole('button', { name: 'Exportar transacciones' }).click(),
    ])

    const path = await download.path()
    expect(path).toBeTruthy()
    const fs = await import('fs')
    const content = fs.readFileSync(path!, 'utf-8')

    expect(content).toContain('monto,tipo,fecha,categoria,metodo,tarjeta,descripcion')
    // La fila con coma en la descripción debe volver escapada entre comillas.
    expect(content).toContain('"Compra E2E, con coma en la descripción"')
    expect(content).toContain(category.name)
  })
})
