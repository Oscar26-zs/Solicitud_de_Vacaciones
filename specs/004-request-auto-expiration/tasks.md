# Tareas de Implementacion - Feature 004: Auto-Expiration de Solicitudes

**Input**: `specs/004-request-auto-expiration/spec.md`, `specs/004-request-auto-expiration/plan.md`, `docs/use-cases.md`, `.specify/memory/constitution.md`
**Prerequisitos**: Feature 000 (fundacion) y Feature 002 (CRUD de solicitudes).
**Tests**: obligatorios por la constitucion seccion 9.
**Fecha**: 2026-08-03
**Version**: 1.0

---

## Formato de tarea

Cada tarea usa el siguiente bloque:

`## [TASK-XXX] Titulo`

- **Prioridad:** Alta | Media | Baja
- **Estado:** [ ] Pendiente
- **Paralela:** Si | No
- **HU:** — (transversal / sin historia directa)
- **Fase:** 1-2
- **Dependencias:** TASK-XXX o Feature 000
- **Capa:** Infrastructure | Application | Tests
- **Archivos a crear:** rutas exactas
- **Trazabilidad:** plan.md / spec.md / CU-XX / RN-XX / RF-XX
- **Descripcion:** que se implementa y por que
- **Criterios de aceptacion:** lista verificable

> **Regla de orden:** dentro de cada fase las tareas se listan en orden de ejecucion. Si B depende de A, A aparece antes que B y B no se inicia hasta completar A. Las tareas `Paralela: Si` pueden ejecutarse en paralelo. El Feature 002 es prerrequisito global de este feature.

---

## Resumen de Fases

| Fase | Descripcion | Tareas |
|------|-------------|:------:|
| 1 | Metodo Expirar en dominio y servicio de background | 2 |
| 2 | Tests de expiracion (integrados) | 1 |
| **Total** | | **3** |

---

# Phase 1: Expiracion Automatica (Service + Domain)

**Proposito:** Implementar el metodo `Expirar()` en la entidad y el `ServicioExpiracionAutomatica` como hosted service.

**Checkpoint:** El servicio expira solicitudes Pending cuya fecha de inicio ya fue alcanzada, libera pendingBalance y registra en historial.

- [ ] T001 Crear ServicioExpiracionAutomatica
  - Prioridad: Alta | Capa: Infrastructure | Fase: 1
  - `src\Vacations.Infrastructure\BackgroundServices\ServicioExpiracionAutomatica.cs`
  - [ ] Hereda de `BackgroundService`
  - [ ] Ejecuta periodicamente (configurable, default cada hora)
  - [ ] Usa `TimeProvider` para obtener fecha actual
  - [ ] Busca solicitudes Pending con FechaInicio <= hoy
  - [ ] Cambia estado a Expired (metodo Expirar del dominio)
  - [ ] Libera pendingBalance del empleado
  - [ ] Registra en HistorialSolicitud con actor SISTEMA_AUTO_EXPIRACION
  - [ ] Maneja errores sin detener el servicio (logging)
  - Dependencias: Feature 000 (repositorios, IUnitOfWork, ITimeProvider, dominio SolicitudVacaciones con metodo Expirar), Feature 002 (CRUD)
  - Descripcion: BackgroundService que expira solicitudes Pending cuya fecha de inicio ya paso. Se registra en Program.cs con `AddHostedService`.
  - Trazabilidad: CU-15, RN-26, `plan.md` seccion 10
# Phase 2: Tests de Expiracion

**Proposito:** Cerrar cobertura con pruebas deterministas usando TimeProvider de fechas fijas.

**Checkpoint:** `dotnet test` verde y comportamiento de expiracion verificado.

- [ ] T002 Tests de expiracion automatica
  - Prioridad: Media | Capa: Tests | Fase: 2
  - `tests\Vacations.Infrastructure.Tests\BackgroundServices\ServicioExpiracionAutomaticaTests.cs`
  - [ ] Test: solicitud con FechaInicio <= hoy -> expira
  - [ ] Test: solicitud con FechaInicio mayor a hoy -> no expira
  - [ ] Test: libera pendingBalance al expirar
  - [ ] Test: registra historial con actor SISTEMA_AUTO_EXPIRACION
  - [ ] Test: no detiene el servicio ante errores
  - [ ] Mock de TimeProvider con fecha fija
  - Dependencias: TASK-059, Feature 000 (proyecto Vacations.Infrastructure.Tests)
  - Descripcion: Pruebas con TimeProvider determinista para el servicio de expiracion.
  - Trazabilidad: CU-15, RN-26, `constitution.md` seccion 9