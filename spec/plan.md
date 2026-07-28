# Plan de Implementación: Sistema de Gestión de Solicitudes de Vacaciones (MVP)

**Branch:** `main` | **Fecha:** 2026-07-28 | **Spec:** `spec/spec.md`, `spec/001-*-005-*`, `docs/use-cases.md`

---

## 1. Resumen

**Objetivo principal:** Proveer un sistema web para gestionar el ciclo completo de solicitudes de **vacaciones** (único tipo de permiso en el MVP): creación por el empleado con validación de saldo y fechas, revisión y decisión por un **Aprobador** (rol plano, sin jerarquía), consulta de solo lectura por RRHH, auto-expiración de solicitudes `Pending` no resueltas tras `[N]` días, y cancelación de solicitudes `Approved` antes del inicio del periodo (con restauración de saldo).

**Problema que resuelve:** Elimina la gestión manual y descentralizada de solicitudes de vacaciones. Automatiza validaciones (saldo, fechas, traslapes), el flujo de aprobación/rechazo con comentario obligatorio al rechazar, la expiración automática de pendientes sin resolver, y la consulta histórica filtrada por RRHH.

**Estrategia técnica general:** Monolito modular en ASP.NET Core MVC (Razor Views) con Clean Architecture en 4 capas (Domain, Application, Infrastructure, Presentation), Entity Framework Core como ORM, ASP.NET Core Identity para autenticación, y `TimeProvider` de .NET para abstracción del tiempo.

---

## 2. Contexto Técnico

| Atributo | Valor | Fuente |
|---|---|---|
| Lenguaje / Versión | C# sobre **.NET 10** (`net10.0`) | `Solicitud_de_Vacaiones.csproj` |
| Framework principal | ASP.NET Core MVC con Razor Views | `constitution.md` sección 6 |
| Almacenamiento | SQL Server LocalDB o SQLite (selección concreta: **NEEDS CLARIFICATION**) | `constitution.md` sección 6 |
| ORM | Entity Framework Core | `constitution.md` sección 6 |
| Autenticación | ASP.NET Core Identity Framework | `spec.md` sección 9 |
| Testing | **NEEDS CLARIFICATION** (se menciona xUnit en constitution.md sección 9 pero no se confirma framework concreto) | `constitution.md` sección 9 |
| Plataforma objetivo | Aplicación web servida por Kestrel. Despliegue objetivo: **NEEDS CLARIFICATION** | — |
| Tipo de proyecto | Monolito modular web | `constitution.md` sección 3 |
| Objetivos de rendimiento | Consulta de saldo ≤ 300ms p95; creación/aprobación ≤ 1s p95; listados paginados ≤ 2s p95 | `constitution.md` sección 10 |
| Restricciones técnicas | Prohibido `DateTime.Now`/`DateTime.UtcNow` en Domain/Application. Prohibido `DELETE` físico. Sin dependencias externas (Redis, RabbitMQ, APIs). Prohibidos frameworks SPA (React, Angular, Vue). Auditoría automática vía interceptor de EF Core. Concurrencia optimista con `RowVersion`. FluentValidation aprobado para validación de entrada. Nombre en español (PascalCase). | `constitution.md` secciones 4, 6, 7, 8 |
| Escala / Alcance | 3 roles (Empleado, Aprobador, RRHH). 47 requisitos funcionales (RF-001 a RF-047). 36 reglas de negocio (RN-01 a RN-36). Número esperado de empleados/solicitudes: **NEEDS CLARIFICATION** | `spec.md` |

### Dependencias principales identificadas

- Entity Framework Core
- ASP.NET Core Identity
- FluentValidation (validación de entrada)
- Middleware de Rate Limiting nativo de .NET
- `TimeProvider` (abstracción nativa de .NET)

### Puntos abiertos del contexto técnico

1. **Motor de base de datos concreto:** `constitution.md` menciona LocalDB o SQLite. **NEEDS CLARIFICATION**
2. **Framework de pruebas concreto:** no se especifica formalmente. **NEEDS CLARIFICATION**
3. **Plataforma de despliegue:** no se especifica. **NEEDS CLARIFICATION**
4. **Volumen de usuarios concurrentes:** no se especifica. **NEEDS CLARIFICATION**
5. **Valor de `[N]` para auto-expiración:** ABIERTO en `spec.md` RN-26.
6. **Tope de carry-over de saldo:** `spec.md` RN-24 indica carry-over ilimitado; `docs/Preguntas_Pendientes.md` D.3 indica "Sin carry-over; caducan en aniversario". **CONTRADICCIÓN — NEEDS CLARIFICATION**
7. **Horizonte futuro máximo para solicitar:** ABIERTO en `spec.md` RN-31.
8. **Manejo de feriados en cálculo de días:** ABIERTO en `spec.md` RN-25.
9. **Estrategia de paginación:** Offset-based según `docs/Preguntas_Pendientes.md` H.1.

