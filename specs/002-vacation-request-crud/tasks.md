# Tareas de Implementación - Feature 002: Vacation Request CRUD



**Input**: `specs/002-vacation-request-crud/spec.md`, `specs/plan.md`, `docs/use-cases.md`, `.specify/memory/constitution.md`, `docs/DESIGN_TOKENS.md`

**Prerequisitos**: Feature 000 (fundacion) y Feature 001 (saldo).

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

- **HU:** HU-01 | HU-02 | HU-03

- **Fase:** 1-4

- **Dependencias:** TASK-XXX o Feature 000

- **Capa:** Application | Infrastructure | Web | Tests

- **Archivos a crear:** rutas exactas

- **Trazabilidad:** plan.md / spec.md / CU-XX / RN-XX / RF-XX

- **Descripcion:** que se implementa y por que

- **Criterios de aceptacion:** lista verificable



> **Regla de orden:** dentro de cada fase las tareas se listan en orden de ejecucion. Si B depende de A, A aparece antes que B y B no se inicia hasta completar A. Las tareas `Paralela: Si` pueden ejecutarse en paralelo. Las tareas del Feature 000 se consideran prerrequisito global.



---



## Resumen de Fases



| Fase | Descripcion | Tareas |

|------|-------------|:------:|

| 1 | DTOs y paginacion compartidos | 1 |

| 2 | Commands CRUD (crear, editar, cancelar) | 4 |

| 3 | Queries (listar, detalle) | 2 |

| 4 | Web (ViewModels, controller, vistas) y Tests | 5 |

| **Total** | | **12** |



---



# Phase 1: DTOs y Paginacion Compartidos (Application)



**Proposito:** Definir los DTOs y la estructura de paginacion compartida que usaran las queries de listar solicitudes.



**Checkpoint:** DTOs inmutables y `PagedResult<T>` disponibles para Application y Web.



- [ ] T001 Crear DTOs compartidos

  - Prioridad: Alta | Capa: Application | Fase: 1

  - `src\Vacations.Application\Common\SolicitudDto.cs`

  - `src\Vacations.Application\Common\SaldoDto.cs`

  - `src\Vacations.Application\Common\EmpleadoDto.cs`

  - `src\Vacations.Application\Common\HistorialEventoDto.cs`

  - `src\Vacations.Application\Common\PagedResult.cs`

  - [ ] Records inmutables

  - [ ] `PagedResult<T>` con Items, TotalCount, PageNumber, PageSize

  - [ ] `AvailablePageSizes` (List [5, 10, 15, 25])

  - [ ] Sin logica de negocio

  - Dependencias: Feature 000

  - Descripcion: DTOs de transferencia de datos entre capas.

  - Trazabilidad: `constitution.md` seccion 8

  - HU: HU-02

# Phase 2: Commands CRUD (Application)



**Proposito:** Implementar los comandos de crear, editar y cancelar solicitudes del empleado.



**Checkpoint:** Los tres comandos validan reglas de negocio, actualizan saldo pendiente y registran historial.



- [ ] T002 Crear comando CrearSolicitudCommand + Handler

  - Prioridad: Alta | Capa: Application | Fase: 2

  - `src\Vacations.Application\Solicitudes\Commands\CrearSolicitudCommand.cs`

  - `src\Vacations.Application\Solicitudes\Commands\CrearSolicitudCommandHandler.cs`

  - [ ] Command con: EmpleadoId, FechaInicio, FechaFin, Motivo

  - [ ] Handler valida saldo disponible (incluyendo pendingBalance)

  - [ ] Handler valida no traslape con otras solicitudes

  - [ ] Handler valida rango de fechas (usa RangoFechas)

  - [ ] Handler congela saldo pendiente

  - [ ] Handler registra en historial

  - [ ] Retorna Guid de la solicitud creada

  - [ ] Maneja DbUpdateConcurrencyException para reintentar

  - Dependencias: TASK-039, Feature 000 (dominio SolicitudVacaciones, saldo, repositorios, IUnitOfWork)

  - Descripcion: Comando para crear una nueva solicitud de vacaciones.

  - Trazabilidad: CU-04, RF-007, RN-02, RN-06, RN-07, RN-10

  - HU: HU-01

- [ ] T003 Crear validador CrearSolicitudCommandValidator

  - Prioridad: Alta | Capa: Application | Fase: 2

  - `src\Vacations.Application\Solicitudes\Commands\CrearSolicitudCommandValidator.cs`

  - [ ] Valida FechaInicio no vacia

  - [ ] Valida FechaFin no vacia

  - [ ] Valida Motivo no vacio, minimo 10 caracteres

  - [ ] NO valida reglas de negocio (saldo, traslape) - eso es del Domain

  - Dependencias: TASK-040

  - Descripcion: Validador FluentValidation para validacion de entrada (no reglas de negocio).

  - Trazabilidad: `constitution.md` seccion 3.6

  - HU: HU-01

