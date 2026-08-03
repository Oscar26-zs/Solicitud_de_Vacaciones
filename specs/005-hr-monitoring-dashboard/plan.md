# Plan: Feature 005 — HR Monitoring Dashboard

**Extraído de:** `specs/plan.md` (secciones relevantes a 005-hr-monitoring-dashboard)  
**Actualizado:** 2026-07-29  
**Estado:** MVP  

---

## Resumen Feature 005

**Objetivo:** Proveer consultas de solo lectura para el rol RRHH. Incluye listado y filtrado de solicitudes de cualquier empleado, consulta de saldos individuales, sin opciones de modificación ni creación.

---

## Queries para Feature 005

- **`ObtenerHistorialRRHHQuery`**: Lista solicitudes de cualquier empleado con filtros por estado, empleado, rango de fechas, y paginación. Incluye información del empleado. Solo lectura, sin opciones de acción.
- **`ObtenerSaldoQuery`**: Consulta saldo de un empleado específico (Acumulado, Consumido, Pendiente, Disponible).

---

## Reglas de Negocio (RN) para Feature 005

| ID | Regla | Descripción |
|----|-------|-------------|
| RN-17 | Acceso completo a historial para RRHH | Lectura ilimitada por antigüedad y estados. |
| RN-18 | Filtrado por RRHH | Filtros por estado/empleado/rango funcionales y precisos. |
| RN-19 | RRHH sin permiso de aprobación | RRHH no ve botones para decidir, solo consultar. |
| RN-22 | RRHH sin permiso para crear/editar solicitudes | RRHH tendrá únicamente permisos de consulta. RRHH no podrá crear, modificar ni registrar solicitudes en nombre de terceros bajo ninguna circunstancia. |

---

## Historias de Usuario para Feature 005

### HU-08: Consultar historial y saldo de cualquier empleado (RRHH)

**Como** RRHH  
**Quiero** consultar el historial y saldo de días de cualquier empleado  
**Para** auditoría y reporte de gestión de vacaciones

**Criterios de Aceptación:**
- Cuando RRHH busca un empleado, entonces el sistema debe mostrar todo el historial de solicitudes para el período seleccionado y el saldo correspondiente.
- RRHH no debe ver ningún botón de aprobación/rechazo en la interfaz (solo lectura).

### HU-09: Filtrar solicitudes por estado, empleado o rango de fechas (RRHH)

**Como** RRHH  
**Quiero** filtrar las solicitudes por estado, empleado o rango de fechas para consultar historial  
**Para** extraer información específica de auditoría

**Criterios de Aceptación:**
- Cuando RRHH aplica filtros (estado, empleado, rango), entonces el sistema debe devolver los resultados que coincidan.
- Si no hay coincidencias con los filtros aplicados, entonces el sistema debe mostrar: "No se encontraron solicitudes que coincidan con los filtros aplicados".
- **Exportación y generación de reportes no están incluidos en esta versión del MVP** (fuera de alcance).

---

## Módulo Responsable

**`Vacations.Application`** (queries de RRHH: `ObtenerHistorialRRHHQuery`)  
**`Vacations.Infrastructure`** (repositorios optimizados para consultas RRHH)  
**`Vacations.Web`** (controller RRHH, vistas de solo lectura, filtros reutilizables)

---

## Consideraciones de Implementación

- Las queries de RRHH incluyen `AsNoTracking()` para optimizar legibilidad, sin necesidad de tracking.
- Los filtros son opcionales y combinables (estado, empleado, rango de fechas).
- La paginación utiliza el mismo formato offset-based que otras listados (pageSize: 5, 10, 15, 25).
- Las vistas de RRHH **no renderizar botones de acción** (aprobar, rechazar, cancelar).
- El acceso está protegido por política `RequiereRRHH` en el controller.
- Se pueden mostrar campos adicionales como número de solicitud, empleado (nombre/email), estado, fechas, días, aprobador (si aplica).
- Mensajes de "sin registros" con filtros aplicados para mejor UX.

---

**Fin de Feature 005 — HR Monitoring Dashboard**
