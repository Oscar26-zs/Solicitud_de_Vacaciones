# Tareas de Implementación — Sistema de Solicitudes de Vacaciones (MVP)

**Input**: `spec/spec.md`, `docs/use-cases.md`, `spec/plan.md`, `.specify/memory/constitution.md`, `spec/DESIGN_TOKENS.md`
**Prerequisites**: `plan.md` (obligatorio), `spec.md` (obligatorio — historias de usuario HU-01 a HU-09), `constitution.md`
**Tests**: obligatorios por la constitución (§9 — cobertura ≥ 80% en Domain y Application). Se crean en la fase de la capa que verifican.
**Organización**: las tareas se agrupan por capa de la arquitectura (Setup → Domain → Infrastructure → Application → Web) para permitir implementar y verificar cada capa como un incremento compilable e independiente.
**Fecha:** 2026-07-29
**Versión:** 3.0 (MVP — ordenado por capas)

---

## Formato de tarea

Cada tarea usa el siguiente bloque:

`## [TASK-XXX] Título`

- **Prioridad:** Alta | Media | Baja
- **Estado:** [ ] Pendiente · [~] En Progreso · [x] Completada
- **Paralela:** Sí (puede ejecutarse en paralelo: no comparte archivos ni dependencias) | No
- **HU:** Historia de usuario asociada (HU-01…HU-09) o «—» si es transversal/fundacional
- **Fase:** número de fase de ejecución (1-5, por capa)
- **Dependencias:** tareas que deben completarse ANTES (se listan primero)
- **Capa:** Domain | Application | Infrastructure | Web | Tests
- **Archivos a crear:** rutas exactas
- **Trazabilidad:** plan.md / spec.md / CU-XX / RN-XX / RF-XX / constitution.md
- **Descripción:** qué se implementa y por qué
- **Criterios de aceptación:** lista verificable (todos deben cumplirse para dar por completa la tarea)

> **Regla de orden:** dentro de cada capa las tareas se listan en orden de ejecución. Si una tarea B depende de A, A aparece antes que B y B no se inicia hasta completar A. Las tareas marcadas como `Paralela: Sí` pueden ejecutarse en paralelo (archivos distintos, sin dependencias entre ellas).

---

## Resumen de Fases

| Fase | Capa | Tareas |
|------|------|:------:|
| 1 | Setup (Estructura de Solución) | 4 |
| 2 | Domain (Base Bloqueante) | 16 |
| 3 | Infrastructure | 12 |
| 4 | Application | 18 |
| 5 | Web | 18 |
| **Total** | | **68** |

> **Nota sobre tiempos:** Este documento no incluye estimaciones de duración por tarea. La implementación la ejecuta un agente (IA), no un desarrollador humano. Los tiempos a nivel macro (funcionalidades completas) se gestionan en la administración del proyecto.

---

# Phase 1: Setup (Estructura de Solución)

**Propósito:** Inicialización del proyecto y estructura base.

**Checkpoint:** Solución compilando, estructura limpia y scaffold eliminado. Se puede iniciar la base.

## [TASK-001] Crear solución y proyectos de Clean Architecture
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** No
- **HU:** —
- **Fase:** 1
- **Dependencias:** Ninguna
- **Capa:** Todas
- **Archivos a crear:**
  - `Vacations.sln`
  - `src/Vacations.Domain/Vacations.Domain.csproj`
  - `src/Vacations.Application/Vacations.Application.csproj`
  - `src/Vacations.Infrastructure/Vacations.Infrastructure.csproj`
  - `src/Vacations.Web/Vacations.Web.csproj`
- **Trazabilidad:** `constitution.md` sección 3 (Clean Architecture)
- **Descripción:** Crear la solución con 4 proyectos siguiendo Clean Architecture. El proyecto `Vacations.Domain` no debe tener dependencias. `Vacations.Application` referencia a Domain. `Vacations.Infrastructure` referencia a Domain y Application. `Vacations.Web` referencia a Application e Infrastructure.
- **Criterios de aceptación:**
  - [ ] Solución `Vacations.sln` creada en la raíz
  - [ ] 4 proyectos creados en carpeta `src/`
  - [ ] Referencias entre proyectos configuradas correctamente
  - [ ] `dotnet build` exitoso sin errores

## [TASK-002] Configurar paquetes NuGet por capa
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** No
- **HU:** —
- **Fase:** 1
- **Dependencias:** TASK-001
- **Capa:** Todas
- **Archivos a modificar:**
  - `src/Vacations.Domain/Vacations.Domain.csproj`
  - `src/Vacations.Application/Vacations.Application.csproj`
  - `src/Vacations.Infrastructure/Vacations.Infrastructure.csproj`
  - `src/Vacations.Web/Vacations.Web.csproj`
