# Checklist de Calidad — `plan.md`

> Usa este checklist para evaluar qué tan completo, consistente y accionable está el plan de implementación.
> **Leyenda:** ✅ = Cumple | ⚠️ = Parcial/Débil | ❌ = No cumple | 🔲 = No aplica

---

## 1. Alineación con Spec y Constitución

| # | Ítem | Resultado | Notas |
|---|------|-----------|-------|
| 1.1 | Los actores del sistema están correctamente definidos (Empleado, Aprobador, RRHH) | ✅ | |
| 1.2 | Los 5 estados de `SolicitudVacaciones` están documentados | ✅ | |
| 1.3 | Las transiciones de estado válidas están explícitas | ✅ | Sección 4 |
| 1.4 | Todas las reglas de negocio (RN-01 a RN-36) están cubiertas implícita o explícitamente | ✅ | |
| 1.5 | Los 47 requisitos funcionales (RF-001 a RF-047) están cubiertos | ⚠️ | No hay trazabilidad RF → plan |
| 1.6 | Los 19 casos de uso (CU-01 a CU-19) están mapeados a endpoints | ✅ | Sección 6 |
| 1.7 | La arquitectura Clean Architecture se respeta (4 capas, dependencias inward) | ✅ | Secciones 3, 7 |
| 1.8 | Domain y Application no dependen de ASP.NET Core ni EF Core | ✅ | |
| 1.9 | Se usa `TimeProvider` nativo de .NET, no `DateTime.Now` | ✅ | |
| 1.10 | Prohibición de `DELETE` físico documentada | ✅ | |
| 1.11 | La nomenclatura español PascalCase es consistente | ✅ | |

---

## 2. Decisiones del PO

| # | Ítem | Resultado | Notas |
|---|------|-----------|-------|
| 2.1 | SQL Server como motor de BD documentado | ✅ | |
| 2.2 | xUnit como framework de testing | ✅ | |
| 2.3 | Sin despliegue en MVP (solo local/dev) | ✅ | |
| 2.4 | 50-100 usuarios concurrentes como supuesto | ✅ | |
| 2.5 | Auto-expiración dinámica (startDate ≤ hoy) | ✅ | |
| 2.6 | Carry-over sin límite de acumulación | ✅ | |
| 2.7 | Horizonte futuro de 2 meses | ✅ | |
| 2.8 | Feriados NO excluidos del cómputo (solo sáb/dom) | ✅ | |
| 2.9 | Paginación offset-based | ✅ | |
| 2.10 | `pendingBalance` implementado (saldo comprometido) | ✅ | |

---

## 3. Definición de Entidades

| # | Ítem | Resultado | Notas |
|---|------|-----------|-------|
| 3.1 | `Empleado` con todos sus atributos | ✅ | |
| 3.2 | `SolicitudVacaciones` con todos sus atributos | ✅ | |
| 3.3 | `SaldoEmpleado` con `accumulated`, `consumed`, `pending`, `available` | ✅ | |
| 3.4 | `HistorialSolicitud` con trazabilidad granular | ✅ | |
| 3.5 | `HistorialSaldo` correctamente marcado como fuera de MVP | ✅ | |
| 3.6 | `RowVersion` para concurrencia optimista en entidades críticas | ✅ | |
| 3.7 | `RangoFechas` value object definido | ✅ | |
| 3.8 | `DiasHabiles` value object **presente en el árbol de proyecto** | ❌ | Mencionado en sección 4 pero ausente en sección 8 |
| 3.9 | Enums (`EstadoSolicitud`, `RolUsuario`) definidos | ✅ | |
| 3.10 | Excepciones de dominio listadas | ✅ | 4 excepciones |

---

## 4. Estructura del Proyecto

| # | Ítem | Resultado | Notas |
|---|------|-----------|-------|
| 4.1 | `Vacations.Domain/` con carpetas Entities, Enums, ValueObjects, Exceptions, Abstractions | ✅ | |
| 4.2 | `Vacations.Application/` con Commands, Queries, Saldos, Expiracion | ✅ | |
| 4.3 | `Vacations.Infrastructure/` con Persistence, Identity, Time, BackgroundServices | ✅ | |
| 4.4 | `Vacations.Web/` con Controllers, ViewModels, Views, Authorization | ✅ | |
| 4.5 | Proyectos de test por capa (4 proyectos) | ✅ | |
| 4.6 | Estrategia de reescritura vs migración claramente definida | ✅ | Reescritura directa |
| 4.7 | `docs/diagrams/` contemplado | ✅ | |
| 4.8 | El scaffold existente se elimina explícitamente | ✅ | |

---

## 5. Contrato de API

| # | Ítem | Resultado | Notas |
|---|------|-----------|-------|
| 5.1 | 13 endpoints definidos con método, ruta y caso de uso | ✅ | |
| 5.2 | CRUD completo de solicitudes (crear, listar, detalle, editar, cancelar) | ✅ | |
| 5.3 | Flujo de aprobación completo (bandeja, aprobar, rechazar) | ✅ | |
| 5.4 | Cancelación de Approved antes del inicio | ✅ | |
| 5.5 | Consultas RRHH (listar, filtrar, ver saldo de empleado) | ✅ | |
| 5.6 | Parámetros de paginación documentados (`?page=&pageSize=`) | ❌ | No se muestran |
| 5.7 | Formatos de respuesta / códigos HTTP documentados | ❌ | No hay códigos de respuesta |
| 5.8 | Endpoints de autenticación (login, logout) incluidos | ⚠️ | Mencionados en DESIGN_TOKENS pero no en plan |

---

## 6. Decisiones Técnicas y Patrones

