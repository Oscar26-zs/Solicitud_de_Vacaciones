# Implementation Plan: Sistema de Gestión de Solicitudes de Vacaciones (MVP)

**Branch**: `main` | **Date**: 2026-07-17 | **Spec**: `spec/spec.md`
**Input**: Especificación funcional en `spec/spec.md` y Constitución en `.specify/memory/constitution.md`

---

## Summary

**Objetivo principal:** Proveer un sistema web centralizado para gestionar el ciclo completo de solicitudes de **vacaciones** (único tipo de permiso en el MVP) dentro de la organización: creación por parte del empleado, revisión y decisión por un **Aprobador** (rol plano, sin jerarquía) y consulta de solo lectura por parte de **RRHH** (fuente: `spec.md`, sección 1).

**Problema que resuelve:** Elimina la gestión manual y descentralizada de solicitudes de vacaciones. Automatiza el flujo de creación, validación de saldos y fechas, notificación a aprobadores, aprobación/rechazo con comentario obligatorio al rechazar, cancelación con restauración de saldo bajo condiciones estrictas, auto-expiración de solicitudes pendientes tras `[N]` días y consulta histórica por RRHH (fuente: `spec.md`, secciones 1 y 3).

**Enfoque técnico de alto nivel:** Aplicación web ASP.NET Core MVC con Razor Views, estructurada como monolito modular en cuatro capas de Clean Architecture (Domain, Application, Infrastructure, Presentation), con Entity Framework Core como ORM, ASP.NET Core Identity para autenticación y gestión de sesiones, autorización basada en políticas, auditoría automática vía interceptors de EF Core, soft delete con Global Query Filter, control de concurrencia optimista con `RowVersion` y abstracción del tiempo mediante `TimeProvider` (fuente: `constitution.md`, secciones 2, 6, 7, 8, 9; `spec.md`, sección 9).

---

## Technical Context

**Language/Version**: C# sobre **.NET 10** (`net10.0`, verificado en `Solicitud_de_Vacaiones/Solicitud_de_Vacaiones.csproj`).
**Primary Framework**: ASP.NET Core MVC con Razor Views (`constitution.md`, sección 9). Prohibidos frameworks SPA (React, Angular, Vue) por Constitución.
**Primary Dependencies**:
- Entity Framework Core (ORM obligatorio por `constitution.md`, sección 9).
- ASP.NET Core Identity Framework para autenticación y sesiones (`spec.md`, sección 9 y cambios recientes punto 8).
- Middleware nativo de Rate Limiting de .NET (`constitution.md`, sección 7).
**Storage**: SQL Server LocalDB o SQLite (`constitution.md`, sección 9). Selección concreta: `NEEDS CLARIFICATION`.
**Testing**: Estrategia por capas obligatoria (unit en Domain sin mocks, unit en Application con mocks, integración en Infrastructure) según `constitution.md`, sección 6. Framework concreto (xUnit / NUnit / MSTest): `NEEDS CLARIFICATION`.
**Target Platform**: Aplicación web servida por Kestrel; despliegue objetivo: `NEEDS CLARIFICATION`.
**Project Type**: Monolito modular web (`constitution.md`, sección 2).
**Performance Goals**:
- Actualización de resultados filtrados en pantallas de consulta menor o igual a 2 segundos para volúmenes razonables (`spec.md`, RF-030).
- Otras metas (usuarios concurrentes objetivo, latencia p95 global, throughput): `NEEDS CLARIFICATION`.
**Constraints**:
- Prohibido `DateTime.Now` / `DateTime.UtcNow` directo en Domain/Application; se usa una abstracción de tiempo tipo `TimeProvider` (`constitution.md`, sección 6).
- Prohibido `DELETE` físico; se aplica soft delete con Global Query Filter en EF Core (`constitution.md`, sección 8).
- Auditoría automática vía sobrescritura de `SaveChangesAsync` con interceptor y Claims (`constitution.md`, sección 8).
- Concurrencia optimista con `RowVersion` obligatoria (`constitution.md`, sección 8).
- Sin dependencias de terceros como Redis, RabbitMQ ni APIs externas; el sistema debe correrse localmente al 100% con LocalDB o SQLite (`constitution.md`, sección 9).
- Sin integraciones externas (nómina, calendario corporativo, SSO, AD) en MVP (`spec.md`, sección 13).
- Zona horaria corporativa única; fechas puras sin componente hora (`spec.md`, RF-042; regla RN-27).
- Cálculo de duración excluye sábados y domingos; tratamiento de feriados: **abierto** (`spec.md`, regla RN-25).
- Valor numérico de `[N]` para auto-expiración: **abierto** (`spec.md`, regla RN-26).
- Tope máximo de carry-over de saldo: **abierto** (`spec.md`, regla RN-24).
- Horizonte futuro máximo para solicitar vacaciones: **abierto** (`spec.md`, regla RN-31).
- Estrategia de paginación del lado del servidor: **abierto** (`spec.md`, sección 7).
**Scale/Scope**:
- 3 roles (Empleado, Aprobador, RRHH), rol de aprobación plano sin jerarquía (`spec.md`, sección 2).
- 47 requisitos funcionales (RF-001 a RF-047) y 36 reglas de negocio (RN-01 a RN-36) definidos en `spec.md`.
- Número esperado de empleados, aprobadores y solicitudes concurrentes: `NEEDS CLARIFICATION`.

