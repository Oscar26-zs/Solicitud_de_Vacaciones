# Tareas de Implementacion - Feature 003: Approval Workflow

**Input**: `specs/003-approval-workflow/spec.md`, `specs/plan.md`, `docs/use-cases.md`, `.specify/memory/constitution.md`, `docs/DESIGN_TOKENS.md`
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
- **HU:** HU-05 | HU-06 | HU-07
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
| 1 | Commands de aprobacion (aprobar, rechazar, cancelar aprobada) | 3 |
| 2 | Query de bandeja de aprobador | 1 |
| 3 | Web (ViewModels, controller, vistas) y Tests | 4 |
| **Total** | | **8** |

---

# Phase 1: Commands de Aprobacion (Application)

**Proposito:** Implementar los comandos de aprobar, rechazar y cancelar aprobada con sus reglas de negocio.

**Checkpoint:** Los tres comandos validan anti-auto-aprobacion, estado correcto, actualizan saldos y registran historial.

- [ ] T001 Crear comando AprobarSolicitudCommand + Handler
  - Prioridad: Alta | Capa: Application | Fase: 1
  - `src\Vacations.Application\Solicitudes\Commands\AprobarSolicitudCommand.cs`
  - `src\Vacations.Application\Solicitudes\Commands\AprobarSolicitudCommandHandler.cs`
  - [ ] Verifica que el aprobador no sea el autor (anti-auto-aprobacion)
  - [ ] Verifica que el aprobador este activo
  - [ ] Verifica estado Pending
  - [ ] Verifica saldo disponible actual (puede haber cambiado)
  - [ ] Mueve dias de pendingBalance a consumedBalance
  - [ ] Registra en historial con actor = email aprobador
  - [ ] Maneja concurrencia optimista
  - Dependencias: Feature 002 (CRUD), Feature 000 (dominio, repositorios, IUnitOfWork)
  - Descripcion: Comando para que un aprobador apruebe una solicitud.
  - Trazabilidad: CU-11, HU-06, RN-03, RN-08, RN-12, RN-13, RN-14
  - HU: HU-06
- [ ] T002 Crear comando RechazarSolicitudCommand + Handler
  - Prioridad: Alta | Capa: Application | Fase: 1
  - `src\Vacations.Application\Solicitudes\Commands\RechazarSolicitudCommand.cs`
  - `src\Vacations.Application\Solicitudes\Commands\RechazarSolicitudCommandHandler.cs`
  - [ ] Verifica aprobador activo y no es autor
  - [ ] Verifica estado Pending
  - [ ] Comentario obligatorio (1-500 caracteres)
  - [ ] Libera pendingBalance
  - [ ] Registra en historial con comentario
  - Dependencias: TASK-051
  - Descripcion: Comando para que un aprobador rechace una solicitud con comentario obligatorio.
  - Trazabilidad: CU-12, HU-06, RN-11
  - HU: HU-06
- [ ] T003 Crear comando CancelarAprobadaCommand + Handler
  - Prioridad: Alta | Capa: Application | Fase: 1
  - `src\Vacations.Application\Solicitudes\Commands\CancelarAprobadaCommand.cs`
  - `src\Vacations.Application\Solicitudes\Commands\CancelarAprobadaCommandHandler.cs`
  - [ ] Solo si estado es Approved
  - [ ] Solo si fecha inicio > fecha actual
  - [ ] Restaura saldo (mueve de consumedBalance a disponible)
  - [ ] Registra en historial
  - Dependencias: TASK-051
  - Descripcion: Comando para que un aprobador cancele una solicitud ya aprobada.
  - Trazabilidad: CU-14, HU-07, RN-04
  - HU: HU-07
# Phase 2: Query de Bandeja (Application)

**Proposito:** Implementar la consulta de la bandeja del aprobador con paginacion y filtros.

**Checkpoint:** Un aprobador lista solicitudes pendientes excluyendo las suyas, con saldo y traslapes.

