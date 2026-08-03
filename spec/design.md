# Diseño Técnico Detallado — Sistema de Solicitudes de Vacaciones (MVP)

**Fecha:** 2026-07-29 · **Versión:** 1.0 · **Fuentes:** `spec/spec.md`, `spec/plan.md`, `spec/tasks.md`, `.specify/memory/constitution.md`, `docs/use-cases.md`, `spec/DESIGN_TOKENS.md`

> Este documento desglosa el diseño técnico de cada capa y componente a nivel de implementación, complementando `plan.md`. Cada sección traza a los casos de uso (CU), requisitos funcionales (RF) y reglas de negocio (RN) de la especificación.

---

## 1. Stack Tecnológico

| Capa | Tecnología |
|------|-----------|
| Lenguaje / runtime | C# sobre **.NET 10** (`net10.0`) |
| Arquitectura | Clean Architecture (monolito modular, 4 capas) |
| Presentación | ASP.NET Core MVC (Razor Views), CSS/JS vanilla |
| ORM | Entity Framework Core 10 + SQL Server |
| Autenticación | ASP.NET Core Identity (IdentityUser<Guid>) |
| Validación de entrada | FluentValidation (ejecución explícita vía `ValidateAsync`) |
| Tiempo | `TimeProvider` nativo de .NET |
| Tests | xUnit (+ Moq en Application) |

---

## 2. Estructura de Solución

```
Vacations.slnx
src/
├── Vacations.Domain/           # Sin dependencias externas
├── Vacations.Application/      # → Domain
├── Vacations.Infrastructure/   # → Application, Domain
├── Vacations.Web/              # → Application + Infrastructure (Sdk.Web)
tests/
├── Vacations.Domain.Tests/
├── Vacations.Application.Tests/
└── Vacations.Web.Tests/        # WebApplicationFactory
```

Dependencias verificadas por el compilador: Domain y Application no referencian infraestructura ni frameworks (constitution §3).

---

## 3. Vacations.Domain

### 3.1 Enums (`Enums/`)

| Enum | Valores |
|------|---------|
| `EstadoSolicitud` | `Pending`, `Approved`, `Rejected`, `Cancelled`, `Expired` |
| `RolUsuario` | `Empleado`, `Aprobador`, `RRHH` |

### 3.2 Value Objects (`ValueObjects/`)

`RangoFechas` — VO inmutable con `IEquatable<RangoFechas>`:
- Factory `RangoFechas.Crear(fechaInicio, fechaFin, fechaActual)`:
  - `inicio < hoy+1` → `ArgOutOfRange` (RN-05/RF-003): "La fecha de inicio no puede ser anterior a mañana"
  - `fin < inicio` → (RN-06/RF-004): "La fecha de fin no puede ser anterior a la de inicio"
  - `inicio > fin.AddMonths(2)` → horizonte 2 meses (RN-31)
- `CalcularDiasHabiles()` → contar días de lunes a viernes inclusivo (RN-25/RF-002, feriados NO excluidos).

### 3.3 Excepciones (`Exceptions/`)
Base `DomainException : Exception`. Derivadas:
- `SaldoInsuficienteException`
- `TraslapeSolicitudesException`
- `AutoAprobacionNoPermitidaException`
- `TransicionEstadoInvalidaException`
- `AprobadorInactivoException`
- `SolicitudNoEncontradaException`

### 3.4 Entidades (`Entities/`)

#### `Empleado`
`Id Guid`, `Email`, `NombreCompleto`, `FechaIngreso DateTime`, `EstaActivo bool`.
Factory `Crear(email, nombreCompleto, fechaIngreso)`. Métodos `Activar()/Desactivar()`.

#### `SolicitudVacaciones`
`Id`, `EmpleadoId`, `FechaInicio`, `FechaFin`, `DiasRequeridos int`, `Estado EstadoSolicitud`, `Motivo string`, `ComentarioAprobador string?`, `AprobadoPor Guid?`, `CreadoEn`, `ActualizadoEn`, `RowVersion byte[]`.
Máquina de estados (`TransicionEstadoInvalidaException` en transición inválida). Métodos:
- `Crear(empleadoId, rango)`: estado inicial `Pending`.
- `Aprobar(aprobadorId)`: valida estado `Pending`, anti-auto-aprobación (`AutoAprobacionNoPermitidaException`), estado→`Approved`, registra `AprobadoPor`.
- `Rechazar(aprobadorId, comentario)`: comentario obligatorio (1..500).
- `Cancelar()`: solo `Pending` → `Cancelled`.
- `CancelarAprobada(fechaActual)`: solo `Approved` y `FechaInicio > fechaActual` → `Cancelled`; si ya inició lanza mensaje de RN-04.
- `Expirar()`: solo `Pending` → `Expired`.

