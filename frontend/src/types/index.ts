export interface Periodo {
  id: string
  etiqueta: string
  fecha_inicio: string
  fecha_fin: string
  ingreso_base: number
  arrastre: number
  esta_cerrado: boolean
  creado_en: string
}

export interface Categoria {
  id: string
  nombre: string
  color: string
  icono: string | null
  esta_activo: boolean
  orden: number
}

export interface MetodoPago {
  id: string
  nombre: string
  requiere_tarjeta: boolean
  esta_activo: boolean
}

export interface Tarjeta {
  id: string
  nombre: string
  ultimos_cuatro: string | null
  banco: string | null
  esta_activo: boolean
}

export interface Transaccion {
  id: string
  periodo_id: string
  categoria_id: string
  metodo_pago_id: string
  tarjeta_id: string | null
  tipo: 'income' | 'expense'
  monto: number
  descripcion: string
  nota: string | null
  fecha: string
  nombre_categoria?: string
  color_categoria?: string
  nombre_metodo_pago?: string
  nombre_tarjeta?: string
}

export interface Presupuesto {
  id: string | null
  periodo_id: string
  categoria_id: string
  monto: number
  porcentaje: number | null
  nombre_categoria?: string
  color_categoria?: string
  gastado?: number
}

// Meta y PagoProximo: Fase 7 de la migración — ya viven en el backend .NET,
// directamente en español (no hubo versión en inglés que traducir).
export interface Meta {
  id: string
  nombre: string
  tipo: 'savings' | 'investment'
  monto_objetivo: number
  monto_actual: number
  fecha_limite: string | null
  esta_activo: boolean
}

// Misma forma tanto para el pago próximo anidado en /api/panel/{id} como
// para el recurso independiente /api/pagos-proximos.
export interface PagoProximo {
  id: string
  periodo_id: string | null
  categoria_id: string
  nombre: string
  monto: number
  fecha_vencimiento: string
  estado: 'pending' | 'paid' | 'overdue'
  es_recurrente: boolean
  intervalo_recurrencia: 'weekly' | 'biweekly' | 'monthly' | null
  nombre_categoria?: string
  color_categoria?: string
}

export interface DatosPanel {
  periodo: Periodo
  disponible: number
  total_gastos: number
  total_ingreso_extra: number
  restante: number
  gastos_por_categoria: Array<{
    categoria_id: string
    nombre_categoria: string
    color_categoria: string
    total: number
    presupuesto: number
  }>
  pagos_proximos: PagoProximo[]
}
