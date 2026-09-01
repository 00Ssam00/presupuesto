# PROMPT COMPLETO PARA CLAUDE CODE

Quiero que construyas una aplicación web completa de presupuesto personal con enfoque en quincenas, manteniendo el diseño visual del prototipo MVP adjunto como referencia principal de UI/UX.

Debes usar el prototipo visual como guía directa para:
- layout general,
- sistema visual,
- jerarquía de información,
- sidebar,
- dashboard,
- tarjetas,
- espaciados,
- componentes,
- tablas,
- formularios,
- visualización de métricas,
- look & feel general.

No quiero rediseñar desde cero. Quiero evolucionar ese MVP a una versión 2 full-stack con backend y PostgreSQL, conservando la misma dirección visual.

---

## CONTEXTO GENERAL

La app está pensada para uso individual y debe enfocarse en control financiero por quincenas.

El usuario quiere una herramienta que permita:
- registrar ingresos y gastos,
- controlar presupuesto por categoría,
- llevar metas de ahorro e inversión,
- visualizar métricas y gráficas,
- manejar rollover entre quincenas,
- importar y exportar datos,
- separar planeación futura del análisis actual.

La estética deseada está inspirada en:
- Notion,
- Apple Wallet,
- dashboards limpios y minimalistas.

Debe sentirse:
- moderna,
- minimalista,
- clara,
- rápida,
- sobria,
- orientada a datos.

No debe verse como template genérico de SaaS.

---

## STACK OBJETIVO

Construye el proyecto con esta base:

### Frontend
- React
- TypeScript
- Vite
- Componentes bien organizados
- Consumo de API REST
- Gráficas limpias
- Responsive

### Backend
- Node.js
- Express
- TypeScript
- Arquitectura modular
- Validaciones de negocio
- Endpoints REST claros

### Base de datos
- PostgreSQL
- Esquema relacional bien definido
- Migraciones o SQL inicial claro
- Seeds de ejemplo

### Infraestructura local
- Docker Compose para PostgreSQL
- Variables de entorno documentadas

---

## OBJETIVO FUNCIONAL

La aplicación debe permitir gestionar presupuesto personal por quincenas.

Regla principal:

**saldo disponible = ingreso quincenal + sobrante o déficit anterior**

Esto implica:
- cada quincena tiene ingreso base,
- cada quincena recibe rollover de la anterior,
- si sobra dinero, se suma a la siguiente,
- si hay déficit, se arrastra como negativo.

La aplicación debe mostrar esto de forma visible y entendible.

---

## TODAS LAS PREGUNTAS HECHAS Y SUS RESPUESTAS

A continuación incluyo el historial funcional consolidado. Debes tratar esto como definición oficial del producto.

### Pregunta 1 — ¿Cuál es el objetivo principal de la app?
**Respuesta:** Gestionar presupuesto personal con enfoque en quincenas para controlar ingresos, gastos, ahorro, inversión, presupuesto por categorías y análisis financiero.

### Pregunta 2 — ¿La app es para uso individual o multiusuario?
**Respuesta:** Uso individual.

### Pregunta 3 — ¿La unidad de tiempo principal será mensual, semanal o quincenal?
**Respuesta:** Quincenal.

### Pregunta 4 — ¿Qué debe tener cada quincena?
**Respuesta:**
- ingreso base,
- rollover de sobrante o déficit anterior,
- saldo disponible calculado.

### Pregunta 5 — ¿Cómo se calcula el saldo disponible?
**Respuesta:**
- ingreso quincenal + saldo anterior.

### Pregunta 6 — ¿Debe existir rollover automático?
**Respuesta:** Sí.

### Pregunta 7 — ¿Qué pasa si la quincena anterior queda negativa?
**Respuesta:** El déficit también se arrastra automáticamente.

### Pregunta 8 — ¿Qué debe mostrar el desglose del saldo?
**Respuesta:**
- ingreso,
- sobrante o déficit anterior,
- total disponible.

### Pregunta 9 — ¿Qué información debe tener una transacción?
**Respuesta:**
- monto,
- tipo,
- fecha,
- categoría,
- método de pago,
- tarjeta si aplica.

### Pregunta 10 — ¿Monto es obligatorio?
**Respuesta:** Sí.

### Pregunta 11 — ¿Tipo de transacción es obligatorio?
**Respuesta:** Sí, ingreso o gasto.

### Pregunta 12 — ¿Fecha obligatoria?
**Respuesta:** Sí. Debe venir por defecto con fecha actual, pero ser editable.

### Pregunta 13 — ¿Categoría obligatoria?
**Respuesta:** Sí.

### Pregunta 14 — ¿Método de pago obligatorio?
**Respuesta:** Sí.

### Pregunta 15 — ¿Tarjeta obligatoria siempre?
**Respuesta:** No. Solo si el método es crédito.

### Pregunta 16 — ¿Cuáles son los métodos de pago?
**Respuesta:**
- Efectivo
- Débito
- Crédito
- Transferencia

### Pregunta 17 — ¿Cómo debe ser el registro de transacciones?
**Respuesta:** Manual y rápido.

