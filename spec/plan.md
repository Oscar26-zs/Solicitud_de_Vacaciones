# Plan de Implementación: Sistema de Gestión de Solicitudes de Vacaciones (MVP)

**Branch:** `main` | **Fecha:** 2026-07-28 | **Spec:** `spec/spec.md`, `spec/001-*-005-*`, `docs/use-cases.md`

---

## 1. Resumen

**Objetivo principal:** Proveer un sistema web para gestionar el ciclo completo de solicitudes de **vacaciones** (único tipo de permiso en el MVP): creación por el empleado con validación de saldo y fechas, revisión y decisión por un **Aprobador** (rol plano, sin jerarquía), consulta de solo lectura por RRHH, auto-expiración de solicitudes `Pending` al alcanzar la fecha de inicio solicitada, y cancelación de solicitudes `Approved` antes del inicio del periodo (con restauración de saldo).

**Problema que resuelve:** Elimina la gestión manual y descentralizada de solicitudes de vacaciones. Automatiza validaciones (saldo, fechas, traslapes), el flujo de aprobación/rechazo con comentario obligatorio al rechazar, la expiración automática de pendientes sin resolver, y la consulta histórica filtrada por RRHH.

**Estrategia técnica general:** Monolito modular en ASP.NET Core MVC (Razor Views) con Clean Architecture en 4 capas (Domain, Application, Infrastructure, Presentation), Entity Framework Core como ORM, ASP.NET Core Identity para autenticación, y `TimeProvider` de .NET para abstracción del tiempo.

---

## 2. Contexto Técnico

| Atributo | Valor | Fuente |
|---|---|---|
| Lenguaje / Versión | C# sobre **.NET 10** (`net10.0`) | `Solicitud_de_Vacaiones.csproj` |
| Framework principal | ASP.NET Core MVC con Razor Views | `constitution.md` sección 6 |
| Almacenamiento | **SQL Server**  | `constitution.md` sección 6 |
| ORM | Entity Framework Core | `constitution.md` sección 6 |
| Autenticación | ASP.NET Core Identity Framework | `spec.md` sección 9 |
| Testing | **xUnit**  | `constitution.md` sección 9 |
| Plataforma objetivo | Aplicación web servida por Kestrel. **MVP sin despliegue — solo entorno local/desarrollo**  | — |
| Tipo de proyecto | Monolito modular web | `constitution.md` sección 3 |
| Objetivos de rendimiento | Consulta de saldo ≤ 300ms p95; creación/aprobación ≤ 1s p95; listados paginados ≤ 2s p95 | `constitution.md` sección 10 |
| Restricciones técnicas | Prohibido `DateTime.Now`/`DateTime.UtcNow` en Domain/Application. Prohibido `DELETE` físico. Sin dependencias externas (Redis, RabbitMQ, APIs). Prohibidos frameworks SPA (React, Angular, Vue). Auditoría automática vía interceptor de EF Core. Concurrencia optimista con `RowVersion`. FluentValidation aprobado para validación de entrada. Nombre en español (PascalCase). | `constitution.md` secciones 4, 6, 7, 8 |
| Escala / Alcance | 3 roles (Empleado, Aprobador, RRHH). 47 requisitos funcionales (RF-001 a RF-047). 36 reglas de negocio (RN-01 a RN-36). **Usuarios concurrentes esperados: 50-100** (supuesto para MVP interno). Número de empleados/solicitudes: depende del tamaño de la organización (se asume ≤ 500 empleados para el MVP). | `spec.md` |

### Dependencias principales identificadas

- Entity Framework Core
- ASP.NET Core Identity
- FluentValidation (validación de entrada, ejecución explícita vía `ValidateAsync` — prohibido pipeline auto-validation)
- Middleware de Rate Limiting nativo de .NET (límites sugeridos para 50-100 usuarios concurrentes: auth 10/min por usuario, escritura 30/min por usuario [crear/editar/aprobar], lectura 120/min por usuario [consultas/listados])
- `TimeProvider` (abstracción nativa de .NET)

### Decisiones del PO aplicadas al contexto técnico