---

## Constitution Check

*GATE: debe pasarse antes de avanzar. Cualquier `FAIL` requiere justificación en Complexity Tracking o enmienda formal a la Constitución (`constitution.md`, sección 12).*

| Principio de la Constitución | Estado | Justificación |
|---|---|---|
| Clean Architecture como monolito modular con dependencias hacia adentro (sección 2) | PASS | La estructura propuesta separa Domain, Application, Infrastructure y Presentation en proyectos independientes; las referencias entre proyectos harán cumplir la dirección de dependencias. |
| Separación estricta en cuatro capas (sección 2) | PASS | Un proyecto por capa: `Vacations.Domain`, `Vacations.Application`, `Vacations.Infrastructure`, `Vacations.Web`. |
| Independencia del framework en Domain y Application (sección 2) | PASS | Domain y Application no referenciarán ASP.NET Core ni EF Core; las interfaces se declaran en Application y se implementan en Infrastructure. |
| Principios SOLID con Inversión de Dependencias vía DI nativa de ASP.NET Core (sección 2) | PASS | Registro de dependencias en `Program.cs`; interfaces con prefijo `I` para repositorios, servicios y abstracción de tiempo. |
| Actores y roles del sistema (sección 3) | FAIL | La Constitución define un rol "Gerente Directo" con relación jerárquica empleado-gerente. La Spec redefine el rol como "Aprobador plano" sin jerarquía y prohíbe explícitamente ciclos jerárquicos (`spec.md`, sección 2 y cambios recientes punto 2). Ver Complexity Tracking. |
| Reglas de negocio no negociables (sección 4) | FAIL | Discrepancias detectadas: (a) la Constitución declara "Una solicitud pendiente a la vez" y la Spec no la incluye; (b) la Constitución declara que una solicitud aprobada no puede cancelarse y la Spec permite cancelarla si el periodo no ha iniciado (`spec.md`, regla RN-04); (c) la Constitución contempla el tipo Médico que no descuenta saldo, la Spec limita el MVP a un único tipo (vacaciones). Ver Complexity Tracking. |
| Estados y transiciones válidas (sección 5) | FAIL | La Constitución define 4 estados finales (Pendiente, Aprobada, Rechazada, Cancelada). La Spec agrega el estado `Expired` para auto-expiración tras `[N]` días (`spec.md`, RF-043; regla RN-26). Ver Complexity Tracking. |
| Estándares de calidad y pruebas por capa (sección 6) | PASS | Se planifica un proyecto de pruebas por capa. Framework concreto: `NEEDS CLARIFICATION`. |
| Abstracción del tiempo (sección 6) | PASS | Se inyecta una abstracción tipo `TimeProvider` en todo cálculo dependiente del tiempo. |
| Controladores delgados (sección 6) | PASS | Los Controllers reciben la entrada, delegan a la capa de Application y devuelven vistas Razor, sin lógica de negocio ni acceso a datos. |
| Autorización basada en políticas y por recurso (sección 7) | PASS | Uso de `[Authorize(Policy=...)]` y verificación de propiedad del recurso en la capa de Application. |
| Claims con identidad del usuario (sección 7) | PASS | ID de empleado y rol almacenados en `Claims` durante el login. |
| Protección Anti-CSRF en mutaciones POST (sección 7) | PASS | `[ValidateAntiForgeryToken]` obligatorio en todas las acciones POST y tag helper correspondiente en formularios Razor. |
| Rate Limiting en rutas críticas (sección 7) | PASS | Middleware nativo configurado para Login y creación de solicitudes. Valores concretos: `NEEDS CLARIFICATION`. |
| Prevención de XSS (sección 7) | PASS | Validación y sanitización de entradas de usuario (motivo, comentario de rechazo) en ViewModels. |
| Soft delete con Global Query Filter (sección 8) | PASS | Se aplicará a las entidades auditables (`LeaveRequest`, `Employee`). |
| Auditoría automática vía `SaveChangesAsync` (sección 8) | PASS | Interceptor de EF Core con Claims y abstracción de tiempo. Auditoría limitada a trazabilidad de solicitudes según `spec.md`, sección 8. |
| Control de concurrencia optimista con `RowVersion` (sección 8) | PASS | Obligatorio en `LeaveRequest` para evitar carreras entre aprobación/cancelación (`spec.md`, RF-025). |
| Sin frameworks SPA de JS (sección 9) | PASS | Solo Razor Views. |
| EF Core como ORM (sección 9) | PASS | Confirmado. |
| Sin dependencias de terceros externos como Redis / RabbitMQ / APIs (sección 9) | PASS | Solo LocalDB o SQLite. |
| Convenciones de nomenclatura y sufijos arquitectónicos (sección 10) | PASS | `PascalCase` / `camelCase`, prefijo `I` para interfaces, sufijos `Controller`, `ViewModel`, `Repository`, `DbContext`, `Service`, `UseCase`, `Command`, `Query`. Código fuente en inglés; UI en español. |
| Documentación viva con Mermaid en `docs/diagrams/` (sección 11) | PASS | Se generarán los tres diagramas obligatorios (Casos de Uso, Máquina de Estados, Secuencia). Se documentará el estado `Expired` derivado de la Spec. |
| Gobernanza: la Constitución es ley máxima (sección 12) | NEEDS CLARIFICATION | Las divergencias marcadas como FAIL deben resolverse mediante enmienda formal a la Constitución o excepción aprobada antes de codificar. |

