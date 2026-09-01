-- =====================================================
-- Presupuesto Personal — Seed con datos reales 2026 (HISTÓRICO)
-- =====================================================
-- OBSOLETO desde 2026-09-01: este archivo ya no refleja el estado de la
-- base de datos. Se conserva solo como referencia histórica (eran datos
-- reales del usuario hasta junio 2026). Para regenerar datos de prueba
-- actualizados, usa `node database/seed-test-data.js`, que trunca todo lo
-- transaccional y siembra las 24 quincenas del año en curso con montos
-- sintéticos y rollover calculado de forma realista.

-- Limpiar datos existentes (orden: dependencias primero)
TRUNCATE upcoming_payments, budgets, transactions, goals, periods,
         cards, payment_methods, categories, app_settings CASCADE;

-- ── Categorías ─────────────────────────────────────
INSERT INTO categories (id, name, color, sort_order) VALUES
  ('87dac2ad-a261-4a9d-b383-22687fb5db95', 'Alimentación',    '#22c55e', 1),
  ('89591f83-92bb-4baa-90c8-946231563bac', 'Transporte',      '#3b82f6', 2),
  ('c8ab75eb-7a5c-4a71-a0e0-a4b9deca4f44', 'Tecnología',      '#8b5cf6', 3),
  ('cee93589-6042-4c20-92f4-fdf67f26283a', 'Personal & Ocio', '#f97316', 4),
  ('1870a76f-ab0a-49cc-a8ac-2431f308c097', 'Ahorro',          '#059669', 5),
  ('41dd69da-222a-461a-bf2d-c48672d38c8e', 'Inversión',       '#0d9488', 6),
  ('1218391f-ea1d-460e-b67c-f96cf78c7d91', 'Educación',       '#eab308', 7),
  ('9cba8f3b-0294-4072-b51a-37fe075c96f8', 'Deudas & Pagos',  '#dc2626', 8);

-- ── Métodos de pago ─────────────────────────────────
INSERT INTO payment_methods (id, name, requires_card) VALUES
  ('f8426d67-87b0-4825-9276-9b1aee838518', 'Efectivo',               FALSE),
  ('b41bace1-129b-457a-ac3e-cf4e25e8ad95', 'Tarjeta Crédito',        TRUE),
  ('de5b96c9-c135-4fdd-9d60-930fc1d4afaf', 'Nequi / Transferencia',  FALSE),
  ('c1dce5a8-e91b-4c0d-8460-071e4c8dda55', 'Débito',                 FALSE);

-- ── Tarjetas ────────────────────────────────────────
INSERT INTO cards (id, name, bank) VALUES
  ('50627786-7bec-4c1e-985e-f378d5dcd9d4', 'TC Personal', 'Bancolombia');

-- ── Períodos 2026 H1 ────────────────────────────────
-- base_income = porción "Samuel" del salario; rollover = saldo final del período anterior
INSERT INTO periods (id, label, start_date, end_date, base_income, rollover, is_closed) VALUES
  ('b655487d-ec10-4043-876b-f8681bdcf38c', '1ra Quincena Enero',   '2026-01-01','2026-01-15',  223065.00,       0.00, TRUE),
  ('44f268e2-60fd-4f8f-852d-3c0a32cb3640', '2da Quincena Enero',   '2026-01-16','2026-01-31',  306629.00,  -86256.00, TRUE),
  ('0a25b008-14a9-4444-9400-d36cd48ec48a', '1ra Quincena Febrero', '2026-02-01','2026-02-15',  336372.00,   21306.00, TRUE),
  ('50e82d9f-0d9a-44d7-b7e4-7285138fc107', '2da Quincena Febrero', '2026-02-16','2026-02-28',  459900.00, -298051.00, TRUE),
  ('7e2672c7-1e1d-49b8-9389-26b68d7bbe50', '1ra Quincena Marzo',   '2026-03-01','2026-03-15',  613200.00,   -8200.00, TRUE),
  ('d046cfdd-7977-4136-8d39-92190b76b818', '2da Quincena Marzo',   '2026-03-16','2026-03-31', 1022000.00, -298463.00, TRUE),
  ('292d2bf6-89c9-475f-9d24-a18bcd4889ce', '1ra Quincena Abril',   '2026-04-01','2026-04-15', 1022097.00,       0.00, TRUE),
  ('920aa057-82e3-49fc-b027-1e6f29afb8f2', '2da Quincena Abril',   '2026-04-16','2026-04-30',  347480.00,       0.00, FALSE),
  ('e21d70e9-0fe4-42cc-9c8b-0f657685569b', '1ra Quincena Mayo',    '2026-05-01','2026-05-15', 1022000.00,       0.00, FALSE),
  ('7fba2f67-0b29-466e-934b-c96fab42a893', '2da Quincena Mayo',    '2026-05-16','2026-05-31', 1022000.00,       0.00, FALSE),
  ('1962ccf7-d96d-4b2e-8860-9a8c0f7ac3c1', '1ra Quincena Junio',   '2026-06-01','2026-06-15', 1022000.00,       0.00, FALSE),
  ('9c6abb53-3259-4564-9d6d-0b321f4bb6cf', '2da Quincena Junio',   '2026-06-16','2026-06-30', 1022000.00,       0.00, FALSE);

-- ── Presupuestos (basados en distribución histórica del gasto) ──

