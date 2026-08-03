# Casos de Uso - Sistema de Solicitudes de Vacaciones

Última actualización: 2026-07-27

Este documento recoge los casos de uso refinados a partir de las especificaciones en `spec/spec.md`. Cada caso de uso ha sido detallado con pasos de validación, autorización, datos de auditoría y mensajes de error específicos según las reglas de negocio (RN) y requisitos funcionales (RF) definidos.

> **Nota:** CU-01 (Crear empleado y saldo inicial) queda fuera del alcance del MVP. Se asume que los empleados ya existen en el sistema al inicio del proyecto.

> **Leyenda:**
> - **Transversal** = Caso de uso que aplica de fondo en varios escenarios
> - **Extensión** = Extensión opcional de otro CU
> - Los mensajes entre comillas son los textos exactos definidos en spec/spec.md

## Índice de Casos de Uso

| # | Tipo | Caso de Uso | Actor(es) |
|---|------|-------------|-----------|
| CU-01 | Principal | Calcular/acumular saldo mensual | Sistema_Acumulacion (job) |
| CU-02 | Principal | Consultar saldo personal / histórico | Empleado, RRHH |
| CU-03 | Transversal | Registrar movimientos de balance | Sistema |
| CU-04 | Principal | Crear solicitud de vacaciones | Empleado |
| CU-05 | Principal | Ver mis solicitudes / detalle | Empleado |
| CU-06 | Principal | Editar solicitud PENDING | Empleado |
| CU-07 | Principal | Cancelar solicitud (empleado PENDING) | Empleado |
| CU-08 | Transversal | Cálculo de días hábiles | Sistema |
| CU-09 | Transversal | Prevención de traslapes | Sistema |
| CU-10 | Principal | Bandeja de aprobadores | Aprobador |
| CU-11 | Principal | Aprobar solicitud (descuento de saldo) | Aprobador |
| CU-12 | Principal | Rechazar solicitud con comentario | Aprobador |
| CU-13 | Extensión | Ver impacto en saldo antes de decidir | Aprobador |
| CU-14 | Principal | Cancelación APPROVED por aprobador | Aprobador |
| CU-15 | Principal | Auto-expiración PENDING → EXPIRED | Sistema_Expiracion (job) |
| CU-16 | Transversal | Gestión de roles y permisos | Sistema |
| CU-17 | Transversal | Auditoría y trazabilidad global | Sistema |
| CU-18 | Principal | Filtrado y consultas para RRHH | RRHH |
| CU-19 | Transversal | Mensajes UX y manejo de errores | Sistema |

---

## CU-01 — Calcular/acumular saldo mensual
**ID**: CU-01
**Nombre**: Acumulación automática de saldo
**Actor(es)**: SISTEMA_ACUMULACION (job programado)
**Componente**: EmployeeBalanceService / Scheduler
**Prioridad**: P1
**Trazabilidad**: `spec/spec.md` (RN-01, RN-23, RN-24)

### Precondiciones:
- Proceso automático configurado para ejecutarse periódicamente (ejemplo: cada día al finalizar la jornada).

### Flujo principal:
1. El proceso automático revisa todos los empleados que están activos en el sistema.
2. Por cada empleado:
   a. Obtener su fecha de ingreso a la empresa.
   b. Obtener la fecha del último cálculo realizado (si es la primera vez, este valor no existe).
   c. Si es la primera vez, usar la fecha de ingreso como punto de partida.
   d. Si ya se había calculado antes, usar la última fecha registrada como punto de partida.
   e. Calcular cuántos meses completos han transcurrido desde el punto de partida hasta la fecha actual.
   f. Si no hay meses completos nuevos, pasar al siguiente empleado.
   g. Por cada mes completo transcurrido, aumentar el saldo acumulado en 1 día.
   h. Recalcular el saldo disponible como la resta del saldo acumulado menos el saldo consumido.
   i. Registrar cada movimiento en el historial de saldo con:
      - Tipo de movimiento: "ACUMULACIÓN"
      - Valor anterior y valor nuevo del saldo acumulado
      - Motivo: "Acumulación mensual automática"
      - Quién lo hizo: "SISTEMA_ACUMULACION"
      - Fecha y hora del movimiento
   j. Actualizar la fecha del último cálculo al mes más reciente procesado.
3. El proceso guarda un registro de cuántos empleados se procesaron y cuántos días se acumularon en total.

### Flujos alternos:
- **FA-02a**: Si un empleado tiene fecha de ingreso inválida o futura, se salta y se registra el error para revisión administrativa.
- **FA-02b**: Si un empleado no tiene registro de saldo, se le crea uno con valores en cero y luego se procede a acumular.
- **FA-02c**: Los empleados inactivos se excluyen del proceso.

### Excepciones:
| Código | Condición | Mensaje |
|--------|-----------|---------|
| 500 | Error al persistir movimiento | "Error al registrar acumulación en el historial de saldo" |

### Postcondiciones:
- El saldo acumulado aumenta por cada mes completo que no se había contabilizado antes.
- El saldo disponible se recalcula como la diferencia entre acumulado y consumido.
- Cada acumulación queda registrada en el historial de saldo.
- Si el proceso se ejecuta de nuevo, no acumula meses que ya fueron contabilizados.

### Criterios de aceptación:
- Dado empleados activos con meses completos no contabilizados, cuando el proceso se ejecuta, entonces el saldo acumulado aumenta correctamente y se registra en el historial.
- Dado que el proceso ya se ejecutó, si se ejecuta de nuevo no debe acumular los mismos meses dos veces.

---

## CU-02 — Consultar saldo personal / histórico
**ID**: CU-02
**Nombre**: Consultar saldo y movimientos
**Actor(es)**: Empleado, RRHH
**Componente**: EmployeeBalanceService
**Prioridad**: P1
**Trazabilidad**: `spec/spec.md` (HU-04, RF-016, RN-27)

### Precondiciones:
- Usuario autenticado en el sistema.
- El empleado consultado existe en el sistema.