**Resultado del Gate:** **BLOQUEADO**. Existen tres divergencias sustantivas entre `constitution.md` y `spec.md` (rol de aprobación, cancelación de aprobadas, estado `Expired`). Deben resolverse formalmente antes de avanzar. Ver Complexity Tracking.

---

## Project Structure

### Documentación (repositorio)

```
spec/
├── spec.md                              # Especificación funcional (fuente autoritativa del qué)
├── plan.md                              # Este documento
├── DESIGN_TOKENS.md                     # Especificación de UI/UX
├── 001-employee-balance-management/
├── 002-vacation-request-crud/
├── 003-approval-workflow/
├── 004-request-auto-expiration/
└── 005-hr-monitoring-dashboard/

.specify/
├── memory/
│   └── constitution.md                  # Ley máxima del proyecto
└── templates/
    └── plan-template.md

docs/                                    # A crear (constitution.md sección 11)
└── diagrams/
    ├── use-cases.md                     # Mermaid: Casos de Uso
    ├── state-machine.md                 # Mermaid: Máquina de Estados
    └── sequence-approval.md             # Mermaid: Secuencia del flujo de aprobación
```

### Código fuente

**Estado actual del repositorio:** Existe un único proyecto `Solicitud_de_Vacaiones/Solicitud_de_Vacaiones.csproj` que es un scaffold MVC vacío en `.NET 10`. Este scaffold no cumple la separación de cuatro capas exigida por la Constitución (sección 2) y debe ser reemplazado o reorganizado para dar lugar a la estructura de Clean Architecture.

**Estructura objetivo (a crear):**

