# Plan: Feature 003 — Approval Workflow

**Extraído de:** `specs/plan.md` (secciones relevantes a 003-approval-workflow)  
**Actualizado:** 2026-07-29  
**Estado:** MVP  

---

## Resumen Feature 003

**Objetivo:** Implementar el flujo de aprobación/rechazo de solicitudes pendientes por parte de aprobadores. Incluye validaciones de anti-auto-aprobación, aprobador activo, saldo disponible, y cancelación de solicitudes aprobadas antes del inicio del periodo (con restauración de saldo).

---

## Commands para Feature 003

- **`AprobarSolicitudCommand`**: Aprobador aprueba una solicitud Pending. Mueve días de `pendingBalance` a `consumedBalance`. Requiere validación de anti-auto-aprobación y aprobador activo.
- **`RechazarSolicitudCommand`**: Aprobador rechaza una solicitud Pending con comentario obligatorio (1-500 caracteres). Libera `pendingBalance`.
- **`CancelarAprobadaCommand`**: Aprobador cancela una solicitud Approved **antes** de que inicie el periodo de vacaciones. Restaura saldo (mueve de `consumedBalance` a disponible).

---

## Queries para Feature 003

- **`ObtenerBandejaAprobadorQuery`**: Lista solicitudes Pending para aprobadores con filtros opcionales, indicador de traslapes, saldo disponible del empleado, y paginación. Excluye solicitudes del propio aprobador.

---

## Reglas de Negocio (RN) para Feature 003

| ID | Regla | Descripción |
|----|-------|-------------|
| RN-03 | Descuento solo en aprobación | El saldo se actualiza únicamente cuando la solicitud pasa a "aprobada". Si una solicitud aprobada se cancela antes de que inicie el periodo, el saldo se restaura. |
| RN-04 | Restaurar saldo al cancelar solicitud aprobada | Si se cancela una solicitud aprobada **antes de que inicie el periodo de vacaciones**, los días se reintegran al saldo. Solo un aprobador puede cancelar solicitudes aprobadas. No se puede cancelar una solicitud aprobada una vez que haya iniciado el periodo (fecha de inicio ≤ hoy). |
| RN-08 | Advertencia de traslape en solicitudes pendientes/confirmadas | Señalar al aprobador si hay traslapes con otras solicitudes del mismo empleado. |
| RN-12 | Cambio de estado solo por aprobador (excepto cancelación/edición por empleado en Pending) | Solo un aprobador puede cambiar el estado de Pending a Approved o Rejected (excepción: empleado puede cambiar a Cancelled o editar si está en Pending). |
| RN-14 | Cualquier aprobador activo puede aprobar/rechazar cualquier solicitud (rol plano, sin asignación 1-a-1) | No hay asignación de solicitud a un aprobador específico; cualquier aprobador activo puede manejarla. |
| RN-16 | Comentario obligatorio en rechazo | El aprobador debe registrar un motivo al rechazar; visible para el empleado. |
| RN-32 | Aprobador no puede auto-aprobarse | Un aprobador que también sea empleado no puede aprobar sus propias solicitudes; debe resolverlas otro aprobador. |
| RN-33 | Aprobador inactivo bloqueado | Un usuario/aprobador inactivo no puede aprobar ni rechazar solicitudes. |

---

## Módulo Responsable

**`Vacations.Domain`** (métodos de transición de estado en `SolicitudVacaciones`, método `CancelarAprobada`, validaciones in-memory)  
**`Vacations.Application`** (handlers CQRS para aprobación, rechazo, cancelación de aprobadas)  
**`Vacations.Infrastructure`** (consultas optimizadas para bandeja, manejo de concurrencia)  
**`Vacations.Web`** (controller de aprobador, vistas de aprobación/rechazo)

---

## Flujo de Aprobación Detallado

1. **Aprobador accede a bandeja** → Query obtiene solicitudes Pending (excluye propias)
2. **Aprobador ve detalle** → Query muestra saldo actual, días solicitados, saldo estimado, traslapes
3. **Aprobador aprueba**:
   - Validación: no es autor, está activo, estado es Pending
   - Validación: saldo disponible actual es suficiente (puede haber cambiado)
   - Mueve `pendingBalance` → `consumedBalance`
   - Registra en historial con actor = email aprobador
   - Maneja `DbUpdateConcurrencyException` por conflicto
4. **Aprobador rechaza**:
   - Validación: no es autor, está activo, estado es Pending, comentario presente
   - Libera `pendingBalance`
   - Registra en historial con comentario
5. **Aprobador cancela aprobada**:
   - Validación: estado es Approved, fecha inicio > hoy
   - Restaura saldo (mueve de `consumedBalance` a disponible)
   - Libera `pendingBalance` (si la hubo)
   - Registra en historial

---

## Consideraciones de Implementación

- El `RowVersion` en `SolicitudVacaciones` y `SaldoEmpleado` previene doble aprobación simultánea.
- El manejo de `DbUpdateConcurrencyException` debe reintentar o informar al usuario.
- La validación de "aprobador activo" ocurre en el Domain (lanzando excepción) y se verifica contra `Empleado.EstaActivo`.
- Los traslapes se calculan de forma dinámica en la Query, incluyendo solicitudes Pending y Approved.
- El comentario de rechazo es obligatorio (no nulo) y visible en el detalle de solicitud del empleado.

---

**Fin de Feature 003 — Approval Workflow**
