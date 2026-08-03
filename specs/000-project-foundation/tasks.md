# Tareas de Implementación — Feature 000: Fundación del Proyecto (Infraestructura)



**Input**: `specs/spec.md`, `specs/plan.md`, `constitution.md`, `docs/DESIGN_TOKENS.md`

**Prerequisitos**: `specs/000-project-foundation/plan.md` (contexto técnico), `specs/000-project-foundation/spec.md`

**Tests**: Proyectos de tests obligatorios por constitución (§9). xUnit como framework base; fixtures para ITimeProvider y DB in-memory o SQLite para integraciones.

**Organización**: las tareas se agrupan por secciones: Setup, Domain, Abstracciones, Infrastructure, Web, Tests. Cada tarea sigue el bloque maestro (ver formato abajo).

**Fecha:** 2026-08-03

**Versión:** 1.0 (Feature 000)



---



# Setup (Estructura de solución y configuración inicial)



- [ ] T001 Crear solución y proyectos (Clean Architecture)

  - Prioridad: Alta | Capa: Todas | Fase: 1

  - `Vacations.sln`

  - `src/Vacations.Domain/Vacations.Domain.csproj`

  - `src/Vacations.Application/Vacations.Application.csproj`

  - `src/Vacations.Infrastructure/Vacations.Infrastructure.csproj`

  - `src/Vacations.Web/Vacations.Web.csproj`

  - Dependencias: Ninguna

  - Trazabilidad: constitution.md §3, plan.md §2

- [ ] T002 Configurar pipeline CI y plantillas de build

  - Prioridad: Media | Capa: N/A | Fase: 1 | Paralela: Sí

  - Dependencias: TASK-001

  - Trazabilidad: plan.md §8

- [ ] T003 Instalar paquetes NuGet por capa (estrictos)

  - Prioridad: Alta | Capa: Domain/Infrastructure/Web/Tests | Fase: 1

  - Infrastructure: `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.Tools`, `Microsoft.EntityFrameworkCore.SqlServer` (o provider elegido)

  - Web: `Microsoft.AspNetCore.Authentication.JwtBearer` (si aplica), `Swashbuckle.AspNetCore` (OpenAPI)

  - Tests: `xunit`, `Microsoft.NET.Test.Sdk`, `coverlet.collector`

  - Dependencias: TASK-001

  - Trazabilidad: plan.md §2

- [ ] T004 Crear estructura de carpetas por capa y convenciones

  - Prioridad: Alta | Capa: Todas | Fase: 1

  - Dependencias: TASK-001

  - Trazabilidad: plan.md §8

# Domain (modelado y value objects)



- [ ] T005 Modelar entidades base: Empleado, SaldoEmpleado, SolicitudVacaciones

  - Prioridad: Alta | Capa: Domain | Fase: 2 | Paralela: Sí

  - `src/Vacations.Domain/Entities/Empleado.cs`

  - `src/Vacations.Domain/Entities/SaldoEmpleado.cs`

  - `src/Vacations.Domain/Entities/SolicitudVacaciones.cs`

  - Dependencias: TASK-004

  - Trazabilidad: spec.md / constitution.md

- [ ] T006 Definir enums base: EstadoSolicitud, RolUsuario

  - Prioridad: Alta | Capa: Domain | Fase: 2 | Paralela: Sí

  - Dependencias: TASK-005

- [ ] T007 Implementar Value Object `RangoFechas`

  - Prioridad: Alta | Capa: Domain | Fase: 2 | Paralela: Sí

  - Dependencias: TASK-005

- [ ] T008 Implementar Value Object `DiasHabiles` y algoritmo `CalcularDiasHabiles`

  - Prioridad: Alta | Capa: Domain | Fase: 2 | Paralela: Sí

  - Dependencias: TASK-007

- [ ] T009 Modelar HistorialSolicitud y eventos de dominio

  - Prioridad: Media | Capa: Domain | Fase: 2 | Paralela: Sí

  - Dependencias: TASK-005

- [ ] T010 Definir excepciones de dominio y políticas de validación

  - Prioridad: Media | Capa: Domain | Fase: 2 | Paralela: Sí

  - Dependencias: TASK-005

# Abstracciones (contratos)



