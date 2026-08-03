# Plan: Feature 002 — Vacation Request CRUD

**Extraído de:** `specs/plan.md` (secciones relevantes a 002-vacation-request-crud)  
**Actualizado:** 2026-07-29  
**Estado:** MVP  

---

## Resumen Feature 002

**Objetivo:** Gestionar el ciclo completo de CRUD para solicitudes de vacaciones. Incluye creación, lectura, actualización y cancelación de solicitudes con validaciones de fecha, saldo, traslapes y transiciones de estado. El empleado puede editar y cancelar solicitudes pendientes; solo estados y auditoría están disponibles después.

---

## Entidades del Dominio para Feature 002

### `SolicitudVacaciones` *(VacationRequest)*

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

### `HistorialSolicitud` *(VacationRequestHistory)*

Registro de auditoría inmutable para cada acción sobre una solicitud. **Consolida** la entidad `ApprovalAction` definida en `specs/003-approval-workflow/spec.md`, evitando duplicidad de registro.

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

## Enums para Feature 002

| Enum | Valores |
|------|---------| 
| `EstadoSolicitud` *(VacationRequestStatus)* | `Pending`, `Approved`, `Rejected`, `Cancelled`, `Expired` |

---

## Value Objects para Feature 002

### `RangoFechas` *(DateRange)* — Compartido con Feature 001
Encapsula fecha inicio y fecha fin con validaciones: inicio ≤ fin, inicio ≥ mañana, fin ≤ inicio + 2 meses.

---

## Interfaces de Repositorio para Feature 002

### `IRepositorioSolicitudVacaciones`

Define las operaciones de acceso a datos para solicitudes:
- `ObtenerPorIdAsync(Guid id)` → `SolicitudVacaciones?`
- `ObtenerPorEmpleadoAsync(Guid empleadoId)` → `IReadOnlyList<SolicitudVacaciones>`
- `ObtenerPendientesAsync()` → `IReadOnlyList<SolicitudVacaciones>`
- `ExisteTraslapeAsync(Guid empleadoId, DateTime inicio, DateTime fin, Guid? excluirSolicitudId)` → `bool`
- `AgregarAsync(SolicitudVacaciones solicitud)` → `Task`
- `ActualizarAsync(SolicitudVacaciones solicitud)` → `Task`

---

## Commands para Feature 002

- **`CrearSolicitudCommand`**: Crea una nueva solicitud de vacaciones con validaciones de saldo, fechas y traslapes.
- **`EditarSolicitudCommand`**: Edita fechas, fin y motivo de una solicitud **en estado Pending** únicamente. Recalcula días y ajusta `pendingBalance`.
- **`CancelarSolicitudCommand`**: Empleado cancela su propia solicitud **en estado Pending**. Libera `pendingBalance`.

---

## Queries para Feature 002

- **`ObtenerMisSolicitudesQuery`**: Lista las solicitudes del empleado autenticado con paginación, filtros opcionales por estado. Respuesta paginada con offset-based.
- **`ObtenerSolicitudDetalleQuery`**: Obtiene el detalle de una solicitud incluyendo historial de eventos. Verifica permisos (propietario, aprobador, RRHH).

---

## Reglas de Negocio (RN) para Feature 002

| ID | Regla | Descripción |
|----|-------|-------------|
| RN-05 | No permitir solicitudes retroactivas | Fecha de inicio debe ser ≥ mañana (no se puede solicitar para hoy). |
| RN-06 | Fecha de fin no puede ser anterior a inicio | Validación y mensaje "La fecha de fin no puede ser anterior a la de inicio". |
| RN-07 | No traslapar con solicitudes aprobadas ni pendientes | Nuevas solicitudes no pueden incluir días ya comprometidos en otras solicitudes del mismo empleado en estado Approved o Pending. |
| RN-09 | Tipo de permiso | Solo existe un tipo — vacaciones. Se elimina catálogo múltiple; no hay permisos médicos, personales, luto ni "otro". |
| RN-10 | Descripción obligatoria | Campo motivo obligatorio (mín. 10 caracteres) al crear solicitud. |
| RN-11 | Solicitud nueva en estado "pendiente" | Toda solicitud se crea en estado `Pending`. |
| RN-13 | Estados finales no se pueden cambiar | Approved, Rejected, Cancelled, Expired son finales salvo cancelación de Approved antes del inicio del periodo por un aprobador. Una solicitud aprobada no puede tener cambios después de haber sido aprobada, a menos que sea un usuario aprobador quien la cancele y solo si el periodo de vacaciones no ha iniciado. |
| RN-20 | Edición de solicitudes PENDIENTES | Mientras una solicitud esté en estado PENDIENTE el empleado podrá modificar fecha inicio, fecha fin y motivo; después de APROBADA/RECHAZADA/CANCELADA/EXPIRED la solicitud no podrá editarse. Una solicitud aprobada **no puede tener cambios después de haber sido aprobada**, a menos que sea cancelada por un aprobador (y solo si el periodo no ha iniciado). Todas las ediciones deberán registrarse en auditoría de trazabilidad. |
| RN-21 | Solicitudes por días completos | La aplicación no soportará solicitudes por horas, medio día ni fracciones; todas las solicitudes serán por días completos. |
| RN-22 | RRHH sin permiso para crear/editar solicitudes | RRHH tendrá únicamente permisos de consulta. RRHH no podrá crear, modificar ni registrar solicitudes en nombre de terceros bajo ninguna circunstancia. |
| RN-25 | Cálculo de duración en días hábiles | Se excluyen sábados y domingos del cómputo. Feriados: abierto (pendiente de definición). |
| RN-35 | Cancelación parcial no aplica | El sistema no soporta cancelación parcial de solicitudes de vacaciones en esta versión del MVP. |

---

## Dependencias Externas para Feature 002

- Entity Framework Core (para persistencia)
- `RangoFechas` Value Object (Feature 001)
- `SaldoEmpleado` Entity (Feature 001, para validación de saldo)
- FluentValidation (para validación de entrada)

---

## Módulo Responsable

**`Vacations.Domain`** (entidades, value objects, excepciones)  
**`Vacations.Infrastructure`** (repositorios, DbContext, configuraciones, interceptor de auditoría para `HistorialSolicitud`)  
**`Vacations.Application`** (commands y queries de solicitudes)  
**`Vacations.Web`** (controllers y vistas de empleado)

---

## Consideraciones de Implementación

- El `RowVersion` en `SolicitudVacaciones` previene condiciones de carrera en aprobaciones/cancelaciones concurrentes.
- El `HistorialSolicitud` se registra automáticamente via interceptor de `SaveChangesAsync` de EF Core.
- El campo `CamposModificados` se almacena en JSON para trazabilidad granular de ediciones.
- La validación de traslapes ocurre en el Domain, no en la BD, mediante método `ExisteTraslapeAsync` del repositorio.

---

**Fin de Feature 002 — Vacation Request CRUD**
