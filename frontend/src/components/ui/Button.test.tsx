import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import Button from './Button'

describe('Button', () => {
  it('renderiza su contenido', () => {
    render(<Button>Guardar</Button>)
    expect(screen.getByRole('button', { name: 'Guardar' })).toBeInTheDocument()
  })

  it('invoca onClick al hacer click', async () => {
    const onClick = vi.fn()
    render(<Button onClick={onClick}>Guardar</Button>)
    await userEvent.click(screen.getByRole('button'))
    expect(onClick).toHaveBeenCalledTimes(1)
  })

  it('respeta el atributo disabled y no dispara onClick', async () => {
    const onClick = vi.fn()
    render(<Button onClick={onClick} disabled>Guardar</Button>)
    await userEvent.click(screen.getByRole('button'))
    expect(onClick).not.toHaveBeenCalled()
  })

  it('usa type="button" cuando se pasa explícitamente', () => {
    render(<Button type="button">Cancelar</Button>)
    expect(screen.getByRole('button')).toHaveAttribute('type', 'button')
  })
})