| # | Ítem | Decisión | Estado |
|---|------|----------|--------|
| 1 | Motor de base de datos | **SQL Server** (no LocalDB ni SQLite) | ✅ Resuelto |
| 2 | Framework de pruebas | **xUnit** | ✅ Resuelto |
| 3 | Plataforma de despliegue | **Sin despliegue en MVP** — solo local/desarrollo | ✅ Resuelto |
| 4 | Usuarios concurrentes | **50-100 usuarios concurrentes** (supuesto para MVP interno) | ✅ Resuelto |
| 5 | RN-26 — Auto-expiración | La solicitud `Pending` expira cuando se alcanza su fecha de inicio (fecha inicio ≤ hoy). **No es un N fijo.** | ✅ Resuelto |
| 6 | RN-24 — Carry-over | **SÍ hay carry-over, SIN LÍMITE de acumulación.** Acumulación indefinida entre periodos. | ✅ Resuelto |
| 7 | RN-31 — Horizonte futuro | **2 meses** desde la fecha actual | ✅ Resuelto |
| 8 | RN-25 — Feriados | **Los feriados NO se excluyen del cómputo de días hábiles.** Solo se excluyen sábados y domingos. Un feriado en medio del rango cuenta normalmente para el saldo consumido. | ✅ Resuelto |
| 9 | Estrategia de paginación | Offset-based según `docs/Preguntas_Pendientes.md` H.1 | — |
| 10 | Saldo comprometido (A.3) | **SÍ se implementa.** Las solicitudes Pending congelan su saldo (`pendingBalance`). `availableBalance = accumulatedBalance - consumedBalance - pendingBalance`. Se valida contra disponible al crear. | ✅ Resuelto |

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
| Seguridad (sección 8) | PASS | Roles por endpoint, ViewModels contra overposting, validación explícita FluentValidation vía `ValidateAsync` (no auto-validation pipeline), secretos fuera del repo, cabeceras de seguridad (CSP, HSTS, X-Content-Type-Options, X-Frame-Options), rate limiting por tipo de endpoint (auth 5/min, escritura moderado, lectura amplio), casos de abuso documentados |
| Pirámide de pruebas (sección 9) | PASS | Unitarias (xUnit + Moq), Integración (xUnit + WebApplicationFactory), E2E (Playwright) |
| Meta de cobertura ≥ 80% en Domain y Application (sección 9.2) | PASS | Se planifica cobertura |
| Gate de CI obligatorio (sección 9.3) | PASS | Build, formato, analyzers, tests, cobertura, dependencias, diagramas |
| Objetivos de rendimiento (sección 10) | PASS | p95 documentado para cada operación |
| Clasificación y retención de datos (sección 11) | PASS | Datos sensibles (Motivo), retención 5 años |
| Gobernanza de cambios (sección 12) | PASS | Proceso de enmienda, versionado, excepciones documentadas |

---

## 4. Entidades del Dominio

Las siguientes entidades emergen exclusivamente de los casos de uso definidos en `docs/use-cases.md` (CU-01 a CU-19) y las reglas de negocio de `spec.md`:

---

#### `Empleado` *(Employee)*

Representa un usuario del sistema. Es el actor que crea solicitudes y sobre quien se verifican saldos y permisos. Sus roles (Empleado, Aprobador, RRHH) se gestionan a través de ASP.NET Core Identity, no como campo directo de la entidad.

| Atributo | Tipo | Descripción |
|----------|------|-------------|
| `id` | `Guid` | Identificador único |
| `email` | `string` | Correo electrónico (único) |
| `fullName` | `string` | Nombre completo |
| `joinDate` | `DateTime` | Fecha de ingreso a la empresa. Determina la acumulación mensual de saldo. |
| `isActive` | `bool` | Indica si el empleado está activo. Empleados inactivos no pueden crear solicitudes; aprobadores inactivos no pueden aprobar. |

---

#### `SolicitudVacaciones` *(VacationRequest)*

Entidad central que encapsula el ciclo de vida completo de una solicitud de vacaciones. Contiene las reglas de negocio para transiciones de estado y validaciones de fechas.

