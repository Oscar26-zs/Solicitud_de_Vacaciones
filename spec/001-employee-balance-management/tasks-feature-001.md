# Tareas de Feature 001 — Employee Balance Management

**Extraído de:** `spec/tasks.md` (Tareas específicas a Feature 001)  
**Actualizado:** 2026-07-29  
**Versión:** 1.0 (MVP)  

---

## Resumen de Tareas Feature 001

| Fase | Descripción | Tareas | Estimación |
|------|-------------|:------:|:----------:|
| 2 | Domain: Entidades | TASK-007, TASK-009, TASK-010 | 1h 45min |
| 4 | Application: Queries y Commands | TASK-037, TASK-039 | 1h 15min |
| 5 | Web: Controller y Vistas | TASK-049, TASK-055 | 1h |
| **Total Feature 001** | | **7 tareas** | **4h** |

---

# Domain: Entidades de Feature 001

## TASK-007: Crear Value Object RangoFechas
- **Fase:** 2
- **Estimación:** 45min
- **Dependencias:** TASK-003
- **Capa:** Domain
- **Archivos a crear:**
  - `src/Vacations.Domain/ValueObjects/RangoFechas.cs`
- **Trazabilidad:** `plan.md` sección 4 (Value Objects), RN-05, RN-06, RN-31
- **Descripción:** Value Object inmutable que encapsula fecha inicio y fecha fin con validaciones: inicio ≤ fin, inicio ≥ mañana, fin ≤ inicio + 2 meses.
- **Criterios de aceptación:**
  - [ ] Constructor privado, factory method `Crear(fechaInicio, fechaFin, fechaActual)`
  - [ ] Validación: fecha inicio no puede ser anterior a mañana
  - [ ] Validación: fecha fin no puede ser anterior a fecha inicio
  - [ ] Validación: horizonte máximo de 2 meses
  - [ ] Método `CalcularDiasHabiles()` que excluye sábados y domingos
  - [ ] Implementa `IEquatable<RangoFechas>`

## TASK-009: Crear entidad Empleado
- **Fase:** 2
- **Estimación:** 30min
- **Dependencias:** TASK-006
- **Capa:** Domain
- **Archivos a crear:**
  - `src/Vacations.Domain/Entities/Empleado.cs`
- **Trazabilidad:** `plan.md` sección 4 (Entidad Empleado), CU-01, CU-02
- **Descripción:** Entidad que representa un usuario del sistema. Los roles se gestionan vía Identity, no como campo de esta entidad.
- **Criterios de aceptación:**
  - [ ] Propiedades: `Id` (Guid), `Email`, `NombreCompleto`, `FechaIngreso`, `EstaActivo`
  - [ ] Constructor privado + factory method `Crear(...)`
  - [ ] Método `Desactivar()` y `Activar()`
  - [ ] Validaciones en constructor (email no vacío, nombre no vacío)

## TASK-010: Crear entidad SaldoEmpleado
- **Fase:** 2
- **Estimación:** 45min
- **Dependencias:** TASK-009
- **Capa:** Domain
- **Archivos a crear:**
  - `src/Vacations.Domain/Entities/SaldoEmpleado.cs`
- **Trazabilidad:** `plan.md` sección 4 (SaldoEmpleado), CU-01, CU-02, CU-03, RN-01, RN-02, RN-03, RN-24
- **Descripción:** Entidad que gestiona los días de vacaciones. Implementa la fórmula: `availableBalance = accumulatedBalance - consumedBalance - pendingBalance`.
- **Criterios de aceptación:**
  - [ ] Propiedades: `Id`, `EmpleadoId`, `SaldoAcumulado`, `SaldoConsumido`, `SaldoPendiente`, `UltimaActualizacion`, `RowVersion`
  - [ ] Propiedad calculada `SaldoDisponible` (no persistida)
  - [ ] Método `AcumularDias(int dias)` para CU-01
  - [ ] Método `CongelarSaldo(int dias)` para crear solicitud
  - [ ] Método `DescontarSaldo(int dias)` para aprobar solicitud
  - [ ] Método `LiberarSaldoPendiente(int dias)` para rechazar/cancelar/expirar
  - [ ] Método `RestaurarSaldo(int dias)` para cancelar aprobada
  - [ ] Invariante: saldo disponible nunca negativo (lanzar `SaldoInsuficienteException`)

---

# Application: Queries y Commands de Feature 001

## TASK-037: Crear query ObtenerSaldoQuery + Handler
- **Fase:** 4
- **Estimación:** 30min
- **Dependencias:** TASK-014
- **Capa:** Application
- **Archivos a crear:**
  - `src/Vacations.Application/Saldos/Queries/ObtenerSaldoQuery.cs`
  - `src/Vacations.Application/Saldos/Queries/ObtenerSaldoQueryHandler.cs`
- **Trazabilidad:** CU-02, HU-04, RN-27
- **Descripción:** Query para consultar saldo de un empleado.
- **Criterios de aceptación:**
  - [ ] Empleado puede consultar su propio saldo
  - [ ] RRHH puede consultar saldo de cualquier empleado
  - [ ] Retorna: Acumulado, Consumido, Pendiente, Disponible
  - [ ] Respuesta en ≤300ms p95

## TASK-039: Crear comando AcumularSaldoMensualCommand + Handler
- **Fase:** 4
- **Estimación:** 45min
- **Dependencias:** TASK-014, TASK-015
- **Capa:** Application
- **Archivos a crear:**
  - `src/Vacations.Application/Saldos/Commands/AcumularSaldoMensualCommand.cs`
  - `src/Vacations.Application/Saldos/Commands/AcumularSaldoMensualCommandHandler.cs`
- **Trazabilidad:** CU-01, RN-01, RN-23, RN-24
- **Descripción:** Comando para acumular saldo mensual de todos los empleados activos.
- **Criterios de aceptación:**
  - [ ] Procesa solo empleados activos
  - [ ] Calcula meses completos desde fecha de ingreso
  - [ ] Acumula 1 día por mes completo no contabilizado
  - [ ] Carry-over ilimitado
  - [ ] Registra en historial de solicitud (futuro: historial de saldo)

---

# Web: Controllers y Vistas de Feature 001

## TASK-049: Crear SaldoController
- **Fase:** 5
- **Estimación:** 30min
- **Dependencias:** TASK-037
- **Capa:** Web
- **Archivos a crear:**
  - `src/Vacations.Web/Controllers/SaldoController.cs`
- **Trazabilidad:** `plan.md` sección 6, CU-02
- **Descripción:** Controller para consulta de saldo.
- **Criterios de aceptación:**
  - [ ] `[Authorize]`
  - [ ] `GET /saldo` → Mi saldo (empleado)
  - [ ] Muestra: Acumulado, Consumido, Pendiente, Disponible

## TASK-055: Crear vistas de Saldo
- **Fase:** 5
- **Estimación:** 30min
- **Dependencias:** TASK-053, TASK-049
- **Capa:** Web
- **Archivos a crear:**
  - `src/Vacations.Web/Views/Saldo/Index.cshtml`
- **Trazabilidad:** `DESIGN_TOKENS.md`, CU-02
- **Descripción:** Vista para mostrar saldo del empleado.
- **Criterios de aceptación:**
  - [ ] StatCards con: Acumulado, Consumido, Pendiente, Disponible
  - [ ] Barra de progreso visual
  - [ ] Cifras con `tabular-nums`

---

**Fin de Tareas Feature 001 — Employee Balance Management**