- **Trazabilidad:** `plan.md` sección 2 (Dependencias principales)
- **Descripción:** Instalar paquetes NuGet según la capa. Domain: ninguno. Application: FluentValidation. Infrastructure: EF Core, EF Core SqlServer, Identity. Web: ASP.NET Core MVC.
- **Criterios de aceptación:**
  - [ ] Domain NO tiene paquetes NuGet externos
  - [ ] Application tiene `FluentValidation` y `FluentValidation.DependencyInjectionExtensions`
  - [ ] Infrastructure tiene `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
  - [ ] Web tiene referencias necesarias para MVC con Razor
  - [ ] `dotnet restore` exitoso

## [TASK-003] Crear estructura de carpetas por capa
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** No
- **HU:** —
- **Fase:** 1
- **Dependencias:** TASK-001
- **Capa:** Todas
- **Archivos a crear:**
  - `src/Vacations.Domain/Entities/`
  - `src/Vacations.Domain/Enums/`
  - `src/Vacations.Domain/ValueObjects/`
  - `src/Vacations.Domain/Exceptions/`
  - `src/Vacations.Domain/Abstractions/`
  - `src/Vacations.Application/Solicitudes/Commands/`
  - `src/Vacations.Application/Solicitudes/Queries/`
  - `src/Vacations.Application/Saldos/Commands/`
  - `src/Vacations.Application/Saldos/Queries/`
  - `src/Vacations.Application/Common/`
  - `src/Vacations.Infrastructure/Persistence/`
  - `src/Vacations.Infrastructure/Persistence/Configurations/`
  - `src/Vacations.Infrastructure/Persistence/Repositories/`
  - `src/Vacations.Infrastructure/Identity/`
  - `src/Vacations.Infrastructure/BackgroundServices/`
  - `src/Vacations.Web/Controllers/`
  - `src/Vacations.Web/ViewModels/`
  - `src/Vacations.Web/Views/`
  - `src/Vacations.Web/Authorization/`
- **Trazabilidad:** `plan.md` sección 8 (Estructura de archivos)
- **Descripción:** Crear las carpetas necesarias para organizar el código según el plan de implementación.
- **Criterios de aceptación:**
  - [ ] Estructura de carpetas creada en cada proyecto
  - [ ] Archivos `.gitkeep` o README en carpetas vacías (opcional)

## [TASK-004] Eliminar scaffold existente y configurar .gitignore
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** No
- **HU:** —
- **Fase:** 1
- **Dependencias:** TASK-001
- **Capa:** N/A
- **Archivos a eliminar:**
  - `Solicitud_de_Vacaiones/` (carpeta completa del scaffold)
- **Archivos a modificar:**
  - `.gitignore`
- **Trazabilidad:** `plan.md` sección 12 (Riesgo: scaffold no cumple Clean Architecture)
- **Descripción:** Eliminar el proyecto scaffold existente que no cumple con Clean Architecture. Actualizar `.gitignore` para ignorar archivos de build, bin, obj, etc.
- **Criterios de aceptación:**
  - [ ] Carpeta `Solicitud_de_Vacaiones/` eliminada
  - [ ] `.gitignore` actualizado con patrones estándar de .NET
  - [ ] Solución compila sin referencias al scaffold eliminado

---

# Phase 2: Domain (Base Bloqueante)

**Propósito:** Capa de Dominio completa — enums, value objects, excepciones, entidades, abstracciones (repositorios e `IUnitOfWork`) y sus tests unitarios puros.

**⚠️ CRÍTICO:** Ningún trabajo de capas superiores comienza hasta que esta fase esté completa.

**Checkpoint:** Domain compila, entidades con invariantes protegidas y tests unitarios de Domain pasando.

## [TASK-005] Crear enum EstadoSolicitud
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** Sí
- **HU:** —
- **Fase:** 2
- **Dependencias:** TASK-003
- **Capa:** Domain
- **Archivos a crear:**
  - `src/Vacations.Domain/Enums/EstadoSolicitud.cs`
- **Trazabilidad:** `constitution.md` sección 2 (Estados), `spec.md` sección 15 (Glosario)
- **Descripción:** Crear enum con los 5 estados posibles de una solicitud de vacaciones.
- **Criterios de aceptación:**
  - [ ] Enum con valores: `Pending`, `Approved`, `Rejected`, `Cancelled`, `Expired`
  - [ ] Documentación XML en cada valor

## [TASK-006] Crear enum RolUsuario
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** Sí
- **HU:** —
- **Fase:** 2
- **Dependencias:** TASK-003
- **Capa:** Domain
- **Archivos a crear:**
  - `src/Vacations.Domain/Enums/RolUsuario.cs`
- **Trazabilidad:** `constitution.md` sección 1 (Actores), `spec.md` sección 2
- **Descripción:** Crear enum con los 3 roles del sistema.
- **Criterios de aceptación:**
  - [ ] Enum con valores: `Empleado`, `Aprobador`, `RRHH`

## [TASK-007] Crear Value Object RangoFechas
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** Sí
- **HU:** —
- **Fase:** 2
- **Dependencias:** TASK-003
- **Capa:** Domain
- **Archivos a crear:**
  - `src/Vacations.Domain/ValueObjects/RangoFechas.cs`
- **Trazabilidad:** `plan.md` sección 4 (Value Objects), RN-05, RN-06, RN-31
- **Descripción:** Value Object inmutable que encapsula fecha inicio y fecha fin con validaciones: inicio ≤ fin, inicio ≥ mañana, fin ≤ inicio + 2 meses.
- **Criterios de aceptación:**
  - [ ] Constructor privado, factory method `Crear(fechaInicio, fechaFin, fechaActual)`
  - [ ] Validación: fecha inicio no puede ser anterior a mañana
  - [ ] Validación: fecha fin no puede ser anterior a fecha inicio
  - [ ] Validación: horizonte máximo de 2 meses
  - [ ] Método `CalcularDiasHabiles()` que excluye sábados y domingos
  - [ ] Implementa `IEquatable<RangoFechas>`

## [TASK-008] Crear excepciones de dominio
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** Sí
- **HU:** —
- **Fase:** 2
- **Dependencias:** TASK-003
- **Capa:** Domain
- **Archivos a crear:**
  - `src/Vacations.Domain/Exceptions/DomainException.cs` (base)
  - `src/Vacations.Domain/Exceptions/SaldoInsuficienteException.cs`
  - `src/Vacations.Domain/Exceptions/TraslapeSolicitudesException.cs`
  - `src/Vacations.Domain/Exceptions/AutoAprobacionNoPermitidaException.cs`
  - `src/Vacations.Domain/Exceptions/TransicionEstadoInvalidaException.cs`
  - `src/Vacations.Domain/Exceptions/AprobadorInactivoException.cs`
- **Trazabilidad:** `use-cases.md` (excepciones por CU), `constitution.md` sección 7
- **Descripción:** Crear excepciones tipadas para cada regla de negocio que puede fallar.
- **Criterios de aceptación:**
  - [ ] Clase base `DomainException` que hereda de `Exception`
  - [ ] Cada excepción tiene mensaje descriptivo en español
  - [ ] Excepciones son serializables

## [TASK-009] Crear entidad Empleado
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** No
- **HU:** —
- **Fase:** 2
- **Dependencias:** TASK-006
- **Capa:** Domain
- **Archivos a crear:**
  - `src/Vacations.Domain/Entities/Empleado.cs`
- **Trazabilidad:** `plan.md` sección 4 (Entidad Empleado), CU-01, CU-02
- **Descripción:** Entidad que representa un usuario del sistema. Los roles se gestionan vía Identity, no como campo de esta entidad.
- **Criterios de aceptación:**
  - [ ] Propiedades: `Id` (Guid), `Email`, `NombreCompleto`, `FechaIngreso`, `EstaActivo`
  - [ ] Constructor privado + factory method `Crear(...)`
  - [ ] Método `Desactivar()` y `Activar()`
  - [ ] Validaciones en constructor (email no vacío, nombre no vacío)

## [TASK-010] Crear entidad SaldoEmpleado
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** No
- **HU:** —
- **Fase:** 2
- **Dependencias:** TASK-009
- **Capa:** Domain
- **Archivos a crear:**
  - `src/Vacations.Domain/Entities/SaldoEmpleado.cs`
- **Trazabilidad:** `plan.md` sección 4 (SaldoEmpleado), CU-01, CU-02, CU-03, RN-01, RN-02, RN-03, RN-24
- **Descripción:** Entidad que gestiona los días de vacaciones. Implementa la fórmula: `availableBalance = accumulatedBalance - consumedBalance - pendingBalance`.
- **Criterios de aceptación:**
  - [ ] Propiedades: `Id`, `EmpleadoId`, `SaldoAcumulado`, `SaldoConsumido`, `SaldoPendiente`, `UltimaActualizacion`, `RowVersion`
  - [ ] Propiedad calculada `SaldoDisponible` (no persistida)
  - [ ] Método `AcumularDias(int dias)` para CU-01
  - [ ] Método `CongelarSaldo(int dias)` para crear solicitud
  - [ ] Método `DescontarSaldo(int dias)` para aprobar solicitud
  - [ ] Método `LiberarSaldoPendiente(int dias)` para rechazar/cancelar/expirar
  - [ ] Método `RestaurarSaldo(int dias)` para cancelar aprobada
  - [ ] Invariante: saldo disponible nunca negativo (lanzar `SaldoInsuficienteException`)

## [TASK-011] Crear entidad SolicitudVacaciones
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** No
- **HU:** —
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

## [TASK-012] Crear entidad HistorialSolicitud
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** No
- **HU:** —
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

## [TASK-013] Crear interfaz IRepositorioSolicitudVacaciones
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** Sí
- **HU:** —
- **Fase:** 2
- **Dependencias:** TASK-011
- **Capa:** Domain
- **Archivos a crear:**
  - `src/Vacations.Domain/Abstractions/IRepositorioSolicitudVacaciones.cs`
- **Trazabilidad:** `constitution.md` sección 3 (SOLID, inversión de dependencias)
- **Descripción:** Interfaz de repositorio para la entidad SolicitudVacaciones.
- **Criterios de aceptación:**
  - [ ] Método `Task<SolicitudVacaciones?> ObtenerPorIdAsync(Guid id)`
  - [ ] Método `Task<IReadOnlyList<SolicitudVacaciones>> ObtenerPorEmpleadoAsync(Guid empleadoId)`
  - [ ] Método `Task<IReadOnlyList<SolicitudVacaciones>> ObtenerPendientesAsync()`
  - [ ] Método `Task<bool> ExisteTraslapeAsync(Guid empleadoId, DateTime inicio, DateTime fin, Guid? excluirSolicitudId)`
  - [ ] Método `Task AgregarAsync(SolicitudVacaciones solicitud)`
  - [ ] Método `Task ActualizarAsync(SolicitudVacaciones solicitud)`

## [TASK-014] Crear interfaz IRepositorioSaldoEmpleado
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** Sí
- **HU:** —
- **Fase:** 2
- **Dependencias:** TASK-010
- **Capa:** Domain
- **Archivos a crear:**
  - `src/Vacations.Domain/Abstractions/IRepositorioSaldoEmpleado.cs`
- **Trazabilidad:** `constitution.md` sección 3 (SOLID)
- **Descripción:** Interfaz de repositorio para la entidad SaldoEmpleado.
- **Criterios de aceptación:**
  - [ ] Método `Task<SaldoEmpleado?> ObtenerPorEmpleadoIdAsync(Guid empleadoId)`
  - [ ] Método `Task AgregarAsync(SaldoEmpleado saldo)`
  - [ ] Método `Task ActualizarAsync(SaldoEmpleado saldo)`

## [TASK-015] Crear interfaz IRepositorioEmpleado
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** Sí
- **HU:** —
- **Fase:** 2
- **Dependencias:** TASK-009
- **Capa:** Domain
- **Archivos a crear:**
  - `src/Vacations.Domain/Abstractions/IRepositorioEmpleado.cs`
- **Trazabilidad:** `constitution.md` sección 3 (SOLID)
- **Descripción:** Interfaz de repositorio para la entidad Empleado.
- **Criterios de aceptación:**
  - [ ] Método `Task<Empleado?> ObtenerPorIdAsync(Guid id)`
  - [ ] Método `Task<IReadOnlyList<Empleado>> ObtenerActivosAsync()`
  - [ ] Método `Task<bool> ExisteConEmailAsync(string email)`

## [TASK-016] Crear interfaz IUnitOfWork
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** No
- **HU:** —
- **Fase:** 2
- **Dependencias:** TASK-013, TASK-014, TASK-015
- **Capa:** Domain
- **Archivos a crear:**
  - `src/Vacations.Domain/Abstractions/IUnitOfWork.cs`
- **Trazabilidad:** `constitution.md` sección 3 (transacciones)
- **Descripción:** Interfaz para gestionar transacciones y garantizar consistencia.
- **Criterios de aceptación:**
  - [ ] Método `Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)`
  - [ ] Propiedades de acceso a repositorios (opcional, puede inyectarse por separado)

## [TASK-061] Crear proyecto Vacations.Domain.Tests
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** No
- **HU:** —
- **Fase:** 2
- **Dependencias:** TASK-005 a TASK-016
- **Capa:** Tests
- **Archivos a crear:**
  - `tests/Vacations.Domain.Tests/Vacations.Domain.Tests.csproj`
- **Trazabilidad:** `constitution.md` sección 9 (pirámide de pruebas)
- **Descripción:** Proyecto de pruebas unitarias puras para Domain.
- **Criterios de aceptación:**
  - [ ] Usa xUnit
  - [ ] Sin mocks (pruebas puras)
  - [ ] Referencia solo a Vacations.Domain

## [TASK-062] Crear tests de entidad SolicitudVacaciones
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** Sí
- **HU:** —
- **Fase:** 2
- **Dependencias:** TASK-061, TASK-011
- **Capa:** Tests
- **Archivos a crear:**
  - `tests/Vacations.Domain.Tests/Entities/SolicitudVacacionesTests.cs`
- **Trazabilidad:** `constitution.md` sección 2 (transiciones), CU-04, CU-11, CU-12
- **Descripción:** Tests unitarios para la máquina de estados de solicitud.
- **Criterios de aceptación:**
  - [ ] Test: Crear solicitud → estado inicial Pending
  - [ ] Test: Aprobar solicitud Pending → estado Approved
  - [ ] Test: Aprobar por mismo autor → lanza AutoAprobacionNoPermitidaException
  - [ ] Test: Rechazar sin comentario → lanza excepción
  - [ ] Test: Cancelar solicitud Approved cuyo periodo ya inició → lanza excepción
  - [ ] Test: Transición inválida (Approved → Rejected) → lanza TransicionEstadoInvalidaException

## [TASK-063] Crear tests de entidad SaldoEmpleado
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** Sí
- **HU:** —
- **Fase:** 2
- **Dependencias:** TASK-061, TASK-010
- **Capa:** Tests
- **Archivos a crear:**
  - `tests/Vacations.Domain.Tests/Entities/SaldoEmpleadoTests.cs`
- **Trazabilidad:** RN-01, RN-02, RN-03, RN-04
- **Descripción:** Tests unitarios para gestión de saldo.
- **Criterios de aceptación:**
  - [ ] Test: Acumular días incrementa saldo acumulado
  - [ ] Test: Congelar saldo incrementa pendingBalance, reduce disponible
  - [ ] Test: Descontar saldo mueve de pendiente a consumido
  - [ ] Test: Liberar saldo pendiente restaura disponible
  - [ ] Test: Saldo disponible negativo → lanza SaldoInsuficienteException

## [TASK-064] Crear tests de Value Object RangoFechas
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** Sí
- **HU:** —
- **Fase:** 2
- **Dependencias:** TASK-061, TASK-007
- **Capa:** Tests
- **Archivos a crear:**
  - `tests/Vacations.Domain.Tests/ValueObjects/RangoFechasTests.cs`
- **Trazabilidad:** RN-05, RN-06, RN-31
- **Descripción:** Tests unitarios para validaciones de rango de fechas.
- **Criterios de aceptación:**
  - [ ] Test: Fecha inicio anterior a mañana → lanza excepción
  - [ ] Test: Fecha fin anterior a inicio → lanza excepción
  - [ ] Test: Rango mayor a 2 meses → lanza excepción
  - [ ] Test: Calcular días hábiles excluye sábados y domingos
  - [ ] Test: Rango válido → crea correctamente

---

# Phase 3: Infrastructure

**Propósito:** Persistencia e identidad — DbContext, configuraciones EF, repositorios, Identity, servicios de fondo y registro DI. Depende exclusivamente de Domain.

**Checkpoint:** Infrastructure compila con DbContext, Identity, repositorios y background service registrados vía DI.

## [TASK-017] Crear UsuarioAplicacion (Identity)
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** No
- **HU:** —
- **Fase:** 2
- **Dependencias:** TASK-002, TASK-009
- **Capa:** Infrastructure
- **Archivos a crear:**
  - `src/Vacations.Infrastructure/Identity/UsuarioAplicacion.cs`
- **Trazabilidad:** `spec.md` sección 8 (Identity Framework), `plan.md` sección 3
- **Descripción:** Clase que extiende IdentityUser para integrar con la entidad Empleado.
- **Criterios de aceptación:**
  - [ ] Hereda de `IdentityUser<Guid>`
  - [ ] Propiedad `EmpleadoId` para relacionar con entidad Empleado
  - [ ] Propiedad de navegación a `Empleado` (opcional)

## [TASK-018] Crear VacacionesDbContext
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** No
- **HU:** —
- **Fase:** 2
- **Dependencias:** TASK-017, TASK-009, TASK-010, TASK-011, TASK-012
- **Capa:** Infrastructure
- **Archivos a crear:**
  - `src/Vacations.Infrastructure/Persistence/VacacionesDbContext.cs`
- **Trazabilidad:** `plan.md` sección 3 (Infrastructure), `constitution.md` sección 6
- **Descripción:** DbContext de EF Core que integra Identity y las entidades del dominio.
- **Criterios de aceptación:**
  - [ ] Hereda de `IdentityDbContext<UsuarioAplicacion, IdentityRole<Guid>, Guid>`
  - [ ] DbSet para: `Empleados`, `SaldosEmpleado`, `SolicitudesVacaciones`, `HistorialSolicitudes`
  - [ ] Override de `OnModelCreating` para aplicar configuraciones
  - [ ] Configuración de `TimeProvider` inyectado

## [TASK-019] Crear configuración EmpleadoConfiguration
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** Sí
- **HU:** —
- **Fase:** 2
- **Dependencias:** TASK-018
- **Capa:** Infrastructure
- **Archivos a crear:**
  - `src/Vacations.Infrastructure/Persistence/Configurations/EmpleadoConfiguration.cs`
- **Trazabilidad:** `plan.md` sección 4 (Empleado), `constitution.md` sección 4 (nomenclatura BD)
- **Descripción:** Configuración Fluent API para la entidad Empleado.
- **Criterios de aceptación:**
  - [ ] Nombre de tabla: `Empleado`
  - [ ] `Email` único, requerido, máx 256 caracteres
  - [ ] `NombreCompleto` requerido, máx 200 caracteres
  - [ ] Índice en `Email`

## [TASK-020] Crear configuración SaldoEmpleadoConfiguration
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** Sí
- **HU:** —
- **Fase:** 2
- **Dependencias:** TASK-018
- **Capa:** Infrastructure
- **Archivos a crear:**
  - `src/Vacations.Infrastructure/Persistence/Configurations/SaldoEmpleadoConfiguration.cs`
- **Trazabilidad:** `plan.md` sección 4 (SaldoEmpleado)
- **Descripción:** Configuración Fluent API para la entidad SaldoEmpleado.
- **Criterios de aceptación:**
  - [ ] Nombre de tabla: `SaldoEmpleado`
  - [ ] Relación 1:1 con Empleado
  - [ ] `RowVersion` configurado como token de concurrencia
  - [ ] `SaldoDisponible` ignorado (propiedad calculada)

## [TASK-021] Crear configuración SolicitudVacacionesConfiguration
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** Sí
- **HU:** —
- **Fase:** 2
- **Dependencias:** TASK-018
- **Capa:** Infrastructure
- **Archivos a crear:**
  - `src/Vacations.Infrastructure/Persistence/Configurations/SolicitudVacacionesConfiguration.cs`
- **Trazabilidad:** `plan.md` sección 4 (SolicitudVacaciones)
- **Descripción:** Configuración Fluent API para la entidad SolicitudVacaciones.
- **Criterios de aceptación:**
  - [ ] Nombre de tabla: `SolicitudVacaciones`
  - [ ] FK a Empleado
  - [ ] `Estado` almacenado como string
  - [ ] `Motivo` requerido, mín 10 chars, máx 1000 chars
  - [ ] `ComentarioAprobador` opcional, máx 500 chars
  - [ ] `RowVersion` como token de concurrencia
  - [ ] Índices en `EmpleadoId`, `Estado`, `FechaInicio`

## [TASK-022] Crear configuración HistorialSolicitudConfiguration
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** Sí
- **HU:** —
- **Fase:** 2
- **Dependencias:** TASK-018
- **Capa:** Infrastructure
- **Archivos a crear:**
  - `src/Vacations.Infrastructure/Persistence/Configurations/HistorialSolicitudConfiguration.cs`
- **Trazabilidad:** `plan.md` sección 4 (HistorialSolicitud)
- **Descripción:** Configuración Fluent API para la entidad HistorialSolicitud.
- **Criterios de aceptación:**
  - [ ] Nombre de tabla: `HistorialSolicitud`
  - [ ] FK a SolicitudVacaciones
  - [ ] `CamposModificados` como JSON (nvarchar max)
  - [ ] Sin DELETE en cascada (auditoría inmutable)

## [TASK-023] Implementar RepositorioSolicitudVacaciones
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** No
- **HU:** —
- **Fase:** 2
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

## [TASK-024] Implementar RepositorioSaldoEmpleado
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** No
- **HU:** —
- **Fase:** 2
- **Dependencias:** TASK-018, TASK-014
- **Capa:** Infrastructure
- **Archivos a crear:**
  - `src/Vacations.Infrastructure/Persistence/Repositories/RepositorioSaldoEmpleado.cs`
- **Trazabilidad:** CU-01, CU-02, CU-11
- **Descripción:** Implementación del repositorio de saldos usando EF Core.
- **Criterios de aceptación:**
  - [ ] Implementa `IRepositorioSaldoEmpleado`
  - [ ] Manejo de concurrencia con `DbUpdateConcurrencyException`

## [TASK-025] Implementar RepositorioEmpleado
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** No
- **HU:** —
- **Fase:** 2
- **Dependencias:** TASK-018, TASK-015
- **Capa:** Infrastructure
- **Archivos a crear:**
  - `src/Vacations.Infrastructure/Persistence/Repositories/RepositorioEmpleado.cs`
- **Trazabilidad:** CU-01, CU-02
- **Descripción:** Implementación del repositorio de empleados usando EF Core.
- **Criterios de aceptación:**
  - [ ] Implementa `IRepositorioEmpleado`
  - [ ] `ObtenerActivosAsync` filtra por `EstaActivo == true`

## [TASK-026] Crear ServicioExpiracionAutomatica (BackgroundService)
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** No
- **HU:** —
- **Fase:** 2
- **Dependencias:** TASK-018, TASK-023
- **Capa:** Infrastructure
- **Archivos a crear:**
  - `src/Vacations.Infrastructure/BackgroundServices/ServicioExpiracionAutomatica.cs`
- **Trazabilidad:** CU-15, `plan.md` sección 10 (Complejidades), RN-26
- **Descripción:** Background service que expira solicitudes Pending cuya fecha de inicio ya pasó.
- **Criterios de aceptación:**
  - [ ] Hereda de `BackgroundService`
  - [ ] Ejecuta periódicamente (configurable, default cada hora)
  - [ ] Usa `TimeProvider` para obtener fecha actual
  - [ ] Busca solicitudes Pending con `FechaInicio <= hoy`
  - [ ] Cambia estado a `Expired`
  - [ ] Libera `pendingBalance` del empleado
  - [ ] Registra en `HistorialSolicitud` con actor `SISTEMA_AUTO_EXPIRACION`
  - [ ] Maneja errores sin detener el servicio

## [TASK-042] Crear extensión DependencyInjection para Infrastructure
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** No
- **HU:** —
- **Fase:** 2
- **Dependencias:** TASK-023 a TASK-026
- **Capa:** Infrastructure
- **Archivos a crear:**
  - `src/Vacations.Infrastructure/DependencyInjection.cs`
- **Trazabilidad:** `constitution.md` sección 3 (DI nativa)
- **Descripción:** Método de extensión para registrar servicios de Infrastructure.
- **Criterios de aceptación:**
  - [ ] Método `AddInfrastructureServices(this IServiceCollection, IConfiguration)`
  - [ ] Registra DbContext con connection string
  - [ ] Registra Identity
  - [ ] Registra repositorios
  - [ ] Registra BackgroundService de expiración
  - [ ] Registra `TimeProvider`

## [TASK-060] Crear datos de seed (usuarios y saldos iniciales)
- **Prioridad:** Media
- **Estado:** [ ] Pendiente
- **Paralela:** No
- **HU:** —
- **Fase:** 7
- **Dependencias:** TASK-018
- **Capa:** Infrastructure
- **Archivos a crear:**
  - `src/Vacations.Infrastructure/Persistence/SeedData.cs`
- **Trazabilidad:** `spec.md` sección 3 (creación de empleados por seed)
- **Descripción:** Crear datos iniciales para desarrollo y pruebas.
- **Criterios de aceptación:**
  - [ ] Al menos 3 empleados de prueba
  - [ ] Al menos 1 aprobador
  - [ ] Al menos 1 usuario RRHH
  - [ ] Saldos iniciales en 0
  - [ ] Contraseñas de desarrollo documentadas
  - [ ] Seed ejecuta solo si BD está vacía

---

# Phase 4: Application

**Propósito:** Casos de uso — commands, queries, handlers, validadores, DTOs compartidos y registro DI. Depende de Domain y de las abstracciones que Infrastructure implementa.

**Checkpoint:** Application compila con todos los handlers y validadores registrados vía DI, y sus tests unitarios pasan.

## [TASK-027] Crear comando CrearSolicitudCommand + Handler
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** Sí
- **HU:** HU-01
- **Fase:** 3
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

## [TASK-028] Crear validador CrearSolicitudCommandValidator
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** No
- **HU:** HU-01
- **Fase:** 3
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

## [TASK-034] Crear query ObtenerMisSolicitudesQuery + Handler
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** Sí
- **HU:** HU-02
- **Fase:** 3
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

## [TASK-035] Crear query ObtenerSolicitudDetalleQuery + Handler
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** Sí
- **HU:** HU-02
- **Fase:** 3
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

## [TASK-037] Crear query ObtenerSaldoQuery + Handler
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** Sí
- **HU:** HU-04
- **Fase:** 3
- **Dependencias:** TASK-014
- **Capa:** Application
- **Archivos a crear:**
  - `src/Vacations.Application/Saldos/Queries/ObtenerSaldoQuery.cs`
  - `src/Vacations.Application/Saldos/Queries/ObtenerSaldoQueryHandler.cs`
- **Trazabilidad:** CU-02, HU-04, RN-27
- **Descripción:** Query para consultar saldo de un empleado.
- **Criterios de aceptación:**
  - [ ] Empleado puede consultar su propio saldo
  - [ ] RRHH puede consultar saldo de cualquier empleado
  - [ ] Retorna: Acumulado, Consumido, Pendiente, Disponible
  - [ ] Respuesta en ≤300ms p95

## [TASK-029] Crear comando EditarSolicitudCommand + Handler
- **Prioridad:** Media
- **Estado:** [ ] Pendiente
- **Paralela:** Sí
- **HU:** HU-03
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

## [TASK-030] Crear comando CancelarSolicitudCommand + Handler
- **Prioridad:** Media
- **Estado:** [ ] Pendiente
- **Paralela:** Sí
- **HU:** HU-03
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

## [TASK-031] Crear comando AprobarSolicitudCommand + Handler
- **Prioridad:** Media
- **Estado:** [ ] Pendiente
- **Paralela:** Sí
- **HU:** HU-06
- **Fase:** 5
- **Dependencias:** TASK-027
- **Capa:** Application
- **Archivos a crear:**
  - `src/Vacations.Application/Solicitudes/Commands/AprobarSolicitudCommand.cs`
  - `src/Vacations.Application/Solicitudes/Commands/AprobarSolicitudCommandHandler.cs`
- **Trazabilidad:** CU-11, HU-06, RN-03, RN-08, RN-12, RN-13, RN-14
- **Descripción:** Comando para que un aprobador apruebe una solicitud.
- **Criterios de aceptación:**
  - [ ] Verifica que el aprobador no sea el autor (anti-auto-aprobación)
  - [ ] Verifica que el aprobador esté activo
  - [ ] Verifica estado `Pending`
  - [ ] Verifica saldo disponible actual (puede haber cambiado)
  - [ ] Mueve días de `pendingBalance` a `consumedBalance`
  - [ ] Registra en historial con actor = email aprobador
  - [ ] Maneja concurrencia optimista

## [TASK-032] Crear comando RechazarSolicitudCommand + Handler
- **Prioridad:** Media
- **Estado:** [ ] Pendiente
- **Paralela:** Sí
- **HU:** HU-06
- **Fase:** 5
- **Dependencias:** TASK-027
- **Capa:** Application
- **Archivos a crear:**
  - `src/Vacations.Application/Solicitudes/Commands/RechazarSolicitudCommand.cs`
  - `src/Vacations.Application/Solicitudes/Commands/RechazarSolicitudCommandHandler.cs`
- **Trazabilidad:** CU-12, HU-06, RN-11
- **Descripción:** Comando para que un aprobador rechace una solicitud con comentario obligatorio.
- **Criterios de aceptación:**
  - [ ] Verifica aprobador activo y no es autor
  - [ ] Verifica estado `Pending`
  - [ ] Comentario obligatorio (1-500 caracteres)
  - [ ] Libera `pendingBalance`
  - [ ] Registra en historial con comentario

## [TASK-036] Crear query ObtenerBandejaAprobadorQuery + Handler
- **Prioridad:** Media
- **Estado:** [ ] Pendiente
- **Paralela:** Sí
- **HU:** HU-05, HU-07
- **Fase:** 5
- **Dependencias:** TASK-013
- **Capa:** Application
- **Archivos a crear:**
  - `src/Vacations.Application/Solicitudes/Queries/ObtenerBandejaAprobadorQuery.cs`
  - `src/Vacations.Application/Solicitudes/Queries/ObtenerBandejaAprobadorQueryHandler.cs`
- **Trazabilidad:** CU-10, HU-05
- **Descripción:** Query para listar solicitudes Pending para aprobadores. El `pageSize` se recibe como parámetro opcional (default: 10) y puede ser 5, 10, 15 o 25.
- **Criterios de aceptación:**
  - [ ] Excluye solicitudes del propio aprobador
  - [ ] Filtros opcionales: empleado, rango fechas, días
  - [ ] Paginación offset-based con `page` y `pageSize` (soporta 5, 10, 15, 25)
  - [ ] Incluye saldo disponible del empleado
  - [ ] Indica si hay traslape con otras solicitudes
  - [ ] Ordenado de más antiguo a más reciente

## [TASK-033] Crear comando CancelarAprobadaCommand + Handler
- **Prioridad:** Media
- **Estado:** [ ] Pendiente
- **Paralela:** No
- **HU:** HU-03, HU-06
- **Fase:** 5
- **Dependencias:** TASK-031
- **Capa:** Application
- **Archivos a crear:**
  - `src/Vacations.Application/Solicitudes/Commands/CancelarAprobadaCommand.cs`
  - `src/Vacations.Application/Solicitudes/Commands/CancelarAprobadaCommandHandler.cs`
- **Trazabilidad:** CU-14, HU-03, RN-04
- **Descripción:** Comando para que un aprobador cancele una solicitud ya aprobada.
- **Criterios de aceptación:**
  - [ ] Solo si estado es `Approved`
  - [ ] Solo si fecha inicio > fecha actual
  - [ ] Restaura saldo (mueve de `consumedBalance` a disponible)
  - [ ] Registra en historial

## [TASK-038] Crear query ObtenerHistorialRRHHQuery + Handler
- **Prioridad:** Media
- **Estado:** [ ] Pendiente
- **Paralela:** No
- **HU:** HU-08, HU-09
- **Fase:** 6
- **Dependencias:** TASK-013
- **Capa:** Application
- **Archivos a crear:**
  - `src/Vacations.Application/Solicitudes/Queries/ObtenerHistorialRRHHQuery.cs`
  - `src/Vacations.Application/Solicitudes/Queries/ObtenerHistorialRRHHQueryHandler.cs`
- **Trazabilidad:** CU-18, HU-08, HU-09
- **Descripción:** Query para que RRHH consulte y filtre solicitudes de cualquier empleado. El `pageSize` se recibe como parámetro opcional (default: 10) y puede ser 5, 10, 15 o 25.
- **Criterios de aceptación:**
  - [ ] Solo accesible por rol RRHH
  - [ ] Filtros: estado, empleado, rango de fechas
  - [ ] Paginación offset-based con `page` y `pageSize` (soporta 5, 10, 15, 25)
  - [ ] Incluye información del empleado

## [TASK-039] Crear comando AcumularSaldoMensualCommand + Handler
- **Prioridad:** Media
- **Estado:** [ ] Pendiente
- **Paralela:** No
- **HU:** —
- **Fase:** 7
- **Dependencias:** TASK-014, TASK-015
- **Capa:** Application
- **Archivos a crear:**
  - `src/Vacations.Application/Saldos/Commands/AcumularSaldoMensualCommand.cs`
  - `src/Vacations.Application/Saldos/Commands/AcumularSaldoMensualCommandHandler.cs`
- **Trazabilidad:** CU-01, RN-01, RN-23, RN-24
- **Descripción:** Comando para acumular saldo mensual de todos los empleados activos.
- **Criterios de aceptación:**
  - [ ] Procesa solo empleados activos
  - [ ] Calcula meses completos desde fecha de ingreso
  - [ ] Acumula 1 día por mes completo no contabilizado
  - [ ] Carry-over ilimitado
  - [ ] Registra en historial de solicitud (futuro: historial de saldo)

## [TASK-040] Crear DTOs compartidos
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** No
- **HU:** —
- **Fase:** 2
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

## [TASK-041] Crear extensión DependencyInjection para Application
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** No
- **HU:** —
- **Fase:** 2
- **Dependencias:** TASK-027 a TASK-039
- **Capa:** Application
- **Archivos a crear:**
  - `src/Vacations.Application/DependencyInjection.cs`
- **Trazabilidad:** `constitution.md` sección 3 (DI nativa)
- **Descripción:** Método de extensión para registrar servicios de Application.
- **Criterios de aceptación:**
  - [ ] Método `AddApplicationServices(this IServiceCollection)`
  - [ ] Registra todos los handlers
  - [ ] Registra validadores de FluentValidation

## [TASK-065] Crear proyecto Vacations.Application.Tests
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** No
- **HU:** —
- **Fase:** 3
- **Dependencias:** TASK-027 a TASK-039
- **Capa:** Tests
- **Archivos a crear:**
  - `tests/Vacations.Application.Tests/Vacations.Application.Tests.csproj`
- **Trazabilidad:** `constitution.md` sección 9
- **Descripción:** Proyecto de pruebas unitarias con mocks para Application. Se crea cuando la capa Application comienza a compilar (primera historia).
- **Criterios de aceptación:**
  - [ ] Usa xUnit y Moq
  - [ ] Referencia a Vacations.Application y Vacations.Domain

## [TASK-066] Crear tests de CrearSolicitudCommandHandler
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** No
- **HU:** HU-01
- **Fase:** 3
- **Dependencias:** TASK-065, TASK-027
- **Capa:** Tests
- **Archivos a crear:**
  - `tests/Vacations.Application.Tests/Solicitudes/CrearSolicitudCommandHandlerTests.cs`
- **Trazabilidad:** CU-04
- **Descripción:** Tests del handler de creación de solicitud.
- **Criterios de aceptación:**
  - [ ] Test: Crear con saldo suficiente → éxito
  - [ ] Test: Crear con saldo insuficiente → falla
  - [ ] Test: Crear con traslape → falla
  - [ ] Test: Crear congela saldo pendiente
  - [ ] Mock de repositorios y TimeProvider

## [TASK-067] Crear tests de AprobarSolicitudCommandHandler
- **Prioridad:** Media
- **Estado:** [ ] Pendiente
- **Paralela:** No
- **HU:** HU-06
- **Fase:** 5
- **Dependencias:** TASK-065, TASK-031
- **Capa:** Tests
- **Archivos a crear:**
  - `tests/Vacations.Application.Tests/Solicitudes/AprobarSolicitudCommandHandlerTests.cs`
- **Trazabilidad:** CU-11
- **Descripción:** Tests del handler de aprobación.
- **Criterios de aceptación:**
  - [ ] Test: Aprobar mueve saldo de pendiente a consumido
  - [ ] Test: Aprobar por autor → falla
  - [ ] Test: Aprobar por aprobador inactivo → falla
  - [ ] Test: Aprobar con saldo insuficiente (concurrencia) → falla

---

# Phase 5: Web

**Propósito:** Presentación — Program.cs, políticas de autorización, ViewModels, controllers, vistas y layout, más tests de integración del sistema.

**Checkpoint:** Sistema completo y navegable, con autenticación, seed y tests de integración pasando.

## [TASK-043] Configurar Program.cs
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** No
- **HU:** —
- **Fase:** 2
- **Dependencias:** TASK-041, TASK-042
- **Capa:** Web
- **Archivos a crear/modificar:**
  - `src/Vacations.Web/Program.cs`
  - `src/Vacations.Web/appsettings.json`
  - `src/Vacations.Web/appsettings.Development.json`
- **Trazabilidad:** `plan.md` sección 8, `constitution.md` sección 8 (seguridad)
- **Descripción:** Configurar el punto de entrada de la aplicación con todos los servicios.
- **Criterios de aceptación:**
  - [ ] Llama `AddApplicationServices()` y `AddInfrastructureServices()`
  - [ ] Configura Identity con cookies
  - [ ] Configura autorización basada en roles
  - [ ] Configura Rate Limiting
  - [ ] Configura cabeceras de seguridad (HSTS, CSP, X-Frame-Options)
  - [ ] Connection string en appsettings

## [TASK-044] Crear políticas de autorización
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** No
- **HU:** —
- **Fase:** 2
- **Dependencias:** TASK-043
- **Capa:** Web
- **Archivos a crear:**
  - `src/Vacations.Web/Authorization/PoliticasAutorizacion.cs`
- **Trazabilidad:** `constitution.md` sección 1 (Actores), sección 8
- **Descripción:** Definir políticas de autorización basadas en roles.
- **Criterios de aceptación:**
  - [ ] Política `RequiereEmpleado`
  - [ ] Política `RequiereAprobador`
  - [ ] Política `RequiereRRHH`
  - [ ] Política `RequiereAprobadorActivo` (verifica usuario activo)

## [TASK-053] Crear Layout y vistas compartidas
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** No
- **HU:** —
- **Fase:** 2
- **Dependencias:** TASK-043
- **Capa:** Web
- **Archivos a crear:**
  - `src/Vacations.Web/Views/Shared/_Layout.cshtml`
  - `src/Vacations.Web/Views/Shared/_LoginPartial.cshtml`
  - `src/Vacations.Web/Views/Shared/_ValidationScriptsPartial.cshtml`
  - `src/Vacations.Web/Views/_ViewImports.cshtml`
  - `src/Vacations.Web/Views/_ViewStart.cshtml`
- **Trazabilidad:** `DESIGN_TOKENS.md`
- **Descripción:** Layout principal y vistas compartidas siguiendo guía de diseño.
- **Criterios de aceptación:**
  - [ ] Layout con navegación según rol
  - [ ] Usa tokens de color de `DESIGN_TOKENS.md`
  - [ ] Fuente Geist Sans/Mono
  - [ ] Modo claro/oscuro
  - [ ] Menú de usuario con rol visible

## [TASK-059] Crear archivo CSS con tokens de diseño
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** No
- **HU:** —
- **Fase:** 2
- **Dependencias:** TASK-053
- **Capa:** Web
- **Archivos a crear:**
  - `src/Vacations.Web/wwwroot/css/site.css`
- **Trazabilidad:** `DESIGN_TOKENS.md`
- **Descripción:** Hoja de estilos con variables CSS según guía de diseño.
- **Criterios de aceptación:**
  - [ ] Variables CSS para todos los tokens de color
  - [ ] Modo claro (`:root`) y oscuro (`.dark`)
  - [ ] Clases utilitarias para tipografía
  - [ ] Estilos para componentes: cards, badges, buttons, forms, tables

## [TASK-045] Crear ViewModels de Solicitud
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** No
- **HU:** HU-01, HU-02
- **Fase:** 3
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

## [TASK-046] Crear ViewModels de Aprobador
- **Prioridad:** Media
- **Estado:** [ ] Pendiente
- **Paralela:** No
- **HU:** HU-05, HU-06
- **Fase:** 5
- **Dependencias:** TASK-040
- **Capa:** Web
- **Archivos a crear:**
  - `src/Vacations.Web/ViewModels/BandejaAprobadorViewModel.cs`
  - `src/Vacations.Web/ViewModels/AprobarRechazarViewModel.cs`
- **Trazabilidad:** CU-10, CU-11, CU-12
- **Descripción:** ViewModels para las vistas de aprobador.
- **Criterios de aceptación:**
  - [ ] Bandeja incluye indicador de traslape
  - [ ] AprobarRechazar incluye campo para comentario

## [TASK-047] Crear ViewModels de RRHH
- **Prioridad:** Media
- **Estado:** [ ] Pendiente
- **Paralela:** No
- **HU:** HU-08, HU-09
- **Fase:** 6
- **Dependencias:** TASK-040
- **Capa:** Web
- **Archivos a crear:**
  - `src/Vacations.Web/ViewModels/ConsultaRRHHViewModel.cs`
  - `src/Vacations.Web/ViewModels/FiltrosRRHHViewModel.cs`
- **Trazabilidad:** CU-18
- **Descripción:** ViewModels para las vistas de RRHH.
- **Criterios de aceptación:**
  - [ ] Filtros para estado, empleado, fechas
  - [ ] Sin botones de acción (solo lectura)

## [TASK-048] Crear SolicitudVacacionesController
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** No
- **HU:** HU-01, HU-02, HU-03
- **Fase:** 3
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

## [TASK-049] Crear SaldoController
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** No
- **HU:** HU-04
- **Fase:** 3
- **Dependencias:** TASK-037
- **Capa:** Web
- **Archivos a crear:**
  - `src/Vacations.Web/Controllers/SaldoController.cs`
- **Trazabilidad:** `plan.md` sección 6, CU-02
- **Descripción:** Controller para consulta de saldo.
- **Criterios de aceptación:**
  - [ ] `[Authorize]`
  - [ ] `GET /saldo` → Mi saldo (empleado)
  - [ ] Muestra: Acumulado, Consumido, Pendiente, Disponible

## [TASK-050] Crear BandejaAprobadorController
- **Prioridad:** Media
- **Estado:** [ ] Pendiente
- **Paralela:** No
- **HU:** HU-05, HU-06, HU-07
- **Fase:** 5
- **Dependencias:** TASK-046, TASK-031, TASK-032, TASK-036
- **Capa:** Web
- **Archivos a crear:**
  - `src/Vacations.Web/Controllers/BandejaAprobadorController.cs`
- **Trazabilidad:** `plan.md` sección 6, CU-10 a CU-14
- **Descripción:** Controller para funcionalidades de aprobador.
- **Criterios de aceptación:**
  - [ ] `[Authorize(Policy = "RequiereAprobador")]`
  - [ ] `GET /bandeja-aprobador` → Lista pendientes
  - [ ] `GET /bandeja-aprobador/{id}` → Detalle con impacto en saldo
  - [ ] `POST /bandeja-aprobador/{id}/aprobar` → Aprobar
  - [ ] `POST /bandeja-aprobador/{id}/rechazar` → Rechazar con comentario
  - [ ] `POST /solicitudes-vacaciones/{id}/cancelar-aprobada` → Cancelar aprobada

## [TASK-051] Crear RRHHController
- **Prioridad:** Media
- **Estado:** [ ] Pendiente
- **Paralela:** No
- **HU:** HU-08, HU-09
- **Fase:** 6
- **Dependencias:** TASK-047, TASK-037, TASK-038
- **Capa:** Web
- **Archivos a crear:**
  - `src/Vacations.Web/Controllers/RRHHController.cs`
- **Trazabilidad:** `plan.md` sección 6, CU-18
- **Descripción:** Controller para consultas de RRHH.
- **Criterios de aceptación:**
  - [ ] `[Authorize(Policy = "RequiereRRHH")]`
  - [ ] `GET /rrhh/solicitudes` → Lista con filtros
  - [ ] `GET /rrhh/saldos/{empleadoId}` → Saldo de empleado
  - [ ] Sin acciones de modificación (solo lectura)

## [TASK-052] Crear CuentaController (Auth)
- **Prioridad:** Media
- **Estado:** [ ] Pendiente
- **Paralela:** No
- **HU:** —
- **Fase:** 7
- **Dependencias:** TASK-043
- **Capa:** Web
- **Archivos a crear:**
  - `src/Vacations.Web/Controllers/CuentaController.cs`
- **Trazabilidad:** `spec.md` sección 8 (Identity)
- **Descripción:** Controller para login/logout.
- **Criterios de aceptación:**
  - [ ] `GET /cuenta/login` → Form de login
  - [ ] `POST /cuenta/login` → Procesar login
  - [ ] `POST /cuenta/logout` → Cerrar sesión
  - [ ] Redirección según rol después del login

## [TASK-054] Crear vistas de Solicitud (Empleado)
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** No
- **HU:** HU-01, HU-02, HU-03
- **Fase:** 3
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

## [TASK-055] Crear vistas de Saldo
- **Prioridad:** Alta
- **Estado:** [ ] Pendiente
- **Paralela:** No
- **HU:** HU-04
- **Fase:** 3
- **Dependencias:** TASK-053, TASK-049
- **Capa:** Web
- **Archivos a crear:**
  - `src/Vacations.Web/Views/Saldo/Index.cshtml`
- **Trazabilidad:** `DESIGN_TOKENS.md`, CU-02
- **Descripción:** Vista para mostrar saldo del empleado.
- **Criterios de aceptación:**
  - [ ] StatCards con: Acumulado, Consumido, Pendiente, Disponible
  - [ ] Barra de progreso visual
  - [ ] Cifras con `tabular-nums`

## [TASK-056] Crear vistas de Bandeja Aprobador
- **Prioridad:** Media
- **Estado:** [ ] Pendiente
- **Paralela:** No
- **HU:** HU-05, HU-06, HU-07
- **Fase:** 5
- **Dependencias:** TASK-053, TASK-050
- **Capa:** Web
- **Archivos a crear:**
  - `src/Vacations.Web/Views/BandejaAprobador/Index.cshtml`
  - `src/Vacations.Web/Views/BandejaAprobador/Detalle.cshtml`
- **Trazabilidad:** `DESIGN_TOKENS.md`, CU-10 a CU-14
- **Descripción:** Vistas para la bandeja de aprobación.
- **Criterios de aceptación:**
  - [ ] Lista ordenada por antigüedad
  - [ ] Indicador visual de traslape
  - [ ] Botón aprobar deshabilitado si hay traslape con aprobada
  - [ ] Modal o form para comentario de rechazo
  - [ ] Muestra impacto en saldo antes de aprobar

## [TASK-057] Crear vistas de RRHH
- **Prioridad:** Media
- **Estado:** [ ] Pendiente
- **Paralela:** No
- **HU:** HU-08, HU-09
- **Fase:** 6
- **Dependencias:** TASK-053, TASK-051
- **Capa:** Web
- **Archivos a crear:**
  - `src/Vacations.Web/Views/RRHH/Solicitudes.cshtml`
  - `src/Vacations.Web/Views/RRHH/SaldoEmpleado.cshtml`
- **Trazabilidad:** `DESIGN_TOKENS.md`, CU-18
- **Descripción:** Vistas de solo lectura para RRHH.
- **Criterios de aceptación:**
  - [ ] Filtros combinables
  - [ ] Sin botones de acción
  - [ ] Tabla con todos los campos relevantes

## [TASK-058] Crear vistas de Cuenta (Login)
- **Prioridad:** Baja
- **Estado:** [ ] Pendiente
- **Paralela:** No
- **HU:** —
- **Fase:** 7
- **Dependencias:** TASK-053, TASK-052
- **Capa:** Web
- **Archivos a crear:**
  - `src/Vacations.Web/Views/Cuenta/Login.cshtml`
- **Trazabilidad:** `DESIGN_TOKENS.md`, `spec.md` sección 8
- **Descripción:** Vista de login.
- **Criterios de aceptación:**
  - [ ] Formulario centrado
  - [ ] Mensajes de error claros
  - [ ] Diseño monocromático según tokens

## [TASK-068] Crear proyecto Vacations.Web.Tests (integración)
- **Prioridad:** Media
- **Estado:** [ ] Pendiente
- **Paralela:** No
- **HU:** —
- **Fase:** 7
- **Dependencias:** TASK-048 a TASK-060
- **Capa:** Tests
- **Archivos a crear:**
  - `tests/Vacations.Web.Tests/Vacations.Web.Tests.csproj`
  - `tests/Vacations.Web.Tests/IntegrationTestBase.cs`
  - `tests/Vacations.Web.Tests/SolicitudVacacionesControllerTests.cs`
- **Trazabilidad:** `constitution.md` sección 9 (WebApplicationFactory)
- **Descripción:** Proyecto de pruebas de integración con WebApplicationFactory.
- **Criterios de aceptación:**
  - [ ] Usa xUnit y WebApplicationFactory
  - [ ] Base de datos en memoria o contenedor
  - [ ] Test: Usuario no autenticado → redirect a login
  - [ ] Test: Empleado puede crear solicitud
  - [ ] Test: Empleado no puede acceder a bandeja aprobador


---

## Dependencias & Orden de Ejecución

### Dependencias entre fases

- **Phase 1 (Setup):** sin dependencias — puede iniciar de inmediato.
- **Phase 2 (Domain):** depende de Phase 1 — **BLOQUEA todas las capas superiores**. ⚠️ Ningún trabajo de Infrastructure/Application/Web comienza hasta que esta fase termine.
- **Phase 3 (Infrastructure):** depende de Phase 2 — implementa las abstracciones definidas en Domain.
- **Phase 4 (Application):** depende de Phase 2 (interfaces de Domain) y Phase 3 (implementaciones registradas vía DI) — commands/queries usan las abstracciones.
- **Phase 5 (Web):** depende de Phase 3 y Phase 4 — controllers y vistas consumen Application.

### Orden dentro de cada fase

- Las tareas se listan en orden de ejecución: primero las que habilitan a las demás (si B depende de A, A aparece antes y B no se inicia hasta completar A).
- Domain: enums/VO/excepciones → entidades → interfaces → tests.
- Application: commands/queries → validators → persistence → DI → tests.
- Web: Program.cs/políticas/layout → ViewModels → controllers → vistas → tests.
- Las tareas `Paralela: Sí` se pueden lanzar juntas (archivos distintos, sin dependencias entre ellas).
- **No se inicia una tarea hasta completar sus `Dependencias`.**

### Oportunidades de paralelismo

- **[P] Phase 2:** TASK-005/006 (enums), TASK-007/008 (VO/excepciones), TASK-013/014/015 (interfaces), TASK-062/063/064 (tests Domain).
- **[P] Phase 3:** TASK-019..022 (configuraciones EF), TASK-023/024/025 (repositorios).
- **[P] Phase 4:** TASK-027/034/035/037 (handlers y queries independientes), TASK-029/030/031/032 (commands sobre solicitud).
- **[P] Phase 5:** TASK-044/053 (políticas y layout), TASK-045/046/047 (ViewModels), TASK-054/055/056/057/058 (vistas).

### Ruta crítica

```
Phase 1 (Setup)
   ↓