| # | Ítem | Resultado | Notas |
|---|------|-----------|-------|
| 6.1 | FluentValidation para validación de entrada (no auto-pipeline) | ✅ | Mencionado |
| 6.2 | Rate limiting documentado | ⚠️ | Valor inconsistente (10 vs 5/min para auth) |
| 6.3 | Estrategia de logging definida (ILogger, Serilog, etc.) | ❌ | No mencionado |
| 6.4 | Manejo de errores global / Problem Details | ❌ | No mencionado |
| 6.5 | Estrategia de mapeo (AutoMapper, manual, etc.) | ❌ | No mencionado |
| 6.6 | CQRS library (MediatR o similar) definida | ❌ | No mencionado |
| 6.7 | Concurrencia optimista con `RowVersion` + manejo de `DbUpdateConcurrencyException` | ✅ | |
| 6.8 | Interceptor de auditoría (`InterceptorAuditoriaSaveChanges`) | ✅ | |
| 6.9 | BackgroundService para auto-expiración | ✅ | |
| 6.10 | Mecanismo para `AcumularSaldoMensualCommand` definido | ❌ | ¿BackgroundService? ¿Job manual? |
| 6.11 | Zona horaria manejada (UTC vs local, timezone corporativo) | ❌ | No mencionado |
| 6.12 | CSS/JS architecture referenciada desde DESIGN_TOKENS | ❌ | No hay referencia |

---

## 7. Testing

| # | Ítem | Resultado | Notas |
|---|------|-----------|-------|
| 7.1 | Pirámide de pruebas respetada (unitarias, integración, E2E) | ✅ | |
| 7.2 | Framework xUnit confirmado | ✅ | |
| 7.3 | Cobertura ≥ 80% en Domain y Application | ✅ | |
| 7.4 | Pruebas de concurrencia mencionadas | ✅ | |
| 7.5 | Pruebas de seguridad (IDOR, auto-aprobación, CSRF) mencionadas | ⚠️ | En constitution, no en plan |
| 7.6 | E2E con Playwright para flujos críticos | ✅ | Mencionado |

---

## 8. Riesgos y Dependencias

| # | Ítem | Resultado | Notas |
|---|------|-----------|-------|
| 8.1 | 6 riesgos técnicos identificados con impacto, probabilidad y mitigación | ✅ | |
| 8.2 | Grafo de dependencias entre features documentado | ✅ | 001→002→003→004→005 |
| 8.3 | Orden de implementación definido | ✅ | |
| 8.4 | Riesgo de disponibilidad de .NET 10 estable evaluado | ❌ | No mencionado |
| 8.5 | Dependencia de SQL Server en local mitigada (LocalDB) | ✅ | |

---

## 9. Documentación Adicional Requerida

| # | Ítem | Resultado | Notas |
|---|------|-----------|-------|
| 9.1 | `design.md` listado como documento posterior | ✅ | Prioridad Alta |
| 9.2 | `tasks.md` listado como documento posterior | ✅ | Prioridad Media |
| 9.3 | `test-plan.md` listado como documento posterior | ✅ | Prioridad Media |
| 9.4 | Diagramas Mermaid (state-machine, sequence-approval) listados | ✅ | |

---

## 10. Calidad General del Documento

| # | Ítem | Resultado | Notas |
|---|------|-----------|-------|
| 10.1 | Estructura clara y coherente (14 secciones) | ✅ | |
| 10.2 | Lenguaje consistente (español técnico) | ✅ | |
| 10.3 | Tablas y diagramas para facilitar lectura | ✅ | |
| 10.4 | Enlaces cruzados a `spec.md`, `use-cases.md`, `constitution.md` | ✅ | |
| 10.5 | Sin contradicciones internas graves | ⚠️ | Rate limiting inconsistente (10 vs 5/min) |
| 10.6 | El plan es accionable (se puede implementar directamente) | ✅ | Con los gaps señalados |

---

## Resumen

| Categoría | Total Ítems | ✅ | ⚠️ | ❌ | Puntaje |
|-----------|-------------|---|---|---|---------|
| 1. Alineación | 11 | 10 | 1 | 0 | 95% |
| 2. Decisiones PO | 10 | 10 | 0 | 0 | 100% |
| 3. Entidades | 10 | 9 | 0 | 1 | 90% |
| 4. Estructura | 8 | 8 | 0 | 0 | 100% |
| 5. API | 8 | 5 | 1 | 2 | 69% |
| 6. Decisiones Técnicas | 12 | 5 | 1 | 6 | 46% |
| 7. Testing | 6 | 5 | 1 | 0 | 92% |
| 8. Riesgos | 5 | 4 | 0 | 1 | 80% |
| 9. Documentos | 4 | 4 | 0 | 0 | 100% |
| 10. Calidad General | 6 | 5 | 1 | 0 | 92% |
| **Total** | **80** | **65** | **5** | **10** | **85%** |

---

### Acciones recomendadas prioritarias

1. **Añadir `DiasHabiles.cs`** al árbol de ValueObjects en sección 8
2. **Resolver inconsistencia de rate limiting** (5 vs 10/min para auth)
3. **Añadir parámetros de paginación** al contrato API (sección 6)
4. **Definir códigos de respuesta HTTP** en cada endpoint
5. **Definir estrategia de logging** (ILogger, Serilog, etc.)
6. **Definir manejo de errores global** (Problem Details, middleware)
7. **Definir estrategia de mapeo** Domain ↔ ViewModels
8. **Definir mecanismo del job mensual** de acumulación de saldo
9. **Añadir manejo de zona horaria**
10. **Referenciar DESIGN_TOKENS.md** para CSS/JS architecture
