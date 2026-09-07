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

// Goal y UpcomingPayment siguen en inglés: todavía los sirve el backend Node
// (Fase 7 de la migración, pendiente), no el backend .NET ya traducido.
export interface Goal {
  id: string
  name: string
  type: 'savings' | 'investment'
  target_amount: number
  current_amount: number
  deadline: string | null
  is_active: boolean
}

export interface UpcomingPayment {
  id: string
  period_id: string | null
  category_id: string
  name: string
  amount: number
  due_date: string
  status: 'pending' | 'paid' | 'overdue'
  is_recurring: boolean
  recurrence_interval: 'weekly' | 'biweekly' | 'monthly' | null
  category_name?: string
  category_color?: string
}

// Forma en español de un pago próximo tal como lo anida /api/panel/{id} (ya
// migrado a .NET) — distinta de UpcomingPayment de arriba, que sigue siendo la
// forma en inglés del CRUD independiente que todavía sirve Node.
export interface PagoProximoPanel {
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
  pagos_proximos: PagoProximoPanel[]
}