### Flujo principal:
1. El usuario solicita consultar el saldo de un empleado, con opción de ver el historial completo.
2. Sistema verifica si el usuario tiene permiso:
   - Si es un Empleado, solo puede ver su propio saldo.
   - Si es de RRHH, puede ver el saldo de cualquier empleado.
   - Si es un Aprobador, puede ver el saldo en el contexto de una aprobación.
3. Sistema busca el registro de saldo del empleado consultado.
4. Si el empleado no existe, el sistema informa que no fue encontrado.
5. Sistema entrega la siguiente información:
   - Saldo acumulado, saldo consumido, saldo disponible y fecha de última actualización.
6. Si el usuario solicitó ver el historial, el sistema también entrega la lista de movimientos registrados.
7. El historial se presenta ordenado del más reciente al más antiguo.
8. La pantalla muestra los datos con la fecha de consulta y la unidad de medida en días.

### Flujos alternos:
- **FA-03**: RRHH solicita el historial completo de movimientos de un empleado específico.

### Excepciones:
| Código | Condición | Mensaje |
|--------|-----------|---------|
| 403 | Empleado consulta saldo de otro | "No autorizado para consultar este saldo" |
| 404 | Empleado consultado no existe | "Empleado no encontrado" |
| 503 | Servicio de balance no disponible | "Servicio de balance no disponible" |

### Postcondiciones:
- No se realizan cambios en el sistema.
- La información se presenta al usuario con la fecha y quién realizó cada movimiento.

### Requisitos no funcionales:
- La consulta individual debe responder en menos de 300 milisegundos el 95% de las veces.
- Se usa una zona horaria corporativa única para todas las fechas.

### Criterios de aceptación:
- Dado un empleado activo, cuando solicita su saldo, entonces recibe el saldo disponible (acumulado - consumido) junto con el historial de movimientos.
- Dado RRHH, cuando consulta el saldo de cualquier empleado, entonces recibe la misma información.

---

## CU-03 — Registrar movimientos de balance (auditoría)
**ID**: CU-03
**Actor(es)**: Sistema
**Tipo**: Transversal
**Componente**: BalanceHistory / EmployeeBalanceService
**Prioridad**: P1
**Trazabilidad**: `spec/spec.md` (BalanceHistory)

### Descripción:
Este no es un caso de uso con actor que inicia la acción. Es un requisito de auditoría que se ejecuta como consecuencia de otros casos de uso que modifican el balance. Se activa automáticamente desde:
- Acumulación mensual
- Aprobación con descuento de saldo
- Cancelación de APPROVED con restauración de saldo

### Reglas de registro:
1. Cuando cambia el saldo acumulado o el saldo consumido de un empleado, se crea un registro en el historial de saldo con:
   - Identificador del empleado
   - Tipo de movimiento: "ACUMULACIÓN" | "DESCUENTO POR APROBACIÓN" | "RESTAURACIÓN POR CANCELACIÓN"
   - Valor anterior y valor nuevo del saldo afectado
   - Motivo del cambio (texto descriptivo)
   - Quién realizó el cambio: correo del usuario o nombre del sistema ("SISTEMA_ACUMULACION", "SISTEMA_AUTO_EXPIRACION")
   - Fecha y hora del movimiento
2. El registro se guarda en la misma operación que genera el cambio de saldo, asegurando que ambos se guarden o ninguno.
3. Si ocurre un error al guardar el registro de auditoría, la operación principal tampoco debe completarse.

### Excepciones:
| Código | Condición |
|--------|-----------|
| 500 | Error al escribir auditoría → la operación principal no se completa |

### Postcondiciones:
- Movimiento registrado en el historial para que RRHH pueda consultarlo.
- Los registros de auditoría no se pueden modificar ni eliminar una vez creados.

---

## CU-04 — Crear solicitud de vacaciones
**ID**: CU-04
**Nombre**: Crear solicitud de vacaciones (PENDING)
**Actor(es)**: Empleado
**Componente**: VacationRequestService / EmployeeBalanceService
**Prioridad**: P1
**Trazabilidad**: `spec/spec.md` (HU-01, RF-007, RN-06, RN-07, RN-10, RN-21, RN-28, RN-29, RN-30, RN-31, RF-009, RF-038)

### Precondiciones:
- Empleado autenticado con permisos de empleado.
- Empleado activo en el sistema.
- El empleado tiene un registro de saldo disponible.

### Flujo principal:
1. Empleado ingresa los datos de su solicitud: fecha de inicio, fecha de fin y motivo de las vacaciones.
2. Sistema verifica que el motivo:
   - Sea obligatorio (no puede estar vacío).
   - Tenga al menos 10 caracteres.
3. Sistema verifica las fechas:
   - La fecha de inicio debe ser al menos un día después de hoy.
   - La fecha de fin no puede ser anterior a la fecha de inicio.
   - Solo se permiten fechas completas, sin horas.
4. Sistema calcula los días solicitados:
   - Usa la función de cálculo de días hábiles para contar solo los días de semana entre las fechas.
   - El resultado debe ser al menos 1 día completo.
   - Si el cálculo diera fracciones de día, se rechaza la solicitud.
5. Sistema verifica que el empleado tenga saldo suficiente:
   - Compara el saldo disponible del empleado contra los días solicitados.
   - Si no alcanza, se informa al empleado.
6. Sistema verifica que no haya traslapes:
   - Revisa si el empleado tiene otras solicitudes aprobadas o pendientes en las mismas fechas.
   - Si encuentra traslape, se informa al empleado.
7. Sistema muestra un resumen al empleado antes de confirmar:
   - Días solicitados (sin contar sábados ni domingos).
   - Saldo actual disponible.
   - Saldo que quedaría después de la solicitud.
8. Empleado confirma que desea crear la solicitud.
9. Sistema guarda la solicitud con estado "PENDING".
10. Sistema registra en el historial que la solicitud fue creada, indicando quién la creó y cuándo.
11. Sistema hace visible la solicitud en la bandeja de los aprobadores para que la revisen.
12. Sistema confirma la creación exitosa y entrega el identificador de la solicitud.