Phase 2 (Domain) ⚠️ BLOQUEANTE
   ↓
Phase 3 (Infrastructure)
   ↓
Phase 4 (Application)
   ↓
Phase 5 (Web + Tests de Integración)
```

---

## Estrategia de Implementación

### MVP Primero

1. Completar Phase 1 (Setup).
2. Completar Phase 2 (Domain) — **CRÍTICO**: bloquea todas las capas superiores.
3. Completar Phase 3 (Infrastructure) y Phase 4 (Application).
4. Completar Phase 5 (Web) → **MVP Empleado** (HU-01 · 02 · 04) navegable.
5. **PARAR y VALIDAR**: verificar cada historia de forma independiente antes de continuar puliendo.

### Entrega Incremental

1. Setup + Domain → base lista.
2. + Infrastructure + Application → persistencia y casos de uso listos.
3. + Web (crear, ver, consultar saldo) → MVP → validar.
4. + HU-03 (editar/cancelar) → validar.
5. + HU-05/06/07 (aprobación) → validar.
6. + HU-08/09 (RRHH) → validar.
7. + Polish/transversal → producto final.
8. Cada capa/historia añade valor sin romper las anteriores.

### Estrategia en paralelo

- Dentro de cada capa se ejecutan juntas las tareas `Paralela: Sí`.
- Las capas superiores solo se inician cuando la inferior está completa (compila y tests pasan).

---

## Apéndice: Mapeo Tarea → Caso de Uso

| Caso de Uso | Tareas Relacionadas |
|-------------|---------------------|
| CU-01 (Calcular/acumular saldo mensual) | TASK-010, TASK-039 |
| CU-02 (Consultar saldo) | TASK-037, TASK-049, TASK-055 |
| CU-03 (Registrar movimientos de balance) | TASK-010, TASK-039 |
| CU-04 (Crear solicitud) | TASK-011, TASK-027, TASK-028, TASK-048, TASK-054 |
| CU-05 (Ver mis solicitudes) | TASK-034, TASK-035, TASK-048, TASK-054 |
| CU-06 (Editar solicitud) | TASK-029, TASK-048, TASK-054 |
| CU-07 (Cancelar pending) | TASK-030, TASK-048, TASK-054 |
| CU-08 (Días hábiles) | TASK-007 |
| CU-09 (Prevención traslapes) | TASK-013, TASK-023 |
| CU-10 (Bandeja aprobador) | TASK-036, TASK-050, TASK-056 |
| CU-11 (Aprobar) | TASK-031, TASK-050, TASK-056, TASK-067 |
| CU-12 (Rechazar) | TASK-032, TASK-050, TASK-056 |
| CU-13 (Ver impacto saldo) | TASK-036, TASK-056 |
| CU-14 (Cancelar aprobada) | TASK-033, TASK-050, TASK-056 |
| CU-15 (Auto-expiración) | TASK-026 |
| CU-16 (Registro de roles y permisos) | TASK-006, TASK-017, TASK-043, TASK-044 |
| CU-17 (Auditoría y trazabilidad) | TASK-012, TASK-022 |
| CU-18 (Filtrado y consultas RRHH) | TASK-038, TASK-051, TASK-057 |
| CU-19 (Mensajes UX y manejo de errores) | TASK-048, TASK-054, TASK-058 |

---

## Notas

- **[P]/Paralela:** tareas en archivos distintos sin dependencias → pueden ejecutarse en paralelo.
- **[HU]:** etiqueta la historia de usuario que la tarea entrega (trazabilidad).
- **Pruebas:** se escriben en la fase de la capa que verifican; deben fallar antes de implementar (TDD).
- **Commits:** commitear tras cada tarea o grupo lógico; detenerse en cada checkpoint para validar.
- **Evitar:** tareas vagas, conflictos por archivos compartidos y dependencias cruzadas que rompan la independencia de las capas.
- **Tiempos:** sin estimaciones por tarea — los tiempos a nivel macro (funcionalidades completas) se gestionan en la administración del proyecto.
- **Pendientes identificados (documentados, sin tarea propia):**
  - Implementación concreta de `TimeProvider` (`ProveedorTiempoSistema`) — se crea como parte de TASK-018/TASK-042.
  - Componente de paginación `_TablePagination.cshtml` + `pagination.js` referenciados en TASK-040 — se crean dentro de TASK-053/TASK-059 o se añaden como tarea.
  - Cobertura de Application tests: actualmente solo 2 handlers (TASK-066/067) — revisar contra la meta ≥ 80% de constitution §9.

---

**Documento generado para:** Sistema de Gestión de Solicitudes de Vacaciones (MVP)  
**Cobertura:** 68 tareas, 19 casos de uso