- [ ] T004 Crear comando EditarSolicitudCommand + Handler

  - Prioridad: Alta | Capa: Application | Fase: 2

  - `src\Vacations.Application\Solicitudes\Commands\EditarSolicitudCommand.cs`

  - `src\Vacations.Application\Solicitudes\Commands\EditarSolicitudCommandHandler.cs`

  - [ ] Solo permite editar si estado es Pending

  - [ ] Puede modificar: FechaInicio, FechaFin, Motivo

  - [ ] Recalcula dias habiles si cambian fechas

  - [ ] Ajusta pendingBalance si cambian los dias

  - [ ] Registra cambios en historial con changedFields JSON

  - Dependencias: TASK-040

  - Descripcion: Comando para editar una solicitud en estado Pending.

  - Trazabilidad: CU-06, HU-03

  - HU: HU-03

- [ ] T005 Crear comando CancelarSolicitudCommand + Handler

  - Prioridad: Alta | Capa: Application | Fase: 2

  - `src\Vacations.Application\Solicitudes\Commands\CancelarSolicitudCommand.cs`

  - `src\Vacations.Application\Solicitudes\Commands\CancelarSolicitudCommandHandler.cs`

  - [ ] Verifica que el usuario sea el dueno de la solicitud

  - [ ] Solo permite cancelar si estado es Pending

  - [ ] Libera pendingBalance

  - [ ] Registra en historial

  - Dependencias: TASK-040

  - Descripcion: Comando para que un empleado cancele su solicitud Pending.

  - Trazabilidad: CU-07, HU-03

  - HU: HU-03

# Phase 3: Queries (Application)



**Proposito:** Implementar las consultas de listar mis solicitudes (paginadas) y detalle.



**Checkpoint:** Un empleado lista sus solicitudes con paginacion y ve el detalle con historial.



- [ ] T006 Crear query ObtenerMisSolicitudesQuery + Handler

  - Prioridad: Alta | Capa: Application | Fase: 3

  - `src\Vacations.Application\Solicitudes\Queries\ObtenerMisSolicitudesQuery.cs`

  - `src\Vacations.Application\Solicitudes\Queries\ObtenerMisSolicitudesQueryHandler.cs`

  - [ ] Filtro opcional por estado

  - [ ] Paginacion offset-based con page y pageSize (soporta 5, 10, 15, 25)

  - [ ] Ordenado de mas reciente a mas antiguo

  - [ ] Retorna DTO con: Id, Fechas, Dias, Estado, Motivo, ComentarioAprobador

  - Dependencias: TASK-039, Feature 000 (repositorios)

  - Descripcion: Query para que un empleado liste sus propias solicitudes. El pageSize se recibe como parametro opcional (default: 10) y puede ser 5, 10, 15 o 25.

  - Trazabilidad: CU-05, HU-02

  - HU: HU-02

- [ ] T007 Crear query ObtenerSolicitudDetalleQuery + Handler

  - Prioridad: Alta | Capa: Application | Fase: 3

  - `src\Vacations.Application\Solicitudes\Queries\ObtenerSolicitudDetalleQuery.cs`

  - `src\Vacations.Application\Solicitudes\Queries\ObtenerSolicitudDetalleQueryHandler.cs`

  - [ ] Incluye historial de eventos

  - [ ] Verifica que el usuario tenga acceso (es dueno, es aprobador, o es RRHH)

  - Dependencias: TASK-044

  - Descripcion: Query para obtener el detalle de una solicitud incluyendo historial.

  - Trazabilidad: CU-05, HU-02

  - HU: HU-02

# Phase 4: Web (ViewModels, Controller, Vistas) y Tests



**Proposito:** Entregar la interfaz de empleado para crear, listar, ver, editar y cancelar solicitudes.



**Checkpoint:** HU-01, HU-02 y HU-03 utilizables desde la web con validacion y mensajes UX.



