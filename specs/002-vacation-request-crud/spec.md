# Feature 2: Solicitudes de Vacaciones (CRUD Base)

**Versión**: 1.0  
**Última actualización**: 2026-07-17

---

## Resumen

Este feature define la gestión CRUD de solicitudes de vacaciones por parte de empleados. Incluye validaciones de fechas, cálculo de días hábiles (excluye sábados y domingos), validación de saldo disponible (Feature 1), y prevención de traslapes con solicitudes PENDING o APPROVED del mismo empleado.

---

## Alcance

### Incluido
- Crear solicitud de vacaciones (HU-01)
- Ver mis solicitudes (HU-02)
- Editar o cancelar solicitudes PENDING por el empleado (HU-03)
- Validación de fechas: inicio >= mañana; fin >= inicio; horizonte máximo 2 meses
- Cálculo de días hábiles: excluir sábados y domingos (los feriados cuentan para el consumo de saldo)
- Validación de saldo suficiente contra `availableBalance` (incluye descuento de `pendingBalance` por solicitudes Pending existentes)
- Prevención de traslapes con solicitudes APROBADA o PENDIENTE
- Estados: PENDING, APPROVED, REJECTED, CANCELLED, EXPIRED
- Auditoría de eventos básicos (creación, edición, estado)

### Excluido (fuera de MVP)
- Cancelación parcial de solicitudes
- Integraciones externas (calendario, nómina)

---

## Historias de Usuario

HU-01: Solicitar vacaciones con fechas y motivo
- Como empleado quiero solicitar vacaciones con fecha de inicio/fin y motivo.
- Criterios (EARS):
  - Cuando el empleado completa fechas, entonces el sistema muestra los días solicitados (excluyendo sábados y domingos; los feriados sí cuentan) antes de enviar.
  - Si la fecha de inicio < mañana, entonces bloquear con: "La fecha de inicio no puede ser anterior a mañana".
  - Si la fecha de fin < fecha de inicio, entonces bloquear con: "La fecha de fin no puede ser anterior a la de inicio".
  - Si la fecha de inicio > hoy + 2 meses, entonces bloquear con: "La fecha de inicio no puede superar los 2 meses a partir de hoy".
  - Si los días solicitados > saldo disponible (considerando pendingBalance de otras Pending), entonces bloquear con: "Saldo insuficiente para esta solicitud".
  - Si el rango solicitado traslapa con días en solicitudes APPROVED o PENDING del mismo empleado, entonces bloquear con: "La solicitud incluye días que ya están comprometidos".
  - Cuando todas las validaciones pasen, crear la solicitud en estado PENDING y notificar la bandeja de aprobadores.

HU-02: Ver el estado de mis solicitudes
- Como empleado quiero ver mis solicitudes con estado, fechas y días.
- Criterios:
  - Mostrar lista paginada con columnas: ID, fecha inicio, fecha fin, días, estado, fecha de creación, comentario del aprobador si existe.
  - Permitir ver detalle con historial de cambios (auditoría).

HU-03: Editar o cancelar una solicitud PENDING
- Como empleado quiero editar o cancelar mientras esté PENDING.
- Criterios:
  - Mientras PENDING, permitir modificar fecha inicio, fecha fin y motivo; registrar en auditoría.
  - Mientras PENDING, permitir cancelar -> cambia estado a CANCELLED y registrar actor/timestamp.
  - No permitir editar ni cancelar si la solicitud es APPROVED y su periodo ha iniciado.

---

## Reglas de Negocio (selectivas)

- RN-02: Validación de fechas (inicio >= mañana, fin >= inicio, inicio ≤ hoy + 2 meses).
- RN-05: Cálculo de días hábiles excluyendo sábados y domingos (los feriados cuentan para el consumo de saldo).
- RN-06: Validación de saldo suficiente antes de creación / aprobación (consulta a Feature 1).
- RN-07: Prevención de traslapes con solicitudes APPROVED o PENDING del mismo empleado.
- RN-09: Al crear PENDING, notificar bandeja de aprobadores.
- RN-10: Edición sólo en PENDING; registrar auditoría.
- RN-11: Cancelación por empleado sólo en PENDING; aprobador puede cancelar APPROVED antes de inicio (Feature 3).

---

## Modelo de Datos (resumen)

Entidad: VacationRequest
```
id: UUID (PK)
employeeId: UUID (FK -> Employee)
startDate: Date
endDate: Date
daysRequested: int (calculado, excluye sábados/domingo)
status: Enum (PENDING, APPROVED, REJECTED, CANCELLED, EXPIRED)
reason: string
approverComment: string (nullable)
createdAt: DateTime
updatedAt: DateTime
```

Entidad: VacationRequestHistory (auditoría)
```
id: UUID
requestId: UUID (FK -> VacationRequest)
eventType: Enum (CREATED, UPDATED, STATUS_CHANGED, CANCELLED)
actor: string
note: string
timestamp: DateTime
```

---

## Validaciones y Comportamientos

- Fecha de inicio: debe ser >= (hoy + 1 día).
- Fecha de fin: debe ser >= fecha de inicio.
- Cálculo de días: contar días entre startDate y endDate excluyendo sábados y domingos.
- Traslapes: verificar intersección con rangos de solicitudes APROBADA/PENDING del mismo empleado.
- Saldo: antes de crear la solicitud se valida que availableBalance >= daysRequested. Si la validación falla, la creación se bloquea.
- Auditoría: toda creación, edición y cambio de estado genera un registro en VacationRequestHistory.

---

## Estados y Transiciones (resumen)

- PENDING → APPROVED (por aprobador)
- PENDING → REJECTED (por aprobador con comentario obligatorio)
- PENDING → CANCELLED (por empleado)
- APPROVED → CANCELLED (por aprobador **si** inicio > hoy; restaura saldo en Feature 1)
- PENDING → EXPIRED (por job de auto-expiración en Feature 4)

---

## Dependencias

- Feature 1: Gestión de Empleados y Saldos (saldo disponible, restauraciones)
- Feature 3: Flujo de Aprobación (acciones de aprobador)
- Feature 4: Auto-Expiración (job que marca PENDING → EXPIRED)

---

## Criterios de Aceptación del Feature

- Endpoints CRUD básicos para VacationRequest existan y respeten autorizaciones.
- Validaciones de fechas y saldo impiden creaciones inválidas.
- Cálculo de días hábiles correcto (excluye sábados y domingos).
- Prevención de traslapes con solicitudes PENDING/APROVED del mismo empleado.
- Auditoría mínima (VacationRequestHistory) registra eventos principales.

---

**Última actualización**: 2026-07-17  
**Versión**: 1.0
