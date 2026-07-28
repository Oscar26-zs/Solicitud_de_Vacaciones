# Feature 4: Auto-Expiración de Solicitudes

**Versión**: 1.0  
**Última actualización**: 2026-07-17

---

## Resumen

Este feature define la lógica de expiración automática de solicitudes en estado PENDING cuya fecha de inicio (`startDate`) ha sido alcanzada sin ser resueltas. El sistema marcará dichas solicitudes como EXPIRED y registrará el actor como "SISTEMA_AUTO_EXPIRACION".

---

## Alcance

### Incluido
- Job diario que revisa solicitudes PENDING y aplica EXPIRED cuando `startDate ≤ today`
- Cambio de estado a EXPIRED con actor = "SISTEMA_AUTO_EXPIRACION"
- Registro en VacationRequestHistory de la expiración
- Notificación mínima al empleado (placeholder)

### Excluido
- Reglas complejas de reintentos
- Notificaciones ricas (emails, push) — solo placeholder

---

## Comportamiento

- El job se ejecuta periódicamente (diario) y:
  - Consulta solicitudes con status = PENDING y startDate ≤ today (usando `TimeProvider`)
  - Cambia status a EXPIRED
  - Registra en VacationRequestHistory: eventType = STATUS_CHANGED, actor = "SISTEMA_AUTO_EXPIRACION", note = "Auto-expired — start date reached"
  - (Opcional) Marcar notificación pendiente para el empleado

---

## Reglas de Negocio

- RN-26: Solicitudes PENDING cuya fecha de inicio (`startDate`) ya fue alcanzada (startDate ≤ today) pasan a EXPIRED automáticamente.
- Registro de actor obligatorio: actor = "SISTEMA_AUTO_EXPIRACION".

---

## Modelo de Datos (resumen)

Se usa VacationRequest y VacationRequestHistory (Feature 2). No se requieren tablas nuevas.

---

## Criterios de Aceptación

- El job identifica correctamente las solicitudes PENDING cuya fecha de inicio ya fue alcanzada (startDate ≤ today).
- Las solicitudes afectadas cambian a estado EXPIRED.
- Se registra en VacationRequestHistory con actor = "SISTEMA_AUTO_EXPIRACION".
- Empleado recibe notificación placeholder (registro de evento).

---

**Última actualización**: 2026-07-17  
**Versión**: 1.0
