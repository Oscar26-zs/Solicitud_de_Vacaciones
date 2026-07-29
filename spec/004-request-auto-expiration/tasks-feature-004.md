# Tareas de Feature 004 — Request Auto-Expiration

**Extraído de:** `spec/tasks.md` (Tareas específicas a Feature 004)  
**Actualizado:** 2026-07-29  
**Versión:** 1.0 (MVP)  

---

## Resumen de Tareas Feature 004

| Fase | Descripción | Tareas | Estimación |
|------|-------------|:------:|:----------:|
| 3 | Infrastructure: BackgroundService de Expiración | TASK-026 | 1h |
| **Total Feature 004** | | **1 tarea** | **1h** |

---

# Infrastructure: Servicios de Feature 004

## TASK-026: Crear ServicioExpiracionAutomatica (BackgroundService)
- **Fase:** 3
- **Estimación:** 1h
- **Dependencias:** TASK-018, TASK-023
- **Capa:** Infrastructure
- **Archivos a crear:**
  - `src/Vacations.Infrastructure/BackgroundServices/ServicioExpiracionAutomatica.cs`
- **Trazabilidad:** CU-15, `plan.md` sección 10 (Complejidades), RN-26
- **Descripción:** Background service que expira solicitudes Pending cuya fecha de inicio ya pasó.
- **Criterios de aceptación:**
  - [ ] Hereda de `BackgroundService`
  - [ ] Ejecuta periódicamente (configurable, default cada hora)
  - [ ] Usa `TimeProvider` para obtener fecha actual
  - [ ] Busca solicitudes Pending con `FechaInicio <= hoy`
  - [ ] Cambia estado a `Expired`
  - [ ] Libera `pendingBalance` del empleado
  - [ ] Registra en `HistorialSolicitud` con actor `SISTEMA_AUTO_EXPIRACION`
  - [ ] Maneja errores sin detener el servicio

---

**Fin de Tareas Feature 004 — Request Auto-Expiration**