### Flujos alternos:
- **FA-05-1**: Saldo insuficiente → "Saldo insuficiente para esta solicitud".
- **FA-05-2**: Traslape detectado → "La solicitud incluye días que ya están comprometidos en otra solicitud".
- **FA-05-3**: Fracción detectada → "No se permiten solicitudes por horas o fracciones en esta versión".
- **FA-05-4**: Fecha de inicio anterior a mañana → "La fecha de inicio no puede ser anterior a mañana".
- **FA-05-5**: Fecha de fin anterior a fecha de inicio → "La fecha de fin no puede ser anterior a la de inicio".
- **FA-05-6**: Motivo muy corto → "El motivo debe tener al menos 10 caracteres".

### Excepciones:
| Código | Condición | Mensaje |
|--------|-----------|---------|
| 400 | Validación falla | Según validación específica (FA-05-1 a FA-05-6) |
| 500 | Error interno | "Error interno al crear solicitud" |

### Postcondiciones:
- Solicitud creada con estado "PENDING".
- Historial de la solicitud registrado con el evento de creación.
- Solicitud visible en la bandeja de aprobadores.
- El saldo del empleado no se modifica (el descuento ocurre solo cuando se aprueba).

### Criterios de aceptación:
- Dado rango de fechas válido y saldo suficiente, cuando empleado crea solicitud y confirma, entonces la solicitud queda pendiente de aprobación y visible para los aprobadores.
- Dado saldo insuficiente, cuando empleado intenta crear, entonces la operación es rechazada con el mensaje correspondiente.

## CU-05 — Ver mis solicitudes / detalle y auditoría
**ID**: CU-05
**Nombre**: Listar y ver detalle de solicitudes propias
**Actor(es)**: Empleado
**Componente**: VacationRequestService
**Prioridad**: P1
**Trazabilidad**: `spec/spec.md` (HU-02, RF-046)

### Precondiciones:
- Empleado autenticado con permisos de empleado.

### Flujo principal (lista):
1. Empleado solicita ver sus solicitudes de vacaciones, con opción de filtrar por estado (PENDING, APPROVED, REJECTED, CANCELLED, EXPIRED).
2. Sistema verifica que el empleado solo pueda ver sus propias solicitudes.
3. Sistema aplica el filtro de estado si el empleado lo indicó.
4. Sistema entrega la lista ordenada de la más reciente a la más antigua, mostrando los resultados por páginas.
5. Cada solicitud en la lista muestra: identificador, fechas, días solicitados, estado, fecha de creación y comentario del aprobador (si existe).

### Flujo principal (detalle):
1. Empleado selecciona una solicitud para ver su detalle.
2. Sistema verifica que el empleado sea el autor de la solicitud.
3. Sistema muestra toda la información de la solicitud junto con el historial de eventos (creación, cambios de estado, etc.).

### Flujos alternos:
- **FA-06**: Si el empleado no tiene solicitudes, la pantalla muestra "No hay solicitudes".

### Excepciones:
| Código | Condición | Mensaje |
|--------|-----------|---------|
| 403 | Empleado consulta solicitudes de otro | "No autorizado" |
| 404 | Solicitud no encontrada | "Solicitud no encontrada" |

### Postcondiciones:
- La información se muestra al usuario sin realizar cambios en el sistema.

### Requisitos no funcionales:
- La lista se entrega por páginas con tamaño configurable.
- Por defecto se ordena de la más reciente a la más antigua.

### Criterios de aceptación:
- Dado empleado con solicitudes, cuando accede a "Mis solicitudes", entonces ve la lista paginada ordenada por fecha descendente con los campos indicados.
- Dado empleado, cuando solicita detalle de una solicitud propia, entonces ve el historial completo de eventos.

---

## CU-06 — Editar solicitud PENDING
**ID**: CU-06
**Nombre**: Editar solicitud en estado PENDING
**Actor(es)**: Empleado
**Componente**: VacationRequestService
**Prioridad**: P2
**Trazabilidad**: `spec/spec.md` (HU-03, RN-10, RN-20, RN-21, RF-038)

### Precondiciones:
- Empleado autenticado.
- La solicitud existe y está en estado "PENDING".
- El empleado es el autor de la solicitud.

### Flujo principal:
1. Empleado modifica los datos editables de su solicitud: fecha de inicio, fecha de fin y motivo.
2. Sistema verifica que:
   - La solicitud exista y esté pendiente.
   - El empleado que intenta editar sea el autor de la solicitud.
3. Sistema aplica las mismas validaciones que al crear una solicitud:
   - Motivo obligatorio con al menos 10 caracteres.
   - Fecha de inicio debe ser al menos un día después de hoy.
   - Fecha de fin no puede ser anterior a la fecha de inicio.
   - Recalcular días hábiles solicitados.
   - No se permiten fracciones de día.
   - Verificar saldo suficiente si cambiaron los días.
   - Verificar que no haya traslapes si cambiaron las fechas.
4. Sistema guarda los cambios en los campos editables.
5. Si cambiaron las fechas, se actualiza también el cálculo de días solicitados.
6. Sistema registra en el historial qué campos se modificaron, mostrando el valor anterior y el valor nuevo, junto con quién hizo el cambio y cuándo.
7. Sistema actualiza la solicitud en la bandeja de los aprobadores para que vean los cambios.
8. Sistema confirma la actualización exitosa.

### Flujos alternos:
- **FA-07**: Si alguna validación falla, se informa al empleado con el mensaje correspondiente.

### Excepciones:
| Código | Condición | Mensaje |
|--------|-----------|---------|
| 403 | No es el autor | "No autorizado para editar esta solicitud" |
| 403 | Estado no es pendiente | "Solo se pueden editar solicitudes pendientes" |
| 400 | Validación falla | Según validación específica |

### Postcondiciones:
- Solicitud actualizada con los nuevos valores.
- Historial registrado con los campos modificados.
- Aprobadores notificados de los cambios.

