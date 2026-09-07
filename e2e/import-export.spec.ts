import { test, expect } from '@playwright/test'
import { createTestPeriod, deletePeriod, getFirstCategory, getCashPaymentMethod, type TestPeriod } from './helpers/api'

let periodA: TestPeriod
let periodB: TestPeriod
let category: { id: string; nombre: string }
let method: { id: string; nombre: string }

test.beforeAll(async () => {
  periodA = await createTestPeriod({
    etiqueta: 'E2E Import Export A',
    fecha_inicio: '2099-03-01',
    fecha_fin: '2099-03-15',
  })
  periodB = await createTestPeriod({
    etiqueta: 'E2E Import Export B',
    fecha_inicio: '2099-03-16',
    fecha_fin: '2099-03-31',
  })
  category = await getFirstCategory()
  method = await getCashPaymentMethod()
})

test.afterAll(async () => {
  await deletePeriod(periodA.id)
  await deletePeriod(periodB.id)
})

test.describe('Importar / Exportar', () => {
  test('importa un CSV que abarca dos quincenas en una sola pasada, resolviendo cada fila por su fecha', async ({ page }) => {
    const csv = [
      'monto,tipo,fecha,categoria,metodo,descripcion',
      `50000,expense,2099-03-05,${category.nombre},${method.nombre},"Compra E2E, con coma en la descripción"`,
      `30000,expense,2099-03-20,${category.nombre},${method.nombre},Compra en la segunda quincena`,
      `20000,expense,2099-03-06,Categoría Inexistente E2E,${method.nombre},Fila inválida`,
    ].join('\n')

    await page.goto('/import')

    // Ya no hay selector de quincena: la fecha de cada fila decide a cuál pertenece.
    await expect(page.getByLabel('Quincena de destino')).toHaveCount(0)

    await page.locator('input[type="file"]').setInputFiles({
      name: 'transacciones.csv',
      mimeType: 'text/csv',
      buffer: Buffer.from(csv, 'utf-8'),
    })

    await expect(page.getByText('3 filas detectadas')).toBeVisible()
    await page.getByRole('button', { name: /Importar 3 filas/ }).click()

    await expect(page.getByText('2 transacciones importadas, 1 fila con errores.')).toBeVisible()
    await expect(page.getByText(/Fila 3: Categoría no encontrada/)).toBeVisible()

    // Cada transacción quedó en la quincena que le corresponde por fecha.
    await page.goto('/transactions')
    await page.locator('select').first().selectOption({ label: periodA.etiqueta })
    await expect(page.getByText('Compra E2E, con coma en la descripción')).toBeVisible()
    await page.locator('select').first().selectOption({ label: periodB.etiqueta })
    await expect(page.getByText('Compra en la segunda quincena')).toBeVisible()
  })

  test('rechaza una fila cuya fecha no cae en ninguna quincena existente', async ({ page }) => {
    const csv = [
      'monto,tipo,fecha,categoria,metodo,descripcion',
      `10000,expense,2199-01-01,${category.nombre},${method.nombre},Fecha sin quincena`,
    ].join('\n')

    await page.goto('/import')
    await page.locator('input[type="file"]').setInputFiles({
      name: 'sin-quincena.csv',
      mimeType: 'text/csv',
      buffer: Buffer.from(csv, 'utf-8'),
    })
    await page.getByRole('button', { name: /Importar 1 fila/ }).click()

    await expect(page.getByText(/Fila 1: No hay ninguna quincena que incluya la fecha/)).toBeVisible()
  })

  test('exporta las transacciones de una quincena y el CSV se puede volver a leer', async ({ page }) => {
    await page.goto('/import')

    const exportSelect = page.getByLabel('Quincena')
    await exportSelect.selectOption({ label: periodA.etiqueta })

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
    expect(content).toContain(category.nombre)
    // No debe incluir transacciones de la otra quincena.
    expect(content).not.toContain('Compra en la segunda quincena')
  })
})
