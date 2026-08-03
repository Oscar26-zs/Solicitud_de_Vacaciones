# Plan: Feature 004 — Request Auto-Expiration

**Extraído de:** `spec/plan.md` (secciones relevantes a 004-request-auto-expiration)  
**Actualizado:** 2026-07-29  
**Estado:** MVP  

---

## Resumen Feature 004

**Objetivo:** Implementar un servicio de background que expira automáticamente solicitudes Pending cuya fecha de inicio ya ha sido alcanzada o superada. El sistema registra la expiración con actor `SISTEMA_AUTO_EXPIRACION` en el historial.

---

## Servicios para Feature 004

### `ServicioExpiracionAutomatica` (BackgroundService)

Servicio que se ejecuta periódicamente (default cada hora) para expirar solicitudes Pending cuya fecha de inicio ≤ hoy.

**Comportamiento:**
1. Busca todas las solicitudes en estado `Pending`
2. Filtra aquellas con `FechaInicio ≤ hoy` (usando `TimeProvider`)
3. Cambia su estado a `Expired`
4. Libera `pendingBalance` del empleado
5. Registra en `HistorialSolicitud` con:
   - `eventType` = `STATUS_CHANGED`
   - `previousStatus` = `Pending`
   - `newStatus` = `Expired`
   - `actor` = `SISTEMA_AUTO_EXPIRACION`
   - `timestamp` = fecha/hora actual
6. Maneja errores sin detener el servicio (logging)

---

## Reglas de Negocio (RN) para Feature 004

| ID | Regla | Descripción |
|----|-------|-------------|
| RN-26 | Auto-rechazo por inacción | Solicitud Pending sin resolver tras **[N] días** (parámetro configurable) cambia su estado a **Expired** (expirada) automáticamente por vencimiento. El sistema registra el cambio con actor="SISTEMA_AUTO_EXPIRACION" y timestamp. **Nota MVP: La "expiración" se dispara cuando `FechaInicio ≤ hoy`, no por un período de inactividad configurable.** |

---

## Decisiones Técnicas Aplicadas a Feature 004

| # | Ítem | Decisión | Estado |
|---|------|----------|--------|
| 5 | RN-26 — Auto-expiración | La solicitud `Pending` expira cuando se alcanza su fecha de inicio (fecha inicio ≤ hoy). **No es un N fijo.** | ✅ Resuelto |

---

## Módulo Responsable

**`Vacations.Infrastructure`** (`ServicioExpiracionAutomatica` en `BackgroundServices/`)  
**`Vacations.Domain`** (método `Expirar()` en `SolicitudVacaciones`)  
**`Vacations.Application`** (comando `ExpiracionSolicitudesPendientesCommand` si se desea encapsular en handler CQRS)

---

## Consideraciones de Implementación

- El servicio se registra en `Program.cs` con `services.AddHostedService<ServicioExpiracionAutomatica>()`.
- El intervalo de ejecución es configurable (default 1 hora, se puede ajustar en appsettings).
- Se usa `TimeProvider` inyectado para obtener la fecha actual, evitando dependencia de `DateTime.Now`.
- La liberación de `pendingBalance` se hace sobre la entidad `SaldoEmpleado` del empleado.
- El registro en historial ocurre automáticamente (via interceptor de EF Core) o manualmente en el handler.
- Errores de BD no deben detener el servicio; se logean y el ciclo continúa.

---

**Fin de Feature 004 — Request Auto-Expiration**