### Criterios de aceptación:
- Dado solicitud pendiente propia, cuando se edita con datos válidos, entonces los cambios se guardan y se registran en la auditoría.
- Dado solicitud aprobada, cuando se intenta editar, entonces la operación es rechazada.

---

## CU-07 — Cancelar solicitud por empleado (PENDING)
**ID**: CU-07
**Nombre**: Cancelar solicitud propia en estado PENDING
**Actor(es)**: Empleado
**Componente**: VacationRequestService
**Prioridad**: P1
**Trazabilidad**: `spec/spec.md` (RN-11)

### Precondiciones:
- Empleado autenticado.
- La solicitud existe y está en estado "PENDING".
- El empleado es el autor de la solicitud.

### Flujo principal:
1. Empleado solicita cancelar su solicitud de vacaciones.
2. Sistema verifica que:
   - La solicitud exista.
   - La solicitud esté pendiente (no se puede cancelar si ya fue aprobada o rechazada).
   - El empleado sea el autor de la solicitud.
3. El sistema muestra un mensaje de confirmación: "¿Está seguro de cancelar esta solicitud?".
4. Empleado confirma que desea cancelarla.
5. Sistema cambia el estado de la solicitud a "CANCELLED".
6. Sistema registra en el historial que la solicitud fue cancelada, indicando quién la canceló y cuándo.
7. Sistema notifica a los aprobadores que la solicitud fue cancelada.
8. **El saldo del empleado no se modifica** porque nunca se descontaron los días (solo se descuentan al aprobar).

### Flujos alternos:
- **FA-08**: Si se intenta cancelar una solicitud que no está pendiente, el sistema lo impide con un mensaje.

### Excepciones:
| Código | Condición | Mensaje |
|--------|-----------|---------|
| 403 | Estado no es pendiente | "No se puede cancelar una solicitud en estado {estado}" |
| 403 | No es el autor | "No autorizado para cancelar esta solicitud" |
| 404 | Solicitud no existe | "Solicitud no encontrada" |

### Postcondiciones:
- Estado de la solicitud actualizado a "CANCELLED".
- Evento registrado en el historial de la solicitud.
- Saldo del empleado sin cambios.

### Criterios de aceptación:
- Dado solicitud pendiente propia, cuando empleado confirma cancelación, entonces estado = CANCELLED y saldo no se modifica.
- Dado solicitud APPROVED, cuando empleado intenta cancelar, entonces operación rechazada.

---

## CU-08 — Cálculo de días hábiles
**ID**: CU-08
**Actor(es)**: Sistema
**Tipo**: Transversal (lo ejecuta el sistema automáticamente al crear o editar solicitudes)
**Componente**: DateUtils / VacationRequestService
**Prioridad**: P1
**Trazabilidad**: `spec/spec.md` (RN-05, RN-25, RN-30, RF-002)

### Descripción:
Función del sistema para calcular cuántos días de vacaciones corresponden entre dos fechas, contando solo los días de semana (lunes a viernes). No interviene un usuario directamente; es utilizada por otros casos de uso como el de crear o editar solicitudes.

### Lógica:
1. Recibir la fecha de inicio y la fecha de fin del período solicitado (fechas completas, sin horas).
2. Verificar que la fecha de fin no sea anterior a la fecha de inicio.
3. Revisar cada día del período, uno por uno:
   - Si el día es sábado o domingo, no se cuenta.
   - Si el día es de lunes a viernes, se cuenta como día solicitado.
4. Entregar el total de días contados (debe ser al menos 1 día).

### Notas:
- **Feriados**: En esta versión, los feriados nacionales se cuentan como días laborables. Esto se puede ajustar en versiones futuras.
- **Duración mínima**: Se debe solicitar al menos 1 día hábil.
- **Sin fracciones**: Solo se permiten días completos, no medios días ni horas.

### Excepciones:
| Código | Condición |
|--------|-----------|
| 400 | Fecha de inicio o fin inválidas |
| 400 | Fecha de fin anterior a fecha de inicio |

### Criterios de aceptación:
- Dado un período de lunes a viernes, el cálculo debe devolver 5 días.
- Dado un período de viernes a lunes, el cálculo debe devolver 2 días (excluye sábado y domingo).

---

## CU-09 — Prevención de traslapes entre solicitudes
**ID**: CU-09
**Actor(es)**: Sistema
**Tipo**: Transversal (lo ejecuta el sistema automáticamente al crear, editar o revisar solicitudes)
**Componente**: VacationRequestRepository
**Prioridad**: P1
**Trazabilidad**: `spec/spec.md` (RN-07)

### Descripción:
Función del sistema para detectar si un período de vacaciones solicitado se empalma (superpone) con otro período ya solicitado por el mismo empleado. No interviene un usuario directamente; es utilizada al crear o editar solicitudes y al mostrar la bandeja de aprobadores.

### Lógica:
1. Recibir: empleado, fecha de inicio, fecha de fin y, opcionalmente, una solicitud que se debe ignorar (útil al editar).
2. Buscar solicitudes del mismo empleado que:
   - Estén en estado "PENDING" o "APPROVED".
   - Tengan fechas que se empalmen con el período consultado (una solicitud existente comienza antes de que termine la nueva, y viceversa).
   - Si se indicó una solicitud a ignorar, se excluye de la búsqueda.
3. Si se encuentra empalme:
   - Se bloquea la operación, sin importar si la solicitud existente está pendiente o aprobada.
4. Si no hay empalme, se permite la operación.

### Excepciones:
| Código | Condición |
|--------|-----------|
| 400 | Se detectó empalme con otra solicitud pendiente o aprobada |

### Criterios de aceptación:
- Dado que existen solicitudes pendientes o aprobadas, cuando se verifica un período que se empalma, entonces la verificación falla.
- Dado que solo existen solicitudes CANCELLED, REJECTED o EXPIRED, cuando se verifica un período que se empalma, entonces la verificación pasa.

---

## CU-10 — Bandeja de aprobadores
**ID**: CU-10
**Nombre**: Listar solicitudes PENDING para aprobación
**Actor(es)**: Aprobador
**Componente**: ApprovalService / VacationRequestService
**Prioridad**: P1
**Trazabilidad**: `spec/spec.md` (HU-05, RF-046)

