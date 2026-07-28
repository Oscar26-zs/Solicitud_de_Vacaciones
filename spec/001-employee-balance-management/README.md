# Feature 1: Gestión de Empleados y Saldos

📁 **Carpeta**: `spec/features/001-employee-balance-management/`

---

## 📖 Documentación

| Archivo | Propósito |
|---------|-----------|
| [spec.md](./spec.md) | **Especificación funcional** — Requisitos, reglas de negocio, historias de usuario, modelo de datos |

---

## 🎯 Objetivo

Especificar el sistema de acumulación y gestión de saldos de días de vacaciones para empleados, incluyendo:
- ✅ Creación de empleados con saldo inicial = 0
- ✅ Acumulación automática de 1 día por mes laborado
- ✅ Consulta de saldo disponible (HU-04)
- ✅ Historial de cambios de saldo (auditoría)

---

## 🚀 Quick Start

### Para el Product Owner / Stakeholder

1. Lee [spec.md](./spec.md) **secciones 1-3** para entender el alcance
2. Revisa las **Historias de Usuario** en [spec.md](./spec.md) **sección 4**
3. Consulta **Reglas de Negocio** en [spec.md](./spec.md) **sección 5**

### Para el Analista / Arquitecto

1. Lee [spec.md](./spec.md) completo
2. Revisa el **Modelo de Datos** en [spec.md](./spec.md) **sección 6**
3. Entiende **Validaciones y Comportamiento** en [spec.md](./spec.md) **secciones 8-9**

### Para el Desarrollador

1. Lee [spec.md](./spec.md) **sección 6** (Modelo de Datos)


---

## 📊 Estado

| Aspecto | Estado |
|--------|--------|
| ✍️ Especificación | **Completa** |
| 📋 Plan | Pendiente |
| ✅ Tareas | Pendiente |
| 💻 Código | No iniciado |

---

## 🔗 Dependencias

- **No depende de**: Ningún otro feature
- **Depende de**: Estructura de proyecto base (.NET, BD, DI, etc.)
- **Es dependencia de**: 
  - Feature 2: Solicitudes de Vacaciones (CRUD Base)
  - Feature 3: Flujo de Aprobación

---

## 📋 Criterios de Aceptación (Feature Level)

- [ ] Especificación completa y aprobada por PO
- [ ] Modelo de datos documentado
- [ ] Reglas de negocio validadas
- [ ] Historia de usuario HU-04 especificada con criterios EARS

---

## 💡 Contenido de spec.md

**Sección 1: Resumen**
- Objetivo y alcance

**Sección 2: Alcance**
- Incluido vs Excluido

**Sección 3: Historia de Usuario**
- HU-04: Consultar saldo (con criterios EARS)

**Sección 4: Reglas de Negocio**
- RN-01: Acumulación automática
- RN-23: Cálculo de saldo consumido
- RN-24: Saldo disponible

**Sección 5: Modelo de Datos**
- Employee
- EmployeeBalance
- BalanceHistory 🔶 Fuera de alcance MVP (futura fase)

**Sección 6: Validaciones**
- Tabla de validaciones

**Sección 7: Comportamiento del Sistema**
- Acumulación mensual automática
- Consulta de saldo
- Restauración de saldo

---

**Última actualización**: 2026-07-17  
**Versión**: 1.0