-- 1ra Quincena Enero (disponible ~223k)
INSERT INTO budgets (period_id, category_id, amount, percentage) VALUES
  ('b655487d-ec10-4043-876b-f8681bdcf38c','87dac2ad-a261-4a9d-b383-22687fb5db95',  70000, 31.40),
  ('b655487d-ec10-4043-876b-f8681bdcf38c','89591f83-92bb-4baa-90c8-946231563bac',  30000, 13.45),
  ('b655487d-ec10-4043-876b-f8681bdcf38c','c8ab75eb-7a5c-4a71-a0e0-a4b9deca4f44',  30000, 13.45),
  ('b655487d-ec10-4043-876b-f8681bdcf38c','cee93589-6042-4c20-92f4-fdf67f26283a',  53065, 23.79),
  ('b655487d-ec10-4043-876b-f8681bdcf38c','1218391f-ea1d-460e-b67c-f96cf78c7d91',  40000, 17.93);

-- 2da Quincena Enero (disponible ~220k)
INSERT INTO budgets (period_id, category_id, amount, percentage) VALUES
  ('44f268e2-60fd-4f8f-852d-3c0a32cb3640','87dac2ad-a261-4a9d-b383-22687fb5db95',  60000, 27.25),
  ('44f268e2-60fd-4f8f-852d-3c0a32cb3640','89591f83-92bb-4baa-90c8-946231563bac',  80000, 36.35),
  ('44f268e2-60fd-4f8f-852d-3c0a32cb3640','c8ab75eb-7a5c-4a71-a0e0-a4b9deca4f44',   9000,  4.09),
  ('44f268e2-60fd-4f8f-852d-3c0a32cb3640','cee93589-6042-4c20-92f4-fdf67f26283a',  71373, 32.43);

-- 1ra Quincena Febrero (disponible ~357k)
INSERT INTO budgets (period_id, category_id, amount, percentage) VALUES
  ('0a25b008-14a9-4444-9400-d36cd48ec48a','87dac2ad-a261-4a9d-b383-22687fb5db95',  50000, 13.99),
  ('0a25b008-14a9-4444-9400-d36cd48ec48a','89591f83-92bb-4baa-90c8-946231563bac',  80000, 22.39),
  ('0a25b008-14a9-4444-9400-d36cd48ec48a','c8ab75eb-7a5c-4a71-a0e0-a4b9deca4f44',  82000, 22.96),
  ('0a25b008-14a9-4444-9400-d36cd48ec48a','cee93589-6042-4c20-92f4-fdf67f26283a', 145678, 40.77);

-- 2da Quincena Febrero (disponible ~161k)
INSERT INTO budgets (period_id, category_id, amount, percentage) VALUES
  ('50e82d9f-0d9a-44d7-b7e4-7285138fc107','87dac2ad-a261-4a9d-b383-22687fb5db95',  60000, 37.07),
  ('50e82d9f-0d9a-44d7-b7e4-7285138fc107','c8ab75eb-7a5c-4a71-a0e0-a4b9deca4f44',   9000,  5.56),
  ('50e82d9f-0d9a-44d7-b7e4-7285138fc107','cee93589-6042-4c20-92f4-fdf67f26283a',  92849, 57.37);

-- 1ra Quincena Marzo (disponible ~605k)
INSERT INTO budgets (period_id, category_id, amount, percentage) VALUES
  ('7e2672c7-1e1d-49b8-9389-26b68d7bbe50','87dac2ad-a261-4a9d-b383-22687fb5db95',  70000, 11.57),
  ('7e2672c7-1e1d-49b8-9389-26b68d7bbe50','89591f83-92bb-4baa-90c8-946231563bac', 140000, 23.14),
  ('7e2672c7-1e1d-49b8-9389-26b68d7bbe50','c8ab75eb-7a5c-4a71-a0e0-a4b9deca4f44',  95000, 15.70),
  ('7e2672c7-1e1d-49b8-9389-26b68d7bbe50','cee93589-6042-4c20-92f4-fdf67f26283a', 150000, 24.79),
  ('7e2672c7-1e1d-49b8-9389-26b68d7bbe50','9cba8f3b-0294-4072-b51a-37fe075c96f8', 150000, 24.79);

-- 2da Quincena Marzo (disponible ~723k)
INSERT INTO budgets (period_id, category_id, amount, percentage) VALUES
  ('d046cfdd-7977-4136-8d39-92190b76b818','87dac2ad-a261-4a9d-b383-22687fb5db95', 100000, 13.83),
  ('d046cfdd-7977-4136-8d39-92190b76b818','89591f83-92bb-4baa-90c8-946231563bac', 150000, 20.75),
  ('d046cfdd-7977-4136-8d39-92190b76b818','c8ab75eb-7a5c-4a71-a0e0-a4b9deca4f44',  80000, 11.06),
  ('d046cfdd-7977-4136-8d39-92190b76b818','cee93589-6042-4c20-92f4-fdf67f26283a', 200000, 27.66),
  ('d046cfdd-7977-4136-8d39-92190b76b818','1870a76f-ab0a-49cc-a8ac-2431f308c097', 193537, 26.77);

