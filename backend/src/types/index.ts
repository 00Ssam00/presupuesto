export interface Period {
  id: string
  label: string
  start_date: string
  end_date: string
  base_income: number
  rollover: number
  is_closed: boolean
  created_at: string
}

export interface Category {
  id: string
  name: string
  color: string
  icon: string | null
  is_active: boolean
  sort_order: number
  created_at: string
}

export interface PaymentMethod {
  id: string
  name: string
  requires_card: boolean
  is_active: boolean
}

export interface Card {
  id: string
  name: string
  last_four: string | null
  bank: string | null
  is_active: boolean
  created_at: string
}

export interface Transaction {
  id: string
  period_id: string
  category_id: string
  payment_method_id: string
  card_id: string | null
  type: 'income' | 'expense'
  amount: number
  description: string
  note: string | null
  date: string
  created_at: string
  updated_at: string
  category_name?: string
  category_color?: string
  payment_method_name?: string
  card_name?: string
}

export interface Budget {
  id: string
  period_id: string
  category_id: string
  amount: number
  percentage: number | null
  created_at: string
  category_name?: string
  category_color?: string
  spent?: number
}

export interface Goal {
  id: string
  name: string
  type: 'savings' | 'investment'
  target_amount: number
  current_amount: number
  deadline: string | null
  is_active: boolean
  created_at: string
  updated_at: string
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
  created_at: string
  category_name?: string
  category_color?: string
}

export interface DashboardSummary {
  period: Period
  available: number
  total_expenses: number
  total_income_extra: number
  remaining: number
  expenses_by_category: Array<{
    category_id: string
    category_name: string
    category_color: string
    total: number
    budget: number
  }>
  upcoming_payments: UpcomingPayment[]
}
