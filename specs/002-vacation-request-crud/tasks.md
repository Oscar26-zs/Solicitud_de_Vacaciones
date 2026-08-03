# Tareas de Feature 002 — Vacation Request CRUD

**Extraído de:** `spec/tasks.md` (Tareas específicas a Feature 002)  
**Actualizado:** 2026-07-29  
**Versión:** 1.0 (MVP)  

---

## Resumen de Tareas Feature 002

| Fase | Descripción | Tareas |
|------|-------------|:------:|
| 2 | Domain: Entidades (SolicitudVacaciones, HistorialSolicitud) | TASK-011, TASK-012 |
| 3 | Infrastructure: Repositorio y Configuración | TASK-023 |
| 4 | Application: Commands y Queries (CRUD) | TASK-027, TASK-028, TASK-029, TASK-030, TASK-034, TASK-035, TASK-040 |
| 5 | Web: Controller y Vistas | TASK-045, TASK-048, TASK-054 |
| **Total Feature 002** | | **14 tareas** |

---

# Domain: Entidades de Feature 002

## TASK-011: Crear entidad SolicitudVacaciones
- **Fase:** 2
- **Dependencias:** TASK-005, TASK-007, TASK-008, TASK-009
- **Capa:** Domain
- **Archivos a crear:**
  - `src/Vacations.Domain/Entities/SolicitudVacaciones.cs`
- **Trazabilidad:** `plan.md` sección 4 (SolicitudVacaciones), `constitution.md` sección 2, CU-04 a CU-15
- **Descripción:** Entidad central que encapsula el ciclo de vida de una solicitud. Contiene la máquina de estados y validaciones de transición.
- **Criterios de aceptación:**
  - [ ] Propiedades según plan.md: `Id`, `EmpleadoId`, `FechaInicio`, `FechaFin`, `DiasRequeridos`, `Estado`, `Motivo`, `ComentarioAprobador`, `AprobadoPor`, `CreadoEn`, `ActualizadoEn`, `RowVersion`
  - [ ] Constructor privado + factory `Crear(empleadoId, rangoFechas, motivo, diasHabiles)`
  - [ ] Estado inicial siempre `Pending`
  - [ ] Método `Aprobar(aprobadorId)` con validación anti-auto-aprobación
  - [ ] Método `Rechazar(aprobadorId, comentario)` con comentario obligatorio (mín 1 char, máx 500)
  - [ ] Método `Cancelar()` solo si estado es `Pending`
  - [ ] Método `CancelarAprobada(aprobadorId, fechaActual)` solo si estado es `Approved` y fecha inicio > hoy
  - [ ] Método `Expirar()` solo si estado es `Pending`
  - [ ] Lanzar `TransicionEstadoInvalidaException` en transiciones inválidas
  - [ ] Lanzar `AutoAprobacionNoPermitidaException` si aprobadorId == empleadoId

## TASK-012: Crear entidad HistorialSolicitud
- **Fase:** 2
- **Dependencias:** TASK-005, TASK-011
- **Capa:** Domain
- **Archivos a crear:**
  - `src/Vacations.Domain/Entities/HistorialSolicitud.cs`
- **Trazabilidad:** `plan.md` sección 4 (HistorialSolicitud), CU-17, `constitution.md` sección 7 (trazabilidad)
- **Descripción:** Registro de auditoría inmutable para cada acción sobre una solicitud.
- **Criterios de aceptación:**
  - [ ] Propiedades: `Id`, `SolicitudId`, `TipoEvento`, `EstadoAnterior`, `EstadoNuevo`, `CamposModificados`, `Actor`, `Timestamp`, `Comentario`
  - [ ] Enum o constantes para `TipoEvento`: `CREATED`, `UPDATED`, `STATUS_CHANGED`, `CANCELLED`
  - [ ] Factory method `Crear(...)` 
  - [ ] Entidad inmutable (sin setters públicos)

---

# Infrastructure: Repositorios y Configuración de Feature 002

## TASK-023: Implementar RepositorioSolicitudVacaciones
- **Fase:** 3
- **Dependencias:** TASK-018, TASK-013
- **Capa:** Infrastructure
- **Archivos a crear:**
  - `src/Vacations.Infrastructure/Persistence/Repositories/RepositorioSolicitudVacaciones.cs`
- **Trazabilidad:** CU-04, CU-05, CU-06, CU-09, CU-10
- **Descripción:** Implementación del repositorio de solicitudes usando EF Core.
- **Criterios de aceptación:**
  - [ ] Implementa `IRepositorioSolicitudVacaciones`
  - [ ] `ExisteTraslapeAsync` verifica solapamiento con solicitudes Pending o Approved
  - [ ] Queries optimizadas con `AsNoTracking()` donde aplique
  - [ ] Incluye Empleado en consultas que lo requieran

---

# Application: Commands y Queries de Feature 002