### Pregunta 18 — ¿Qué debe mostrar el dashboard?
**Respuesta:**
- saldo disponible actual,
- desglose del saldo,
- gastos totales,
- presupuesto restante,
- próximos pagos.

### Pregunta 19 — ¿Qué debe incluir cada próximo pago?
**Respuesta:**
- nombre,
- monto,
- fecha límite,
- categoría,
- estado.

### Pregunta 20 — ¿Qué gráficas debe incluir el dashboard?
**Respuesta:**
- gastos por categoría,
- presupuesto vs gasto real,
- ingresos vs egresos,
- evolución del ahorro,
- gastos por método o tarjeta,
- próximos pagos por categoría.

### Pregunta 21 — ¿Qué filtros de tiempo debe tener?
**Respuesta:**
- quincenal por defecto,
- mensual,
- semestral,
- anual.

### Pregunta 22 — ¿Cómo debe abrir la app?
**Respuesta:** En la quincena actual.

### Pregunta 23 — ¿Se deben poder ver quincenas anteriores?
**Respuesta:** Sí.

### Pregunta 24 — ¿Se deben mostrar quincenas futuras en el dashboard?
**Respuesta:** No.

### Pregunta 25 — ¿Para qué sirven las quincenas futuras?
**Respuesta:** Solo para configuración y planeación.

### Pregunta 26 — ¿Qué se puede configurar en quincenas futuras?
**Respuesta:**
- presupuesto por categoría,
- metas,
- gastos previstos,
- fechas.

### Pregunta 27 — ¿Los gastos previstos se convierten automáticamente en gastos reales?
**Respuesta:** No.

### Pregunta 28 — ¿Cómo se manejan los gastos previstos cuando llega la fecha?
**Respuesta:** Se revisan manualmente.

### Pregunta 29 — ¿Qué tipos de importación debe soportar la app?
**Respuesta:**
- CSV,
- Excel,
- manual.

### Pregunta 30 — ¿Qué se puede importar?
**Respuesta:**
- transacciones,
- porcentajes de presupuesto.

### Pregunta 31 — Ejemplo de presupuesto importado
**Respuesta:** inversión 20%.

### Pregunta 32 — ¿Cuáles son los campos mínimos para importar transacciones?
**Respuesta:**
- monto,
- tipo.

### Pregunta 33 — ¿Qué hacer si faltan datos en la importación?
**Respuesta:** pedirlos manualmente.

### Pregunta 34 — ¿Se deben aceptar transacciones incompletas?
**Respuesta:** No.

### Pregunta 35 — ¿Cómo debe ser el presupuesto?
**Respuesta:**
- por categoría,
- por porcentaje o monto,
- editable manualmente o importado.

### Pregunta 36 — ¿Qué metas debe manejar la app?
**Respuesta:**
- ahorro,
- inversión.

### Pregunta 37 — ¿Cómo visualizar metas?
**Respuesta:** con progreso visible.

### Pregunta 38 — Inspiración visual
**Respuesta:** Notion + Apple Wallet.

### Pregunta 39 — Estilo visual
**Respuesta:** minimalista, limpio, sin ruido.

### Pregunta 40 — Prioridad visual
**Respuesta:** enfocada en datos importantes.

### Pregunta 41 — Tema claro/oscuro
**Respuesta:** automático según hora local.

### Pregunta 42 — ¿Debe existir override manual del tema?
**Respuesta:** Sí.

### Pregunta 43 — Categorías iniciales propuestas
**Respuesta inicial propuesta:** ahorro, inversión, iPhone/tecnología, transporte, alimentación y gastos personales.

### Pregunta 44 — Ajuste sobre categorías
**Respuesta:**
- ahorro,
- inversión,
- iPhone puede entrar como tecnología,
- el resto están correctas.

### Pregunta 45 — Categorías finales iniciales
**Respuesta:**
- Ahorro
- Inversión
- Tecnología
- Transporte
- Alimentación
- Gastos personales

### Pregunta 46 — ¿Pantallas del MVP confirmadas?
**Respuesta:** Sí, se confirmó este conjunto como base.

### Pregunta 47 — Pantallas del MVP
**Respuesta:**
1. Dashboard
2. Transacciones
3. Presupuesto
4. Metas
5. Importar / Exportar
6. Configuración

### Pregunta 48 — ¿Frontend solo o con backend desde el inicio?
**Respuesta:** Después del MVP, continuar con backend real.

### Pregunta 49 — Decisión para versión 2
**Respuesta:** Sí, seguir con versión 2 incluyendo backend y PostgreSQL.

### Pregunta 50 — Confirmación final sobre categorías
**Respuesta textual del usuario:** “50: ahorro, inversión, iphone puede entrar como tecnología, y el resto están correctas”.

### Pregunta 51 — Confirmación de pantallas
**Respuesta implícita usada para el brief:** se mantienen las pantallas:
- Dashboard
- Transacciones
- Presupuesto
- Metas
- Importar / Exportar
- Configuración

### Pregunta 52 — Tecnología elegida
**Respuesta final:** construir versión 2 con backend y PostgreSQL.

---

## BRIEF CONSOLIDADO DEL PRODUCTO

