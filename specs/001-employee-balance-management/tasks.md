# Tareas de Implementación — Feature 001: Employee Balance Management



**Input**: `specs/001-employee-balance-management/spec.md`, `specs/plan.md` (secciones Saldos), `docs/use-cases.md`, `.specify/memory/constitution.md`, `docs/DESIGN_TOKENS.md`

**Prerequisites**: Feature 000 (fundación) — solución y proyectos, dominio base (Empleado, SaldoEmpleado con pendingBalance, value objects, interfaces), infraestructura (DbContext, repositorios), esqueleto Web y frameworks de tests. **Obligatorio completar el Feature 000 antes de ejecutar estas tareas.**

**Tests**: obligatorios por la constitución (§9 — cobertura ≥ 80% en Domain y Application). Se crean en la fase de la historia que verifican.

**Organización**: las tareas se agrupan por capa siguiendo el orden de implementación de HU-04 y permiten verificación independiente como entregable funcional.

**Fecha**: 2026-08-03

**Versión**: 1.0 (numeración de TASK alineada al `specs/tasks.md` maestro)



---



## Formato de tarea



Cada tarea usa el siguiente bloque:



`## [TASK-XXX] Título`



- **Prioridad:** Alta | Media | Baja

- **Estado:** [ ] Pendiente · [~] En Progreso · [x] Completada

- **Paralela:** Sí (puede ejecutarse en paralelo: no comparte archivos ni dependencias) | No

- **HU:** Historia de usuario asociada (HU-04) o «—» si es transversal/fundacional

- **Fase:** número de fase de ejecución (1-3)

- **Dependencias:** TASK-XXX (del Feature 000 o de este feature) o «Feature 000»

- **Capa:** Domain | Application | Infrastructure | Web | Tests

- **Archivos a crear:** rutas exactas

- **Trazabilidad:** plan.md / spec.md / CU-XX / RN-XX / RF-XX / constitution.md

- **Descripción:** qué se implementa y por qué

- **Criterios de aceptación:** lista verificable (todos deben cumplirse para dar por completa la tarea)



> **Regla de orden:** dentro de cada fase las tareas se listan en orden de ejecución. Si una tarea B depende de A, A aparece antes que B y B no se inicia hasta completar A. Las tareas marcadas como `Paralela: Sí` pueden ejecutarse en paralelo (archivos distintos, sin dependencias entre ellas). Las tareas del Feature 000 se consideran prerrequisito global: ninguna tarea de este feature comienza hasta que el Feature 000 esté completo.



---



## Resumen de Fases



| Fase | Descripción | Tareas |

|------|-------------|:------:|

| 1 | Servicio de acumulación mensual de saldo (Application) | 3 |

| 2 | Consulta de saldo (Application + Web) | 3 |

| 3 | Tests (Domain + Application) | 2 |

| **Total** | | **8** |



> **Nota sobre tiempos:** Este documento no incluye estimaciones de duración por tarea. La implementación la ejecuta un agente (IA), no un desarrollador humano, y en marcos ágiles se planifica por entregables funcionales completos, no por horas por tarea.



---



# Phase 1: Servicio de Acumulación Mensual de Saldo (Application)



**Propósito:** Implementar la acumulación automática de saldo (+1 día por mes completo laborado desde la fecha de ingreso) como servicio de aplicación reutilizable.



**Checkpoint:** El servicio calcula y persiste la acumulación mensual correctamente con `TimeProvider` inyectado; la acumulación es idempotente.



- [ ] T001 Crear ServicioAcumulacionSaldoMensual

  - Prioridad: Alta | Capa: Application | Fase: 1

  - `src/Vacations.Application/Saldos/ServicioAcumulacionSaldoMensual.cs`

  - [ ] Usa `TimeProvider` (no `DateTime.Now`)

  - [ ] Procesa solo empleados activos

  - [ ] Mes completo = mes calendario completo desde la fecha de ingreso

  - [ ] Empleado con ingreso a mitad de mes no acumula ese primer mes

  - [ ] Carry-over ilimitado (no resetea al cierre de año)

  - Dependencias: Feature 000 (ITimeProvider, SaldoEmpleado, IRepositorioEmpleado, IRepositorioSaldoEmpleado)

  - Trazabilidad: `spec.md` RN-01, CU-01, HU-04

  - HU: HU-04

- [ ] T002 Crear comando AcumularSaldoMensualCommand + Handler

  - Prioridad: Alta | Capa: Application | Fase: 1

  - `src/Vacations.Application/Saldos/Commands/AcumularSaldoMensualCommand.cs`

  - `src/Vacations.Application/Saldos/Commands/AcumularSaldoMensualCommandHandler.cs`

  - [ ] Persiste incrementos en `accumulatedBalance`

  - [ ] Usa IUnitOfWork para atomicidad

  - [ ] No acumula dos veces el mismo mes (idempotente)

  - [ ] Registra el último mes acumulado para evitar reprocesos

  - Dependencias: TASK-031, Feature 000 (IUnitOfWork)

  - Trazabilidad: CU-01, CU-03, RN-01

  - HU: HU-04