- [ ] T008 Crear ViewModels de Solicitud

  - Prioridad: Alta | Capa: Web | Fase: 4

  - `src\Vacations.Web\ViewModels\CrearSolicitudViewModel.cs`

  - `src\Vacations.Web\ViewModels\EditarSolicitudViewModel.cs`

  - `src\Vacations.Web\ViewModels\DetalleSolicitudViewModel.cs`

  - `src\Vacations.Web\ViewModels\ListaSolicitudesViewModel.cs`

  - [ ] Solo propiedades necesarias para cada vista

  - [ ] DataAnnotations para validacion del lado del cliente

  - [ ] Propiedades para mostrar mensajes de error

  - [ ] ListaSolicitudesViewModel expone AvailablePageSizes y SelectedPageSize

  - Dependencias: TASK-039

  - Descripcion: ViewModels para las vistas de solicitudes.

  - Trazabilidad: `constitution.md` seccion 8 (overposting)

  - HU: HU-01, HU-02, HU-03

- [ ] T009 Crear SolicitudVacacionesController

  - Prioridad: Alta | Capa: Web | Fase: 4

  - `src\Vacations.Web\Controllers\SolicitudVacacionesController.cs`

  - [ ] `[Authorize(Policy = "RequiereEmpleado")]`

  - [ ] GET /solicitudes-vacaciones -> Lista mis solicitudes

  - [ ] GET /solicitudes-vacaciones/{id} -> Detalle

  - [ ] GET /solicitudes-vacaciones/crear -> Form de creacion

  - [ ] POST /solicitudes-vacaciones -> Crear

  - [ ] GET /solicitudes-vacaciones/{id}/editar -> Form de edicion

  - [ ] PUT /solicitudes-vacaciones/{id} -> Editar

  - [ ] POST /solicitudes-vacaciones/{id}/cancelar -> Cancelar

  - [ ] Manejo de errores con mensajes UX

  - Dependencias: TASK-046, TASK-040, TASK-042, TASK-043, TASK-044, TASK-045

  - Descripcion: Controller para CRUD de solicitudes del empleado.

  - Trazabilidad: `plan.md` seccion 6, CU-04 a CU-07

  - HU: HU-01, HU-02, HU-03

- [ ] T010 Crear vistas de Solicitud (Empleado)

  - Prioridad: Alta | Capa: Web | Fase: 4

  - `src\Vacations.Web\Views\SolicitudVacaciones\Index.cshtml`

  - `src\Vacations.Web\Views\SolicitudVacaciones\Detalle.cshtml`

  - `src\Vacations.Web\Views\SolicitudVacaciones\Crear.cshtml`

  - `src\Vacations.Web\Views\SolicitudVacaciones\Editar.cshtml`

  - [ ] Lista con tabla paginada

  - [ ] Badges de estado con colores (ambar=pending, esmeralda=approved, rojo=rejected, gris=cancelled/expired)

  - [ ] Formularios con validacion del lado del cliente

  - [ ] Confirmacion antes de cancelar

  - Dependencias: TASK-047, Feature 000 (Layout base, DESIGN_TOKENS)

  - Descripcion: Vistas Razor para solicitudes del empleado.

  - Trazabilidad: `DESIGN_TOKENS.md`, CU-04 a CU-07

  - HU: HU-01, HU-02, HU-03

- [ ] T011 Crear tests de CrearSolicitudCommandHandler

  - Prioridad: Alta | Capa: Tests | Fase: 4

  - `tests\Vacations.Application.Tests\Solicitudes\CrearSolicitudCommandHandlerTests.cs`

  - [ ] Test: crear con saldo suficiente -> exito

  - [ ] Test: crear con saldo insuficiente -> falla

  - [ ] Test: crear con traslape -> falla

  - [ ] Test: crear congela saldo pendiente

  - [ ] Mock de repositorios y TimeProvider

  - Dependencias: TASK-040, Feature 000 (proyecto Vacations.Application.Tests)

  - Descripcion: Tests del handler de creacion de solicitud.

  - Trazabilidad: `constitution.md` seccion 9, CU-04

  - HU: HU-01

- [ ] T012 Tests de integracion del CRUD de solicitudes

  - Prioridad: Media | Capa: Tests | Fase: 4

  - `tests\Vacations.Infrastructure.Tests\Solicitudes\CrudSolicitudesIntegracionTests.cs`

  - [ ] Crear persiste solicitud y congela pendingBalance

  - [ ] Listar retorna paginacion correcta

  - [ ] Editar solo si Pending

  - [ ] Cancelar libera pendingBalance

  - [ ] dotnet test verde

  - Dependencias: TASK-048, TASK-049, Feature 000 (proyecto Vacations.Infrastructure.Tests)

  - Descripcion: Pruebas de integracion con base en memoria: crear, listar, detalle, editar y cancelar contra el DbContext real.

  - Trazabilidad: `constitution.md` seccion 9, CU-04 a CU-07

  - HU: HU-01, HU-02, HU-03