-- 1ra Quincena Abril (base 1,022,097 — 100% Samuel)
INSERT INTO budgets (period_id, category_id, amount, percentage) VALUES
  ('292d2bf6-89c9-475f-9d24-a18bcd4889ce','87dac2ad-a261-4a9d-b383-22687fb5db95', 150000, 14.67),
  ('292d2bf6-89c9-475f-9d24-a18bcd4889ce','89591f83-92bb-4baa-90c8-946231563bac', 200000, 19.57),
  ('292d2bf6-89c9-475f-9d24-a18bcd4889ce','c8ab75eb-7a5c-4a71-a0e0-a4b9deca4f44', 100000,  9.78),
  ('292d2bf6-89c9-475f-9d24-a18bcd4889ce','cee93589-6042-4c20-92f4-fdf67f26283a', 300000, 29.35),
  ('292d2bf6-89c9-475f-9d24-a18bcd4889ce','1870a76f-ab0a-49cc-a8ac-2431f308c097', 272097, 26.62);

-- 2da Quincena Abril (base 347,480 — 34% Samuel)
INSERT INTO budgets (period_id, category_id, amount, percentage) VALUES
  ('920aa057-82e3-49fc-b027-1e6f29afb8f2','87dac2ad-a261-4a9d-b383-22687fb5db95',  80000, 23.03),
  ('920aa057-82e3-49fc-b027-1e6f29afb8f2','89591f83-92bb-4baa-90c8-946231563bac',  80000, 23.03),
  ('920aa057-82e3-49fc-b027-1e6f29afb8f2','c8ab75eb-7a5c-4a71-a0e0-a4b9deca4f44',  40000, 11.51),
  ('920aa057-82e3-49fc-b027-1e6f29afb8f2','cee93589-6042-4c20-92f4-fdf67f26283a', 147480, 42.44);

-- ── Transacciones — 1ra Quincena Enero ─────────────
INSERT INTO transactions (period_id, category_id, payment_method_id, type, amount, description, date) VALUES
  ('b655487d-ec10-4043-876b-f8681bdcf38c','1870a76f-ab0a-49cc-a8ac-2431f308c097','de5b96c9-c135-4fdd-9d60-930fc1d4afaf','income',  74355.10,'Nu — ahorro Ene-15 (10%)',              '2026-01-15'),
  ('b655487d-ec10-4043-876b-f8681bdcf38c','41dd69da-222a-461a-bf2d-c48672d38c8e','de5b96c9-c135-4fdd-9d60-930fc1d4afaf','income', 297420.00,'Acc. Moderado — inversión Ene-15 (40%)','2026-01-15'),
  ('b655487d-ec10-4043-876b-f8681bdcf38c','41dd69da-222a-461a-bf2d-c48672d38c8e','de5b96c9-c135-4fdd-9d60-930fc1d4afaf','income', 148710.00,'Acc. Riesgo — inversión Ene-15 (20%)',  '2026-01-15'),
  ('b655487d-ec10-4043-876b-f8681bdcf38c','87dac2ad-a261-4a9d-b383-22687fb5db95','f8426d67-87b0-4825-9276-9b1aee838518','expense',  11500,'Papas MTRJ',                            '2026-01-03'),
  ('b655487d-ec10-4043-876b-f8681bdcf38c','87dac2ad-a261-4a9d-b383-22687fb5db95','f8426d67-87b0-4825-9276-9b1aee838518','expense',   4500,'Papas Queso',                           '2026-01-04'),
  ('b655487d-ec10-4043-876b-f8681bdcf38c','cee93589-6042-4c20-92f4-fdf67f26283a','f8426d67-87b0-4825-9276-9b1aee838518','expense',  33600,'ChapStick',                             '2026-01-05'),
  ('b655487d-ec10-4043-876b-f8681bdcf38c','87dac2ad-a261-4a9d-b383-22687fb5db95','f8426d67-87b0-4825-9276-9b1aee838518','expense',   9000,'Buñuelos',                              '2026-01-06'),
  ('b655487d-ec10-4043-876b-f8681bdcf38c','9cba8f3b-0294-4072-b51a-37fe075c96f8','b41bace1-129b-457a-ac3e-cf4e25e8ad95','expense',  22454,'Pago TC',                               '2026-01-07'),
  ('b655487d-ec10-4043-876b-f8681bdcf38c','cee93589-6042-4c20-92f4-fdf67f26283a','f8426d67-87b0-4825-9276-9b1aee838518','expense',  18790,'Pigasus',                               '2026-01-08'),
  ('b655487d-ec10-4043-876b-f8681bdcf38c','cee93589-6042-4c20-92f4-fdf67f26283a','de5b96c9-c135-4fdd-9d60-930fc1d4afaf','expense',  41414,'Salida Paola',                          '2026-01-10'),
  ('b655487d-ec10-4043-876b-f8681bdcf38c','87dac2ad-a261-4a9d-b383-22687fb5db95','f8426d67-87b0-4825-9276-9b1aee838518','expense',  33320,'Comida',                                '2026-01-11'),
  ('b655487d-ec10-4043-876b-f8681bdcf38c','c8ab75eb-7a5c-4a71-a0e0-a4b9deca4f44','b41bace1-129b-457a-ac3e-cf4e25e8ad95','expense',   3900,'iCloud',                                '2026-01-12'),
  ('b655487d-ec10-4043-876b-f8681bdcf38c','1218391f-ea1d-460e-b67c-f96cf78c7d91','de5b96c9-c135-4fdd-9d60-930fc1d4afaf','expense',  51400,'Certificado Uni',                       '2026-01-13'),
  ('b655487d-ec10-4043-876b-f8681bdcf38c','c8ab75eb-7a5c-4a71-a0e0-a4b9deca4f44','c1dce5a8-e91b-4c0d-8460-071e4c8dda55','expense',  25193,'Datos móvil',                           '2026-01-13'),
  ('b655487d-ec10-4043-876b-f8681bdcf38c','87dac2ad-a261-4a9d-b383-22687fb5db95','f8426d67-87b0-4825-9276-9b1aee838518','expense',   5000,'Papas',                                 '2026-01-14'),
  ('b655487d-ec10-4043-876b-f8681bdcf38c','89591f83-92bb-4baa-90c8-946231563bac','de5b96c9-c135-4fdd-9d60-930fc1d4afaf','expense',   5000,'Moto Paola',                            '2026-01-14'),
  ('b655487d-ec10-4043-876b-f8681bdcf38c','cee93589-6042-4c20-92f4-fdf67f26283a','f8426d67-87b0-4825-9276-9b1aee838518','expense',  40000,'Ancheta Mamá',                          '2026-01-15'),
  ('b655487d-ec10-4043-876b-f8681bdcf38c','87dac2ad-a261-4a9d-b383-22687fb5db95','f8426d67-87b0-4825-9276-9b1aee838518','expense',   4250,'Helado',                                '2026-01-15');

