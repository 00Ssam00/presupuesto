import { describe, it, expect, vi, afterEach } from 'vitest'
import { render, screen, act } from '@testing-library/react'
import ToastHost from './ToastHost'
import { showToast } from '../../lib/toast'

describe('ToastHost', () => {
  afterEach(() => {
    vi.useRealTimers()
  })

  it('no renderiza nada sin toasts', () => {
    const { container } = render(<ToastHost />)
    expect(container).toBeEmptyDOMElement()
  })

  it('muestra un toast cuando se llama a showToast', () => {
    render(<ToastHost />)
    act(() => {
      showToast('La conexión está lenta.', 'error')
    })
    expect(screen.getByText('La conexión está lenta.')).toBeInTheDocument()
  })

  it('descarta el toast automáticamente después de 5 segundos', () => {
    vi.useFakeTimers()
    render(<ToastHost />)
    act(() => {
      showToast('mensaje temporal', 'error')
    })
    expect(screen.getByText('mensaje temporal')).toBeInTheDocument()

    act(() => {
      vi.advanceTimersByTime(5000)
    })
    expect(screen.queryByText('mensaje temporal')).not.toBeInTheDocument()
  })

  it('permite descartar el toast manualmente', async () => {
    render(<ToastHost />)
    act(() => {
      showToast('cerrar esto', 'error')
    })
    const closeBtn = screen.getByRole('button', { name: 'Cerrar' })
    act(() => {
      closeBtn.click()
    })
    expect(screen.queryByText('cerrar esto')).not.toBeInTheDocument()
  })
})