| Atributo | Tipo | Descripción |
|----------|------|-------------|
| `id` | `Guid` | Identificador único |
| `employeeId` | `Guid` | FK → `Empleado` |
| `startDate` | `DateTime` | Fecha de inicio del periodo solicitado |
| `endDate` | `DateTime` | Fecha de fin del periodo solicitado |
| `daysRequested` | `int` | Días hábiles calculados (excluye sábados y domingos; los feriados sí cuentan para el consumo de saldo) |
| `status` | `EstadoSolicitud` | Estado actual: `Pending`, `Approved`, `Rejected`, `Cancelled`, `Expired` |
| `reason` | `string` | Motivo de la solicitud (mín. 10 caracteres) |
| `approverComment` | `string?` | Comentario del aprobador al rechazar (obligatorio en rechazo, máx. 500 caracteres) |
| `resolvedBy` | `Guid?` | Id del aprobador que aprobó/rechazó/canceló la solicitud |
| `createdAt` | `DateTime` | Timestamp de creación |
| `updatedAt` | `DateTime` | Timestamp de última modificación |
| `rowVersion` | `byte[]` | Versión de fila para concurrencia optimista. Impide doble aprobación simultánea. |

---

#### `SaldoEmpleado` *(EmployeeBalance)*

Gestiona los días de vacaciones del empleado. Incluye `pendingBalance` que congela los días de solicitudes Pending, impidiendo que se comprometan más días de los realmente disponibles. `availableBalance = accumulatedBalance - consumedBalance - pendingBalance`. Garantiza el invariante de saldo no negativo mediante concurrencia optimista.

| Atributo | Tipo | Descripción |
|----------|------|-------------|
| `id` | `Guid` | Identificador único |
| `employeeId` | `Guid` | FK → `Empleado` (1:1) |
| `accumulatedBalance` | `int` | Saldo acumulado (1 día por mes completo laborado). **Incluye carry-over ilimitado** — los días no usados se acumulan entre periodos sin tope. |
| `consumedBalance` | `int` | Días consumidos por solicitudes aprobadas |
| `pendingBalance` | `int` | Días comprometidos por solicitudes en estado `Pending` (congelados). Se libera al aprobar/rechazar/cancelar/expirar. |
| `availableBalance` | `int` | **Propiedad calculada** (`accumulatedBalance - consumedBalance - pendingBalance`). No se persiste. |
| `lastUpdatedAt` | `DateTime` | Timestamp del último cambio |
| `rowVersion` | `byte[]` | Versión de fila para concurrencia optimista. Evita saldos negativos por aprobaciones concurrentes. |

---

#### `HistorialSolicitud` *(VacationRequestHistory)*

Registro de auditoría inmutable para cada acción sobre una solicitud. **Consolida** la entidad `ApprovalAction` definida en spec/003, evitando duplicidad de registro.

| Atributo | Tipo | Descripción |
|----------|------|-------------|
| `id` | `Guid` | Identificador único |
| `requestId` | `Guid` | FK → `SolicitudVacaciones` |
| `eventType` | `string` | Tipo de evento: `CREATED`, `UPDATED`, `STATUS_CHANGED`, `CANCELLED` |
| `previousStatus` | `EstadoSolicitud?` | Estado anterior (útil en transiciones) |
| `newStatus` | `EstadoSolicitud?` | Nuevo estado (útil en transiciones) |
| `changedFields` | `string?` | JSON con campos modificados en ediciones (`{"campo": {"old": "...", "new": "..."}}`) |
| `actor` | `string` | Quién realizó la acción (email del usuario o `SISTEMA_AUTO_EXPIRACION`) |
| `timestamp` | `DateTime` | Momento del evento |
| `comment` | `string?` | Comentario adicional (motivo de rechazo, nota de expiración, etc.) |

---

#### `HistorialSaldo` *(BalanceHistory)* — 🔶 Fuera de alcance MVP (futura fase)

> Esta entidad está definida pero **no se implementa en el MVP**. Queda disponible para una fase futura donde se requiera auditoría granular de movimientos de saldo.

Registro de auditoría inmutable para cada movimiento de saldo. Insert-only.

