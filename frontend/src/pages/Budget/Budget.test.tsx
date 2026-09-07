import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import Budget from './index'
import { obtenerPresupuestos, guardarPresupuesto } from '../../lib/api'
import { useReferenceData } from '../../lib/ReferenceDataContext'

vi.mock('../../lib/api')
vi.mock('../../lib/ReferenceDataContext')

const period = {
  id: 'p1', etiqueta: 'Quincena de prueba', fecha_inicio: '2026-01-01', fecha_fin: '2026-01-15',
  ingreso_base: 1000000, arrastre: 0, esta_cerrado: false, creado_en: '2026-01-01T00:00:00.000Z',
}

beforeEach(() => {
  vi.mocked(useReferenceData).mockReturnValue({
    categories: [], methods: [], cards: [],
    periods: [period],
    currentPeriod: period,
    loading: false,
    error: '',
    refresh: vi.fn(),
  })
})

describe('Budget', () => {
  it('muestra las categorías sin presupuesto asignado con monto en 0, listas para editar', async () => {
    vi.mocked(obtenerPresupuestos).mockResolvedValue([
      { id: 'b1', periodo_id: 'p1', categoria_id: 'c1', monto: 50000, porcentaje: null, nombre_categoria: 'Alimentación', color_categoria: '#000', gastado: 20000 },
      { id: null, periodo_id: 'p1', categoria_id: 'c2', monto: 0, porcentaje: null, nombre_categoria: 'Transporte', color_categoria: '#111', gastado: 0 },
    ])

    render(<Budget />)

    await waitFor(() => expect(screen.getByText('Transporte')).toBeInTheDocument())
    expect(screen.getByText('Alimentación')).toBeInTheDocument()

    const inputs = screen.getAllByRole('spinbutton') as HTMLInputElement[]
    expect(inputs.map(i => i.value)).toEqual(['50000', '0'])
  })

  it('asigna presupuesto a una categoría sin fila existente (upsert por categoria_id)', async () => {
    vi.mocked(obtenerPresupuestos).mockResolvedValue([
      { id: null, periodo_id: 'p1', categoria_id: 'c2', monto: 0, porcentaje: null, nombre_categoria: 'Transporte', color_categoria: '#111', gastado: 0 },
    ])
    vi.mocked(guardarPresupuesto).mockResolvedValue({ id: 'new-b', periodo_id: 'p1', categoria_id: 'c2', monto: 80000, porcentaje: null })

    render(<Budget />)
    await waitFor(() => expect(screen.getByText('Transporte')).toBeInTheDocument())

    const input = screen.getByRole('spinbutton')
    await userEvent.clear(input)
    await userEvent.type(input, '80000')
    await userEvent.tab()

    await waitFor(() => {
      expect(guardarPresupuesto).toHaveBeenCalledWith({ periodo_id: 'p1', categoria_id: 'c2', monto: 80000 })
    })
  })
})