---

## 3. Validación de la Constitución

*Fuente: `.specify/memory/constitution.md` (304 líneas)*

| Principio | Estado | Observación |
|---|---|---|
| Clean Architecture como monolito modular con dependencias hacia adentro | PASS | La estructura de 4 proyectos independientes garantiza la dirección de dependencias |
| Separación estricta en cuatro capas | PASS | `Vacations.Domain`, `Vacations.Application`, `Vacations.Infrastructure`, `Vacations.Web` |
| Independencia del framework en Domain y Application | PASS | Domain y Application no referencian ASP.NET Core ni EF Core |
| Principios SOLID con Inversión de Dependencias vía DI nativa | PASS | Interfaces para repositorios, servicios y abstracción de tiempo |
| Actores y roles del sistema (sección 1) | PASS | Empleado, Aprobador (rol plano), RRHH — alineado con `spec.md` |
| Estados y transiciones (sección 2) | PASS | 5 estados (Pending, Approved, Rejected, Cancelled, Expired) con transiciones documentadas — alineado con `spec.md` |
| Validación en el servidor (sección 3.5) | PASS | Toda regla de negocio se ejecuta en el servidor |
| Separación de validación de entrada vs. reglas de negocio (sección 3.6) | PASS | FluentValidation para entrada; Domain para reglas de negocio |
| Nomenclatura en español PascalCase (sección 4) | PASS | Consistente con el idioma del proyecto |
| Diagramas como código Mermaid (sección 5) | PASS | Se crearán diagramas en `docs/diagrams/` |
| Restricciones tecnológicas (sección 6) | PASS | ASP.NET Core MVC, EF Core, Identity, SQL Server/SQLite, FluentValidation |
| Invariantes universales (sección 7) | PASS | Saldo no negativo, fecha inicio ≤ fin, sin fechas pasadas, estado inicial Pending, transiciones válidas, inmutabilidad de estados finales, prohibición de auto-aprobación, trazabilidad obligatoria, cálculo en servidor |
| Seguridad (sección 8) | PASS | Roles por endpoint, ViewModels contra overposting, validación explícita FluentValidation, secretos fuera del repo, cabeceras de seguridad, rate limiting, casos de abuso documentados |
| Pirámide de pruebas (sección 9) | PASS | Unitarias (xUnit + Moq), Integración (xUnit + WebApplicationFactory), E2E (Playwright) |
| Meta de cobertura ≥ 80% en Domain y Application (sección 9.2) | PASS | Se planifica cobertura |
| Gate de CI obligatorio (sección 9.3) | PASS | Build, formato, analyzers, tests, cobertura, dependencias, diagramas |
| Objetivos de rendimiento (sección 10) | PASS | p95 documentado para cada operación |
| Clasificación y retención de datos (sección 11) | PASS | Datos sensibles (Motivo), retención 5 años |
| Gobernanza de cambios (sección 12) | PASS | Proceso de enmienda, versionado, excepciones documentadas |

### Resultado del Gate: **PASS**

No se detectan violaciones. La Constitución y la Spec están alineadas. El punto abierto sobre carry-over (`spec.md` RN-24 vs `docs/Preguntas_Pendientes.md` D.3) requiere resolución pero no bloquea el plan.

---

## 4. Entidades del Dominio

Las siguientes entidades emergen exclusivamente de los casos de uso definidos en `docs/use-cases.md` (CU-01 a CU-19) y las reglas de negocio de `spec.md`:

| Entidad | Casos de Uso | Responsabilidad |
|---|---|---|
| `Empleado` (Employee) | CU-01, CU-02, CU-04, CU-05, CU-06, CU-07, CU-10, CU-16, CU-18 | Representa un usuario del sistema con datos de empleo (fecha de ingreso, rol, activo/inactivo). Es el actor que crea solicitudes y sobre quien se verifican saldos y permisos. |
| `SolicitudVacaciones` (VacationRequest) | CU-04, CU-05, CU-06, CU-07, CU-10, CU-11, CU-12, CU-14, CU-15 | Entidad central que encapsula el ciclo de vida de una solicitud de vacaciones: fechas, motivo, estado (Pending/Approved/Rejected/Cancelled/Expired), aprobador que resolvió, comentario de rechazo. Contiene las reglas de negocio para transiciones de estado. |
| `SaldoEmpleado` (EmployeeBalance) | CU-01, CU-02, CU-03, CU-04, CU-11, CU-13, CU-14 | Gestiona el saldo acumulado (1 día/mes completo laborado), saldo consumido (días de solicitudes aprobadas) y saldo disponible. Garantiza el invariante de saldo no negativo. |
| `HistorialSolicitud` (VacationRequestHistory) | CU-05, CU-17 | Registro de auditoría inmutable para cada cambio de estado, edición o acción sobre una solicitud. Contiene: tipo de evento, actor, timestamp, valor anterior/nuevo. |
| `HistorialSaldo` (BalanceHistory) | CU-03, CU-17 | Registro de auditoría inmutable para cada movimiento de saldo (acumulación, descuento por aprobación, restauración por cancelación). |

### Value Objects

| Value Object | Uso |
|---|---|
| `RangoFechas` (DateRange) | Encapsula fecha inicio y fecha fin con validaciones (inicio ≤ fin, inicio futura). Utilizado por `SolicitudVacaciones`. |
| `DiasHabiles` (BusinessDays) | Valor calculado que representa días solicitados excluyendo sábados y domingos. |

### Enums

| Enum | Valores |
|---|---|
| `EstadoSolicitud` (VacationRequestStatus) | `Pending`, `Approved`, `Rejected`, `Cancelled`, `Expired` |
| `TipoMovimientoSaldo` (BalanceMovementType) | `Acumulacion`, `DescuentoPorAprobacion`, `RestauracionPorCancelacion` |
| `RolUsuario` (UserRole) | `Empleado`, `Aprobador`, `RRHH` |

Ninguna entidad contiene atributos de Entity Framework ni depende de Infrastructure.

---

## 5. Módulos del Sistema

### Módulo 1: `Vacations.Domain` — Reglas de negocio y entidades

**Responsabilidad:** Contiene las entidades, value objects, enums, excepciones de dominio e interfaces de abstracción (repositorios, `TimeProvider`). No tiene dependencias externas.

**Justificación:** Es el núcleo de Clean Architecture. Las reglas de negocio (transiciones de estado, validación de saldo, prevención de traslapes, anti-auto-aprobación) deben residir aquí sin depender de frameworks.

### Módulo 2: `Vacations.Application` — Casos de uso y orquestación

**Responsabilidad:** Implementa los casos de uso (commands y queries) que orquestan la interacción entre el Domain y la Infraestructura. Coordina validaciones, invoca reglas de dominio, y gestiona transacciones.

**Justificación:** Separa la lógica de orquestación (que pertenece a la aplicación) de las reglas de negocio puras (que pertenecen al dominio). Cada caso de uso de `docs/use-cases.md` se traduce en un command/query.

### Módulo 3: `Vacations.Infrastructure` — Persistencia, identidad y servicios externos

**Responsabilidad:** Implementa los repositorios definidos en Domain, el DbContext de EF Core, las configuraciones de entidad, los interceptores de auditoría, la integración con ASP.NET Core Identity, y servicios de background (auto-expiración).

**Justificación:** Aísla los detalles de infraestructura (ORM, base de datos, autenticación) para que Domain y Application permanezcan independientes del framework.

### Módulo 4: `Vacations.Web` — Presentación (ASP.NET Core MVC)

**Responsabilidad:** Controladores MVC delgados, ViewModels, vistas Razor, componentes de vista, autorización basada en políticas, archivos estáticos (CSS, JS).

**Justificación:** Separa la capa de presentación de la lógica de aplicación. Los controladores solo orquestan HTTP y delegan a Application.

### Módulo 5: `tests/` — Pruebas por capa

**Responsabilidad:** Pruebas unitarias (Domain, sin mocks), unitarias con mocks (Application), de integración (Infrastructure contra BD real), y de integración de sistema (Web con WebApplicationFactory).

**Justificación:** La Constitución (sección 9) exige pirámide de pruebas con cobertura ≥ 80% en Domain y Application.