### Precondiciones:
- Usuario autenticado con permisos de aprobador.
- Aprobador activo en el sistema.

### Flujo principal:
1. Aprobador ingresa a su bandeja de solicitudes pendientes, con opción de filtrar por empleado, rango de fechas o cantidad de días.
2. Sistema verifica que el usuario tenga permisos de aprobador activo.
3. Sistema busca todas las solicitudes en estado "PENDING".
4. Sistema excluye las solicitudes creadas por el mismo aprobador (no puede aprobar sus propias solicitudes).
5. Sistema aplica los filtros que el aprobador haya indicado:
   - Por nombre o correo del empleado solicitante.
   - Por rango de fechas de la solicitud.
   - Por cantidad de días solicitados.
6. Sistema entrega la lista ordenada de la más antigua a la más reciente (las que llevan más tiempo esperando primero), mostrando los resultados por páginas.
7. Cada solicitud en la lista muestra: nombre del empleado, identificador, fechas, días solicitados, motivo y saldo disponible actual.
8. Por cada solicitud, el sistema verifica si hay empalme con otras solicitudes del mismo empleado:
   - Si hay empalme con otras solicitudes pendientes, muestra una advertencia.
   - Si hay empalme con una solicitud aprobada, deshabilita el botón de aprobar y muestra el motivo.
9. Si no hay solicitudes pendientes, la pantalla muestra "No hay solicitudes pendientes".

### Flujos alternos:
- **FA-11**: No hay solicitudes pendientes, se muestra el mensaje correspondiente.

### Excepciones:
| Código | Condición | Mensaje |
|--------|-----------|---------|
| 403 | Usuario no es aprobador o está inactivo | "No autorizado: se requiere rol Aprobador activo" |

### Postcondiciones:
- Lista de solicitudes pendientes presentada al aprobador para que pueda revisarlas.

### Requisitos no funcionales:
- Los filtros se pueden combinar entre sí y se recuerdan durante la sesión.
- La lista se entrega por páginas.

### Criterios de aceptación:
- Dado aprobador activo, cuando accede a la bandeja, entonces recibe todas las solicitudes pendientes de otros empleados ordenadas por antigüedad.
- Dado empalme con solicitud aprobada, cuando se muestra la bandeja, entonces el botón de aprobar aparece deshabilitado con el mensaje correspondiente.

---

## CU-11 — Aprobar solicitud (descuento de saldo)
**ID**: CU-11
**Nombre**: Aprobar solicitud PENDING
**Actor(es)**: Aprobador
**Componente**: ApprovalService / EmployeeBalanceService
**Prioridad**: P1
**Trazabilidad**: `spec/spec.md` (HU-06, RF-022, RF-024, RF-044, RN-08, RN-12, RN-13, RN-14)

### Precondiciones:
- La solicitud existe y está en estado "PENDING".
- Aprobador autenticado y activo en el sistema.
- El aprobador no es el autor de la solicitud (no se puede auto-aprobar).

### Flujo principal:
1. Aprobador elige aprobar una solicitud pendiente, con la opción de agregar un comentario.
2. Sistema verifica que:
   - La solicitud exista y esté pendiente.
   - El aprobador esté activo.
   - El aprobador no sea el mismo empleado que solicitó las vacaciones.
3. Sistema verifica que el empleado todavía tenga saldo suficiente (el saldo pudo haber cambiado desde que se creó la solicitud).
4. Sistema se asegura de que otro aprobador no haya procesado ya esta solicitud al mismo tiempo.
5. Sistema realiza los siguientes pasos de forma conjunta (todo se guarda o no se guarda nada):
   a. Cambia el estado de la solicitud a "APPROVED".
   b. Guarda un registro de la acción de aprobación, indicando quién aprobó, cuándo y el comentario (si lo hay).
   c. Descuenta los días solicitados del saldo consumido del empleado.
   d. Recalcula el saldo disponible del empleado.
   e. Registra el movimiento en el historial de saldo, indicando el tipo "DESCUENTO POR APROBACIÓN", el valor anterior y nuevo, el motivo y quién lo hizo.
   f. Registra en el historial de la solicitud que cambió a "APPROVED".
6. Sistema notifica al empleado que su solicitud fue aprobada.
7. Sistema confirma la aprobación exitosa.

### Flujos alternos:
- **FA-12**: Si el saldo del empleado ya no es suficiente al momento de aprobar, se informa al aprobador.
- **FA-12b**: Si otro aprobador ya procesó esta solicitud, se informa que ya fue atendida.

### Excepciones:
| Código | Condición | Mensaje |
|--------|-----------|---------|
| 403 | Auto-aprobación | "No puedes aprobar ni rechazar tu propia solicitud; otro aprobador debe resolverla" |
| 403 | Aprobador inactivo | "Aprobador inactivo" |
| 409 | Saldo insuficiente | "No se puede aprobar: saldo insuficiente al momento de la aprobación" |
| 409 | Ya fue procesada | "La solicitud ya fue procesada por otro aprobador" |

### Postcondiciones:
- Estado de la solicitud actualizado a "APPROVED".
- Saldo consumido del empleado incrementado en los días solicitados.
- Saldo disponible del empleado recalculado.
- Movimientos registrados en el historial de saldo y en el historial de la solicitud.
- Acción de aprobación guardada.

### Criterios de aceptación:
- Dado solicitud pendiente con saldo suficiente, cuando aprobador válido la aprueba, entonces la solicitud pasa a aprobada, el saldo se descuenta y la auditoría queda registrada.
- Dado intento de auto-aprobación, cuando aprobador intenta aprobar su propia solicitud, entonces la operación es bloqueada con el mensaje correspondiente.

---

## CU-12 — Rechazar solicitud con comentario obligatorio
**ID**: CU-12
**Nombre**: Rechazar solicitud PENDING
**Actor(es)**: Aprobador
**Componente**: ApprovalService
**Prioridad**: P1
**Trazabilidad**: `spec/spec.md` (HU-06, RF-023, RF-024, RN-04, RN-12, RN-14)

