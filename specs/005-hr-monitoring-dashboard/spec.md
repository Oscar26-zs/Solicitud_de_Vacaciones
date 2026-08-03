# Feature 5: Consultas y Filtrado para RRHH

**Versión**: 1.0  
**Última actualización**: 2026-07-17

---

## Resumen

Este feature provee herramientas de consulta y filtrado para RRHH: historial de solicitudes por empleado, filtrado por estado/fechas, y auditoría de trazabilidad. Es un componente de solo lectura que consume datos de las Features 1-4.

---

## Alcance

### Incluido
- Consultar historial completo de cualquier empleado (HU-08)
- Filtrar solicitudes por estado, empleado, rango de fechas (HU-09)
- Acceso de solo lectura para RRHH (sin permisos de aprobación ni edición)
- Paginación y exportación básica (CSV opcional)
- Visualización de trazabilidad (VacationRequestHistory). 🔶 BalanceHistory fuera de alcance MVP.

### Excluido
- Operaciones de escritura (aprobaciones, cancelaciones)
- Dashboards analíticos avanzados (KPIs, gráficos complejos)

---

## Historias de Usuario

HU-08: Consultar historial de un empleado
- Como RRHH quiero ver todo el historial de solicitudes y movimientos de saldo de un empleado.
- Criterios:
  - Mostrar lista paginada con filtros por fechas y estado
  - Permitir ver detalle de cada solicitud y su history

HU-09: Filtrar solicitudes
- Como RRHH quiero filtrar solicitudes por estado/empleado/fechas para reportes rápidos.
- Criterios:
  - Filtros combinables (estado + fecha + empleado)
  - Respuesta paginada y ordenable

---

## Modelo de Datos (resumen)

Consumir:
- VacationRequest (Feature 2)
- VacationRequestHistory (Feature 2)
- EmployeeBalance (Feature 1). 🔶 BalanceHistory fuera de alcance MVP.

---

## Validaciones y Accesos

- Acceso restringido al rol RRHH.
- Lectura solamente; cualquier intento de escribir debe retornar 403.
- Paginación por defecto: 20 items.

---

## Criterios de Aceptación

- Endpoints de consulta con filtros funcionan y retornan datos correctos.
- RRHH puede ver historial completo por empleado.
- Acceso denegado para roles no-RRHH.
- Paginación y ordenación funcionan correctamente.

---

**Última actualización**: 2026-07-17  
**Versión**: 1.0