---

## 6. Contrato de API

Cada endpoint responde a un caso de uso documentado en `docs/use-cases.md`:

| Método | Ruta | Caso de Uso | Descripción |
|---|---|---|---|
| `POST` | `/solicitudes-vacaciones` | CU-04 — Crear solicitud | Empleado crea una solicitud de vacaciones |
| `GET` | `/solicitudes-vacaciones` | CU-05 — Ver mis solicitudes | Empleado lista sus solicitudes paginadas |
| `GET` | `/solicitudes-vacaciones/{id}` | CU-05 — Ver detalle | Empleado ve detalle + historial de una solicitud |
| `PUT` | `/solicitudes-vacaciones/{id}` | CU-06 — Editar solicitud Pending | Empleado edita fechas o motivo |
| `POST` | `/solicitudes-vacaciones/{id}/cancelar` | CU-07 — Cancelar Pending | Empleado cancela solicitud pendiente |
| `GET` | `/saldo` | CU-02 — Consultar saldo | Empleado/HR consulta saldo e historial |
| `GET` | `/bandeja-aprobador` | CU-10 — Bandeja aprobador | Aprobador lista solicitudes Pending de todos los empleados |
| `GET` | `/bandeja-aprobador/{id}` | CU-13 — Ver impacto saldo | Aprobador ve detalle con saldo estimado |
| `POST` | `/bandeja-aprobador/{id}/aprobar` | CU-11 — Aprobar | Aprobador aprueba con descuento de saldo |
| `POST` | `/bandeja-aprobador/{id}/rechazar` | CU-12 — Rechazar | Aprobador rechaza con comentario obligatorio |
| `POST` | `/solicitudes-vacaciones/{id}/cancelar-aprobada` | CU-14 — Cancelar Approved | Aprobador cancela Approved antes del inicio |
| `GET` | `/rrhh/solicitudes` | CU-18 — Consultas RRHH | RRHH lista/filtra solicitudes de cualquier empleado |
| `GET` | `/rrhh/salarios/{empleadoId}` | CU-02/CU-18 — Saldo empleado | RRHH consulta saldo de un empleado específico |

No se proponen endpoints adicionales. Cada ruta tiene trazabilidad directa a un caso de uso del Spec.

---

## 7. Validación de Dependencias

```
Presentation (Vacations.Web)
    ↓ depende de
Application (Vacations.Application)
    ↓ depende de
Domain (Vacations.Domain)
    ↑ depende de
Infrastructure (Vacations.Infrastructure)
```

### Flujo de dependencias

- **Presentation → Application:** Los Controladores dependen de interfaces de Application (commands, queries). No conocen Domain directamente.
- **Application → Domain:** Los handlers de Application dependen de entidades del Domain, interfaces de repositorios, y abstracciones (`ITimeProvider`). No conocen Infrastructure.
- **Infrastructure → Application:** Infrastructure implementa las interfaces definidas en Application y Domain (repositorios, `ITimeProvider`). Infrastructure referencia Application para resolver las interfaces que implementa.
- **Infrastructure → Domain:** Infrastructure implementa las interfaces de repositorio definidas en Domain. El DbContext de EF Core mapea entidades de Domain.

### Verificación

- **Domain** no depende de ninguna capa externa. No contiene referencias a ASP.NET Core, EF Core, ni frameworks de terceros. **PASS**
- **Application** depende solo de Domain. **PASS**
- **Infrastructure** depende de Application y Domain. **PASS**
- **Presentation** depende de Application. **PASS**

No se detectan violaciones de dependencias.

---

## 8. Estructura del Proyecto

### Estado actual del repositorio

```
Solicitud_de_Vacaiones/               # Scaffold MVC vacío (net10.0)
├── Controllers/HomeController.cs
├── Models/ErrorViewModel.cs
├── Views/{Home,Shared}/
├── Program.cs                        # Solo AddControllersWithViews
├── Solicitud_de_Vacaiones.csproj     # Sin paquetes NuGet adicionales
└── appsettings.json

.specify/
└── memory/constitution.md

spec/
├── spec.md
├── DESIGN_TOKENS.md
├── 001-employee-balance-management/
├── 002-vacation-request-crud/
├── 003-approval-workflow/
├── 004-request-auto-expiration/
└── 005-hr-monitoring-dashboard/

docs/
├── Preguntas_Pendientes.md
├── use-cases.md
└── use-case-diagrams.md
```