-- ── Transacciones — 2da Quincena Enero ─────────────
INSERT INTO transactions (period_id, category_id, payment_method_id, type, amount, description, date) VALUES
  ('44f268e2-60fd-4f8f-852d-3c0a32cb3640','1870a76f-ab0a-49cc-a8ac-2431f308c097','de5b96c9-c135-4fdd-9d60-930fc1d4afaf','income', 102209.60,'Nu — ahorro Ene-30 (10%)',              '2026-01-30'),
  ('44f268e2-60fd-4f8f-852d-3c0a32cb3640','41dd69da-222a-461a-bf2d-c48672d38c8e','de5b96c9-c135-4fdd-9d60-930fc1d4afaf','income', 408838.40,'Acc. Moderado — inversión Ene-30 (40%)','2026-01-30'),
  ('44f268e2-60fd-4f8f-852d-3c0a32cb3640','41dd69da-222a-461a-bf2d-c48672d38c8e','de5b96c9-c135-4fdd-9d60-930fc1d4afaf','income', 204419.20,'Acc. Riesgo — inversión Ene-30 (20%)',  '2026-01-30'),
  ('44f268e2-60fd-4f8f-852d-3c0a32cb3640','cee93589-6042-4c20-92f4-fdf67f26283a','de5b96c9-c135-4fdd-9d60-930fc1d4afaf','expense',  40000,'Flores Paola',                          '2026-01-17'),
  ('44f268e2-60fd-4f8f-852d-3c0a32cb3640','cee93589-6042-4c20-92f4-fdf67f26283a','f8426d67-87b0-4825-9276-9b1aee838518','expense',  29400,'Libro Paola',                           '2026-01-17'),
  ('44f268e2-60fd-4f8f-852d-3c0a32cb3640','87dac2ad-a261-4a9d-b383-22687fb5db95','f8426d67-87b0-4825-9276-9b1aee838518','expense',   6950,'Queso',                                 '2026-01-18'),
  ('44f268e2-60fd-4f8f-852d-3c0a32cb3640','87dac2ad-a261-4a9d-b383-22687fb5db95','f8426d67-87b0-4825-9276-9b1aee838518','expense',   7200,'Helado',                                '2026-01-19'),
  ('44f268e2-60fd-4f8f-852d-3c0a32cb3640','cee93589-6042-4c20-92f4-fdf67f26283a','f8426d67-87b0-4825-9276-9b1aee838518','expense',   3000,'Salida mamá',                           '2026-01-20'),
  ('44f268e2-60fd-4f8f-852d-3c0a32cb3640','87dac2ad-a261-4a9d-b383-22687fb5db95','f8426d67-87b0-4825-9276-9b1aee838518','expense',   1800,'Galletas',                              '2026-01-21'),
  ('44f268e2-60fd-4f8f-852d-3c0a32cb3640','87dac2ad-a261-4a9d-b383-22687fb5db95','f8426d67-87b0-4825-9276-9b1aee838518','expense',  11000,'Pimienta Roja',                         '2026-01-22'),
  ('44f268e2-60fd-4f8f-852d-3c0a32cb3640','89591f83-92bb-4baa-90c8-946231563bac','de5b96c9-c135-4fdd-9d60-930fc1d4afaf','expense',  58743,'Moto (02-06 Feb)',                       '2026-01-23'),
  ('44f268e2-60fd-4f8f-852d-3c0a32cb3640','87dac2ad-a261-4a9d-b383-22687fb5db95','f8426d67-87b0-4825-9276-9b1aee838518','expense',   4000,'Papas',                                 '2026-01-24'),
  ('44f268e2-60fd-4f8f-852d-3c0a32cb3640','89591f83-92bb-4baa-90c8-946231563bac','f8426d67-87b0-4825-9276-9b1aee838518','expense',  12274,'Pasajes taller',                        '2026-01-25'),
  ('44f268e2-60fd-4f8f-852d-3c0a32cb3640','cee93589-6042-4c20-92f4-fdf67f26283a','f8426d67-87b0-4825-9276-9b1aee838518','expense',   6000,'Galletas Paola',                        '2026-01-26'),
  ('44f268e2-60fd-4f8f-852d-3c0a32cb3640','87dac2ad-a261-4a9d-b383-22687fb5db95','f8426d67-87b0-4825-9276-9b1aee838518','expense',   6000,'Pastel de pollo',                       '2026-01-27'),
  ('44f268e2-60fd-4f8f-852d-3c0a32cb3640','c8ab75eb-7a5c-4a71-a0e0-a4b9deca4f44','b41bace1-129b-457a-ac3e-cf4e25e8ad95','expense',   8500,'Apple Music',                           '2026-01-28'),
  ('44f268e2-60fd-4f8f-852d-3c0a32cb3640','cee93589-6042-4c20-92f4-fdf67f26283a','f8426d67-87b0-4825-9276-9b1aee838518','expense',   4200,'Regalo',                                '2026-01-30');

