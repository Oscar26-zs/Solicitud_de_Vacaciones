# Tareas de Implementacion - Feature 005: HR Monitoring Dashboard

**Input**: `specs/005-hr-monitoring-dashboard/spec.md`, `specs/005-hr-monitoring-dashboard/plan.md`, `docs/use-cases.md`, `.specify/memory/constitution.md`, `docs/DESIGN_TOKENS.md`
**Prerequisitos**: Feature 000 (fundacion), Feature 001 (saldo) y Feature 002 (CRUD de solicitudes).
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
- **HU:** HU-08 | HU-09
- **Fase:** 1-3
- **Dependencias:** TASK-XXX o Feature 000
- **Capa:** Application | Web | Tests
- **Archivos a crear:** rutas exactas
- **Trazabilidad:** plan.md / spec.md / CU-XX / RN-XX / RF-XX
- **Descripcion:** que se implementa y por que
- **Criterios de aceptacion:** lista verificable

> **Regla de orden:** dentro de cada fase las tareas se listan en orden de ejecucion. Si B depende de A, A aparece antes que B y B no se inicia hasta completar A. Las tareas `Paralela: Si` pueden ejecutarse en paralelo. El Feature 002 es prerrequisito global de este feature.

---

## Resumen de Fases

| Fase | Descripcion | Tareas |
|------|-------------|:------:|
| 1 | Query de historial RRHH | 1 |
| 2 | Web (ViewModels, controller, vistas) | 3 |
| 3 | Tests | 1 |
| **Total** | | **5** |

---

# Phase 1: Query de Historial (Application)

**Proposito:** Implementar la consulta de solo lectura para RRHH con filtros y paginacion.

**Checkpoint:** RRHH lista y filtra solicitudes de cualquier empleado sin opciones de accion.

- [ ] T001 Crear query ObtenerHistorialRRHHQuery + Handler
  - Prioridad: Media | Capa: Application | Fase: 1
  - `src\Vacations.Application\Solicitudes\Queries\ObtenerHistorialRRHHQuery.cs`
  - `src\Vacations.Application\Solicitudes\Queries\ObtenerHistorialRRHHQueryHandler.cs`
  - [ ] Solo accesible por rol RRHH
  - [ ] Filtros: estado, empleado, rango de fechas
  - [ ] Paginacion offset-based con page y pageSize (soporta 5, 10, 15, 25)
  - [ ] Incluye informacion del empleado
  - [ ] Usa AsNoTracking() para optimizar
  - Dependencias: Feature 000 (repositorios), Feature 002 (DTOs compartidos, TASK-039)
  - Descripcion: Query para que RRHH consulte y filtre solicitudes de cualquier empleado. El pageSize se recibe como parametro opcional (default: 10) y puede ser 5, 10, 15 o 25.
  - Trazabilidad: CU-18, HU-08, HU-09, RN-17, RN-18
  - HU: HU-08, HU-09
# Phase 2: Web (ViewModels, Controller, Vistas)

**Proposito:** Entregar la interfaz de solo lectura para RRHH sin botones de accion.

**Checkpoint:** HU-08 y HU-09 utilizables desde la web con filtros combinables y mensaje de sin registros.

- [ ] T002 Crear ViewModels de RRHH
  - Prioridad: Media | Capa: Web | Fase: 2
  - `src\Vacations.Web\ViewModels\ConsultaRRHHViewModel.cs`
  - `src\Vacations.Web\ViewModels\FiltrosRRHHViewModel.cs`
  - [ ] Filtros para estado, empleado, fechas
  - [ ] Sin botones de accion (solo lectura)
  - Dependencias: Feature 002 (DTOs compartidos, TASK-039)
  - Descripcion: ViewModels para las vistas de RRHH.
  - Trazabilidad: CU-18, RN-19, RN-22
  - HU: HU-08, HU-09
- [ ] T003 Crear RRHHController
  - Prioridad: Media | Capa: Web | Fase: 2
  - `src\Vacations.Web\Controllers\RRHHController.cs`
  - [ ] `[Authorize(Policy = "RequiereRRHH")]`
  - [ ] GET /rrhh/solicitudes -> Lista con filtros
  - [ ] GET /rrhh/saldos/{empleadoId} -> Saldo de empleado
  - [ ] Sin acciones de modificacion (solo lectura)
  - Dependencias: TASK-062, TASK-061, Feature 001 (ObtenerSaldoQuery)
  - Descripcion: Controller para consultas de RRHH.
  - Trazabilidad: `plan.md` seccion 6, CU-18, RN-19
  - HU: HU-08, HU-09
- [ ] T004 Crear vistas de RRHH
  - Prioridad: Media | Capa: Web | Fase: 2
  - `src\Vacations.Web\Views\RRHH\Solicitudes.cshtml`
  - `src\Vacations.Web\Views\RRHH\SaldoEmpleado.cshtml`
  - [ ] Filtros combinables
  - [ ] Sin botones de accion
  - [ ] Tabla con todos los campos relevantes
  - [ ] Mensaje "No se encontraron solicitudes que coincidan con los filtros aplicados" cuando no hay resultados
  - Dependencias: TASK-063, Feature 000 (Layout base, DESIGN_TOKENS)
  - Descripcion: Vistas de solo lectura para RRHH.
  - Trazabilidad: `DESIGN_TOKENS.md`, CU-18, RN-19, RN-22
  - HU: HU-08, HU-09
# Phase 3: Tests

**Proposito:** Cerrar cobertura del historial RRHH con pruebas unitarias y de integracion.

**Checkpoint:** `dotnet test` verde para las pruebas del feature.

- [ ] T005 Crear tests de ObtenerHistorialRRHHQueryHandler
  - Prioridad: Media | Capa: Tests | Fase: 3
  - `tests\Vacations.Application.Tests\Solicitudes\ObtenerHistorialRRHHQueryHandlerTests.cs`
  - [ ] Test: filtro por estado devuelve coincidencias
  - [ ] Test: filtro por empleado devuelve coincidencias
  - [ ] Test: rango de fechas aplica correctamente
  - [ ] Test: paginacion correcta (page y pageSize 5/10/15/25)
  - [ ] Test: sin coincidencias retorna lista vacia con mensaje
  - [ ] Mock de repositorios
  - Dependencias: TASK-061, Feature 000 (proyecto Vacations.Application.Tests)
  - Descripcion: Tests del handler de historial RRHH con mocks de repositorios.
  - Trazabilidad: `constitution.md` seccion 9, CU-18
  - HU: HU-08, HU-09