### Estructura objetivo

```
src/
├── Vacations.Domain/                      # Capa de Dominio (nuevo)
│   ├── Entities/
│   │   ├── Empleado.cs
│   │   ├── SolicitudVacaciones.cs
│   │   ├── SaldoEmpleado.cs
│   │   ├── HistorialSolicitud.cs
│   │   └── HistorialSaldo.cs
│   ├── Enums/
│   │   ├── EstadoSolicitud.cs
│   │   ├── TipoMovimientoSaldo.cs
│   │   └── RolUsuario.cs
│   ├── ValueObjects/
│   │   └── RangoFechas.cs
│   ├── Exceptions/
│   │   ├── SaldoInsuficienteException.cs
│   │   ├── TraslapeSolicitudesException.cs
│   │   ├── AutoAprobacionNoPermitidaException.cs
│   │   └── TransicionEstadoInvalidaException.cs
│   └── Abstractions/
│       └── IRepositorioSolicitudVacaciones.cs
│       └── IRepositorioSaldoEmpleado.cs
│
├── Vacations.Application/                # Capa de Aplicación (nuevo)
│   ├── Solicitudes/
│   │   ├── Commands/
│   │   │   ├── CrearSolicitudCommand.cs
│   │   │   ├── EditarSolicitudCommand.cs
│   │   │   ├── CancelarSolicitudCommand.cs
│   │   │   ├── AprobarSolicitudCommand.cs
│   │   │   └── RechazarSolicitudCommand.cs
│   │   └── Queries/
│   │       ├── ObtenerMisSolicitudesQuery.cs
│   │       ├── ObtenerSolicitudDetalleQuery.cs
│   │       ├── ObtenerBandejaAprobadorQuery.cs
│   │       └── ObtenerHistorialRRHHQuery.cs
│   ├── Saldos/
│   │   ├── Commands/
│   │   │   ├── AcumularSaldoMensualCommand.cs
│   │   │   └── AjustarSaldoCommand.cs
│   │   └── Queries/
│   │       └── ObtenerSaldoQuery.cs
│   └── Expiracion/
│       └── Commands/
│           └── ExpiracionSolicitudesPendientesCommand.cs
│
├── Vacations.Infrastructure/             # Capa de Infraestructura (nuevo)
│   ├── Persistence/
│   │   ├── VacacionesDbContext.cs
│   │   ├── Configurations/
│   │   │   ├── SolicitudVacacionesConfiguration.cs
│   │   │   ├── SaldoEmpleadoConfiguration.cs
│   │   │   └── HistorialSolicitudConfiguration.cs
│   │   ├── Repositories/
│   │   │   ├── RepositorioSolicitudVacaciones.cs
│   │   │   └── RepositorioSaldoEmpleado.cs
│   │   └── Interceptors/
│   │       └── InterceptorAuditoriaSaveChanges.cs
│   ├── Identity/
│   │   └── UsuarioAplicacion.cs
│   ├── Time/
│   │   └── ProveedorTiempoSistema.cs
│   └── BackgroundServices/
│       └── ServicioExpiracionAutomatica.cs
│
└── Vacations.Web/                        # Capa de Presentación (nuevo, migrar scaffold)
    ├── Controllers/
    │   ├── SolicitudVacacionesController.cs
    │   ├── SaldoController.cs
    │   ├── BandejaAprobadorController.cs
    │   ├── RRHHController.cs
    │   └── CuentaController.cs
    ├── ViewModels/
    │   ├── CrearSolicitudViewModel.cs
    │   ├── EditarSolicitudViewModel.cs
    │   ├── ListaSolicitudesViewModel.cs
    │   ├── DetalleSolicitudViewModel.cs
    │   ├── BandejaAprobadorViewModel.cs
    │   └── ConsultaRRHHViewModel.cs
    ├── Views/
    │   ├── SolicitudVacaciones/
    │   ├── Saldo/
    │   ├── BandejaAprobador/
    │   ├── RRHH/
    │   ├── Cuenta/
    │   └── Shared/
    ├── Authorization/
    │   ├── PoliticasAutorizacion.cs
    │   └── RequisitoEsAprobadorActivo.cs
    ├── Program.cs                         # Modificado: Add capas, Identity, DbContext
    └── appsettings.json

tests/                                     # Proyectos de prueba (nuevos)
├── Vacations.Domain.Tests/
├── Vacations.Application.Tests/
├── Vacations.Infrastructure.Tests/
└── Vacations.Web.Tests/

docs/
└── diagrams/
    ├── use-cases.md
    ├── state-machine.md
    └── sequence-approval.md
```