#### `SaldoEmpleado`
`Id`, `EmpleadoId`, `SaldoAcumulado`, `SaldoConsumido`, `SaldoPendiente`, `UltimaActualizacion`, `RowVersion`.
`SaldoDisponible => SaldoAcumulado - SaldoConsumido - SaldoPendiente` (calculada, no persistida; plan.md §4).
Métodos (todos lanzan `SaldoInsuficienteException` si `SaldoDisponible < 0`):
- `AcumularDias(int)` → incrementa acumulado.
- `CongelarSaldo(int)` → incrementa pendiente (crear; plan.md §5 Módulo 1 flujo pendingBalance).
- `DescontarSaldo(int)` → pendiente→consumido (aprobar).
- `LiberarSaldoPendiente(int)` → decrementa pendiente (rechazar/cancelar/expirar).
- `RestaurarSaldo(int)` → decrementa consumido (cancelar aprobada).

#### `HistorialSolicitud`
Inmutable (sin setters públicos). `Id`, `SolicitudId`, `TipoEvento string` (`CREATED`, `UPDATED`, `STATUS_CHANGED`, `CANCELLED`), `EstadoAnterior EstadoSolicitud?`, `EstadoNuevo EstadoSolicitud?`, `CamposModificados string?` (JSON), `Actor string`, `Timestamp`, `Comentario string?`.
Factory `Crear(...)`. (Auditoría: CU-17, RF-032.)

### 3.5 Abstracciones (`Abstractions/`)
- `IRepositorioSolicitudVacaciones`: `ObtenerPorIdAsync`, `ObtenerPorEmpleadoAsync`, `ObtenerPendientesAsync`, `ExisteTraslapeAsync(empleadoId, inicio, fin, excluirSolicitudId?)`, `AgregarAsync`, `ActualizarAsync`.
- `IRepositorioSaldoEmpleado`: `ObtenerPorEmpleadoIdAsync`, `AgregarAsync`, `ActualizarAsync`.
- `IRepositorioEmpleado`: `ObtenerPorIdAsync`, `ObtenerActivosAsync`, `ExisteConEmailAsync`.
- `IUnitOfWork`: `SaveChangesAsync(cancellationToken)`.

### 3.6 Tests (Domain, puras, xUnit)
- `SolicitudVacacionesTests` (TASK-062): Pending inicial, aprobar→Approved, auto-aprobación lanza, rechazo sin comentario lanza, cancelar aprobada iniciada lanza, Approved→Rejected lanza.
- `SaldoEmpleadoTests` (TASK-063): acumular, congelar (reduce disponible), descontar (mueve pendiente→consumido), liberar, negativo→`SaldoInsuficienteException`.
- `RangoFechasTests` (TASK-064): inicio anterior a mañana, fin<inicio, >2 meses, días hábiles excluye sab/dom, rango válido.

---

## 4. Vacations.Application

CQRS ligero sin librería externa (commands/queries + handlers registrados vía `AddApplicationServices`).

### 4.1 Common/DTOs
- `SolicitudDto`, `SaldoDto`, `EmpleadoDto`, `HistorialEventoDto`, `PagedResult<T>` (`Items`, `TotalCount`, `PageNumber`, `PageSize`, `AvailablePageSizes = [5,10,15,25]`). Records inmutables (constitution §8 overposting).

### 4.2 Comandos / Handlers (`Solicitudes/Commands/`)
| Command | Handler |
|---------|---------|
| `CrearSolicitudCommand { EmpleadoId, FechaInicio, FechaFin, Motivo }` (CU-04) | Valida saldo (`SaldoDisponible` incl. pendiente), traslape, creea en `Pending`, congela pendiente, historial CREATED |
| `EditarSolicitudCommand` (CU-06) | Solo Pending; edita fechas/motivo; recalculadías; ajusta pendiente; historial UPDATED con `CamposModificados` JSON |
| `CancelarSolicitudCommand` (CU-07) | Dueño; solo Pending; libera pendiente; historial CANCELLED |
| `AprobarSolicitudCommand` (CU-11) | anti-auto-aprobación, aprobador activo, Pending, re-verifica saldo; `DescontarSaldo`; historial STATUS_CHANGED |
| `RechazarSolicitudCommand` (CU-12) | comentario obligatorio; `LiberarSaldoPendiente`; historial STATUS_CHANGED |
| `CancelarAprobadaCommand` (CU-14) | Approved + futuro; `RestaurarSaldo`; historial |
| `AcumularSaldoMensualCommand` (CU-01) | job mensual, empleados activos |
| `ExpiracionSolicitudesPendientesCommand` (CU-15) | `Solicitud.Expirar()` todas con `FechaInicio<=hoy`, libera pendiente |