-- ── Transacciones — 1ra Quincena Febrero ────────────
INSERT INTO transactions (period_id, category_id, payment_method_id, type, amount, description, date) VALUES
  ('0a25b008-14a9-4444-9400-d36cd48ec48a','1870a76f-ab0a-49cc-a8ac-2431f308c097','de5b96c9-c135-4fdd-9d60-930fc1d4afaf','income', 102200.00,'Nu — ahorro Feb-15 (10%)',              '2026-02-15'),
  ('0a25b008-14a9-4444-9400-d36cd48ec48a','41dd69da-222a-461a-bf2d-c48672d38c8e','de5b96c9-c135-4fdd-9d60-930fc1d4afaf','income', 357700.00,'Acc. Moderado — inversión Feb-15 (35%)','2026-02-15'),
  ('0a25b008-14a9-4444-9400-d36cd48ec48a','41dd69da-222a-461a-bf2d-c48672d38c8e','de5b96c9-c135-4fdd-9d60-930fc1d4afaf','income', 204400.00,'Acc. Riesgo — inversión Feb-15 (20%)',  '2026-02-15'),
  ('0a25b008-14a9-4444-9400-d36cd48ec48a','89591f83-92bb-4baa-90c8-946231563bac','f8426d67-87b0-4825-9276-9b1aee838518','expense',  45560,'Pasajes (08-13 Feb)',                   '2026-02-01'),
  ('0a25b008-14a9-4444-9400-d36cd48ec48a','cee93589-6042-4c20-92f4-fdf67f26283a','f8426d67-87b0-4825-9276-9b1aee838518','expense',   8400,'Regalo',                                '2026-02-02'),
  ('0a25b008-14a9-4444-9400-d36cd48ec48a','87dac2ad-a261-4a9d-b383-22687fb5db95','f8426d67-87b0-4825-9276-9b1aee838518','expense',   8400,'Helados',                               '2026-02-03'),
  ('0a25b008-14a9-4444-9400-d36cd48ec48a','87dac2ad-a261-4a9d-b383-22687fb5db95','f8426d67-87b0-4825-9276-9b1aee838518','expense',   4200,'Helados (sin pagar)',                   '2026-02-04'),
  ('0a25b008-14a9-4444-9400-d36cd48ec48a','87dac2ad-a261-4a9d-b383-22687fb5db95','f8426d67-87b0-4825-9276-9b1aee838518','expense',   4000,'Platanitos',                            '2026-02-04'),
  ('0a25b008-14a9-4444-9400-d36cd48ec48a','c8ab75eb-7a5c-4a71-a0e0-a4b9deca4f44','b41bace1-129b-457a-ac3e-cf4e25e8ad95','expense',  52688,'Game Pass',                             '2026-02-05'),
  ('0a25b008-14a9-4444-9400-d36cd48ec48a','cee93589-6042-4c20-92f4-fdf67f26283a','de5b96c9-c135-4fdd-9d60-930fc1d4afaf','expense', 191420,'Salida Paola',                          '2026-02-07'),
  ('0a25b008-14a9-4444-9400-d36cd48ec48a','89591f83-92bb-4baa-90c8-946231563bac','de5b96c9-c135-4fdd-9d60-930fc1d4afaf','expense',  12000,'Cívica',                                '2026-02-08'),
  ('0a25b008-14a9-4444-9400-d36cd48ec48a','cee93589-6042-4c20-92f4-fdf67f26283a','f8426d67-87b0-4825-9276-9b1aee838518','expense',  30000,'Corte de pelo',                         '2026-02-09'),
  ('0a25b008-14a9-4444-9400-d36cd48ec48a','89591f83-92bb-4baa-90c8-946231563bac','f8426d67-87b0-4825-9276-9b1aee838518','expense',   9655,'Corte — pasaje ida',                   '2026-02-09'),
  ('0a25b008-14a9-4444-9400-d36cd48ec48a','cee93589-6042-4c20-92f4-fdf67f26283a','f8426d67-87b0-4825-9276-9b1aee838518','expense',  18300,'Farmatodo',                             '2026-02-10'),
  ('0a25b008-14a9-4444-9400-d36cd48ec48a','89591f83-92bb-4baa-90c8-946231563bac','f8426d67-87b0-4825-9276-9b1aee838518','expense',   9734,'Corte — pasaje vuelta',                '2026-02-09'),
  ('0a25b008-14a9-4444-9400-d36cd48ec48a','89591f83-92bb-4baa-90c8-946231563bac','f8426d67-87b0-4825-9276-9b1aee838518','expense',  20308,'Pasaje vuelta oficina',                '2026-02-10'),
  ('0a25b008-14a9-4444-9400-d36cd48ec48a','87dac2ad-a261-4a9d-b383-22687fb5db95','f8426d67-87b0-4825-9276-9b1aee838518','expense',   2500,'Platanitos',                            '2026-02-11'),
  ('0a25b008-14a9-4444-9400-d36cd48ec48a','c8ab75eb-7a5c-4a71-a0e0-a4b9deca4f44','c1dce5a8-e91b-4c0d-8460-071e4c8dda55','expense',  25193,'Datos móvil',                           '2026-02-12'),
  ('0a25b008-14a9-4444-9400-d36cd48ec48a','cee93589-6042-4c20-92f4-fdf67f26283a','f8426d67-87b0-4825-9276-9b1aee838518','expense',  20000,'Churro',                                '2026-02-13'),
  ('0a25b008-14a9-4444-9400-d36cd48ec48a','cee93589-6042-4c20-92f4-fdf67f26283a','b41bace1-129b-457a-ac3e-cf4e25e8ad95','expense', 150000,'Reloj',                                 '2026-02-14'),
  ('0a25b008-14a9-4444-9400-d36cd48ec48a','c8ab75eb-7a5c-4a71-a0e0-a4b9deca4f44','b41bace1-129b-457a-ac3e-cf4e25e8ad95','expense',   4000,'iCloud',                                '2026-02-14'),
  ('0a25b008-14a9-4444-9400-d36cd48ec48a','87dac2ad-a261-4a9d-b383-22687fb5db95','f8426d67-87b0-4825-9276-9b1aee838518','expense',   5000,'Papas',                                 '2026-02-14'),
  ('0a25b008-14a9-4444-9400-d36cd48ec48a','89591f83-92bb-4baa-90c8-946231563bac','de5b96c9-c135-4fdd-9d60-930fc1d4afaf','expense',   8000,'Pasaje ida (aplicación)',               '2026-02-14'),
  ('0a25b008-14a9-4444-9400-d36cd48ec48a','87dac2ad-a261-4a9d-b383-22687fb5db95','f8426d67-87b0-4825-9276-9b1aee838518','expense',   7700,'Comida',                                '2026-02-15'),
  ('0a25b008-14a9-4444-9400-d36cd48ec48a','89591f83-92bb-4baa-90c8-946231563bac','de5b96c9-c135-4fdd-9d60-930fc1d4afaf','expense',  14200,'Pasaje vuelta (aplicación)',            '2026-02-15'),
  ('0a25b008-14a9-4444-9400-d36cd48ec48a','1218391f-ea1d-460e-b67c-f96cf78c7d91','f8426d67-87b0-4825-9276-9b1aee838518','expense',   4470,'Materiales laboratorio',               '2026-02-15');