### Módulos modificados

| Módulo | Acción | Característica(s) |
|---|---|---|
| `src/Vacations.Domain` | Crear | Todas (001-005) |
| `src/Vacations.Application` | Crear | Todas (001-005) |
| `src/Vacations.Infrastructure` | Crear | Todas (001-005) |
| `src/Vacations.Web` | Crear (migrar scaffold existente) | Todas (001-005) |
| `Solicitud_de_Vacaiones` (existente) | Migrar a `Vacations.Web` o eliminar | — |
| `docs/diagrams/` | Crear | — |
| `tests/` (4 proyectos) | Crear | — |

---

## 9. Decisión de la Estructura

La estructura de **monolito modular en 4 proyectos separados + proyectos de test independientes** es consistente con la Constitución por las siguientes razones:

1. **Clean Architecture explícita** (`constitution.md` sección 3): La separación en proyectos garantiza que el compilador verifique automáticamente la dirección de dependencias (Domain sin referencias a ASP.NET Core ni EF Core).

2. **Independencia del framework** (`constitution.md` sección 3.3): Domain y Application no deben contener referencias a frameworks externos. Proyectos separados previenen agregar accidentalmente paquetes como `Microsoft.AspNetCore.*` o `Microsoft.EntityFrameworkCore` en capas internas.

3. **Pirámide de pruebas** (`constitution.md` sección 9): Un proyecto de test por capa habilita la ejecución aislada de pruebas unitarias puras (Domain), unitarias con mocks (Application), de integración (Infrastructure) y de sistema (Web).

4. **Nomenclatura en español** (`constitution.md` sección 4): Los nombres de entidades, controladores, vistas y rutas siguen la convención de español PascalCase establecida.

5. **Monolito modular, no microservicios** (`constitution.md` sección 3): Se descartan microservicios por ser prematuros para el MVP.

6. **Razor Views, no SPA** (`constitution.md` sección 6.1): Se descartan React, Angular y Vue por prohibición expresa.

7. **El scaffold existente** (`Solicitud_de_Vacaiones/`) es un proyecto MVC vacío en .NET 10 que no cumple la separación de capas. Debe migrarse a `Vacations.Web` o eliminarse. **NEEDS CLARIFICATION** sobre la estrategia de migración concreta.

---

## 10. Seguimiento de la Complejidad

No existen excepciones arquitectónicas. La Constitución y la Spec están alineadas. Todos los principios se cumplen (PASS en todas las validaciones de la Sección 3).

### Complejidades técnicas identificadas

| Elemento | Tipo | Motivo | Justificación |
|---|---|---|---|
| `ServicioExpiracionAutomatica` | Nuevo BackgroundService | Feature 4 (`004-request-auto-expiration`) requiere un job programado diario que expire solicitudes `Pending` tras `[N]` días. | No existe servicio equivalente. Alternativa: job de BD (descartada por ser menos testeable). Se implementa como `BackgroundService` de ASP.NET Core. |
| `ProveedorTiempoSistema` (TimeProvider) | Nueva abstracción | La Constitución (sección 7 invariante 9) exige que el cálculo de días ocurra en el servidor. El Domain no debe depender de `DateTime.Now`. | Se usa `TimeProvider` de .NET (nativo desde .NET 8+). Alternativa: interfaz propia. Se opta por la nativa para reducir código custom. |
| `InterceptorAuditoriaSaveChanges` | Nuevo interceptor EF Core | La Spec (sección 8) y la Constitución (sección 7 invariante 8) exigen trazabilidad obligatoria en cada transición de estado. | Interceptor de `SaveChangesAsync` que registra automáticamente en `HistorialSolicitud` y `HistorialSaldo`. Alternativa: eventos manuales en cada handler (descartada por riesgo de olvido). |
| `RowVersion` para concurrencia optimista | Configuración EF Core | La Constitución (sección 7 invariante 1) exige que el saldo nunca sea negativo. Sin concurrencia, dos aprobaciones simultáneas podrían sobrescribir el saldo. | `RowVersion` en `SaldoEmpleado` y `SolicitudVacaciones`. Manejo de `DbUpdateConcurrencyException` en Application. |