### 4.3 Queries (`Solicitudes/Queries/`, `Saldos/Queries/`)
- `ObtenerMisSolicitudesQuery` (CU-05) — paginado (5/10/15/25), filtro estado, orden desc.
- `ObtenerSolicitudDetalleQuery` (CU-05) — incluye historial, verifica acceso.
- `ObtenerBandejaAprobadorQuery` (CU-10) — pendientes de otros, filtros, saldo, traslape.
- `ObtenerHistorialRRHHQuery` (CU-18) — rol RRHH, filtros.
- `ObtenerSaldoQuery` (CU-02) — propio/RRHH, ≤300ms.

### 4.4 Validators (FluentValidation, solo entrada)
`CrearSolicitudCommandValidator` (T-028): fecha inicio/fin no vacías, motivo ≥10. NO valida saldo/traslape (eso es Domain). Patrón: validar también Aprobar/Rechazar (comentario 1..500) y Editar.

### 4.5 DI
`AddApplicationServices(this IServiceCollection)` — registra todos los handlers con scoped y los validadores vía `AddValidatorsFromAssembly`.

---

## 5. Vacations.Infrastructure

### 5.1 Identity
`UsuarioAplicacion : IdentityUser<Guid>` con `EmpleadoId Guid?` y navegación a `Empleado`.

### 5.2 DbContext
`VacacionesDbContext : IdentityDbContext<UsuarioAplicacion, IdentityRole<Guid>, Guid>`.
DbSets: `Empleados`, `SaldosEmpleado`, `SolicitudesVacaciones`, `HistorialSolicitudes`.
Inyecta `TimeProvider` para timestamps (`CreadoEn`, `ActualizadoEn`, `Historial.Timestamp`).
`OnModelCreating` aplica `ApplyConfigurationsFromAssembly`.

### 5.3 Configuraciones Fluent API
- `EmpleadoConfiguration`: tabla `Empleado`, `Email` unique/256, `NombreCompleto` 200, índice Email.
- `SaldoEmpleadoConfiguration`: tabla `SaldoEmpleado`, 1:1 con Empleado, `RowVersion` → `IsRowVersion().IsConcurrencyToken()`, `SaldoDisponible` → `Ignore()`.
- `SolicitudVacacionesConfiguration`: tabla, FK Empleado (restrict), `Estado` → string (HasConversion), `Motivo` requerido (10..1000), `ComentarioAprobador` max 500, `RowVersion` concurrencia, índices `EmpleadoId`, `Estado`, `FechaInicio`.
- `HistorialSolicitudConfiguration`: tabla, FK Solicitud (no cascade — auditoría inmutable), `CamposModificados` como JSON string.

### 5.4 Repositorios
- `RepositorioSolicitudVacaciones`: implementa interfaz; `ExisteTraslapeAsync` busca Pending/Approved del mismo empleado con solapamiento de rango; `AsNoTracking` en consultas.
- `RepositorioSaldoEmpleado`: manag `DbUpdateConcurrencyException`.
- `RepositorioEmpleado`: `ObtenerActivosAsync` filtra `EstaActivo`.

### 5.5 Servicios
- `Time/` — `ProveedorTiempoSistema` (envuelve `TimeProvider.System`).
- `Persistence/InterceptorAuditoriaSaveChanges` — interceptor `SaveChangesAsync` que registra en `HistorialSolicitud` transiciones de estado automáticamente (constitution §7.8).
- `BackgroundServices/ServicioExpiracionAutomatica` (`BackgroundService`, periodo configurable) — invoca `ExpiracionSolicitudesPendientesCommand` con `TimeProvider`.
- `Persistence/SeedData` — empleados + usuarios Identity de prueba (3 empleados, 1 aprobador, 1 RRHH; saldos en 0; solo si BD vacía). Contraseñas de desarrollo documentadas.

### 5.6 DI
`AddInfrastructureServices(IServiceCollection, IConfiguration)` — `AddDbContext<VacacionesDbContext>(SqlServer, connString)`, `AddDefaultIdentity<UsuarioAplicacion>()`, repositorios, `TimeProvider`, `AddHostedService<ServicioExpiracionAutomatica>`, seed condicional.