-- ── Transacciones — 2da Quincena Febrero ────────────
INSERT INTO transactions (period_id, category_id, payment_method_id, type, amount, description, date) VALUES
  ('50e82d9f-0d9a-44d7-b7e4-7285138fc107','41dd69da-222a-461a-bf2d-c48672d38c8e','de5b96c9-c135-4fdd-9d60-930fc1d4afaf','income', 378140.00,'Acc. Moderado — inversión Feb-28 (37%)','2026-02-28'),
  ('50e82d9f-0d9a-44d7-b7e4-7285138fc107','41dd69da-222a-461a-bf2d-c48672d38c8e','de5b96c9-c135-4fdd-9d60-930fc1d4afaf','income', 183960.00,'Acc. Riesgo — inversión Feb-28 (18%)',  '2026-02-28'),
  ('50e82d9f-0d9a-44d7-b7e4-7285138fc107','9cba8f3b-0294-4072-b51a-37fe075c96f8','f8426d67-87b0-4825-9276-9b1aee838518','expense',   2800,'Compra pasada',                        '2026-02-16'),
  ('50e82d9f-0d9a-44d7-b7e4-7285138fc107','9cba8f3b-0294-4072-b51a-37fe075c96f8','f8426d67-87b0-4825-9276-9b1aee838518','expense',   2300,'Compra pasada',                        '2026-02-16'),
  ('50e82d9f-0d9a-44d7-b7e4-7285138fc107','87dac2ad-a261-4a9d-b383-22687fb5db95','f8426d67-87b0-4825-9276-9b1aee838518','expense',   2800,'Café',                                  '2026-02-17'),
  ('50e82d9f-0d9a-44d7-b7e4-7285138fc107','89591f83-92bb-4baa-90c8-946231563bac','de5b96c9-c135-4fdd-9d60-930fc1d4afaf','expense',   3300,'Moto Paola',                            '2026-02-18'),
  ('50e82d9f-0d9a-44d7-b7e4-7285138fc107','87dac2ad-a261-4a9d-b383-22687fb5db95','f8426d67-87b0-4825-9276-9b1aee838518','expense',   4000,'Platanitos',                            '2026-02-19'),
  ('50e82d9f-0d9a-44d7-b7e4-7285138fc107','87dac2ad-a261-4a9d-b383-22687fb5db95','f8426d67-87b0-4825-9276-9b1aee838518','expense',   4000,'Papas',                                 '2026-02-20'),
  ('50e82d9f-0d9a-44d7-b7e4-7285138fc107','87dac2ad-a261-4a9d-b383-22687fb5db95','f8426d67-87b0-4825-9276-9b1aee838518','expense',  14400,'Helado',                                '2026-02-21'),
  ('50e82d9f-0d9a-44d7-b7e4-7285138fc107','87dac2ad-a261-4a9d-b383-22687fb5db95','f8426d67-87b0-4825-9276-9b1aee838518','expense',  21000,'Pao — Papas/domicilio',                 '2026-02-22'),
  ('50e82d9f-0d9a-44d7-b7e4-7285138fc107','87dac2ad-a261-4a9d-b383-22687fb5db95','f8426d67-87b0-4825-9276-9b1aee838518','expense',   7800,'Empanadas',                             '2026-02-23'),
  ('50e82d9f-0d9a-44d7-b7e4-7285138fc107','87dac2ad-a261-4a9d-b383-22687fb5db95','f8426d67-87b0-4825-9276-9b1aee838518','expense',   4000,'Platanitos',                            '2026-02-24'),
  ('50e82d9f-0d9a-44d7-b7e4-7285138fc107','87dac2ad-a261-4a9d-b383-22687fb5db95','f8426d67-87b0-4825-9276-9b1aee838518','expense',   8200,'Helado',                                '2026-02-25'),
  ('50e82d9f-0d9a-44d7-b7e4-7285138fc107','c8ab75eb-7a5c-4a71-a0e0-a4b9deca4f44','b41bace1-129b-457a-ac3e-cf4e25e8ad95','expense',   8500,'Apple Music',                           '2026-02-27'),
  ('50e82d9f-0d9a-44d7-b7e4-7285138fc107','cee93589-6042-4c20-92f4-fdf67f26283a','f8426d67-87b0-4825-9276-9b1aee838518','expense',   4200,'Regalo',                                '2026-02-28');

