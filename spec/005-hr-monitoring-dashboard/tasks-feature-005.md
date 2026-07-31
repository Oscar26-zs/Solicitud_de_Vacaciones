# Tareas de Feature 005 — HR Monitoring Dashboard

**Extraído de:** `spec/tasks.md` (Tareas específicas a Feature 005)  
**Actualizado:** 2026-07-29  
**Versión:** 1.0 (MVP)  

---

## Resumen de Tareas Feature 005

| Fase | Descripción | Tareas |
|------|-------------|:------:|
| 4 | Application: Query de RRHH | TASK-038 |
| 5 | Web: Controller y Vistas de RRHH | TASK-047, TASK-051, TASK-057 |
| **Total Feature 005** | | **4 tareas** |

---

# Application: Queries de Feature 005

## TASK-038: Crear query ObtenerHistorialRRHHQuery + Handler
- **Fase:** 4
- **Dependencias:** TASK-013
- **Capa:** Application
- **Archivos a crear:**
  - `src/Vacations.Application/Solicitudes/Queries/ObtenerHistorialRRHHQuery.cs`
  - `src/Vacations.Application/Solicitudes/Queries/ObtenerHistorialRRHHQueryHandler.cs`
- **Trazabilidad:** CU-18, HU-08, HU-09
- **Descripción:** Query para que RRHH consulte y filtre solicitudes de cualquier empleado. El `pageSize` se recibe como parámetro opcional (default: 10) y puede ser 5, 10, 15 o 25.
- **Criterios de aceptación:**
  - [ ] Solo accesible por rol RRHH
  - [ ] Filtros: estado, empleado, rango de fechas
  - [ ] Paginación offset-based con `page` y `pageSize` (soporta 5, 10, 15, 25)
  - [ ] Incluye información del empleado

---

# Web: Controllers y Vistas de Feature 005

## TASK-047: Crear ViewModels de RRHH
- **Fase:** 5
- **Dependencias:** TASK-040
- **Capa:** Web
- **Archivos a crear:**
  - `src/Vacations.Web/ViewModels/ConsultaRRHHViewModel.cs`
  - `src/Vacations.Web/ViewModels/FiltrosRRHHViewModel.cs`
- **Trazabilidad:** CU-18
- **Descripción:** ViewModels para las vistas de RRHH.
- **Criterios de aceptación:**
  - [ ] Filtros para estado, empleado, fechas
  - [ ] Sin botones de acción (solo lectura)

## TASK-051: Crear RRHHController
- **Fase:** 5
- **Dependencias:** TASK-047, TASK-037, TASK-038
- **Capa:** Web
- **Archivos a crear:**
  - `src/Vacations.Web/Controllers/RRHHController.cs`
- **Trazabilidad:** `plan.md` sección 6, CU-18
- **Descripción:** Controller para consultas de RRHH.
- **Criterios de aceptación:**
  - [ ] `[Authorize(Policy = "RequiereRRHH")]`
  - [ ] `GET /rrhh/solicitudes` → Lista con filtros
  - [ ] `GET /rrhh/saldos/{empleadoId}` → Saldo de empleado
  - [ ] Sin acciones de modificación (solo lectura)

## TASK-057: Crear vistas de RRHH
- **Fase:** 5
- **Dependencias:** TASK-053, TASK-051
- **Capa:** Web
- **Archivos a crear:**
  - `src/Vacations.Web/Views/RRHH/Solicitudes.cshtml`
  - `src/Vacations.Web/Views/RRHH/SaldoEmpleado.cshtml`
- **Trazabilidad:** `DESIGN_TOKENS.md`, CU-18
- **Descripción:** Vistas de solo lectura para RRHH.
- **Criterios de aceptación:**
  - [ ] Filtros combinables
  - [ ] Sin botones de acción
  - [ ] Tabla con todos los campos relevantes

---

**Fin de Tareas Feature 005 — HR Monitoring Dashboard**
