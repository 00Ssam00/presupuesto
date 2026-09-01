# README — Paquete para Claude Code

Este paquete contiene el material necesario para seguir el proyecto en Claude Code con contexto completo, sin perder decisiones funcionales ni visuales.

## Archivos incluidos

- `PROMPT_COMPLETO_CLAUDE_CODE.md`
  - Prompt maestro con todo el contexto del producto.
  - Incluye el historial de preguntas y respuestas.
  - Incluye reglas funcionales, UX, UI, backend, PostgreSQL y entregables esperados.

- `README_CLAUDE_CODE.md`
  - Este archivo.

- `mvp-prototype-reference.png`
  - Imagen de referencia del prototipo MVP original.
  - Debe usarse para mantener el mismo estilo visual en la versión 2.

## Cómo usar esto en Claude Code

### Opción recomendada
1. Abre Claude Code en la carpeta del proyecto.
2. Copia el contenido de `PROMPT_COMPLETO_CLAUDE_CODE.md`.
3. Pégalo como prompt principal.
4. Adjunta también la imagen `mvp-prototype-reference.png`.
5. Pide que construya o continúe la versión full-stack respetando esa referencia visual.

### Prompt sugerido para arrancar

Puedes usar algo como:

> Usa el archivo `PROMPT_COMPLETO_CLAUDE_CODE.md` como fuente principal de verdad y toma la imagen `mvp-prototype-reference.png` como referencia visual obligatoria para mantener el diseño del MVP. Construye la versión 2 completa con React, TypeScript, Express y PostgreSQL.

## Qué contiene el prompt

El prompt ya incluye:
- objetivo del producto,
- reglas del modelo financiero,
- pantallas del MVP,
- stack técnico deseado,
- requerimientos de backend,
- estructura visual esperada,
- todas las preguntas hechas y sus respuestas consolidadas,
- entregables esperados.

## Recomendación práctica

Cuando Claude Code empiece a generar archivos:
- pídele primero la estructura del proyecto,
- luego el esquema de base de datos,
- luego backend,
- luego frontend,
- luego integración,
- luego mejoras visuales finales.

## Nota sobre la imagen

La imagen debe tomarse del prototipo MVP que ya construiste y usarse como referencia visual directa para:
- layout,
- paleta,
- componentes,
- densidad,
- jerarquía,
- estructura de dashboard.

No debe tomarse como wireframe genérico sino como referencia principal de continuidad visual.

## Estado actual de la infraestructura

- La base de datos PostgreSQL vive en **Neon** (proyecto `presupuesto`, región `aws-us-west-2`), no en Supabase ni en Docker local.
- `DATABASE_URL` en `.env` apunta al endpoint *pooler* de Neon con `sslmode=require`.
- `database/schema.sql` es la fuente de verdad del esquema; se ejecuta directamente contra Neon (vía `psql` o su consola SQL).

## Tests

- Backend (Vitest + Supertest, mockeando la conexión a la BD): `npm run test --prefix backend`
- Frontend (Vitest + React Testing Library): `npm run test --prefix frontend`
- End-to-end (Playwright, requiere backend y frontend corriendo — los levanta solo si hace falta): `npm run test:e2e` desde la raíz
- Todo junto: `npm run test:all` desde la raíz

Los tests e2e corren contra la base de datos Neon real (no hay entorno de test aislado). Crean y eliminan sus propios períodos de prueba con el prefijo `E2E` para no dejar datos residuales.