-- ── Transacciones — 1ra Quincena Marzo ─────────────
INSERT INTO transactions (period_id, category_id, payment_method_id, type, amount, description, date) VALUES
  ('7e2672c7-1e1d-49b8-9389-26b68d7bbe50','41dd69da-222a-461a-bf2d-c48672d38c8e','de5b96c9-c135-4fdd-9d60-930fc1d4afaf','income', 204400.00,'Acc. Moderado — inversión Mar-15 (20%)','2026-03-15'),
  ('7e2672c7-1e1d-49b8-9389-26b68d7bbe50','41dd69da-222a-461a-bf2d-c48672d38c8e','de5b96c9-c135-4fdd-9d60-930fc1d4afaf','income', 102200.00,'Acc. Riesgo — inversión Mar-15 (10%)',  '2026-03-15'),
  ('7e2672c7-1e1d-49b8-9389-26b68d7bbe50','9cba8f3b-0294-4072-b51a-37fe075c96f8','b41bace1-129b-457a-ac3e-cf4e25e8ad95','expense', 404000,'Pago Tarjeta Crédito',                  '2026-03-01'),
  ('7e2672c7-1e1d-49b8-9389-26b68d7bbe50','c8ab75eb-7a5c-4a71-a0e0-a4b9deca4f44','b41bace1-129b-457a-ac3e-cf4e25e8ad95','expense',  73800,'Claude (AI)',                            '2026-03-02'),
  ('7e2672c7-1e1d-49b8-9389-26b68d7bbe50','87dac2ad-a261-4a9d-b383-22687fb5db95','f8426d67-87b0-4825-9276-9b1aee838518','expense',  27000,'Comida',                                '2026-03-03'),
  ('7e2672c7-1e1d-49b8-9389-26b68d7bbe50','87dac2ad-a261-4a9d-b383-22687fb5db95','f8426d67-87b0-4825-9276-9b1aee838518','expense',   1000,'Platanitos',                            '2026-03-03'),
  ('7e2672c7-1e1d-49b8-9389-26b68d7bbe50','87dac2ad-a261-4a9d-b383-22687fb5db95','f8426d67-87b0-4825-9276-9b1aee838518','expense',   3200,'Pan',                                   '2026-03-04'),
  ('7e2672c7-1e1d-49b8-9389-26b68d7bbe50','87dac2ad-a261-4a9d-b383-22687fb5db95','f8426d67-87b0-4825-9276-9b1aee838518','expense',   2800,'Café',                                  '2026-03-04'),
  ('7e2672c7-1e1d-49b8-9389-26b68d7bbe50','87dac2ad-a261-4a9d-b383-22687fb5db95','f8426d67-87b0-4825-9276-9b1aee838518','expense',  12500,'Salchipapas',                           '2026-03-05'),
  ('7e2672c7-1e1d-49b8-9389-26b68d7bbe50','cee93589-6042-4c20-92f4-fdf67f26283a','f8426d67-87b0-4825-9276-9b1aee838518','expense',   6000,'Paola',                                 '2026-03-05'),
  ('7e2672c7-1e1d-49b8-9389-26b68d7bbe50','9cba8f3b-0294-4072-b51a-37fe075c96f8','f8426d67-87b0-4825-9276-9b1aee838518','expense',   1000,'4×1000',                               '2026-03-06'),
  ('7e2672c7-1e1d-49b8-9389-26b68d7bbe50','89591f83-92bb-4baa-90c8-946231563bac','de5b96c9-c135-4fdd-9d60-930fc1d4afaf','expense',  28120,'Aplicación transporte',                '2026-03-06'),
  ('7e2672c7-1e1d-49b8-9389-26b68d7bbe50','89591f83-92bb-4baa-90c8-946231563bac','f8426d67-87b0-4825-9276-9b1aee838518','expense',  68100,'Pasajes',                               '2026-03-07'),
  ('7e2672c7-1e1d-49b8-9389-26b68d7bbe50','89591f83-92bb-4baa-90c8-946231563bac','f8426d67-87b0-4825-9276-9b1aee838518','expense',  50400,'Pasajes',                               '2026-03-08'),
  ('7e2672c7-1e1d-49b8-9389-26b68d7bbe50','89591f83-92bb-4baa-90c8-946231563bac','de5b96c9-c135-4fdd-9d60-930fc1d4afaf','expense',  10150,'Moto oficina',                          '2026-03-09'),
  ('7e2672c7-1e1d-49b8-9389-26b68d7bbe50','87dac2ad-a261-4a9d-b383-22687fb5db95','f8426d67-87b0-4825-9276-9b1aee838518','expense',  11800,'Tostao',                                '2026-03-10'),
  ('7e2672c7-1e1d-49b8-9389-26b68d7bbe50','87dac2ad-a261-4a9d-b383-22687fb5db95','f8426d67-87b0-4825-9276-9b1aee838518','expense',   8600,'Takis',                                 '2026-03-10'),
  ('7e2672c7-1e1d-49b8-9389-26b68d7bbe50','87dac2ad-a261-4a9d-b383-22687fb5db95','f8426d67-87b0-4825-9276-9b1aee838518','expense',   4800,'Helado',                                '2026-03-11'),
  ('7e2672c7-1e1d-49b8-9389-26b68d7bbe50','c8ab75eb-7a5c-4a71-a0e0-a4b9deca4f44','c1dce5a8-e91b-4c0d-8460-071e4c8dda55','expense',  25193,'Datos móvil',                           '2026-03-12'),
  ('7e2672c7-1e1d-49b8-9389-26b68d7bbe50','c8ab75eb-7a5c-4a71-a0e0-a4b9deca4f44','b41bace1-129b-457a-ac3e-cf4e25e8ad95','expense',   4000,'iCloud',                                '2026-03-12'),
  ('7e2672c7-1e1d-49b8-9389-26b68d7bbe50','87dac2ad-a261-4a9d-b383-22687fb5db95','f8426d67-87b0-4825-9276-9b1aee838518','expense',   4000,'Platanitos',                            '2026-03-13'),
  ('7e2672c7-1e1d-49b8-9389-26b68d7bbe50','cee93589-6042-4c20-92f4-fdf67f26283a','de5b96c9-c135-4fdd-9d60-930fc1d4afaf','expense',   7000,'Salida aniversario',                    '2026-03-14'),
  ('7e2672c7-1e1d-49b8-9389-26b68d7bbe50','cee93589-6042-4c20-92f4-fdf67f26283a','b41bace1-129b-457a-ac3e-cf4e25e8ad95','expense', 150000,'iRacing (suscripción anual)',            '2026-03-15');