## TASK-027: Crear comando CrearSolicitudCommand + Handler
- **Fase:** 4
- **Dependencias:** TASK-011, TASK-013, TASK-014
- **Capa:** Application
- **Archivos a crear:**
  - `src/Vacations.Application/Solicitudes/Commands/CrearSolicitudCommand.cs`
  - `src/Vacations.Application/Solicitudes/Commands/CrearSolicitudCommandHandler.cs`
- **Trazabilidad:** CU-04, RF-007, RN-02, RN-06, RN-07, RN-10
- **Descripción:** Comando para crear una nueva solicitud de vacaciones.
- **Criterios de aceptación:**
  - [ ] Command con: `EmpleadoId`, `FechaInicio`, `FechaFin`, `Motivo`
  - [ ] Handler valida saldo disponible (incluyendo pendingBalance)
  - [ ] Handler valida no traslape con otras solicitudes
  - [ ] Handler valida rango de fechas (usa `RangoFechas`)
  - [ ] Handler congela saldo pendiente
  - [ ] Handler registra en historial
  - [ ] Retorna `Guid` de la solicitud creada
  - [ ] Maneja `DbUpdateConcurrencyException` para reintentar

## TASK-028: Crear validador CrearSolicitudCommandValidator
- **Fase:** 4
- **Dependencias:** TASK-027
- **Capa:** Application
- **Archivos a crear:**
  - `src/Vacations.Application/Solicitudes/Commands/CrearSolicitudCommandValidator.cs`
- **Trazabilidad:** `constitution.md` sección 3.6 (validación de entrada)
- **Descripción:** Validador FluentValidation para validación de entrada (no reglas de negocio).
- **Criterios de aceptación:**
  - [ ] Valida `FechaInicio` no vacía
  - [ ] Valida `FechaFin` no vacía
  - [ ] Valida `Motivo` no vacío, mínimo 10 caracteres
  - [ ] NO valida reglas de negocio (saldo, traslape) — eso es del Domain

## TASK-029: Crear comando EditarSolicitudCommand + Handler
- **Fase:** 4
- **Dependencias:** TASK-027
- **Capa:** Application
- **Archivos a crear:**
  - `src/Vacations.Application/Solicitudes/Commands/EditarSolicitudCommand.cs`
  - `src/Vacations.Application/Solicitudes/Commands/EditarSolicitudCommandHandler.cs`
- **Trazabilidad:** CU-06, HU-03
- **Descripción:** Comando para editar una solicitud en estado Pending.
- **Criterios de aceptación:**
  - [ ] Solo permite editar si estado es `Pending`
  - [ ] Puede modificar: `FechaInicio`, `FechaFin`, `Motivo`
  - [ ] Recalcula días hábiles si cambian fechas
  - [ ] Ajusta `pendingBalance` si cambian los días
  - [ ] Registra cambios en historial con `changedFields` JSON

## TASK-030: Crear comando CancelarSolicitudCommand + Handler
- **Fase:** 4
- **Dependencias:** TASK-027
- **Capa:** Application
- **Archivos a crear:**
  - `src/Vacations.Application/Solicitudes/Commands/CancelarSolicitudCommand.cs`
  - `src/Vacations.Application/Solicitudes/Commands/CancelarSolicitudCommandHandler.cs`
- **Trazabilidad:** CU-07, HU-03
- **Descripción:** Comando para que un empleado cancele su solicitud Pending.
- **Criterios de aceptación:**
  - [ ] Verifica que el usuario sea el dueño de la solicitud
  - [ ] Solo permite cancelar si estado es `Pending`
  - [ ] Libera `pendingBalance`
  - [ ] Registra en historial

## TASK-034: Crear query ObtenerMisSolicitudesQuery + Handler
- **Fase:** 4
- **Dependencias:** TASK-013
- **Capa:** Application
- **Archivos a crear:**
  - `src/Vacations.Application/Solicitudes/Queries/ObtenerMisSolicitudesQuery.cs`
  - `src/Vacations.Application/Solicitudes/Queries/ObtenerMisSolicitudesQueryHandler.cs`
- **Trazabilidad:** CU-05, HU-02
- **Descripción:** Query para que un empleado liste sus propias solicitudes. El `pageSize` se recibe como parámetro opcional (default: 10) y puede ser 5, 10, 15 o 25.
- **Criterios de aceptación:**
  - [ ] Filtro opcional por estado
  - [ ] Paginación offset-based con `page` y `pageSize` (soporta 5, 10, 15, 25)
  - [ ] Ordenado de más reciente a más antiguo
  - [ ] Retorna DTO con: Id, Fechas, Días, Estado, Motivo, ComentarioAprobador

## TASK-035: Crear query ObtenerSolicitudDetalleQuery + Handler
- **Fase:** 4
- **Dependencias:** TASK-013
- **Capa:** Application
- **Archivos a crear:**
  - `src/Vacations.Application/Solicitudes/Queries/ObtenerSolicitudDetalleQuery.cs`
  - `src/Vacations.Application/Solicitudes/Queries/ObtenerSolicitudDetalleQueryHandler.cs`