```
src/
├── Vacations.Domain/                    # Capa de Dominio
│   ├── Entities/                        # LeaveRequest, Employee, LeaveBalance, AuditEntry
│   ├── ValueObjects/                    # DateRange, WorkingDays
│   ├── Enums/                           # LeaveRequestStatus (Pending, Approved, Rejected, Cancelled, Expired)
│   ├── Exceptions/                      # InsufficientBalanceException, OverlappingRequestException, SelfApprovalNotAllowedException
│   └── Abstractions/                    # ITimeProvider
│
├── Vacations.Application/               # Capa de Aplicación (casos de uso)
│   ├── LeaveRequests/
│   │   ├── Commands/                    # Create, Edit, Cancel, Approve, Reject
│   │   └── Queries/                     # GetMyRequests, GetPendingApproverInbox, GetHrHistory
│   ├── Balances/                        # AccrueMonthlyBalance, GetBalance
│   ├── AutoExpiration/                  # ExpirePendingRequestsService
│   └── Abstractions/                    # ILeaveRequestRepository, ILeaveBalanceRepository, IEmployeeRepository, ICurrentUserService
│
├── Vacations.Infrastructure/            # Capa de Infraestructura
│   ├── Persistence/
│   │   ├── VacationsDbContext.cs
│   │   ├── Configurations/              # IEntityTypeConfiguration<T>
│   │   ├── Repositories/                # Implementaciones de las interfaces de Application
│   │   └── Interceptors/                # AuditSaveChangesInterceptor
│   ├── Identity/                        # ApplicationUser (ASP.NET Core Identity)
│   ├── Time/                            # SystemTimeProvider
│   └── BackgroundServices/              # AutoExpirationHostedService (estrategia: NEEDS CLARIFICATION)
│
└── Vacations.Web/                       # Capa de Presentación (ASP.NET Core MVC)
    ├── Controllers/                     # LeaveRequestsController, ApproverInboxController, HrDashboardController, AccountController
    ├── ViewModels/
    ├── Views/
    │   ├── LeaveRequests/
    │   ├── ApproverInbox/
    │   ├── HrDashboard/
    │   └── Shared/
    ├── ViewComponents/
    ├── wwwroot/
    │   ├── css/
    │   └── js/
    ├── Authorization/                   # Policies y Handlers (constitution.md sección 7)
    ├── Program.cs
    └── appsettings.json

tests/
├── Vacations.Domain.Tests/              # Unit puras, sin mocks
├── Vacations.Application.Tests/         # Unit con mocks de repositorios
├── Vacations.Infrastructure.Tests/      # Integración contra SQLite en memoria
└── Vacations.Web.Tests/                 # Integración con WebApplicationFactory
```

**Módulos modificados / creados:**

- **Crear:** los cuatro proyectos `Vacations.Domain`, `Vacations.Application`, `Vacations.Infrastructure`, `Vacations.Web` y sus cuatro proyectos de prueba correspondientes.
- **Modificar / Retirar:** el scaffold `Solicitud_de_Vacaiones/Solicitud_de_Vacaiones.csproj` debe migrarse a `Vacations.Web` o eliminarse. Decisión de migración concreta: `NEEDS CLARIFICATION`.
- **Crear:** carpeta `docs/diagrams/` con los tres diagramas obligatorios en Mermaid.

---

## Structure Decision

Se elige una estructura de **monolito modular en cuatro proyectos separados** (`Domain`, `Application`, `Infrastructure`, `Web`) por las siguientes razones trazables a la Constitución:

1. La Constitución (sección 2) exige explícitamente Clean Architecture con cuatro capas claramente definidas y dependencias que siempre apunten hacia adentro. Separar cada capa en un proyecto independiente permite que el compilador aplique automáticamente la dirección de dependencias mediante las referencias de proyecto, evitando violaciones accidentales.
2. La Constitución (sección 2) exige que Domain y Application no dependan de ASP.NET Core ni de Entity Framework Core. La única forma verificable de garantizar esto es que esos dos proyectos no incluyan las referencias de esos paquetes NuGet.
3. La Constitución (sección 6) establece una estrategia de pruebas diferenciada por capa (unitarias puras en Domain, unitarias con mocks en Application, integración en Infrastructure). Un proyecto por capa habilita esa segregación de pruebas y su ejecución independiente.
4. La Constitución (sección 10) impone sufijos arquitectónicos obligatorios (`Repository`, `DbContext`, `Controller`, `ViewModel`, `Command`, `Query`, `UseCase`, `Service`) cuya ubicación es distinta por capa; la separación en proyectos hace explícita esa ubicación.
5. La Constitución (sección 9) prohíbe frameworks SPA de JS, por lo que no se contempla un proyecto frontend independiente; toda la Presentación reside en `Vacations.Web` con Razor Views.

Se descartan explícitamente:

- **Un único proyecto MVC monolítico:** no permite verificar automáticamente la independencia del framework en Domain y Application (viola sección 2 de la Constitución).
- **Microservicios:** contradice la decisión de "monolito modular" de la Constitución (sección 2).
- **Frontend SPA separado (React/Angular/Vue):** prohibido por la Constitución (sección 9).

---

## Complexity Tracking

*Sección obligatoria porque el Constitution Check reportó `FAIL` en tres principios. Cada excepción arquitectónica requiere motivo, alternativa considerada y justificación (`constitution.md`, sección 12).*

| Excepción arquitectónica | Motivo | Alternativa considerada y por qué se descartó |
|---|---|---|
| Rol de aprobación **plano** ("Aprobador" que puede resolver solicitudes de cualquier empleado activo) en lugar del rol "Gerente Directo" con jerarquía definido en `constitution.md` sección 3. | Requerido por `spec.md` sección 2 y cambios recientes punto 2, que redefinen el modelo organizacional del MVP como plano y prohíben explícitamente ciclos jerárquicos. | **Alternativa considerada:** Mantener el modelo jerárquico de la Constitución y añadir asignación empleado-gerente. **Descartada porque:** la Spec del MVP excluye explícitamente la asignación de jefes por RRHH, la reasignación al cambiar de jefe y la prevención de ciclos jerárquicos (fuera de alcance MVP, sección 13). Implementarlo introduciría entidades, endpoints y flujos no soportados por el Spec. Debe formalizarse mediante enmienda a `constitution.md` sección 3 antes de codificar. |
| Añadir el estado **`Expired`** a la máquina de estados definida en `constitution.md` sección 5. | Requerido por `spec.md` RF-043 y regla RN-26 para representar la auto-expiración de solicitudes `Pending` que no fueron resueltas tras `[N]` días. | **Alternativa considerada:** Reutilizar el estado `Rejected` marcado con un actor especial `SISTEMA_AUTO_EXPIRACION`. **Descartada porque:** oscurece la semántica de auditoría y trazabilidad, mezcla decisiones humanas con acciones automáticas del sistema y no coincide con el vocabulario que la Spec utiliza en el Glosario (sección 15) y en RF-032, RF-037 y RF-043. Debe formalizarse mediante enmienda a `constitution.md` sección 5 antes de codificar. |
| Permitir la **cancelación de solicitudes `Approved`** por parte de un aprobador cuando el periodo de vacaciones aún no ha iniciado, con restauración de saldo. La Constitución (sección 4) declara la cancelación válida únicamente en estado `Pendiente`. | Requerido por `spec.md` regla RN-04, RF-015 y RF-047, para permitir corregir aprobaciones prematuras sin comprometer la integridad del saldo (que solo se restaura si el periodo aún no ha comenzado). | **Alternativa considerada:** Prohibir toda cancelación de aprobadas y obligar al empleado a consumir el permiso. **Descartada porque:** genera fricción operativa incompatible con la regla RN-04 del Spec. Debe formalizarse mediante enmienda a `constitution.md` sección 4 antes de codificar. |

**Acción requerida antes de codificar:** Las tres excepciones deben formalizarse mediante enmienda a la Constitución siguiendo el proceso de cambio de `constitution.md` sección 12. Adicionalmente, deben resolverse los ítems marcados como `NEEDS CLARIFICATION` en las secciones Technical Context y Constitution Check antes de iniciar la fase de diseño detallado (`design.md`).

---

*Fin del Plan.*