-- ── Transacciones — 2da Quincena Marzo (ingreso, sin gastos registrados) ─
INSERT INTO transactions (period_id, category_id, payment_method_id, type, amount, description, date) VALUES
  ('d046cfdd-7977-4136-8d39-92190b76b818','41dd69da-222a-461a-bf2d-c48672d38c8e','de5b96c9-c135-4fdd-9d60-930fc1d4afaf','income',1022000.00,'Salario Mar-30 — 100% disponible Samuel','2026-03-30');

-- ── Próximos pagos (período actual: 2da Quincena Abril) ──
INSERT INTO upcoming_payments (period_id, category_id, name, amount, due_date, status, is_recurring, recurrence_interval) VALUES
  ('920aa057-82e3-49fc-b027-1e6f29afb8f2','c8ab75eb-7a5c-4a71-a0e0-a4b9deca4f44','iCloud',        4000,'2026-04-12','paid',    TRUE,'monthly'),
  ('920aa057-82e3-49fc-b027-1e6f29afb8f2','c8ab75eb-7a5c-4a71-a0e0-a4b9deca4f44','Datos móvil',  25193,'2026-04-13','pending', TRUE,'monthly'),
  ('920aa057-82e3-49fc-b027-1e6f29afb8f2','c8ab75eb-7a5c-4a71-a0e0-a4b9deca4f44','Apple Music',   8500,'2026-04-28','pending', TRUE,'monthly'),
  ('920aa057-82e3-49fc-b027-1e6f29afb8f2','c8ab75eb-7a5c-4a71-a0e0-a4b9deca4f44','Claude (AI)',  73800,'2026-05-01','pending', TRUE,'monthly');

-- ── Configuración ──────────────────────────────────
INSERT INTO app_settings (key, value) VALUES
  ('currency',  'COP'),
  ('locale',    'es-CO'),
  ('theme',     'light'),
  ('user_name', 'Samuel');