### Precondiciones:
- La solicitud existe y está en estado "PENDING".
- Aprobador autenticado y activo en el sistema.
- El aprobador no es el autor de la solicitud.

### Flujo principal:
1. Aprobador elige rechazar una solicitud pendiente e ingresa un comentario explicando el motivo del rechazo.
2. Sistema verifica que:
   - La solicitud exista y esté pendiente.
   - El aprobador esté activo.
   - El aprobador no sea el mismo empleado que solicitó las vacaciones.
3. Sistema valida el comentario:
   - No puede estar vacío.
   - No puede exceder los 500 caracteres.
4. Sistema realiza los siguientes pasos:
   a. Cambia el estado de la solicitud a "REJECTED".
   b. Guarda un registro de la acción de rechazo, indicando quién rechazó, cuándo y el comentario.
   c. Guarda el comentario del aprobador en la solicitud para que el empleado lo vea.
   d. Registra en el historial de la solicitud el cambio de estado.
5. **El saldo del empleado no se modifica** porque nunca se descontaron los días.
6. Sistema notifica al empleado que su solicitud fue rechazada, mostrando el comentario como "Motivo de rechazo: [comentario]".
7. Sistema confirma el rechazo exitoso.

### Flujos alternos:
- **FA-13**: Si el comentario está vacío, el sistema lo rechaza indicando que es obligatorio.

### Excepciones:
| Código | Condición | Mensaje |
|--------|-----------|---------|
| 400 | Comentario vacío | "El comentario es obligatorio" |
| 400 | Comentario muy largo | "El comentario no puede exceder los 500 caracteres" |
| 403 | Auto-aprobación | "No puedes aprobar ni rechazar tu propia solicitud; otro aprobador debe resolverla" |
| 403 | Aprobador inactivo | "Aprobador inactivo" |

### Postcondiciones:
- Estado de la solicitud actualizado a "REJECTED".
- Comentario del rechazo visible para el empleado en el detalle de la solicitud.
- Evento registrado en el historial de la solicitud.
- Saldo del empleado sin cambios.

### Criterios de aceptación:
- Dado solicitud pendiente, cuando aprobador rechaza con comentario válido, entonces estado = REJECTED y el comentario queda registrado y visible al empleado.
- Dado intento de rechazo sin comentario, entonces la operación es rechazada.

---

## CU-13 — Ver impacto en saldo antes de decidir
**ID**: CU-13
**Tipo**: Extensión (se muestra al aprobador al abrir detalle de solicitud pendiente)
**Actor(es)**: Aprobador
**Componente**: ApprovalService / EmployeeBalanceService
**Prioridad**: P1
**Trazabilidad**: `spec/spec.md` (HU-07, RF-026)

### Descripción:
Esta funcionalidad se activa cuando el aprobador abre el detalle de una solicitud pendiente, ya sea desde la bandeja (CU-10) o antes de aprobar (CU-11). Muestra al aprobador cómo quedaría el saldo del empleado si se aprobara la solicitud.

### Flujo:
1. Aprobador abre el detalle de una solicitud pendiente.
2. Sistema consulta el saldo disponible actual del empleado solicitante.
3. Sistema calcula el saldo que quedaría después de aprobar (saldo actual - días solicitados).
4. Sistema presenta al aprobador:
   - Saldo actual del empleado.
   - Días que está solicitando.
   - Saldo estimado que le quedaría si se aprueba.
5. Si el saldo estimado queda en negativo, el sistema muestra una advertencia destacada indicando que la aprobación excedería el saldo disponible.
6. Si no se puede consultar el saldo, se muestra un aviso de error y se permite reintentar.

### Excepciones:
| Código | Condición | Mensaje |
|--------|-----------|---------|
| 503 | Servicio de saldo no disponible | "Servicio de balance no disponible" |

### Postcondiciones:
- La información se presenta al aprobador sin realizar cambios en el sistema.

---

## CU-14 — Cancelación de APPROVED por aprobador (restauración)
**ID**: CU-14
**Nombre**: Cancelar solicitud APPROVED antes del inicio
**Actor(es)**: Aprobador
**Componente**: ApprovalService / EmployeeBalanceService
**Prioridad**: P2
**Trazabilidad**: `spec/spec.md` (RN-04, RN-14, RF-047)

### Precondiciones:
- La solicitud existe y está en estado "APPROVED".
- Aprobador autenticado y activo en el sistema.
- La fecha de inicio de las vacaciones aún no ha llegado (es posterior a hoy).

### Flujo principal:
1. Aprobador elige cancelar una solicitud ya aprobada, con la opción de agregar un comentario.
2. Sistema verifica que:
   - La solicitud exista y esté aprobada.
   - La fecha de inicio de las vacaciones sea posterior a hoy (no se puede cancelar si ya empezaron).
   - El aprobador esté activo (cualquier aprobador activo puede cancelar, no necesariamente el que aprobó).
3. Sistema realiza los siguientes pasos de forma conjunta:
   a. Cambia el estado de la solicitud a "CANCELLED".
   b. Guarda un registro de la acción de cancelación, indicando quién canceló, cuándo y el comentario (si lo hay).
   c. Restaura el saldo del empleado: devuelve los días al saldo consumido (los quita del consumo).
   d. Recalcula el saldo disponible del empleado.
   e. Registra el movimiento en el historial de saldo, indicando el tipo "RESTAURACIÓN POR CANCELACIÓN", el valor anterior y nuevo, el motivo y quién lo hizo.
   f. Registra en el historial de la solicitud el cambio a cancelada.
4. Sistema notifica al empleado que su solicitud fue cancelada.
5. Sistema confirma la cancelación exitosa.

### Flujos alternos:
- **FA-15**: Si se intenta cancelar una solicitud aprobada cuyas vacaciones ya empezaron, el sistema lo impide con un mensaje.

