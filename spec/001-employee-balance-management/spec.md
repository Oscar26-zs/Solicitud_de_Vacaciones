# Feature 1: Gestión de Empleados y Saldos

**Versión**: 1.0  
**Última actualización**: 2026-07-17  
**Estado**: En especificación

---

## Resumen

Este feature cubre la gestión del ciclo de vida de empleados y sus saldos de días de vacaciones. Incluye acumulación automática de saldo (1 día por mes laborado completo), consulta de saldo disponible, y el cálculo de saldo consumido. El sistema soporta carry-over ilimitado entre períodos sin límite superior. La creación de empleados se realizará mediante seed inicial de datos (fuera del alcance del MVP).

---

## Alcance

### Incluido

- ✅ Acumulación automática de saldo (1 día por mes completo laborado desde fecha de ingreso)
- ✅ Consulta de saldo disponible (HU-04)
- ✅ Cálculo de saldo consumido (sumatoria de APROBADAS + CANCELADAS incompletas)
- ✅ Carry-over ilimitado entre períodos sin tope definido
- ✅ Modelo de datos de empleado y balance
- ✅ Historial de cambios de saldo

### Excluido (fuera de MVP)

- ❌ Creación de empleados (se realizará mediante seed inicial de datos)
- ❌ Creación de aprobadores (se realizará mediante seed inicial de datos)
- ❌ Offboarding de empleados
- ❌ Gestión de jefes directos o estructuras jerárquicas
- ❌ Integración con nómina o sistemas externos

---

## Historias de Usuario

### HU-04: Consultar mi saldo de días disponibles

**Como** empleado  
**Quiero** ver mi saldo total anual, saldo actual e histórico de descuentos por aprobadas  
**Para** saber cuánto tiempo de vacaciones tengo disponible

#### Criterios de Aceptación (Formato EARS)

1. **Cuando** el empleado accede a "Mi saldo"
   - **Entonces** el sistema debe mostrar:
	 - Saldo inicial anual (calculado por acumulación desde fecha ingreso)
	 - Días consumidos (sumatoria de solicitudes APROBADAS)
	 - Saldo disponible = Saldo inicial - Días consumidos
   - **Y** la información debe incluir:
	 - Fecha de consulta
	 - Unidad de tiempo (días)

2. **Si** el empleado solicita un desglose histórico
   - **Entonces** el sistema debe mostrar:
	 - Movimientos de saldo (acumulaciones mensuales)
	 - Descuentos por solicitudes APROBADAS
	 - Restauraciones por solicitudes CANCELADAS (antes del inicio del período)
	 - Timestamp y actor para cada movimiento

3. **Mientras** el empleado está activo
   - **Entonces** el saldo debe ser siempre consultable
   - **Y** debe reflejar en tiempo real los cambios resultado de aprobaciones/cancelaciones

---

## Reglas de Negocio

### RN-01: Acumulación automática de saldo

- **Descripción**: Cada empleado acumula 1 día de vacaciones por cada mes completo laborado desde su fecha de ingreso.
- **Cálculo**: 
  - Mes completo laborado = mes calendario completo desde la fecha de ingreso (no mes natural)
  - Acumulación = (Meses completos desde ingreso) × 1 día
  - Sin tope máximo (carry-over ilimitado)
- **Casos especiales**:
  - Empleado ingresa a mitad de mes: cuenta como mes incompleto (no acumula hasta completar un mes calendario desde su fecha de ingreso)
  - Transferencia de período a período: saldo sobrante pasa completo al siguiente período

### RN-23: Cálculo de saldo consumido

- **Descripción**: El saldo consumido es la sumatoria de días en solicitudes APROBADAS.
- **Fórmula**: 
  ```
  Saldo consumido = SUM(días en solicitudes con estado APROBADA)
  ```
- **Consideraciones**:
  - Solo incluye solicitudes APROBADAS (estado = Approved)
  - No incluye rechazadas ni expiradas
  - Cada día solicitado cuenta como 1 (sin fraccionamientos)

### RN-24: Saldo disponible

- **Descripción**: El saldo disponible es la diferencia entre el saldo acumulado y el consumido.
- **Fórmula**:
  ```
  Saldo disponible = Saldo acumulado - Saldo consumido
  ```
