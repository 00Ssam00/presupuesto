import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import Badge from './Badge'

describe('Badge', () => {
  it('renderiza su contenido de texto', () => {
    render(<Badge>Pendiente</Badge>)
    expect(screen.getByText('Pendiente')).toBeInTheDocument()
  })

  it('aplica un estilo inline personalizado', () => {
    render(<Badge style={{ background: 'rgb(1, 2, 3)' }}>Custom</Badge>)
    expect(screen.getByText('Custom')).toHaveStyle({ background: 'rgb(1, 2, 3)' })
  })
})