### Excepciones:
| Código | Condición | Mensaje |
|--------|-----------|---------|
| 403 | Vacaciones ya iniciadas | "No se puede cancelar: el periodo de vacaciones ya ha iniciado" |
| 403 | Aprobador inactivo | "Aprobador inactivo" |
| 404 | Solicitud no encontrada | "Solicitud no encontrada" |

### Postcondiciones:
- Estado de la solicitud actualizado a "CANCELLED".
- Saldo del empleado restaurado: los días vuelven del saldo consumido al saldo disponible.
- Movimientos registrados en el historial de saldo y en el historial de la solicitud.
- Acción de cancelación guardada.

### Criterios de aceptación:
- Dado solicitud aprobada con inicio futuro, cuando aprobador la cancela, entonces estado = CANCELLED y el saldo se restaura completamente.
- Dado solicitud aprobada con inicio ya vencido, cuando aprobador intenta cancelarla, entonces la operación es bloqueada.

---

## CU-15 — Auto-expiración de solicitudes PENDING → EXPIRED
**ID**: CU-15
**Nombre**: Auto-expirar solicitudes pendientes tras N días
**Actor(es)**: SISTEMA_AUTO_EXPIRACION (job programado)
**Componente**: ExpirationJob
**Prioridad**: P2
**Trazabilidad**: `spec/spec.md` (RN-26, RF-043)

### Precondiciones:
- Existe un número N configurado en el sistema que define cuántos días puede estar una solicitud pendiente antes de vencer.
- El proceso automático está configurado para ejecutarse diariamente.

### Flujo principal:
1. El proceso automático diario busca todas las solicitudes en estado "PENDING" que fueron creadas hace más de N días.
2. Por cada solicitud encontrada:
   a. Cambia su estado a "EXPIRED".
   b. Registra en el historial que la solicitud expiró, indicando que lo hizo el sistema automático, la fecha y el motivo.
   c. Deja una notificación pendiente para el empleado (aún no se envía correo electrónico).
3. El sistema guarda un registro de cuántas solicitudes expiraron en esta ejecución.
4. **El saldo del empleado no se modifica** porque nunca se descontaron los días.

### Flujos alternos:
- **FA-16**: Si falla la notificación, se registra el error pero la expiración ya queda hecha.
- **FA-16b**: Si hay error al actualizar varias solicitudes, el proceso lo intentará de nuevo en la próxima ejecución.

### Excepciones:
| Código | Condición |
|--------|-----------|
| 500 | Error al actualizar solicitudes en lote |

### Postcondiciones:
- Las solicitudes pendientes con más de N días de antigüedad pasan a estado "EXPIRED".
- Cada expiración queda registrada en el historial de la solicitud.
- Si el proceso se ejecuta de nuevo, no afecta solicitudes que ya están expiradas.

### Criterios de aceptación:
- Dado N configurado, cuando el proceso se ejecuta, entonces las solicitudes pendientes con más de N días pasan a expiradas.
- Dado una solicitud ya expirada, cuando el proceso se ejecuta de nuevo, permanece expirada sin cambios.

---

## CU-16 — Gestión de roles y permisos
**ID**: CU-16
**Actor(es)**: Sistema
**Tipo**: Transversal (se ejecuta en cada operación del sistema)
**Componente**: AuthService / Middleware (ASP.NET Core Identity)
**Prioridad**: P1
**Trazabilidad**: `spec/spec.md` (Sección Seguridad)

### Descripción:
Caso de uso transversal que aplica a todas las operaciones del sistema. No tiene flujo independiente; es una precondición de todos los demás casos de uso.

### Roles definidos:
| Rol | Permisos |
|-----|----------|
| Empleado | CRUD solicitudes propias, consulta saldo propio |
| Aprobador | Aprobar/rechazar cualquier solicitud PENDING (excepto propias), cancelar APPROVED antes del inicio, ver bandeja |
| RRHH | Consultas read-only de solicitudes y balances de cualquier empleado. Sin botones de approve/reject/edit |

### Reglas de autorización:
1. Middleware valida token JWT y rol en cada endpoint.
2. Empleado solo accede a recursos propios (employeeId == authenticatedUserId).
3. Aprobador activo puede actuar sobre solicitudes de cualquier empleado excepto las propias.
4. Aprobador inactivo no puede aprobar/rechazar/cancelar (RF-024).
5. RRHH es read-only: no puede crear, editar, aprobar ni cancelar solicitudes (RN-19, RF-039).
6. Sesiones gestionadas por ASP.NET Core Identity Framework. Expiración configurable por inactividad.

### Excepciones:
| Código | Condición |
|--------|-----------|
| 401 | No autenticado |
| 403 | Autenticado pero sin rol suficiente |

### Criterios de aceptación:
- Dado rol Aprobador, cuando intenta aprobar solicitud propia, entonces acción bloqueada con 403.
- Dado rol RRHH, cuando accede a UI de solicitudes, entonces no ve botones de approve/reject/edit.

---

## CU-17 — Auditoría y trazabilidad global
**ID**: CU-17
**Actor(es)**: Sistema
**Tipo**: Transversal (se ejecuta automáticamente tras cada cambio de estado o saldo)
**Componente**: VacationRequestHistory / BalanceHistory
**Prioridad**: P1
**Trazabilidad**: `spec/spec.md` (RF-032)

### Descripción:
Requisito transversal de auditoría que se ejecuta como postcondición de los casos de uso que crean, modifican o cambian el estado de solicitudes o balances.

### Reglas de registro:

**VacationRequestHistory** (para solicitudes):
| eventType | Disparado por | Actor |
|-----------|---------------|-------|
| CREATED | CU-04 (Crear solicitud) | Email del empleado |
| UPDATED | CU-06 (Editar solicitud) | Email del empleado |
| STATUS_CHANGED | CU-11 (Aprobar), CU-12 (Rechazar), CU-14 (Cancelar), CU-15 (Expiar) | Email del aprobador o SISTEMA_AUTO_EXPIRACION |
| CANCELLED | CU-07 (Cancelar empleado) | Email del empleado |

Cada registro incluye: requestId, eventType, actor, note, timestamp (UTC).