- **Restricciones**:
  - Nunca puede ser negativo (validación en punto de aprobación)
  - Debe ser visible para el empleado en tiempo real
  - Cambios en solicitudes APROBADAS/CANCELADAS se reflejan inmediatamente

---

## Modelo de Datos

### Entidad: Employee

```
id: UUID (PK)
email: string (unique)
fullName: string
status: Enum (Active, Inactive)
joinDate: DateTime
createdAt: DateTime
updatedAt: DateTime
```

### Entidad: EmployeeBalance

```
id: UUID (PK)
employeeId: UUID (FK -> Employee)
accumulatedBalance: int (saldo acumulado, calculado)
consumedBalance: int (saldo consumido)
availableBalance: int (computado: acumulado - consumido)
lastUpdatedAt: DateTime (timestamp del último cambio)
```

### Entidad: BalanceHistory (auditoría)

```
id: UUID (PK)
employeeId: UUID (FK -> Employee)
movementType: Enum (ACUMULATION, APPROVAL_DISCOUNT, CANCELLATION_RESTORE)
previousBalance: int
newBalance: int
reason: string (e.g., "Approved request #123")
actor: string (SISTEMA_ACUMULACION, SISTEMA_AUTO_EXPIRACION, user@email)
timestamp: DateTime
```

---

## Validaciones

| Validación | Condición | Mensaje de Error |
|---|---|---|
| Saldo no negativo | `disponible >= 0` | "No se puede aprobar: saldo insuficiente" |
| Acumulación mínima | Empleado debe tener al menos 1 mes laborado | "Empleado aún no tiene saldo acumulado" |
| Fecha de ingreso válida | `joinDate <= hoy` | "Fecha de ingreso inválida" |

---

## Comportamiento del Sistema

### Acumulación mensual automática

- **Cuándo**: Fin de cada mes calendario o en trigger diario que detecte fin de mes
- **Quién**: Sistema automático (actor = "SISTEMA_ACUMULACION")
- **Qué**: 
  1. Calcular meses completos desde `joinDate` hasta hoy
  2. Incrementar `accumulatedBalance` en +1 por cada mes
  3. Registrar en `BalanceHistory`
  4. Actualizar `availableBalance` = acumulado - consumido

### Consulta de saldo

- **Permiso**: El empleado accede a su propio saldo; RRHH puede consultar cualquier empleado
- **Tiempo real**: La consulta siempre refleja el estado actual (acumulado - consumido)

### Restauración de saldo por cancelación

- **Cuándo**: Se cancela una solicitud APROBADA antes de que inicie el período
- **Quién**: Usuario aprobador (actor = email del aprobador)
- **Qué**:
  1. Restar días de `consumedBalance`
  2. Recalcular `availableBalance`
  3. Registrar en `BalanceHistory` con tipo = "CANCELLATION_RESTORE"

---

## Dependencias

- **Feature 2**: Solicitudes de Vacaciones (para calcular saldo consumido)
- **Feature 3**: Flujo de Aprobación (para descuentos y restauraciones)

---

## Consideraciones Técnicas

1. **Performance**: La consulta de saldo debe ser O(1) o O(log n) — precalcular y cachear si es necesario
2. **Integridad**: Transacciones ACID para cambios de saldo
3. **Auditoría**: Cada movimiento debe registrarse en `BalanceHistory`
4. **Timezone**: Usar UTC para todos los timestamps; conversión local solo en UI

---

## Plan de Implementación (paso a paso)

Ver [plan.md](./plan.md) para detalles técnicos.  
Ver [tasks.md](./tasks.md) para tareas prácticas.

---

## Criterios de Aceptación del Feature

- ✅ Datos de empleados cargados mediante seed inicial (creación fuera del MVP)
- ✅ Acumulación automática (+1 mes) funcionando mensualmente
- ✅ HU-04 implementada (consulta de saldo)
- ✅ Saldo consumido calculado correctamente
- ✅ Carry-over sin límite superior
- ✅ Auditoría de BalanceHistory completa
- ✅ Tests unitarios y de integración pasando