| Atributo | Tipo | Descripción |
|----------|------|-------------|
| `id` | `Guid` | Identificador único |
| `employeeId` | `Guid` | FK → `Empleado` |
| `movementType` | `TipoMovimientoSaldo` | `Acumulacion`, `CongelamientoPorCreacion`, `DescuentoPorAprobacion`, `RestauracionPorCancelacion`, `RestauracionPorRechazo`, `RestauracionPorExpiracion` |
| `previousBalance` | `int` | Saldo disponible antes del movimiento |
| `newBalance` | `int` | Saldo disponible después del movimiento |
| `reason` | `string` | Motivo del cambio (ej. "Approved request #123") |
| `actor` | `string` | Quién generó el movimiento (email o `SISTEMA_ACUMULACION`) |
| `timestamp` | `DateTime` | Momento del movimiento |

---

### Value Objects

#### `RangoFechas` *(DateRange)*
Encapsula fecha inicio y fecha fin con validaciones: inicio ≤ fin, inicio ≥ mañana, fin ≤ inicio + 2 meses. Invariante: el rango nunca puede ser inválido.

#### `DiasHabiles` *(BusinessDays)*
Valor calculado que representa días solicitados excluyendo sábados, domingos y feriados. Los feriados se definirán como una lista de fechas configurables (por año) en Infrastructure. El cálculo es un invariante de dominio — se implementa como lógica pura (sin dependencia externa).

---

### Enums

| Enum | Valores |
|------|---------|
| `EstadoSolicitud` *(VacationRequestStatus)* | `Pending`, `Approved`, `Rejected`, `Cancelled`, `Expired` |
| `TipoMovimientoSaldo` *(BalanceMovementType)* 🔶 Fuera de alcance MVP | `Acumulacion`, `CongelamientoPorCreacion`, `DescuentoPorAprobacion`, `RestauracionPorCancelacion`, `RestauracionPorRechazo`, `RestauracionPorExpiracion` |
| `RolUsuario` *(UserRole)* | `Empleado`, `Aprobador`, `RRHH` |

> **Regla:** Ninguna entidad contiene atributos de Entity Framework ni depende de Infrastructure. Anotaciones como `rowVersion` (mapeado a `byte[]` por EF) se configuran exclusivamente en la capa de Infrastructure.

---

### 4.1 Validación de Entidades vs. Especificaciones

| Entidad | Atributos en sub-spec | Plan alineado | Mejora aplicada |
|---------|----------------------|:------------:|-----------------|
| `Empleado` | `id`, `email`, `fullName`, `status`, `joinDate` *(001)* | ✅ | Sin cambios. Roles por Identity. |
| `SaldoEmpleado` | `id`, `employeeId`, `accumulatedBalance`, `consumedBalance`, `availableBalance`, `lastUpdatedAt` *(001)* | ✅ | `availableBalance` → propiedad calculada (`accumulated - consumed - pending`). `pendingBalance` añadido. `rowVersion` añadido. |
| `HistorialSaldo` | `id`, `employeeId`, `movementType`, `previousBalance`, `newBalance`, `reason`, `actor`, `timestamp` *(001)* | 🔶 Fuera de alcance MVP | Definida pero no se implementa en el MVP (futura fase). |
| `SolicitudVacaciones` | `id`, `employeeId`, `startDate`, `endDate`, `daysRequested`, `status`, `reason`, `approverComment`, `createdAt`, `updatedAt` *(002)* | ✅ | Añadidos `resolvedBy` y `rowVersion`. |
| `HistorialSolicitud` | `id`, `requestId`, `eventType`, `actor`, `note`, `timestamp` *(002)* | ⚠️ Mejorado | spec/002 usa `note` genérico. Plan añade `previousStatus`/`newStatus` + `changedFields` (JSON) para trazabilidad granular. |
| `ApprovalAction` | `id`, `requestId`, `approverId`, `action`, `comment`, `timestamp` *(003)* | ⚠️ Consolidado | spec/003 define entidad separada. El plan la consolida dentro de `HistorialSolicitud`, evitando duplicidad de auditoría. |

**Resultado:** 5/5 entidades cubiertas en especificación. `HistorialSaldo` 🔶 fuera de alcance MVP (futura fase). `ApprovalAction` integrada en `HistorialSolicitud`. Mejoras añadidas: `rowVersion` (×2), `resolvedBy`, trazabilidad granular.

