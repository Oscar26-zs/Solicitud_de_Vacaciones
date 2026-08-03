# Feature 4: Auto-Expiración de Solicitudes

-4C1 Carpeta: `specs/004-request-auto-expiration/`

---

## Documentación

| Archivo | Propósito |
|---------|-----------|
| [spec.md](./spec.md) | Especificación funcional del feature (requerido) |
| [plan.md](./plan.md) | Plan de trabajo del feature |
| [tasks.md](./tasks.md) | Tareas asociadas al feature |

---

## Quick Start

Para el Product Owner / Analista:
1. Leer `spec.md` para validar RN-26 y RF-043.
2. Confirmar criterios de aceptación y alcance.

Para el Desarrollador:
1. Leer `spec.md` (secciones de validaciones y transiciones).
2. Implementar job periódico que mueva PENDING -> EXPIRED según RN-26.

Para QA:
1. Preparar casos que simulen solicitudes antiguas y validar transición a EXPIRED.

---

## Estado

- Especificación: Completa
- Plan: Completa
- Tareas: Completa
- Implementación: No iniciada

---

## Dependencias

- Depende de: Feature 2 (Solicitud de Vacaciones) para estado y creación de requests.
- Es dependencia de: Ninguna en particular.

---

## Criterios de Aceptación

- [ ] RN-26 definido y documentado en `spec.md`.
- [ ] Job diario implementado que expira solicitudes según RF-043.
- [ ] Logs de auditoría generados cuando una solicitud cambia a EXPIRED.

---

**Última actualización**: 2026-07-17