- [ ] T004 Crear query ObtenerBandejaAprobadorQuery + Handler
  - Prioridad: Alta | Capa: Application | Fase: 2
  - `src\Vacations.Application\Solicitudes\Queries\ObtenerBandejaAprobadorQuery.cs`
  - `src\Vacations.Application\Solicitudes\Queries\ObtenerBandejaAprobadorQueryHandler.cs`
  - [ ] Excluye solicitudes del propio aprobador
  - [ ] Filtros opcionales: empleado, rango fechas, dias
  - [ ] Paginacion offset-based con page y pageSize (soporta 5, 10, 15, 25)
  - [ ] Incluye saldo disponible del empleado
  - [ ] Indica si hay traslape con otras solicitudes
  - [ ] Ordenado de mas antiguo a mas reciente
  - Dependencias: Feature 000 (repositorios), Feature 002 (DTOs compartidos)
  - Descripcion: Query para listar solicitudes Pending para aprobadores. El pageSize se recibe como parametro opcional (default: 10) y puede ser 5, 10, 15 o 25.
  - Trazabilidad: CU-10, HU-05
  - HU: HU-05
# Phase 3: Web y Tests

**Proposito:** Entregar la interfaz de aprobador (bandeja, aprobar, rechazar, cancelar aprobada) y cerrar cobertura.

**Checkpoint:** HU-05, HU-06 y HU-07 utilizables desde la web con impacto en saldo visible.

- [ ] T005 Crear ViewModels de Aprobador
  - Prioridad: Alta | Capa: Web | Fase: 3
  - `src\Vacations.Web\ViewModels\BandejaAprobadorViewModel.cs`
  - `src\Vacations.Web\ViewModels\AprobarRechazarViewModel.cs`
  - [ ] Bandeja incluye indicador de traslape
  - [ ] AprobarRechazar incluye campo para comentario
  - [ ] Incluye saldo disponible del empleado
  - Dependencias: Feature 002 (DTOs compartidos, TASK-039)
  - Descripcion: ViewModels para las vistas de aprobador.
  - Trazabilidad: CU-10, CU-11, CU-12
  - HU: HU-05, HU-06, HU-07
- [ ] T006 Crear BandejaAprobadorController
  - Prioridad: Alta | Capa: Web | Fase: 3
  - `src\Vacations.Web\Controllers\BandejaAprobadorController.cs`
  - [ ] `[Authorize(Policy = "RequiereAprobador")]`
  - [ ] GET /bandeja-aprobador -> Lista pendientes
  - [ ] GET /bandeja-aprobador/{id} -> Detalle con impacto en saldo
  - [ ] POST /bandeja-aprobador/{id}/aprobar -> Aprobar
  - [ ] POST /bandeja-aprobador/{id}/rechazar -> Rechazar con comentario
  - [ ] POST /solicitudes-vacaciones/{id}/cancelar-aprobada -> Cancelar aprobada
  - [ ] Manejo de errores con mensajes UX
  - Dependencias: TASK-055, TASK-051, TASK-052, TASK-053, TASK-054
  - Descripcion: Controller para funcionalidades de aprobador.
  - Trazabilidad: `plan.md` seccion 6, CU-10 a CU-14
  - HU: HU-05, HU-06, HU-07
- [ ] T007 Crear vistas de Bandeja Aprobador
  - Prioridad: Alta | Capa: Web | Fase: 3
  - `src\Vacations.Web\Views\BandejaAprobador\Index.cshtml`
  - `src\Vacations.Web\Views\BandejaAprobador\Detalle.cshtml`
  - [ ] Lista ordenada por antiguedad
  - [ ] Indicador visual de traslape
  - [ ] Boton aprobar deshabilitado si hay traslape con aprobada
  - [ ] Modal o form para comentario de rechazo
  - [ ] Muestra impacto en saldo antes de aprobar
  - Dependencias: TASK-056, Feature 000 (Layout base, DESIGN_TOKENS)
  - Descripcion: Vistas para la bandeja de aprobacion.
  - Trazabilidad: `DESIGN_TOKENS.md`, CU-10 a CU-14
  - HU: HU-05, HU-06, HU-07
- [ ] T008 Crear tests de aprobacion
  - Prioridad: Alta | Capa: Tests | Fase: 3
  - `tests\Vacations.Application.Tests\Solicitudes\AprobarSolicitudCommandHandlerTests.cs`
  - [ ] Test: aprobar con saldo suficiente -> exito
  - [ ] Test: anti-auto-aprobacion -> falla
  - [ ] Test: aprobador inactivo -> falla
  - [ ] Test: estado no Pending -> falla
  - [ ] Test: mueve pendingBalance a consumedBalance
  - [ ] Mock de repositorios y TimeProvider
  - Dependencias: TASK-051, TASK-052, TASK-053, Feature 000 (proyecto Vacations.Application.Tests)
  - Descripcion: Tests del handler de aprobacion.
  - Trazabilidad: `constitution.md` seccion 9, CU-11
  - HU: HU-05, HU-06, HU-07