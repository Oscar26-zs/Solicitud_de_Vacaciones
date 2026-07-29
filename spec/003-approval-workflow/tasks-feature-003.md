# Tareas de Feature 003 — Approval Workflow

**Extraído de:** `spec/tasks.md` (Tareas específicas a Feature 003)  
**Actualizado:** 2026-07-29  
**Versión:** 1.0 (MVP)  

---

## Resumen de Tareas Feature 003

| Fase | Descripción | Tareas | Estimación |
|------|-------------|:------:|:----------:|
| 4 | Application: Commands de Aprobación/Rechazo/Cancelación Aprobada + Query Bandeja | TASK-031, TASK-032, TASK-033, TASK-036 | 3h 15min |
| 5 | Web: Controller y Vistas de Aprobador | TASK-046, TASK-050, TASK-056 | 3h 30min |
| **Total Feature 003** | | **7 tareas** | **6h 45min** |

---

# Application: Commands y Queries de Feature 003

## TASK-031: Crear comando AprobarSolicitudCommand + Handler
- **Fase:** 4
- **Estimación:** 1h
- **Dependencias:** TASK-027
- **Capa:** Application
- **Archivos a crear:**
  - `src/Vacations.Application/Solicitudes/Commands/AprobarSolicitudCommand.cs`
  - `src/Vacations.Application/Solicitudes/Commands/AprobarSolicitudCommandHandler.cs`
- **Trazabilidad:** CU-11, HU-06, RN-03, RN-08, RN-12, RN-13, RN-14
- **Descripción:** Comando para que un aprobador apruebe una solicitud.
- **Criterios de aceptación:**
  - [ ] Verifica que el aprobador no sea el autor (anti-auto-aprobación)
  - [ ] Verifica que el aprobador esté activo
  - [ ] Verifica estado `Pending`
  - [ ] Verifica saldo disponible actual (puede haber cambiado)
  - [ ] Mueve días de `pendingBalance` a `consumedBalance`
  - [ ] Registra en historial con actor = email aprobador
  - [ ] Maneja concurrencia optimista

## TASK-032: Crear comando RechazarSolicitudCommand + Handler
- **Fase:** 4
- **Estimación:** 45min
- **Dependencias:** TASK-027
- **Capa:** Application
- **Archivos a crear:**
  - `src/Vacations.Application/Solicitudes/Commands/RechazarSolicitudCommand.cs`
  - `src/Vacations.Application/Solicitudes/Commands/RechazarSolicitudCommandHandler.cs`
- **Trazabilidad:** CU-12, HU-06, RN-11
- **Descripción:** Comando para que un aprobador rechace una solicitud con comentario obligatorio.
- **Criterios de aceptación:**
  - [ ] Verifica aprobador activo y no es autor
  - [ ] Verifica estado `Pending`
  - [ ] Comentario obligatorio (1-500 caracteres)
  - [ ] Libera `pendingBalance`
  - [ ] Registra en historial con comentario

## TASK-033: Crear comando CancelarAprobadaCommand + Handler
- **Fase:** 4
- **Estimación:** 45min
- **Dependencias:** TASK-031
- **Capa:** Application
- **Archivos a crear:**
  - `src/Vacations.Application/Solicitudes/Commands/CancelarAprobadaCommand.cs`
  - `src/Vacations.Application/Solicitudes/Commands/CancelarAprobadaCommandHandler.cs`
- **Trazabilidad:** CU-14, HU-03, RN-04
- **Descripción:** Comando para que un aprobador cancele una solicitud ya aprobada.
- **Criterios de aceptación:**
  - [ ] Solo si estado es `Approved`
  - [ ] Solo si fecha inicio > fecha actual
  - [ ] Restaura saldo (mueve de `consumedBalance` a disponible)
  - [ ] Registra en historial

## TASK-036: Crear query ObtenerBandejaAprobadorQuery + Handler
- **Fase:** 4
- **Estimación:** 45min
- **Dependencias:** TASK-013
- **Capa:** Application
- **Archivos a crear:**
  - `src/Vacations.Application/Solicitudes/Queries/ObtenerBandejaAprobadorQuery.cs`
  - `src/Vacations.Application/Solicitudes/Queries/ObtenerBandejaAprobadorQueryHandler.cs`
- **Trazabilidad:** CU-10, HU-05
- **Descripción:** Query para listar solicitudes Pending para aprobadores. El `pageSize` se recibe como parámetro opcional (default: 10) y puede ser 5, 10, 15 o 25.
- **Criterios de aceptación:**
  - [ ] Excluye solicitudes del propio aprobador
  - [ ] Filtros opcionales: empleado, rango fechas, días
  - [ ] Paginación offset-based con `page` y `pageSize` (soporta 5, 10, 15, 25)
  - [ ] Incluye saldo disponible del empleado
  - [ ] Indica si hay traslape con otras solicitudes
  - [ ] Ordenado de más antiguo a más reciente

---

# Web: Controllers y Vistas de Feature 003

## TASK-046: Crear ViewModels de Aprobador
- **Fase:** 5
- **Estimación:** 20min
- **Dependencias:** TASK-040
- **Capa:** Web
- **Archivos a crear:**
  - `src/Vacations.Web/ViewModels/BandejaAprobadorViewModel.cs`
  - `src/Vacations.Web/ViewModels/AprobarRechazarViewModel.cs`
- **Trazabilidad:** CU-10, CU-11, CU-12
- **Descripción:** ViewModels para las vistas de aprobador.
- **Criterios de aceptación:**
  - [ ] Bandeja incluye indicador de traslape
  - [ ] AprobarRechazar incluye campo para comentario

## TASK-050: Crear BandejaAprobadorController
- **Fase:** 5
- **Estimación:** 1h
- **Dependencias:** TASK-046, TASK-031, TASK-032, TASK-036
- **Capa:** Web
- **Archivos a crear:**
  - `src/Vacations.Web/Controllers/BandejaAprobadorController.cs`
- **Trazabilidad:** `plan.md` sección 6, CU-10 a CU-14
- **Descripción:** Controller para funcionalidades de aprobador.
- **Criterios de aceptación:**
  - [ ] `[Authorize(Policy = "RequiereAprobador")]`
  - [ ] `GET /bandeja-aprobador` → Lista pendientes
  - [ ] `GET /bandeja-aprobador/{id}` → Detalle con impacto en saldo
  - [ ] `POST /bandeja-aprobador/{id}/aprobar` → Aprobar
  - [ ] `POST /bandeja-aprobador/{id}/rechazar` → Rechazar con comentario
  - [ ] `POST /solicitudes-vacaciones/{id}/cancelar-aprobada` → Cancelar aprobada

## TASK-056: Crear vistas de Bandeja Aprobador
- **Fase:** 5
- **Estimación:** 1h 30min
- **Dependencias:** TASK-053, TASK-050
- **Capa:** Web
- **Archivos a crear:**
  - `src/Vacations.Web/Views/BandejaAprobador/Index.cshtml`
  - `src/Vacations.Web/Views/BandejaAprobador/Detalle.cshtml`
- **Trazabilidad:** `DESIGN_TOKENS.md`, CU-10 a CU-14
- **Descripción:** Vistas para la bandeja de aprobación.
- **Criterios de aceptación:**
  - [ ] Lista ordenada por antigüedad
  - [ ] Indicador visual de traslape
  - [ ] Botón aprobar deshabilitado si hay traslape con aprobada
  - [ ] Modal o form para comentario de rechazo
  - [ ] Muestra impacto en saldo antes de aprobar

---

**Fin de Tareas Feature 003 — Approval Workflow**