Construye una app web de presupuesto personal con enfoque en quincenas, diseñada para uso individual.

### Funcionalidades principales
- dashboard financiero,
- CRUD de transacciones,
- presupuesto por categoría,
- metas de ahorro e inversión,
- próximos pagos,
- importación y exportación,
- configuración de categorías, tarjetas y quincenas,
- soporte a planeación futura.

### Reglas clave
- No mezclar planificación futura con análisis actual.
- El dashboard solo muestra presente e historial relevante.
- Las quincenas futuras sirven para configurar, no para registrar automáticamente movimientos reales.
- El registro manual debe ser muy rápido.
- Registrar un gasto debe tomar menos de 10 segundos.

---

## REQUERIMIENTOS DE UX/UI

Usa como referencia principal la imagen del prototipo MVP adjunta.

### Debes conservar del MVP
- sidebar izquierda vertical,
- header superior limpio,
- tarjeta principal de saldo disponible,
- tarjetas KPI,
- tarjetas con bordes suaves,
- tablas limpias,
- formularios compactos,
- dashboard con varias gráficas,
- look minimalista y sobrio,
- paleta neutra con acento teal,
- modo claro y oscuro,
- tipografía sans limpia,
- navegación rápida por secciones.

### Estilo exacto esperado
- base visual similar a Notion y Apple Wallet,
- superficies beige/neutras en light mode,
- dark mode elegante y no agresivo,
- sombras suaves,
- bordes sutiles,
- sin gradientes llamativos tipo startup genérica,
- sin estilo futurista neón,
- sin decoración innecesaria,
- priorizar claridad y orden.

### Reglas de diseño
- no centrar todo; alinear contenido para lectura rápida,
- jerarquía clara de números y labels,
- componentes reutilizables,
- responsive móvil/tablet/desktop,
- accesibilidad razonable,
- una experiencia limpia y realista.

---

## FUNCIONALIDAD DETALLADA

### 1. Dashboard
Debe mostrar:
- saldo disponible,
- ingreso,
- rollover,
- restante,
- gastos totales,
- presupuesto restante,
- ahorro acumulado,
- inversión ejecutada,
- próximos pagos.

Debe incluir gráficas para:
- gastos por categoría,
- presupuesto vs gasto,
- ingresos vs egresos,
- evolución del ahorro,
- método/tarjeta,
- pagos por categoría.

### 2. Transacciones
Debe permitir:
- listar,
- filtrar,
- crear,
- editar,
- eliminar.

Campos:
- monto,
- tipo,
- fecha,
- categoría,
- método,
- tarjeta si el método es crédito,
- nota.

Validación:
- si método = crédito, tarjeta obligatoria.

### 3. Presupuesto
- presupuesto por categoría,
- porcentaje,
- límite quincenal,
- comparación con gasto real,
- edición manual.

### 4. Metas
- metas de ahorro e inversión,
- porcentaje de progreso,
- valor actual,
- valor objetivo.

### 5. Importar / Exportar
- importar CSV,
- preparar estructura para Excel,
- exportar transacciones,
- exportar presupuesto.

### 6. Configuración
- categorías,
- tarjetas,
- métodos de pago,
- quincenas futuras,
- preferencias visuales.

---

## REQUERIMIENTOS DEL BACKEND

### API REST
Crea endpoints claros para:
- health,
- bootstrap inicial,
- dashboard summary,
- transactions CRUD,
- budgets,
- goals,
- settings base.

### PostgreSQL
Necesito tablas como mínimo para:
- users,
- periods,
- categories,
- budgets,
- payment_methods,
- cards,
- transactions,
- goals,
- upcoming_payments.

### Reglas de dominio
- periodos por quincena,
- rollover explícito por periodo,
- relación entre presupuestos y periodos,
- relación entre transacciones y categorías,
- relación entre transacciones y métodos de pago,
- soporte opcional de tarjeta,
- categorías configurables.

---

## ENTREGABLES ESPERADOS

Quiero que generes un proyecto completo listo para desarrollo en Claude Code con:

1. estructura de carpetas clara,
2. frontend funcional,
3. backend funcional,
4. SQL o migraciones,
5. docker-compose para PostgreSQL,
6. seeds,
7. README técnico,
8. variables de entorno de ejemplo,
9. componentes base reutilizables,
10. diseño fiel al MVP adjunto.

---

## INSTRUCCIONES IMPORTANTES PARA CLAUDE CODE

- Usa este prompt como fuente principal de verdad.
- Usa la imagen del MVP adjunta para mantener la continuidad visual.
- No reinventes la interfaz.
- Evoluciona el MVP a un producto full-stack real.
- Si alguna decisión no está explícita, prioriza consistencia con el MVP y con el brief consolidado.
- Si necesitas crear nuevos componentes, deben sentirse parte natural del diseño original.
- Mantén código limpio, modular y escalable.
- Usa nombres claros y estructura mantenible.

---

## REFERENCIA VISUAL ADJUNTA

Se adjunta una imagen del prototipo MVP original.
Úsala como referencia directa de diseño para replicar el estilo visual y la estructura base del producto.