**BalanceHistory** (para cambios de saldo):
| movementType | Disparado por | Actor |
|--------------|---------------|-------|
| ACUMULATION | CU-01 (Acumulación mensual) | SISTEMA_ACUMULACION |
| APPROVAL_DISCOUNT | CU-11 (Aprobar) | Email del aprobador |
| CANCELLATION_RESTORE | CU-14 (Cancelar APPROVED) | Email del aprobador |

Cada registro incluye: employeeId, movementType, previousBalance, newBalance, reason, actor, timestamp (UTC).

**Consultas:**
- RRHH puede consultar `GET /api/vacation-requests/{id}/history` para ver el rastro de cualquier solicitud.
- Empleado puede ver el historial de sus propias solicitudes.
- Los registros de auditoría son inmutables (insert-only).

### Excepciones:
| Código | Condición |
|--------|-----------|
| 500 | Error al persistir auditoría → rollback de operación principal |

---

## CU-18 — Filtrado y consultas para RRHH
**ID**: CU-18
**Nombre**: Consultas filtradas de solicitudes y balances
**Actor(es)**: RRHH
**Componente**: VacationRequestService / EmployeeBalanceService
**Prioridad**: P2
**Trazabilidad**: `spec/spec.md` (HU-08, HU-09, RN-19, RF-029, RF-030, RF-039)

### Precondiciones:
- Usuario autenticado con permisos de RRHH.
- Importante: RRHH solo puede ver información, no puede crear, editar ni aprobar solicitudes.

### Flujo principal:
1. Usuario de RRHH solicita consultar solicitudes de vacaciones, pudiendo filtrar por estado, empleado o rango de fechas.
2. Sistema verifica que el usuario tenga permisos de RRHH.
3. Sistema aplica los filtros indicados:
   - Por estado (PENDING, APPROVED, REJECTED, CANCELLED, EXPIRED).
   - Por empleado específico.
   - Por rango de fechas de creación.
4. Sistema entrega la lista ordenada de la más reciente a la más antigua, mostrando los resultados por páginas.
5. Cada solicitud en la lista muestra: identificador, nombre y correo del empleado, fechas, días solicitados, estado, fecha de creación y comentario del aprobador.
6. Si se solicita ver el saldo de un empleado, también se puede consultar su historial de movimientos.
7. La pantalla solo permite visualizar la información, sin botones para aprobar, rechazar, editar ni crear solicitudes.
8. Si no hay resultados con los filtros aplicados, se muestra el mensaje correspondiente.

### Flujos alternos:
- **FA-19**: Sin resultados, se muestra "No se encontraron solicitudes que coincidan con los filtros aplicados".
- **FA-19b**: Si RRHH intenta crear o editar una solicitud, el sistema lo bloquea porque no tiene permisos para hacerlo.

### Excepciones:
| Código | Condición | Mensaje |
|--------|-----------|---------|
| 400 | Filtros inválidos | "Filtros inválidos" |
| 403 | Usuario no es RRHH | "No autorizado: se requiere rol RRHH" |

### Postcondiciones:
- Resultados presentados en pantalla solo para visualización; no se realizan cambios en el sistema.

### Requisitos no funcionales:
- La consulta debe responder en menos de 2 segundos para volúmenes razonables de datos.
- Los filtros se pueden combinar entre sí.

### Criterios de aceptación:
- Dado filtros válidos, cuando RRHH consulta, entonces recibe resultados paginados que coinciden con los filtros aplicados.
- Dado RRHH autenticado, cuando accede a la pantalla, entonces no ve botones para aprobar, rechazar ni editar solicitudes.

---

## CU-19 — Mensajes UX y manejo de errores
**ID**: CU-19
**Actor(es)**: Sistema
**Tipo**: Transversal (aplica a todos los casos de uso)
**Componente**: API / Frontend
**Prioridad**: P1
**Trazabilidad**: `spec/spec.md` (RF-035)

### Descripción:
Requisito transversal que aplica a todos los casos de uso. Define cómo se comunican los errores y validaciones al usuario final.

### Reglas:
1. Backend devuelve errores estructurados con: código HTTP, mensaje localizable, código de error interno (opcional).
2. Frontend captura el error y muestra el mensaje correspondiente al usuario.
3. Mensajes de validación específicos (definidos en spec/spec.md y replicados en cada CU):
   - "Saldo insuficiente para esta solicitud"
   - "La fecha de inicio no puede ser anterior a mañana"
   - "La fecha de fin no puede ser anterior a la de inicio"
   - "La solicitud incluye días que ya están comprometidos en otra solicitud"
   - "El motivo debe tener al menos 10 caracteres"
   - "No se permiten solicitudes por horas o fracciones en esta versión"
   - "No puedes aprobar ni rechazar tu propia solicitud; otro aprobador debe resolverla"
   - "No se puede aprobar: saldo insuficiente al momento de la aprobación"
   - "No se puede cancelar: el periodo de vacaciones ya ha iniciado"
   - "El comentario es obligatorio"
4. Diálogos de confirmación para acciones destructivas (cancelar solicitud).
5. Errores inesperados (500) → mostrar mensaje genérico "Error interno del servidor" + log técnico.

### Formato de error:
```json
{
  "code": "INSUFFICIENT_BALANCE",
  "message": "Saldo insuficiente para esta solicitud",
  "details": {}
}
```

### Criterios de aceptación:
- Mensajes de validación se corresponden con los textos definidos en la spec.
- Errores inesperados muestran mensaje genérico sin exponer detalles técnicos.

---

## Notas finales
- Cada CU referencia la especificación en `spec/spec.md` para trazabilidad.
- Los casos de uso transversales (Tipo: Transversal) aplican de fondo como parte del funcionamiento del sistema.
- Las prioridades se mantienen: P1 (críticos: crear solicitud, aprobar, consultar), P2 (operaciones batch, cancelaciones administrativas), P3 (consultas avanzadas RRHH).
- Todos los mensajes entre comillas dobles son los textos exactos definidos en `spec/spec.md`, sección de mensajes de validación.
