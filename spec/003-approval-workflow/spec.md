# Feature 3: Flujo de Aprobación

**Versión**: 1.0  
**Última actualización**: 2026-07-17

---

## Resumen

Este feature define las responsabilidades y reglas del proceso de aprobación de solicitudes de vacaciones: bandeja de aprobadores, opciones para aprobar/rechazar con comentarios, restricciones (no auto-aprobación), y reglas de restauración de saldo. Está pensado para trabajar sobre las solicitudes creadas por Feature 2 y los saldos gestionados por Feature 1.

---

## Alcance

### Incluido
- Bandeja de aprobadores (lista de solicitudes PENDING)
- Aprobar solicitudes (descuento de saldo en Feature 1)
- Rechazar solicitudes con comentario obligatorio
- Prevención de auto-aprobación (un aprobador no puede aprobar sus propias solicitudes)
- Ver impacto en saldo antes de aprobar
- Cancelación de solicitudes APROVED por aprobador antes de inicio (restaura saldo)
- Bloqueo de aprobadores inactivos

### Excluido
- Niveles múltiples de aprobación
- Integraciones externas

---

## Historias de Usuario

HU-05: Bandeja de aprobadores
- Como aprobador quiero ver solicitudes PENDING para tomar decisión.
- Criterios:
  - Mostrar lista con filtros (por empleado, fechas, días)
  - Indicar días solicitados y saldo disponible del empleado

HU-06: Aprobar / Rechazar
- Como aprobador quiero aprobar o rechazar con comentario obligatorio al rechazar.
- Criterios:
  - Al aprobar: descontar días del saldo del empleado (Feature 1)
  - Al rechazar: requerir comentario y no descontar saldo
  - Un aprobador no puede aprobar su propia solicitud

HU-07: Ver impacto en saldo
- Como aprobador quiero ver el saldo disponible del empleado antes de aprobar.
- Criterios:
  - Mostrar saldo disponible en la ficha de la solicitud (consulta a Feature 1)

---

## Reglas de Negocio (selectivas)

- RN-03: Un aprobador no puede aprobar sus propias solicitudes.
- RN-04: Rechazo requiere comentario obligatorio.
- RN-08: Descuento de saldo solo al aprobar.
- RN-12: Aprobador inactivo no puede aprobar.
- RN-13: Al aprobar, registrar actor y timestamp en auditoría.
- RN-14: Cancelación de APROVED solo antes de inicio y por aprobador; restaura saldo.

---

## Modelo de Datos (resumen)

Se usa la entidad VacationRequest (Feature 2) y EmployeeBalance (Feature 1). Adicionalmente:

Entidad: ApprovalAction (registro de decisión)
```
id: UUID
requestId: UUID (FK -> VacationRequest)
approverId: UUID
action: Enum (APPROVED, REJECTED, CANCELLED_BY_APPROVER)
comment: string (nullable)
timestamp: DateTime
```

---

## Validaciones y Comportamientos

- Antes de aprobar, verificar que employee.availableBalance >= daysRequested.
- Bloquear aprobación si aprobador == request.employeeId.
- Rechazo requiere campo comment no vacío.
- Al aprobar: crear registro en HistorialSolicitud (STATUS_CHANGED), cambiar status a APPROVED, y mover pendingBalance → consumedBalance en SaldoEmpleado. *(BalanceHistory fuera de alcance MVP — futura fase)*.
- Al cancelar APROVED (por aprobador antes de inicio): crear ApprovalAction con action=CANCELLED_BY_APPROVER y restaurar saldo en Feature 1.

---

## Estados y Transiciones Relevantes

- PENDING → APPROVED (aprobador) — aplica descuento en Feature 1
- PENDING → REJECTED (aprobador con comentario)
- APPROVED → CANCELLED (aprobador si inicio > hoy)

---

## Dependencias

- Feature 1: EmployeeBalance (consulta y restauración de saldo)
- Feature 2: VacationRequest (fuente de solicitudes)
- Feature 4: Auto-expiración (para PENDING → EXPIRED)

---

## Criterios de Aceptación

- Bandeja de aprobadores muestra solicitudes PENDING con datos necesarios.
- Aprobar crea registro ApprovalAction y descuenta saldo (Feature 1).
- Rechazar crea registro ApprovalAction y guarda comentario obligatorio.
- No se permite auto-aprobación ni aprobadores inactivos.

---

**Última actualización**: 2026-07-17  
**Versión**: 1.0