---

## 6. Vacations.Web

### 6.1 Program.cs
- `builder.Services.AddApplicationServices().AddInfrastructureServices(builder.Configuration)`.
- `AddControllersWithViews()`, cookies Identity con `HttpOnly`, `Secure`, `SameSite=Lax`, deslizamiento `SlidingExpiration`.
- `AddAuthorization(...)` con políticas.
- `AddRateLimiter(...)` (auth: 5/min; escritura: 30/min; lectura: 120/min) — alineado con constitution §8.6.
- Middleware de seguridad en prod: HSTS, CSP, `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`.
- `MapControllerRoute` por defecto.

### 6.2 Autorización (`Authorization/`)
Políticas: `RequiereEmpleado`, `RequiereAprobador`, `RequiereRRHH`, más requisito de aprobador activo (`RequisitoEsAprobadorActivo`) que valida `EstaActivo` del empleado.

### 6.3 Controllers (delgados, delegan a Application)
- `SolicitudVacacionesController` (`[Authorize(Policy="RequiereEmpleado")]`): CRUD, `GET /solicitudes-vacaciones`, detalle, crear/editar (`[Bind]`), cancelar.
- `SaldoController` (`[Authorize]`): `GET /saldo`.
- `BandejaAprobadorController` (`RequiereAprobador`): bandeja, detalle con impacto, aprobar/rechazar (comentario), cancelar aprobada.
- `RRHHController` (`RequiereRRHH`): `GET /rrhh/solicitudes`, `GET /rrhh/saldos/{empleadoId}` (read-only).
- `CuentaController`: login/logout (Identity), redirección según rol.

### 6.4 ViewModels
`CrearSolicitudViewModel`, `EditarSolicitudViewModel`, `DetalleSolicitudViewModel`, `ListaSolicitudesViewModel`, `BandejaAprobadorViewModel`, `AprobarRechazarViewModel`, `ConsultaRRHHViewModel`, `FiltrosRRHHViewModel`, `LoginViewModel`. DataAnnotations (validación cliente) + propiedades de mensaje.

### 6.5 Vistas y diseño (DESIGN_TOKENS)
- `Views/Shared/_Layout.cshtml` con navegación según rol, menú de usuario, theme toggle.
- CSS en `wwwroot/css/site.css` con tokens monocromáticos okLCh (claro/oscuro), fuente Geist, componentes `.card`, `.btn`, `.badge`, `.table`, `.dialog`, `.sheet`, `.toast`, `.alert`, `.calendar-range`.
- Vistas: `SolicitudVacaciones/{Index,Detalle,Crear,Editar}`, `Saldo/Index`, `BandejaAprobador/{Index,Detalle}`, `RRHH/{Solicitudes,SaldoEmpleado}`, `Cuenta/Login`.
- Badges de estado (ámbar=pending, esmeralda=approved, rojo=rejected, gris=cancelled/expired).

### 6.6 Tests de integración (`Vacations.Web.Tests`, WebApplicationFactory)
- No autenticado → redirect a login.
- Empleado autenticado crea solicitud.
- Empleado no puede acceder a bandeja (403/redirect). BD in-memory o SQLite real + SeedData.

---

## 7. Transversal

- **Seguridad:** anti-CSRF automático (form tags token); `[Authorize]` y políticas en cada endpoint; `[Bind]`/ViewModels contra overposting; validación de acceso al recurso (dueño vs tercero, constitution §1).
- **Concurrencia:** `RowVersion` + captura de `DbUpdateConcurrencyException` en handlers de escritura (reintento/lógica definida) — constitution §7.1, plan §12.
- **Auditoría:** interceptor EF (`HistorialSolicitud`) + handlers registran CREATED/UPDATED/STATUS_CHANGED/CANCELLED con actor y timestamp. `HistorialSaldo` y `HistorialSolicitud` — `HistorialSaldo` fuera de MVP.
- **Mensajes UX:** excepciones de dominio se mapean a mensajes del `spec.md` §10 y se muestran en vistas/toasts (CU-19).

---

## 8. Pendientes documentados (ver tasks.md Notas)

- `TimeProvider` concreto → en Infrastructure (`ProveedorTiempoSistema`). Se crea con TASK-018/042.
- `_TablePagination.cshtml` + `pagination.js` → dentro de TASK-053/059.
- Cobertura de Application Tests: TASK-066/067 cubren Crear y Aprobar; revisar meta ≥ 80% de domain/application (constitution §9.2) según hore de tiempo.