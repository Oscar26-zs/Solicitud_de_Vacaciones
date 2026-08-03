# Plan: Feature 001 — Employee Balance Management

**Extraído de:** `specs/plan.md` (secciones relevantes a 001-employee-balance-management)  
**Actualizado:** 2026-07-29  
**Estado:** MVP  

---

## Resumen Feature 001

**Objetivo:** Gestionar el ciclo de vida de empleados y sus saldos de días de vacaciones. Incluye acumulación automática de saldo (1 día por mes completo laborado), consulta de saldo disponible, y cálculo de saldo consumido sin creación manual de empleados (solo por seed).

---

## Entidades del Dominio para Feature 001

### `Empleado` *(Employee)*

Representa un usuario del sistema. Es el actor que crea solicitudes y sobre quien se verifican saldos y permisos. Sus roles (Empleado, Aprobador, RRHH) se gestionan a través de ASP.NET Core Identity, no como campo directo de la entidad.

| Atributo | Tipo | Descripción |
|----------|------|-------------|
| `id` | `Guid` | Identificador único |
| `email` | `string` | Correo electrónico (único) |
| `fullName` | `string` | Nombre completo |
| `joinDate` | `DateTime` | Fecha de ingreso a la empresa. Determina la acumulación mensual de saldo. |
| `isActive` | `bool` | Indica si el empleado está activo. Empleados inactivos no pueden crear solicitudes; aprobadores inactivos no pueden aprobar. |

---

### `SaldoEmpleado` *(EmployeeBalance)*

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

## Value Objects para Feature 001

### `RangoFechas` *(DateRange)*
Encapsula fecha inicio y fecha fin con validaciones: inicio ≤ fin, inicio ≥ mañana, fin ≤ inicio + 2 meses. Invariante: el rango nunca puede ser inválido. Incluye método `CalcularDiasHabiles()` que excluye sábados y domingos.

---

## Enums para Feature 001

| Enum | Valores |
|------|---------| 
| `RolUsuario` *(UserRole)* | `Empleado`, `Aprobador`, `RRHH` |

---

## Interfaces de Repositorio para Feature 001

### `IRepositorioSaldoEmpleado`

Define las operaciones de acceso a datos para saldos:
- `ObtenerPorEmpleadoIdAsync(Guid empleadoId)` → `SaldoEmpleado?`
- `AgregarAsync(SaldoEmpleado saldo)` → `Task`
- `ActualizarAsync(SaldoEmpleado saldo)` → `Task`

### `IRepositorioEmpleado`

Define las operaciones de acceso a datos para empleados:
- `ObtenerPorIdAsync(Guid id)` → `Empleado?`
- `ObtenerActivosAsync()` → `IReadOnlyList<Empleado>`
- `ExisteConEmailAsync(string email)` → `bool`

---

## Queries Compartidas para Feature 001

### `ObtenerSaldoQuery`
Query para consultar saldo de un empleado. Empleado puede consultar su propio saldo; RRHH puede consultar saldo de cualquier empleado. Retorna: Acumulado, Consumido, Pendiente, Disponible. Respuesta en ≤300ms p95.

---

## Reglas de Negocio (RN) para Feature 001

| ID | Regla | Descripción |
|----|-------|-------------|
| RN-01 | Saldo anual disponible | Cada empleado se carga mediante seed inicial (sin creación en el sistema). El saldo se acumula a razón de **1 día por cada mes completo laborado** desde la fecha de ingreso. El valor "días por mes laborado" es configurable (por defecto 1 día/mes). Los empleados inician con saldo 0 en el seed, el cual se acumula automáticamente tras cada mes laborado. |
| RN-02 | No solicitar más días que los disponibles | El sistema impide crear solicitud que exceda saldo y muestra "Saldo insuficiente para esta solicitud". |
| RN-03 | Descuento solo en aprobación | El saldo se actualiza únicamente cuando la solicitud pasa a "aprobada". Si una solicitud aprobada se cancela antes de que inicie el periodo, el saldo se restaura. |
| RN-04 | Restaurar saldo al cancelar solicitud aprobada | Si se cancela una solicitud aprobada **antes de que inicie el periodo de vacaciones**, los días se reintegran al saldo. Solo un aprobador puede cancelar solicitudes aprobadas. No se puede cancelar una solicitud aprobada una vez que haya iniciado el periodo (fecha de inicio ≤ hoy). |
| RN-23 | Cálculo de duración en días hábiles (Feature 002 associate) | Se excluyen sábados y domingos del cómputo. Feriados: abierto (pendiente de definición). |
| RN-24 | Carry-over ilimitado | Los días no usados se acumulan de un periodo a otro (no se pierden al cierre de año). Tope máximo de acumulación: **abierto (sin límite)**. |
| RN-27 | Zona horaria única | Todos los empleados operan en la misma zona horaria corporativa. No se soportan zonas horarias distintas. |
| RN-32 | Aprobador no puede auto-aprobarse | Un aprobador que también sea empleado no puede aprobar sus propias solicitudes; debe resolverlas otro aprobador. |
| RN-33 | Aprobador inactivo bloqueado | Un usuario/aprobador inactivo no puede aprobar ninguna solicitud. |
| RN-36 | Offboarding no aplica | La gestión de offboarding de empleados no se incluye en esta versión; el estado activo/inactivo del usuario es suficiente para controlar accesos. |

---

## Dependencias Externas para Feature 001

- Entity Framework Core (para persistencia)
- ASP.NET Core Identity Framework (para gestión de usuarios y roles)
- `TimeProvider` de .NET (para abstracción del tiempo)

---

## Módulo Responsable

**`Vacations.Domain`** (entidades, value objects, excepciones)  
**`Vacations.Infrastructure`** (repositorios, DbContext, configuraciones)  
**`Vacations.Application`** (queries de saldo, commands de acumulación)  
**`Vacations.Web`** (controllers y vistas de saldo)

---

## Decisiones Técnicas Aplicadas a Feature 001

| # | Ítem | Decisión | Estado |
|---|------|----------|--------|
| 1 | Motor de base de datos | **SQL Server** (no LocalDB ni SQLite) | ✅ Resuelto |
| 2 | Creación de empleados | **Seed initializer** (no endpoint de creación en MVP) | ✅ Resuelto |
| 6 | RN-24 — Carry-over | **SÍ hay carry-over, SIN LÍMITE de acumulación.** Acumulación indefinida entre periodos. | ✅ Resuelto |
| 10 | Saldo comprometido (A.3) | **SÍ se implementa.** Las solicitudes Pending congelan su saldo (`pendingBalance`). `availableBalance = accumulatedBalance - consumedBalance - pendingBalance`. Se valida contra disponible al crear. | ✅ Resuelto |

---

## Consideraciones de Implementación

- El `SaldoEmpleado` incluye `rowVersion` para concurrencia optimista, evitando saldos negativos por aprobaciones simultáneas.
- La acumulación mensual se ejecuta via comando `AcumularSaldoMensualCommand`, procesando solo empleados activos.
- El valor de `pendingBalance` se congela al crear solicitud y se libera al aprobar/rechazar/cancelar/expirar.
- No se implementa `HistorialSaldo` en el MVP (fuera de alcance); queda para fase futura.

---

**Fin de Feature 001 — Employee Balance Management**