- [ ] T003 Crear pruebas del servicio de acumulación

  - Prioridad: Alta | Capa: Tests | Fase: 1

  - `tests/Vacations.Application.Tests/Saldos/AcumularSaldoMensualCommandHandlerTests.cs`

  - [ ] Test: empleado con 3 meses completos → +3 días

  - [ ] Test: ingreso a mitad de mes → no acumula el primer mes

  - [ ] Test: reproceso no duplica acumulaciones (idempotencia)

  - [ ] Test: empleado inactivo excluido

  - [ ] Mock de ITimeProvider para fechas fijas

  - Dependencias: TASK-031, TASK-032

  - Trazabilidad: `constitution.md` sección 9 (cobertura ≥ 80%), CU-01

  - HU: HU-04

# Phase 2: Consulta de Saldo (Application + Web)



**Propósito:** Entregar HU-04: el empleado consulta su saldo disponible en tiempo real (acumulado - consumido - pendiente).



**Checkpoint:** Un empleado autenticado ve su saldo (Acumulado, Consumido, Pendiente, Disponible) con cifras correctas.



- [ ] T004 Crear query ObtenerSaldoQuery + Handler

  - Prioridad: Alta | Capa: Application | Fase: 2

  - `src/Vacations.Application/Saldos/Queries/ObtenerSaldoQuery.cs`

  - `src/Vacations.Application/Saldos/Queries/ObtenerSaldoQueryHandler.cs`

  - [ ] Empleado puede consultar su propio saldo

  - [ ] RRHH puede consultar saldo de cualquier empleado

  - [ ] Retorna: Acumulado, Consumido, Pendiente, Disponible

  - [ ] Respuesta en ≤300ms p95

  - Dependencias: Feature 000 (IRepositorioSaldoEmpleado, IRepositorioEmpleado)

  - Trazabilidad: CU-02, HU-04, RN-27

  - HU: HU-04

- [ ] T005 Crear SaldoController

  - Prioridad: Alta | Capa: Web | Fase: 2

  - `src/Vacations.Web/Controllers/SaldoController.cs`

  - `src/Vacations.Web/ViewModels/SaldoViewModel.cs`

  - [ ] `[Authorize]`

  - [ ] `GET /saldo` → Mi saldo (empleado)

  - [ ] Muestra: Acumulado, Consumido, Pendiente, Disponible

  - [ ] `SaldoViewModel` solo con propiedades necesarias

  - Dependencias: TASK-034, Feature 000 (Layout base, autenticación)

  - Trazabilidad: `plan.md` sección 6 (API), CU-02, `constitution.md` sección 8 (overposting)

  - HU: HU-04

- [ ] T006 Crear vistas de Saldo

  - Prioridad: Alta | Capa: Web | Fase: 2

  - `src/Vacations.Web/Views/Saldo/Index.cshtml`

  - [ ] StatCards con: Acumulado, Consumido, Pendiente, Disponible

  - [ ] Barra de progreso visual

  - [ ] Cifras con `tabular-nums`

  - [ ] Muestra fecha de consulta y unidad (días)

  - Dependencias: TASK-035, Feature 000 (Layout base, DESIGN_TOKENS)

  - Trazabilidad: `DESIGN_TOKENS.md`, CU-02

  - HU: HU-04

# Phase 3: Tests de Cierre (Domain + Application)



**Propósito:** Cerrar la cobertura de HU-04 con pruebas de integración del flujo completo de saldo y verificar el estado del feature.



**Checkpoint:** `dotnet test` verde para todos los proyectos de tests del feature; cobertura ≥ 80% en Domain y Application.



- [ ] T007 Crear tests de ObtenerSaldoQueryHandler

  - Prioridad: Media | Capa: Tests | Fase: 3

  - `tests/Vacations.Application.Tests/Saldos/ObtenerSaldoQueryHandlerTests.cs`

  - [ ] Test: empleado consulta su propio saldo → datos correctos

  - [ ] Test: RRHH consulta saldo de otro empleado → permitido

  - [ ] Test: empleado no puede consultar saldo ajeno → denegado

  - [ ] Disponible = acumulado - consumido - pendiente

  - [ ] Mock de repositorios y TimeProvider

  - Dependencias: TASK-034, Feature 000 (proyecto Vacations.Application.Tests)

  - Trazabilidad: `constitution.md` sección 9, CU-02

  - HU: HU-04

- [ ] T008 Tests de integración del flujo de saldo (Feature 001)

  - Prioridad: Media | Capa: Tests | Fase: 3

  - `tests/Vacations.Infrastructure.Tests/Saldos/FlujoSaldoIntegracionTests.cs`

  - [ ] Acumulación persiste `accumulatedBalance` correctamente

  - [ ] Reproceso no duplica acumulaciones

  - [ ] `ObtenerSaldoQuery` refleja el estado persistido

  - [ ] Concurrencia optimista (rowVersion) sin saldos negativos

  - [ ] `dotnet test` verde

  - Dependencias: TASK-036, TASK-037, Feature 000 (proyecto Vacations.Infrastructure.Tests)

  - Trazabilidad: `constitution.md` sección 9, CU-01, CU-02, CU-03

  - HU: HU-04