---

## 11. Documentos posteriores requeridos

| Documento | Contenido | Prioridad |
|---|---|---|
| `design.md` | Diseño detallado: entidades, value objects, excepciones, interfaces de repositorios, handlers CQRS, configuraciones de EF Core, middleware de autorización, ViewModels | Alta |
| `docs/diagrams/state-machine.md` | Diagrama Mermaid de máquina de estados con 5 estados y transiciones válidas | Alta |
| `docs/diagrams/sequence-approval.md` | Diagrama Mermaid de secuencia del flujo de aprobación | Alta |
| `tasks.md` | Desglose de tareas prácticas con estimaciones, dependencias y criterios de aceptación | Media |
| `test-plan.md` | Estrategia de pruebas: casos de prueba por feature, escenarios de concurrencia y borde | Media |

---

## 12. Riesgos técnicos identificados

| Riesgo | Impacto | Probabilidad | Mitigación |
|---|---|---|---|
| Contradicción carry-over (spec vs Preguntas_Pendientes) | Medio: afecta cálculo de saldo anual | Alta | Resolver con PO antes de implementar `SaldoEmpleado` |
| Scaffold existente no cumple Clean Architecture | Medio: requiere refactorización de estructura | Alta | Migrar scaffold a `Vacations.Web` o crear desde cero. **NEEDS CLARIFICATION** |
| Condiciones de carrera en aprobación/cancelación concurrente | Alto: saldo negativo o doble descuento | Media | `RowVersion` + manejo de `DbUpdateConcurrencyException` en cada handler de aprobación |
| Cálculo de días hábiles sin feriados definidos | Medio: puede requerir cambios posteriores | Alta | Aislar lógica en método `CalcularDiasHabiles` con interfaz intercambiable |
| Auto-expiración con valor `[N]` sin definir | Bajo: el valor es configurable | Baja | Usar `IConfiguration` con valor por defecto (30 días sugerido) |
| Paginación offset-based con concurrencia extrema | Bajo: posibles duplicados/saltos | Baja | Documentado como known limitation aceptada por el PO |

---

## 13. Dependencias entre features

```
Feature 001 (Employee Balance)
  └── Es dependencia de: Features 002, 003

Feature 002 (Vacation Request CRUD)
  ├── Depende de: Feature 001 (validación de saldo)
  └── Es dependencia de: Features 003, 004, 005

Feature 003 (Approval Workflow)
  ├── Depende de: Feature 002 (solicitudes existentes)
  └── Depende de: Feature 001 (descuento/restauración de saldo)

Feature 004 (Auto-Expiration)
  └── Depende de: Feature 002 (solicitudes Pending)

Feature 005 (HR Monitoring Dashboard)
  ├── Depende de: Feature 002 (historial de solicitudes)
  ├── Depende de: Feature 001 (saldos)
  └── Depende de: Feature 003 (registros de aprobación)
```

**Orden de implementación:** 001 → 002 → 003 → 004 → 005

---

## 14. Puntos pendientes (NEEDS CLARIFICATION)

| # | Ítem | Impacto |
|---|---|---|
| 1 | Motor de base de datos concreto (LocalDB vs. SQLite) | Configuración de EF Core y scripts de seeding |
| 2 | Framework de pruebas concreto (xUnit, NUnit, MSTest) | Estructura de proyectos de test |
| 3 | Plataforma de despliegue objetivo | Configuración de HSTS, CORS, pipelines |
| 4 | Volumen de usuarios concurrentes esperado | Configuración de Rate Limiting y pooling |
| 5 | Estrategia de migración del scaffold existente | Estimación inicial del setup |
| 6 | Valor numérico de `[N]` para auto-expiración (RN-26) | Configuración por defecto del BackgroundService |
| 7 | **Contradicción carry-over:** `spec.md` RN-24 dice carry-over ilimitado; `docs/Preguntas_Pendientes.md` D.3 dice "Sin carry-over; caducan en aniversario" | Afecta lógica de `SaldoEmpleado` y acumulación anual |
| 8 | Horizonte futuro máximo para solicitar (RN-31) | Validación de fechas |
| 9 | Manejo de feriados en cálculo de días (RN-25) | Lógica de `CalcularDiasHabiles` |
| 10 | Nombre técnico del "estado/bloqueo" mencionado por el PO en A.3 (saldo comprometido) | Modelo de datos |