- **Trazabilidad:** CU-05, HU-02
- **Descripción:** Query para obtener el detalle de una solicitud incluyendo historial.
- **Criterios de aceptación:**
  - [ ] Incluye historial de eventos
  - [ ] Verifica que el usuario tenga acceso (es dueño, es aprobador, o es RRHH)

## TASK-040: Crear DTOs compartidos
- **Fase:** 4
- **Dependencias:** TASK-027
- **Capa:** Application
- **Archivos a crear:**
  - `src/Vacations.Application/Common/SolicitudDto.cs`
  - `src/Vacations.Application/Common/SaldoDto.cs`
  - `src/Vacations.Application/Common/EmpleadoDto.cs`
  - `src/Vacations.Application/Common/HistorialEventoDto.cs`
  - `src/Vacations.Application/Common/PagedResult.cs`
- **Trazabilidad:** `constitution.md` sección 8 (ViewModels contra overposting)
- **Descripción:** DTOs para transferir datos entre capas.
- **Criterios de aceptación:**
  - [ ] Records inmutables
  - [ ] Sin lógica de negocio
  - [ ] `PagedResult<T>` con: Items, TotalCount, PageNumber, PageSize, AvailablePageSizes (List<int> con [5, 10, 15, 25])
  - [ ] El ViewModel de paginación expone `AvailablePageSizes` y `SelectedPageSize` para que la vista renderice el `<select class="page-size-select">`

---

# Web: Controllers y Vistas de Feature 002

## TASK-045: Crear ViewModels de Solicitud
- **Fase:** 5
- **Dependencias:** TASK-040
- **Capa:** Web
- **Archivos a crear:**
  - `src/Vacations.Web/ViewModels/CrearSolicitudViewModel.cs`
  - `src/Vacations.Web/ViewModels/EditarSolicitudViewModel.cs`
  - `src/Vacations.Web/ViewModels/DetalleSolicitudViewModel.cs`
  - `src/Vacations.Web/ViewModels/ListaSolicitudesViewModel.cs`
- **Trazabilidad:** `constitution.md` sección 8 (overposting)
- **Descripción:** ViewModels para las vistas de solicitudes.
- **Criterios de aceptación:**
  - [ ] Solo propiedades necesarias para cada vista
  - [ ] DataAnnotations para validación del lado del cliente
  - [ ] Propiedades para mostrar mensajes de error

## TASK-048: Crear SolicitudVacacionesController
- **Fase:** 5
- **Dependencias:** TASK-045, TASK-027 a TASK-035
- **Capa:** Web
- **Archivos a crear:**
  - `src/Vacations.Web/Controllers/SolicitudVacacionesController.cs`
- **Trazabilidad:** `plan.md` sección 6 (API), CU-04 a CU-07
- **Descripción:** Controller para CRUD de solicitudes del empleado.
- **Criterios de aceptación:**
  - [ ] `[Authorize(Policy = "RequiereEmpleado")]`
  - [ ] `GET /solicitudes-vacaciones` → Lista mis solicitudes
  - [ ] `GET /solicitudes-vacaciones/{id}` → Detalle
  - [ ] `GET /solicitudes-vacaciones/crear` → Form de creación
  - [ ] `POST /solicitudes-vacaciones` → Crear
  - [ ] `GET /solicitudes-vacaciones/{id}/editar` → Form de edición
  - [ ] `PUT /solicitudes-vacaciones/{id}` → Editar
  - [ ] `POST /solicitudes-vacaciones/{id}/cancelar` → Cancelar
  - [ ] Manejo de errores con mensajes UX

## TASK-054: Crear vistas de Solicitud (Empleado)
- **Fase:** 5
- **Dependencias:** TASK-053, TASK-048
- **Capa:** Web
- **Archivos a crear:**
  - `src/Vacations.Web/Views/SolicitudVacaciones/Index.cshtml`
  - `src/Vacations.Web/Views/SolicitudVacaciones/Detalle.cshtml`
  - `src/Vacations.Web/Views/SolicitudVacaciones/Crear.cshtml`
  - `src/Vacations.Web/Views/SolicitudVacaciones/Editar.cshtml`
- **Trazabilidad:** `DESIGN_TOKENS.md`, CU-04 a CU-07
- **Descripción:** Vistas Razor para solicitudes del empleado.
- **Criterios de aceptación:**
  - [ ] Lista con tabla paginada
  - [ ] Badges de estado con colores (ámbar=pending, esmeralda=approved, rojo=rejected, gris=cancelled/expired)
  - [ ] Formularios con validación del lado del cliente
  - [ ] Confirmación antes de cancelar

---

**Fin de Tareas Feature 002 — Vacation Request CRUD**