- [ ] T011 Definir interfaces de repositorios

  - Prioridad: Alta | Capa: Domain / Application | Fase: 2 | Paralela: Sí

  - Dependencias: TASK-005

- [ ] T012 Definir IUnitOfWork

  - Prioridad: Alta | Capa: Domain / Infrastructure | Fase: 2 | Paralela: Sí

  - Dependencias: TASK-011

- [ ] T013 Definir ITimeProvider (contrato)

  - Prioridad: Alta | Capa: Domain / Infrastructure / Tests | Fase: 2 | Paralela: Sí

  - Dependencias: Ninguna

# Infrastructure (implementaciones y configuraciones)



- [ ] T014 Crear VacationsDbContext y configuraciones EF Core

  - Prioridad: Alta | Capa: Infrastructure | Fase: 2

  - Dependencias: TASK-005, TASK-011

- [ ] T015 Configurar RowVersion y concurrencia optimista

  - Prioridad: Media | Capa: Infrastructure | Fase: 2 | Paralela: Sí

  - Dependencias: TASK-014

- [ ] T016 Implementar repositorios EF Core concretos

  - Prioridad: Alta | Capa: Infrastructure | Fase: 2 | Paralela: Sí

  - Dependencias: TASK-014, TASK-011

- [ ] T017 Implementar migraciones iniciales y seed de datos

  - Prioridad: Alta | Fase: 2

  - Dependencias: TASK-014

- [ ] T018 Configurar Identity (infraestructura de usuarios/roles)

  - Prioridad: Media | Capa: Infrastructure | Fase: 2 | Paralela: Sí

  - Dependencias: TASK-017

- [ ] T019 Implementar `ProveedorTiempoSistema` (ITimeProvider)

  - Prioridad: Alta | Capa: Infrastructure/Tests | Fase: 2 | Paralela: Sí

  - Dependencias: TASK-013

- [ ] T020 Implementar `InterceptorAuditoriaSaveChanges`

  - Prioridad: Media | Capa: Infrastructure | Fase: 2 | Paralela: Sí

  - Dependencias: TASK-014, TASK-016

- [ ] T021 Exponer adaptador de logging (abstracción)

  - Prioridad: Baja | Capa: Infrastructure | Fase: 2 | Paralela: Sí

  - Dependencias: TASK-014

# Web skeleton (esqueleto básico)



- [ ] T022 Crear Program.cs / Host minimal y configuración base

  - Prioridad: Alta | Capa: Web | Fase: 2

  - Dependencias: TASK-001, TASK-014

- [ ] T023 Configurar autenticación/autorization por políticas

  - Prioridad: Alta | Capa: Web | Fase: 2 | Paralela: Sí

  - Dependencias: TASK-018, TASK-022

- [ ] T024 Implementar layout base y assets (wwwroot, design tokens)

  - Prioridad: Media | Capa: Web | Fase: 2 | Paralela: Sí

  - Dependencias: TASK-022

# Tests base (proyectos y fixtures)



- [ ] T025 Crear Vacations.Domain.Tests y fixtures básicos

  - Prioridad: Alta | Capa: Tests | Fase: 3 | Paralela: Sí

  - Dependencias: TASK-005, TASK-013

- [ ] T026 Crear Vacations.Application.Tests

  - Prioridad: Media | Capa: Tests | Fase: 3 | Paralela: Sí

  - Dependencias: TASK-025

- [ ] T027 Crear Vacations.Infrastructure.Tests (integración)

  - Prioridad: Alta | Capa: Tests | Fase: 3

  - Dependencias: TASK-017, TASK-014

- [ ] T028 Crear Vacations.Web.Tests (integración mínima)

  - Prioridad: Media | Capa: Tests | Fase: 3 | Paralela: Sí

  - Dependencias: TASK-022, TASK-023

- [ ] T029 Configurar runners, cobertura y reporting (xUnit + coverlet)

  - Prioridad: Media | Capa: Tests / CI | Fase: 3 | Paralela: Sí

  - Dependencias: TASK-002, TASK-025

- [ ] T030 Crear fixtures comunes y helpers para pruebas

  - Prioridad: Media | Capa: Tests | Fase: 3 | Paralela: Sí

  - Dependencias: TASK-025, TASK-027