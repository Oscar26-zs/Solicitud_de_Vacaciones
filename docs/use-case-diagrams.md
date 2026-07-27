# Diagramas de Casos de Uso — Mermaid

> Generado desde `docs/use-cases.md` (v2 — 2026-07-27)
> `«actor»` = persona o sistema que inicia la acción

## Índice

| # | Caso de Uso | Actor(es) | Tipo |
|---|-------------|-----------|------|
| [CU-01](#cu01--crear-empleado-y-saldo-inicial) | Crear empleado y saldo inicial | Admin | Principal |
| [CU-02](#cu02--calcularacumular-saldo-mensual) | Calcular/acumular saldo mensual | Sistema_Acumulacion | Principal |
| [CU-03](#cu03--consultar-saldo-personal--histórico) | Consultar saldo personal / histórico | Empleado, RRHH | Principal |
| [CU-04](#cu04--registrar-movimientos-de-balance) | Registrar movimientos de balance | Sistema | Transversal |
| [CU-05](#cu05--crear-solicitud-pending) | Crear solicitud de vacaciones | Empleado | Principal |
| [CU-06](#cu06--ver-mis-solicitudes--detalle) | Ver mis solicitudes / detalle | Empleado | Principal |
| [CU-07](#cu07--editar-solicitud-pending) | Editar solicitud PENDING | Empleado | Principal |
| [CU-08](#cu08--cancelar-solicitud-por-empleado) | Cancelar solicitud (empleado) | Empleado | Principal |
| [CU-09](#cu09--cálculo-de-días-hábiles) | Cálculo de días hábiles | Sistema | Transversal |
| [CU-10](#cu10--prevención-de-traslapes) | Prevención de traslapes | Sistema | Transversal |
| [CU-11](#cu11--bandeja-de-aprobadores) | Bandeja de aprobadores | Aprobador | Principal |
| [CU-12](#cu12--aprobar-solicitud) | Aprobar solicitud | Aprobador | Principal |
| [CU-13](#cu13--rechazar-solicitud) | Rechazar solicitud | Aprobador | Principal |
| [CU-14](#cu14--ver-impacto-en-saldo) | Ver impacto en saldo | Aprobador | Extensión |
| [CU-15](#cu15--cancelar-approved-por-aprobador) | Cancelar APPROVED (aprobador) | Aprobador | Principal |
| [CU-16](#cu16--auto-expiración-pending--expired) | Auto-expiración PENDING → EXPIRED | Sistema_Expiracion | Principal |
| [CU-17](#cu17--gestión-de-roles-y-permisos) | Gestión de roles y permisos | Sistema | Transversal |
| [CU-18](#cu18--auditoría-y-trazabilidad-global) | Auditoría y trazabilidad global | Sistema | Transversal |
| [CU-19](#cu19--filtrado-y-consultas-para-rrhh) | Filtrado y consultas para RRHH | RRHH | Principal |
| [CU-20](#cu20--mensajes-ux-y-manejo-de-errores) | Mensajes UX y manejo de errores | Sistema | Transversal |

---

## CU-01 — Crear empleado y saldo inicial

```mermaid
graph TB
    subgraph S["Sistema de Vacaciones"]
        CU01(("CU-01: Crear empleado<br/>y saldo inicial"))
        V1[Verificar que el correo<br/>tenga formato válido]
        V2[Confirmar que el correo<br/>no esté registrado]
        V3[Verificar que el nombre<br/>no esté vacío]
        V4[Verificar que la fecha<br/>de ingreso no sea futura]
        R1[Asignar permisos<br/>de empleado]
        R2[Guardar empleado<br/>con estado Activo]
        R3[Crear registro de saldo<br/>con valores en cero]
        R4[Confirmar creación<br/>y entregar identificador]

        CU01 --> V1 --> V2 --> V3 --> V4 --> R1 --> R2 --> R3 --> R4
    end
    Admin["«actor» Admin"] --> CU01
```

---

## CU-02 — Calcular/acumular saldo mensual

```mermaid
graph TB
    subgraph S["Sistema de Vacaciones"]
        CU02(("CU-02: Acumulación automática<br/>de saldo mensual"))
        P1[Proceso revisa todos<br/>los empleados activos]
        P2[Por cada empleado:<br/>revisar fecha de ingreso<br/>y último cálculo]
        P3{¿Hay meses completos<br/>no contabilizados?}
        P4[Sí: aumentar saldo<br/>acumulado en 1 día<br/>por cada mes]
        P5[Recalcular saldo<br/>disponible]
        P6[Registrar en historial<br/>de saldo: quién, cuándo<br/>y motivo]
        P7[Actualizar fecha<br/>del último cálculo]
        P8[Siguiente empleado]

        P1 --> P2 --> P3
        P3 -->|Sí| P4 --> P5 --> P6 --> P7 --> P8
        P3 -->|No| P8
    end
    SA["«actor» Sistema_Acumulacion<br/>(Proceso automático)"] --> CU02
```

---

## CU-03 — Consultar saldo personal / histórico

```mermaid
graph TB
    subgraph S["Sistema de Vacaciones"]
        CU03(("CU-03: Consultar saldo<br/>y movimientos"))
        A1{¿Es empleado?}
        A2{¿Consulta su<br/>propio saldo?}
        A3[Permitir consulta]
        A4[Denegar: no autorizado]
        Q1[Consultar registro de saldo]
        Q2[Entregar: saldo acumulado,<br/>consumido y disponible]
        Q3{¿Solicitó ver<br/>historial?}
        Q4[Entregar lista de<br/>movimientos ordenada<br/>del más reciente al más antiguo]

        CU03 --> A1
        A1 -->|Sí| A2
        A2 -->|Sí| A3
        A2 -->|No| A4
        A1 -->|No, es RRHH| A3
        A3 --> Q1 --> Q2 --> Q3
        Q3 -->|Sí| Q4
        Q3 -->|No| Fin
    end
    Emp["«actor» Empleado"] --> CU03
    RRHH["«actor» RRHH"] --> CU03
```

---

## CU-04 — Registrar movimientos de balance

```mermaid
graph LR
    subgraph S["Sistema de Vacaciones"]
        CU04(("CU-04: Registrar<br/>historial de saldo"))
        T1["Tipos de movimiento:<br/>ACUMULACIÓN<br/>DESCUENTO POR APROBACIÓN<br/>RESTAURACIÓN POR CANCELACIÓN"]
        T2["Cada registro guarda:<br/>valor anterior, valor nuevo,<br/>motivo, quién lo hizo,<br/>fecha y hora"]
        T3["Se guarda junto con<br/>la operación principal:<br/>todo o nada"]
        CU04 --> T1 --> T2 --> T3
    end
    Sis["«actor» Sistema"] --> CU04
```

---

## CU-05 — Crear solicitud PENDING

```mermaid
graph TB
    subgraph S["Sistema de Vacaciones"]
        CU05(("CU-05: Crear solicitud<br/>de vacaciones"))
        V1[Validar motivo:<br/>obligatorio y<br/>mínimo 10 caracteres]
        V2[Validar fechas:<br/>inicio desde mañana,<br/>fin no anterior a inicio]
        V3[Calcular<br/>días hábiles]
        V4[Validar: mínimo 1 día,<br/>sin fracciones]
        V5[Validar: saldo disponible<br/>>= días solicitados]
        V6[Verificar<br/>que no haya empalmes]
        V7[Mostrar resumen al empleado:<br/>días, saldo actual,<br/>saldo estimado]
        V8[Empleado confirma]
        V9[Guardar solicitud<br/>como Pendiente]
        V10[Registrar en historial:<br/>solicitud creada]
        V11[Hacer visible en<br/>bandeja de aprobadores]

        CU05 --> V1 --> V2 --> V3 --> V4 --> V5 --> V6 --> V7 --> V8 --> V9 --> V10 --> V11
    end
    Emp["«actor» Empleado"] --> CU05
```

---

## CU-06 — Ver mis solicitudes / detalle

```mermaid
graph TB
    subgraph S["Sistema de Vacaciones"]
        CU06(("CU-06: Ver mis<br/>solicitudes"))
        V1{¿El empleado ve<br/>solo sus propias<br/>solicitudes?}
        V2[Denegar acceso]
        F1[Aplicar filtro<br/>por estado si se indicó]
        F2[Entregar lista ordenada<br/>de la más reciente<br/>a la más antigua]
        D1{Ruta: ver detalle<br/>de una solicitud?}
        D2[Entregar todos los datos<br/>+ historial de eventos]

        CU06 --> V1
        V1 -->|No| V2
        V1 -->|Sí| F1 --> F2
        D1 -->|Sí| D2
    end
    Emp["«actor» Empleado"] --> CU06
```

---

## CU-07 — Editar solicitud PENDING

```mermaid
graph TB
    subgraph S["Sistema de Vacaciones"]
        CU07(("CU-07: Editar solicitud<br/>pendiente"))
        V1{¿Solicitud está<br/>pendiente?}
        V2[Denegar: solo se pueden<br/>editar pendientes]
        V3{¿Es el autor?}
        V4[Denegar: no autorizado]
        V5[Aplicar validaciones:<br/>motivo, fechas, saldo,<br/>empalmes]
        V5a[Recalcular<br/>días hábiles]
        V5b[Revisar<br/>empalmes]
        V6[Guardar cambios:<br/>fechas, motivo, días]
        V7[Registrar en historial:<br/>qué cambió, valor anterior,<br/>valor nuevo]
        V8[Actualizar en bandeja<br/>de aprobadores]

        CU07 --> V1
        V1 -->|No| V2
        V1 -->|Sí| V3
        V3 -->|No| V4
        V3 -->|Sí| V5
        V5 --> V5a --> V5b --> V6 --> V7 --> V8
    end
    Emp["«actor» Empleado"] --> CU07
```

---

## CU-08 — Cancelar solicitud por empleado

```mermaid
graph TB
    subgraph S["Sistema de Vacaciones"]
        CU08(("CU-08: Cancelar solicitud<br/>propia pendiente"))
        V1{¿Solicitud está<br/>pendiente?}
        V2[Denegar]
        V3{¿Es el autor?}
        V4[Denegar]
        C1[Mostrar confirmación:<br/>¿Está seguro de<br/>cancelar esta solicitud?]
        C2[Empleado confirma]
        C3[Cambiar estado<br/>a Cancelada]
        C4[Registrar en historial:<br/>quién canceló y cuándo]
        C5[Notificar a aprobadores]
        C6[No modificar el saldo<br/>del empleado]

        CU08 --> V1
        V1 -->|No| V2
        V1 -->|Sí| V3
        V3 -->|No| V4
        V3 -->|Sí| C1 --> C2 --> C3 --> C4 --> C5 --> C6
    end
    Emp["«actor» Empleado"] --> CU08
```

---

## CU-09 — Cálculo de días hábiles

```mermaid
graph LR
    subgraph S["Sistema de Vacaciones"]
        CU09(("CU-09: Calcular<br/>días hábiles"))
        L1[Recibir fecha inicio<br/>y fecha fin]
        L2[Revisar cada día<br/>del período]
        L3{¿Sábado o<br/>domingo?}
        L4[No contar ese día]
        L5[Contar como<br/>día solicitado]
        L6[Entregar total<br/>de días contados]

        L1 --> L2
        L2 --> L3
        L3 -->|Sí| L4
        L3 -->|No| L5
        L5 --> L2
        L4 --> L2
        L2 -->|Fin del período| L6
    end
    Sis["«actor» Sistema"] --> CU09
```

---

## CU-10 — Prevención de traslapes

```mermaid
graph TB
    subgraph S["Sistema de Vacaciones"]
        CU10(("CU-10: Verificar<br/>empalmes"))
        B1[Buscar solicitudes del<br/>mismo empleado que:<br/>- estén pendientes o aprobadas<br/>- tengan fechas que se empalmen]
        B2{¿Hay empalme<br/>con alguna?}
        B3[Bloquear: La solicitud<br/>incluye días ya<br/>comprometidos]
        B4[Permitir: no hay<br/>empalmes]

        B1 --> B2
        B2 -->|Sí| B3
        B2 -->|No| B4
    end
    Sis["«actor» Sistema"] --> CU10
```

---

## CU-11 — Bandeja de aprobadores

```mermaid
graph TB
    subgraph S["Sistema de Vacaciones"]
        CU11(("CU-11: Bandeja de<br/>aprobadores"))
        V1{¿Usuario tiene<br/>permisos de<br/>aprobador activo?}
        V2[Denegar acceso]
        L1[Buscar solicitudes<br/>pendientes de todos]
        L2[Excluir las que el<br/>aprobador creó]
        L3[Aplicar filtros:<br/>empleado, fechas, días]
        L4[Ordenar de la más<br/>antigua a la más reciente]
        L5[Por cada solicitud:<br/>verificar empalmes]
        L6{¿Empalma con<br/>aprobada?}
        L7[Deshabilitar botón<br/>aprobar + advertencia]
        L8[Mostrar advertencia:<br/>Existen otras solicitudes<br/>que se solapan]

        CU11 --> V1
        V1 -->|No| V2
        V1 -->|Sí| L1 --> L2 --> L3 --> L4 --> L5
        L5 --> L6
        L6 -->|Sí| L7
        L6 -->|No, solo pendiente| L8
        L6 -->|Sin empalme| L9[Mostrar normal]
    end
    Ap["«actor» Aprobador"] --> CU11
```

---

## CU-12 — Aprobar solicitud

```mermaid
graph TB
    subgraph S["Sistema de Vacaciones"]
        CU12(("CU-12: Aprobar solicitud<br/>pendiente"))
        V1{¿Solicitud existe<br/>y está pendiente?}
        V2[Denegar]
        V3{¿Aprobador activo<br/>y no es el autor?}
        V4[Denegar: auto-aprobación<br/>no permitida]
        S1{¿Saldo suficiente<br/>al momento de<br/>aprobar?}
        S2[Denegar: Saldo<br/>insuficiente]
        C1[Asegurar que otro<br/>aprobador no haya<br/>procesado ya]
        T1[Todo junto:]
        T2[Estado = Aprobada]
        T3[Guardar acción de<br/>aprobación]
        T4[Descontar días<br/>del saldo consumido]
        T5[Registrar en<br/>historial de saldo:<br/>DESCUENTO POR APROBACIÓN]
        T6[Registrar en historial<br/>de solicitud: Aprobada]
        T7[Notificar al empleado]

        CU12 --> V1
        V1 -->|No| V2
        V1 -->|Sí| V3
        V3 -->|No| V4
        V3 -->|Sí| S1
        S1 -->|No| S2
        S1 -->|Sí| C1 --> T1
        T1 --> T2 --> T3 --> T4 --> T5 --> T6 --> T7
    end
    Ap["«actor» Aprobador"] --> CU12
```

---

## CU-13 — Rechazar solicitud

```mermaid
graph TB
    subgraph S["Sistema de Vacaciones"]
        CU13(("CU-13: Rechazar solicitud<br/>pendiente"))
        C1[Validar comentario:<br/>obligatorio y<br/>máximo 500 caracteres]
        C2[Denegar: El comentario<br/>es obligatorio]
        V1{¿Aprobador válido,<br/>activo y no es<br/>el autor?}
        V2[Denegar]
        T1[Estado = Rechazada]
        T2[Guardar acción de<br/>rechazo con comentario]
        T3[Guardar comentario<br/>visible al empleado]
        T4[Registrar en historial]
        T5[No modificar saldo]
        T6[Notificar empleado<br/>con el motivo<br/>de rechazo]

        CU13 --> C1
        C1 -->|Inválido| C2
        C1 -->|Válido| V1
        V1 -->|No| V2
        V1 -->|Sí| T1 --> T2 --> T3 --> T4 --> T5 --> T6
    end
    Ap["«actor» Aprobador"] --> CU13
```

---

## CU-14 — Ver impacto en saldo

```mermaid
graph LR
    subgraph S["Sistema de Vacaciones"]
        CU14(("CU-14: Ver impacto<br/>en saldo"))
        C1[Consultar saldo<br/>actual del empleado]
        C2[Calcular saldo estimado<br/>= saldo actual<br/>- días solicitados]
        C3{¿Saldo estimado<br/>negativo?}
        C4[Advertir: Esta<br/>aprobación excedería<br/>el saldo disponible]
        C5[Mostrar: saldo actual,<br/>días solicitados y<br/>saldo estimado]

        C1 --> C2 --> C3
        C3 -->|Sí| C4
        C3 -->|No| C5
    end
    Ap["«actor» Aprobador"] --> CU14

```

---

## CU-15 — Cancelar APPROVED por aprobador

```mermaid
graph TB
    subgraph S["Sistema de Vacaciones"]
        CU15(("CU-15: Cancelar solicitud<br/>aprobada antes del inicio"))
        V1{¿Solicitud está<br/>aprobada?}
        V2[Denegar]
        V3{¿Fecha de inicio<br/>aún no ha llegado?}
        V4[Denegar: Periodo<br/>ya iniciado]
        T1[Todo junto:]
        T2[Estado = Cancelada]
        T3[Guardar acción de<br/>cancelación]
        T4[Restaurar saldo:<br/>devolver días al<br/>saldo consumido]
        T5[Registrar en<br/>historial de saldo:<br/>RESTAURACIÓN POR CANCELACIÓN]
        T6[Registrar en historial<br/>de solicitud: Cancelada<br/>por aprobador]
        T7[Notificar al empleado]

        CU15 --> V1
        V1 -->|No| V2
        V1 -->|Sí| V3
        V3 -->|No| V4
        V3 -->|Sí| T1
        T1 --> T2 --> T3 --> T4 --> T5 --> T6 --> T7
    end
    Ap["«actor» Aprobador"] --> CU15
```

---

## CU-16 — Auto-expiración PENDING → EXPIRED

```mermaid
graph TB
    subgraph S["Sistema de Vacaciones"]
        CU16(("CU-16: Auto-expiar<br/>solicitudes pendientes"))
        J1[Proceso diario con<br/>N días configurado<br/>para vencer]
        J2[Buscar pendientes con<br/>más de N días de<br/>antigüedad]
        J3{¿Hay solicitudes<br/>para expirar?}
        J4[Por cada una:]
        J5[Estado = Expirada]
        J6[Registrar en historial:<br/>sistema automático,<br/>fecha y motivo]
        J7[Dejar notificación<br/>pendiente]
        J8[Guardar cuántas<br/>solicitudes expiraron]
        J9[No modificar saldo<br/>del empleado]

        J1 --> J2 --> J3
        J3 -->|Sí| J4 --> J5 --> J6 --> J7 --> J8
        J3 -->|No| J8
        CU16 --> J9
    end
    SE["«actor» Sistema_Expiracion<br/>(Proceso automático)"] --> CU16
```

---

## CU-17 — Gestión de roles y permisos

```mermaid
graph LR
    subgraph S["Sistema de Vacaciones"]
        CU17(("CU-17: Control de<br/>acceso por rol"))
        R1["Roles:<br/>Empleado | Aprobador | RRHH"]
        R2["Cada operación verifica<br/>que el usuario tenga<br/>el permiso necesario"]
        R3["Si no está autenticado<br/>o no tiene permiso,<br/>se deniega el acceso"]
        CU17 --> R1 --> R2 --> R3
    end
    Sis["«actor» Sistema"] --> CU17
```

---

## CU-18 — Auditoría y trazabilidad global

```mermaid
graph LR
    subgraph S["Sistema de Vacaciones"]
        CU18(("CU-18: Auditoría<br/>y trazabilidad"))
        VRH["Historial de solicitudes:<br/>CREADA | ACTUALIZADA |<br/>CAMBIO DE ESTADO | CANCELADA"]
        BH["Historial de saldo:<br/>ACUMULACIÓN |<br/>DESCUENTO POR APROBACIÓN |<br/>RESTAURACIÓN POR CANCELACIÓN"]
        Q["RRHH y empleados<br/>pueden consultar<br/>el historial"]
        CU18 --> VRH
        CU18 --> BH
        CU18 --> Q
    end
    Sis["«actor» Sistema"] --> CU18
```

---

## CU-19 — Filtrado y consultas para RRHH

```mermaid
graph TB
    subgraph S["Sistema de Vacaciones"]
        CU19(("CU-19: Consultas<br/>para RRHH"))
        V1{¿Usuario tiene<br/>permisos de RRHH?}
        V2[Denegar acceso]
        F1[Aplicar filtros:<br/>estado, empleado,<br/>rango de fechas]
        F2[Entregar lista ordenada<br/>de la más reciente<br/>a la más antigua]
        F3[Solo visualización:<br/>sin botones de aprobar,<br/>rechazar ni editar]
        F4{¿Sin resultados?}
        F5[Mostrar que no se<br/>encontraron solicitudes]

        CU19 --> V1
        V1 -->|No| V2
        V1 -->|Sí| F1 --> F2 --> F3
        F4 -->|Sí| F5
        F4 -->|No| F6[Mostrar resultados]
    end
    RRHH["«actor» RRHH"] --> CU19
```

---

## CU-20 — Mensajes UX y manejo de errores

```mermaid
graph LR
    subgraph S["Sistema de Vacaciones"]
        CU20(("CU-20: Mensajes y<br/>errores"))
        M1["El sistema entrega<br/>mensajes claros cuando<br/>algo no es válido"]
        M2["La pantalla muestra<br/>el mensaje al usuario"]
        M3["Errores inesperados:<br/>mensaje genérico<br/>sin detalles técnicos"]
        M4["Confirmación antes de<br/>acciones importantes:<br/>ej. cancelar solicitud"]
        CU20 --> M1 --> M2
        CU20 --> M3
        CU20 --> M4
    end
    Sis["«actor» Sistema"] --> CU20
```
