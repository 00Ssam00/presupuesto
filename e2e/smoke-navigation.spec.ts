import { test, expect } from '@playwright/test'

// Cubre como regresión el bug donde el proxy de Vite apuntaba al puerto
// equivocado (3001 en vez de 3000) y toda la app mostraba "No se pudo
// conectar al servidor." Cualquier fallo de comunicación frontend-backend
// debería hacer fallar alguna de estas aserciones.

test.describe('Navegación general', () => {
  test('el Dashboard carga datos reales del backend', async ({ page }) => {
    await page.goto('/dashboard')
    await expect(page.getByText('No se pudo conectar al servidor.')).toHaveCount(0)
    await expect(page.getByText('SALDO DISPONIBLE', { exact: true })).toBeVisible()
  })

  test('recorre todas las secciones del sidebar sin errores de conexión', async ({ page }) => {
    await page.goto('/dashboard')

    const sections: Array<{ link: string; heading: string }> = [
      { link: 'Transacciones', heading: 'Transacciones' },
      { link: 'Presupuesto', heading: 'Presupuesto' },
      { link: 'Metas', heading: 'Metas' },
      { link: 'Importar / Exportar', heading: 'Importar / Exportar' },
      { link: 'Configuración', heading: 'Configuración' },
    ]

    for (const { link, heading } of sections) {
      await page.getByRole('link', { name: link }).click()
      await expect(page.getByRole('heading', { name: heading })).toBeVisible()
      await expect(page.getByText('No se pudo conectar al servidor.')).toHaveCount(0)
    }
  })

  test('la sección de Configuración muestra fechas de quincena legibles, no ISO crudo', async ({ page }) => {
    await page.goto('/settings')
    await expect(page.getByRole('heading', { name: 'Quincenas' })).toBeVisible()
    // Regresión: las quincenas no deben mostrar timestamps ISO sin formatear (con "T" y "Z").
    await expect(page.getByText(/\d{4}-\d{2}-\d{2}T/)).toHaveCount(0)
  })
})
