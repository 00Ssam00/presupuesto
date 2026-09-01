-- =====================================================
-- Presupuesto Personal — Esquema PostgreSQL
-- Ejecutar contra la base de datos Neon (psql o consola SQL de Neon)
-- =====================================================

CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Categorías
CREATE TABLE categories (
  id           UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  name         TEXT NOT NULL,
  color        TEXT NOT NULL DEFAULT '#01696f',
  icon         TEXT,
  is_active    BOOLEAN NOT NULL DEFAULT TRUE,
  sort_order   INT NOT NULL DEFAULT 0,
  created_at   TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Métodos de pago
CREATE TABLE payment_methods (
  id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  name          TEXT NOT NULL,
  requires_card BOOLEAN NOT NULL DEFAULT FALSE,
  is_active     BOOLEAN NOT NULL DEFAULT TRUE
);

-- Tarjetas (solo para método crédito)
CREATE TABLE cards (
  id         UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  name       TEXT NOT NULL,
  last_four  CHAR(4),
  bank       TEXT,
  is_active  BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Quincenas (períodos)
CREATE TABLE periods (
  id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  label       TEXT NOT NULL,
  start_date  DATE NOT NULL,
  end_date    DATE NOT NULL,
  base_income NUMERIC(12,2) NOT NULL DEFAULT 0,
  rollover    NUMERIC(12,2) NOT NULL DEFAULT 0,
  is_closed   BOOLEAN NOT NULL DEFAULT FALSE,
  created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT periods_dates_check CHECK (end_date > start_date)
);

-- Presupuestos (por categoría y quincena)
CREATE TABLE budgets (
  id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  period_id   UUID NOT NULL REFERENCES periods(id) ON DELETE CASCADE,
  category_id UUID NOT NULL REFERENCES categories(id) ON DELETE CASCADE,
  amount      NUMERIC(12,2) NOT NULL DEFAULT 0,
  percentage  NUMERIC(5,2),
  created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  UNIQUE(period_id, category_id)
);

-- Transacciones
CREATE TABLE transactions (
  id                UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  period_id         UUID NOT NULL REFERENCES periods(id) ON DELETE CASCADE,
  category_id       UUID NOT NULL REFERENCES categories(id),
  payment_method_id UUID NOT NULL REFERENCES payment_methods(id),
  card_id           UUID REFERENCES cards(id),
  type              TEXT NOT NULL CHECK (type IN ('income', 'expense')),
  amount            NUMERIC(12,2) NOT NULL,
  description       TEXT NOT NULL,
  note              TEXT,
  date              DATE NOT NULL DEFAULT CURRENT_DATE,
  created_at        TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at        TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Metas (ahorro e inversión)
CREATE TABLE goals (
  id             UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  name           TEXT NOT NULL,
  type           TEXT NOT NULL CHECK (type IN ('savings', 'investment')),
  target_amount  NUMERIC(12,2) NOT NULL,
  current_amount NUMERIC(12,2) NOT NULL DEFAULT 0,
  deadline       DATE,
  is_active      BOOLEAN NOT NULL DEFAULT TRUE,
  created_at     TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at     TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Próximos pagos
CREATE TABLE upcoming_payments (
  id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  period_id           UUID REFERENCES periods(id) ON DELETE SET NULL,
  category_id         UUID NOT NULL REFERENCES categories(id),
  name                TEXT NOT NULL,
  amount              NUMERIC(12,2) NOT NULL,
  due_date            DATE NOT NULL,
  status              TEXT NOT NULL DEFAULT 'pending' CHECK (status IN ('pending', 'paid', 'overdue')),
  is_recurring        BOOLEAN NOT NULL DEFAULT FALSE,
  recurrence_interval TEXT CHECK (recurrence_interval IN ('weekly', 'biweekly', 'monthly')),
  created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Configuración de la app
CREATE TABLE app_settings (
  key        TEXT PRIMARY KEY,
  value      TEXT NOT NULL,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Índices para performance
CREATE INDEX idx_transactions_period   ON transactions(period_id);
CREATE INDEX idx_transactions_category ON transactions(category_id);
CREATE INDEX idx_transactions_date     ON transactions(date DESC);
CREATE INDEX idx_budgets_period        ON budgets(period_id);
CREATE INDEX idx_upcoming_due          ON upcoming_payments(due_date);