---

## 5. Módulos del Sistema

### Módulo 1: `Vacations.Domain` — Reglas de negocio y entidades

**Responsabilidad:** Contiene las entidades, value objects, enums, excepciones de dominio e interfaces de abstracción (repositorios, `TimeProvider`). No tiene dependencias externas.

**Justificación:** Es el núcleo de Clean Architecture. Las reglas de negocio (transiciones de estado, validación de saldo contra `availableBalance` incluyendo `pendingBalance`, prevención de traslapes, anti-auto-aprobación) deben residir aquí sin depender de frameworks.

**Flujo de `pendingBalance`:**
1. **Crear solicitud Pending** → incrementa `pendingBalance` en `daysRequested` (se descuenta de `availableBalance`).
2. **Aprobar solicitud** → mueve `pendingBalance → consumedBalance` (mismo `daysRequested`).
3. **Rechazar/Cancelar/Expirar solicitud** → decrementa `pendingBalance` (se libera el saldo congelado).

### Módulo 2: `Vacations.Application` — Casos de uso y orquestación

**Responsabilidad:** Implementa los casos de uso (commands y queries) que orquestan la interacción entre el Domain y la Infraestructura. Coordina validaciones, invoca reglas de dominio, y gestiona transacciones.

**Justificación:** Separa la lógica de orquestación (que pertenece a la aplicación) de las reglas de negocio puras (que pertenecen al dominio). Cada caso de uso de `docs/use-cases.md` se traduce en un command/query.

### Módulo 3: `Vacations.Infrastructure` — Persistencia, identidad y servicios externos

**Responsabilidad:** Implementa los repositorios definidos en Domain, el DbContext de EF Core (provider SQL Server), las configuraciones de entidad, el interceptor de auditoría de solicitudes (solo `HistorialSolicitud`), la integración con ASP.NET Core Identity, y servicios de background (auto-expiración).

**Justificación:** Aísla los detalles de infraestructura (ORM, base de datos, autenticación) para que Domain y Application permanezcan independientes del framework.

### Módulo 4: `Vacations.Web` — Presentación (ASP.NET Core MVC)

**Responsabilidad:** Controladores MVC delgados, ViewModels, vistas Razor, componentes de vista, autorización basada en políticas, archivos estáticos (CSS, JS).

**Justificación:** Separa la capa de presentación de la lógica de aplicación. Los controladores solo orquestan HTTP y delegan a Application.

### Módulo 5: `tests/` — Pruebas por capa

**Responsabilidad:** Pruebas unitarias (Domain, sin mocks), unitarias con mocks (Application), de integración (Infrastructure contra BD real — SQL Server LocalDB o Testcontainers), y de integración de sistema (Web con WebApplicationFactory). Framework: **xUnit**.

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
│   │   └── HistorialSaldo.cs                   # 🔶 Fuera de alcance MVP (futura fase)
│   ├── Enums/
│   │   ├── EstadoSolicitud.cs
│   │   ├── TipoMovimientoSaldo.cs               # 🔶 Fuera de alcance MVP (futura fase)
│   │   └── RolUsuario.cs
│   ├── ValueObjects/
│   │   └── RangoFechas.cs
│   ├── Exceptions/
│   │   ├── SaldoInsuficienteException.cs
│   │   ├── TraslapeSolicitudesException.cs
│   │   ├── AutoAprobacionNoPermitidaException.cs
│   │   └── TransicionEstadoInvalidaException.cs
│   └── Abstractions/
│       ├── IRepositorioSolicitudVacaciones.cs
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
│   ├── BackgroundServices/
│   │   └── ServicioExpiracionAutomatica.cs
│   └── Services/
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

tests/                                     # Proyectos de prueba xUnit (nuevos)
├── Vacations.Domain.Tests/                # Unitarias puras (sin mocks)
├── Vacations.Application.Tests/           # Unitarias con mocks
├── Vacations.Infrastructure.Tests/        # Integración contra BD real
└── Vacations.Web.Tests/                   # Integración de sistema WebApplicationFactory

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
| `src/Vacations.Infrastructure` | Crear | Todas (001-005). Provider SQL Server + proveedor de feriados |
| `src/Vacations.Web` | Crear (migrar scaffold existente) | Todas (001-005) |
| `Solicitud_de_Vacaiones` (existente) | **Eliminar** (reescritura directa — no migrar). Los 4 proyectos se crean desde cero. | — |
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

7. **El scaffold existente** (`Solicitud_de_Vacaiones/`) es un proyecto MVC vacío en .NET 10 que no cumple la separación de capas. **Estrategia: reescritura directa.** Se crearán los 4 proyectos desde cero (`Vacations.Domain`, `Application`, `Infrastructure`, `Web`) y se eliminará el scaffold original. Esto evita arrastrar deuda técnica y es más rápido que una migración incremental. El único código aprovechable del scaffold es `Program.cs` (configuración base MVC) que sirve como referencia para `Vacations.Web/Program.cs`.

---

## 10. Seguimiento de la Complejidad

No existen excepciones arquitectónicas. La Constitución y la Spec están alineadas. Todos los principios se cumplen (PASS en todas las validaciones de la Sección 3).

### Complejidades técnicas identificadas

| Elemento | Tipo | Motivo | Justificación |
|---|---|---|---|
| `ServicioExpiracionAutomatica` | Nuevo BackgroundService | Feature 4 (`004-request-auto-expiration`) requiere un job programado diario que expire solicitudes `Pending` cuya fecha de inicio ya haya sido alcanzada (startDate ≤ hoy). | No existe servicio equivalente. Alternativa: job de BD (descartada por ser menos testeable). Se implementa como `BackgroundService` de ASP.NET Core. La lógica de expiración es dinámica: compara `startDate` contra la fecha actual provista por `TimeProvider`. |
| `ProveedorTiempoSistema` (TimeProvider) | Nueva abstracción | La Constitución (sección 7 invariante 9) exige que el cálculo de días ocurra en el servidor. El Domain no debe depender de `DateTime.Now`. | Se usa `TimeProvider` de .NET (nativo desde .NET 8+). Alternativa: interfaz propia. Se opta por la nativa para reducir código custom. |
| `InterceptorAuditoriaSaveChanges` | Nuevo interceptor EF Core | La Spec (sección 8) y la Constitución (sección 7 invariante 8) exigen trazabilidad obligatoria en cada transición de estado. | Interceptor de `SaveChangesAsync` que registra automáticamente en `HistorialSolicitud` (solo cambios de estado de solicitudes). `HistorialSaldo` queda fuera de alcance MVP (futura fase). Alternativa: eventos manuales en cada handler (descartada por riesgo de olvido). |
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
| Scaffold existente no cumple Clean Architecture | Medio: requiere refactorización de estructura | Alta | **Estrategia: reescritura directa.** Se crean los 4 proyectos desde cero (`Vacations.Domain`, `Application`, `Infrastructure`, `Web`) y se migra el código del scaffold (`Solicitud_de_Vacaiones/`) a `Vacations.Web`. El scaffold original se elimina. Esto evita arrastrar deuda técnica. |
| Condiciones de carrera en aprobación/cancelación concurrente | Alto: saldo negativo o doble descuento | Media | `RowVersion` + manejo de `DbUpdateConcurrencyException` en cada handler de aprobación |
| Cálculo de días hábiles sin repositorio de feriados | Bajo: solo sábados y domingos, lógica simple | Baja | Se implementa como método puro en Domain. Sin dependencias externas. |
| Auto-expiración: lógica dinámica contra fecha de inicio | Bajo: reemplaza la lógica de N fijo | Baja | El `BackgroundService` compara `startDate` ≤ hoy usando `TimeProvider`. Sin valor configurable `[N]`. |
| Paginación offset-based con concurrencia extrema | Bajo: posibles duplicados/saltos | Baja | Documentado como known limitation aceptada por el PO |
| Dependencia de SQL Server en entorno local | Bajo: los desarrolladores deben tener SQL Server instalado o usar LocalDB | Media | Documentar prerequisito; usar LocalDB como alternativa de desarrollo ligera. |

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