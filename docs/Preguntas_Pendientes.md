# Especificación de Aclaraciones Pendientes — Gestión de Solicitudes de Permisos/Vacaciones (NovaLeave)

**Feature Branch**: `001-vacation-request-pending`
**Versión de este documento**: 1.0 (Consolidado)
**Estado**: Draft — Awaiting PO Decisions
**Constitution Reference**: NovaLeave — Constitution v3.0.0

## Fuentes fusionadas

| # | Documento | Aporte |
|---|---|---|
| 1 | `preguntas_product_owner_1_1.md` | Preguntas consolidadas de Grupos 01, 02 y 03, organizadas en 6 ejes temáticos |
| 2 | `spec-pending-clarifications.md` | Especificación formal con User Stories (US-7, US-8), Decisiones de Diseño (D-004 a D-006), Requisitos Funcionales (FR-017 a FR-027), Edge Cases y Assumptions |
| 3 | `Preguntas_Fase_1__G4.docx` | Preguntas del Grupo 04 sobre límites de anticipación/duración, más dos propuestas de valor agregado (simulador prospectivo "innovador" vs. evaluación preventiva "alternativa") |

## Cómo leer este documento

Cada punto pendiente se documenta con esta estructura:

- **Título / Problema** — qué está sin resolver.
- **Contexto** — de dónde viene la duda y qué la origina.
- **Por qué importa** — a quién beneficia resolverla y qué riesgo corre el sistema si no se resuelve.
- **Decisión actual en la especificación** *(si existe)* — lo que hoy dice el borrador, siempre marcado como no confirmado.
- **Opciones** — caminos posibles, sin inclinar la balanza por ninguno.
- **Decisión requerida del PO** — la pregunta puntual y accionable.
- **Regla candidata (EARS)** — cómo se vería el requisito una vez decidido, con placeholders `[ENTRE_CORCHETES]` para los valores que el PO debe definir. Esto **no es un requisito confirmado**, es un borrador de sintaxis para que el PO visualice el impacto de cada opción.
- **Referencias cruzadas** — ID original en cada documento fuente (Pn = pregunta del documento 1, USn/FR-n/D-n = documento 2, Gn = documento 3).

> ⚠️ Ninguna propuesta, simulación o "aporte innovador" mencionado en los documentos originales se trata aquí como requisito aprobado. Se presenta como **propuesta** sujeta a validación de alcance, costo y prioridad por parte del PO.

---

## A. Tipos de Permisos y Ciclo de Vida de una Solicitud

### A.1 Catálogo de tipos de permiso y sus reglas de consumo/aprobación

**Contexto**: No está definido qué tipos de permiso existen (vacaciones, licencia médica, permiso personal, etc.) ni cuáles consumen saldo o requieren aprobación manual. Se menciona como ejemplo que una licencia médica con certificado previo podría no requerir aprobación manual.

**Por qué importa**: Sin este catálogo, no se puede diseñar el modelo de datos de `LeaveType`, ni las validaciones de saldo, ni el flujo de aprobación (algunos tipos podrían saltarse el paso del jefe).

**Decisión actual en la especificación**: Ninguna. No existe una entidad `LeaveType` definida en el spec base.

**Opciones**:
1. Catálogo fijo (hardcoded) para el MVP con 2–3 tipos (ej. vacaciones, permiso personal).
2. Catálogo configurable por HR desde el inicio.
3. Diferenciar tipos "con aprobación" vs. "de solo registro" (auto-aprobados si cumplen requisitos, ej. certificado médico adjunto).

**Decisión requerida del PO**: ¿Qué tipos de permiso entran al MVP y cuáles de ellos, si los hay, se auto-aprueban sin pasar por el jefe?

**Regla candidata (EARS)**: `SI el tipo de solicitud es [TIPO_AUTO_APROBADO] Y cumple [CONDICIÓN_DOCUMENTAL], ENTONCES el sistema deberá aprobar la solicitud automáticamente sin intervención del jefe directo.`

**Referencias cruzadas**: P (Sección 1, pregunta 1) · Grupo 02 "Definiciones Funcionales Primarias"

---

**✅ RESUELTO — Decisión del PO**: Solo existirá UN tipo de permiso: **vacaciones**. No hay licencia médica, permiso personal, ni tipos auto-aprobados. Eliminar cualquier mención a `LeaveType` como catálogo múltiple; todo el sistema gira en torno a un único tipo (vacaciones).

---

### A.2 Solicitudes de media jornada vs. día completo

**Contexto**: No se sabe si el sistema debe soportar solicitudes de medio día o solo unidades de día completo.

**Por qué importa**: Afecta directamente el modelo de cálculo de saldo (¿se descuenta en unidades de 0.5?) y la UI de selección de fechas.

**Decisión actual en la especificación**: No definida.

**Opciones**: (a) Solo días completos en el MVP; (b) soportar medios días desde el inicio.

**Decisión requerida del PO**: ¿El MVP soporta medios días o se pospone a una iteración futura?

**Regla candidata (EARS)**: `DONDE el sistema soporte solicitudes fraccionadas, el sistema deberá permitir unidades de [0.5] día y descontar el saldo proporcionalmente.`

**Referencias cruzadas**: P (Sección 1, pregunta 2)

---

**✅ RESUELTO — Decisión del PO**: NO se soportan solicitudes de medio día. Solo unidades de día completo.

---

### A.3 Guardado como borrador, modificación y cancelación de solicitudes en curso

**Contexto**: No está definido si una solicitud en estado `Pending` o `Approved` puede modificarse, cancelarse o guardarse como borrador antes de enviarse.

**Por qué importa**: Impacta la experiencia del empleado (evita reenvíos duplicados) y determina si se necesita un estado `Draft` en la máquina de estados de `LeaveRequest`.

**Decisión actual en la especificación**: El caso de **cancelación/enmienda de solicitudes ya aprobadas** sí está draftado como **User Story 8** (ver sección F de este documento), pero la edición de una solicitud aún `Pending` (antes de que el jefe la resuelva) y el guardado como borrador **no están cubiertos** en ningún documento fuente — es un vacío detectado en la fusión.

**Opciones**:
1. Permitir editar/retirar una solicitud `Pending` libremente mientras no haya sido resuelta.
2. Solo permitir retirarla (cancelar), no editarla; para cambiar fechas, el empleado debe cancelar y crear una nueva.
3. Agregar un estado `Draft` que no notifica al jefe hasta que el empleado confirma el envío.

**Decisión requerida del PO**: ¿Se permite editar o retirar una solicitud mientras está `Pending`? ¿Se necesita un estado `Draft` en el MVP?

**Regla candidata (EARS)**: `MIENTRAS una solicitud esté en estado Pending, el sistema deberá permitir al empleado [retirarla / editarla] hasta que el jefe la resuelva.`

**Referencias cruzadas**: P (Sección 1, pregunta 4) · relacionado con US-8 / FR-027

---

**✅ RESUELTO — Decisión del PO**: No existe estado `Draft`. Una solicitud en estado `Pending` SÍ puede editarse por el empleado mientras no haya sido resuelta por el aprobador. Además, debe existir un estado de saldo que impida crear nuevas solicitudes mientras haya una solicitud activa que comprometa ese saldo (aclarar con el equipo de desarrollo el nombre exacto de ese estado/bloqueo, ya que la respuesta del PO lo menciona pero no lo formaliza con un nombre; proponer algo como "saldo comprometido" o similar y dejarlo marcado como pendiente de nombre técnico si es ambiguo).

---

### A.4 Documentación de respaldo (comprobantes)

**Contexto**: No está definido si es obligatorio adjuntar comprobantes (ej. certificado médico) antes de que el jefe apruebe, o si puede subirse después.

**Por qué importa**: Afecta el flujo de aprobación: si el comprobante es obligatorio antes, el jefe no debería poder aprobar sin él; si es posterior, se necesita un mecanismo de seguimiento y posible reversión.

**Decisión actual en la especificación**: No definida.

**Opciones**: (a) Obligatorio antes de habilitar la aprobación; (b) opcional al momento de solicitar, pero exigible después con posibilidad de revertir la aprobación si no se sube a tiempo.

**Decisión requerida del PO**: ¿En qué punto del flujo se exige el comprobante, y para qué tipos de permiso aplica?

**Referencias cruzadas**: P (Sección 1, pregunta 5)

---

## B. Validaciones de Fechas, Duración y Concurrencia

### B.1 Manejo de solapamiento de fechas (overlap)

**Contexto**: No está claro si una nueva solicitud debe rechazarse solo cuando choca con solicitudes ya `Approved`, o también cuando choca con otras en estado `Pending`.

**Por qué importa**: Define si el sistema previene conflictos de forma temprana (más restrictivo, evita que dos solicitudes compitan por el mismo saldo) o solo al momento de aprobar (más flexible, pero puede generar rechazos tardíos y frustración).

**Decisión actual en la especificación**: El spec base solo menciona "solapamientos" como parte de la validación básica (junto con fechas pasadas y rangos invertidos), sin especificar contra qué estados se valida.

**Opciones**:
1. Rechazar solo contra `Approved`.
2. Rechazar contra `Approved` y `Pending`.
3. Permitir solapamiento en `Pending` pero advertir al jefe al momento de aprobar.

**Decisión requerida del PO**: ¿Contra qué estados de solicitud se valida el solapamiento de fechas?

**Regla candidata (EARS)**: `SI las fechas de una nueva solicitud se solapan con una solicitud en estado [Approved / Approved o Pending] del mismo empleado, ENTONCES el sistema deberá rechazar la creación de la solicitud.`

**Referencias cruzadas**: P (Sección 2, pregunta 1) · relacionado con la pregunta de concurrencia en la Sección I.1

---

**✅ RESUELTO — Decisión del PO**: Una solicitud NO puede crearse si sus fechas chocan con OTRA solicitud del mismo empleado que esté en estado `Approved` O `Pending` (no solo `Approved`). Se bloquea la creación en ambos casos (RN-07 actualizado). El mensaje para el usuario: "La solicitud incluye días que ya están comprometidos en otra solicitud".

---

### B.2 Calendario laboral: días hábiles vs. días calendario

**Contexto**: No está definido si feriados y fines de semana se excluyen del cálculo de duración de un permiso.

**Por qué importa**: Cambia directamente el cálculo del saldo consumido. Es una de las decisiones con mayor impacto en la lógica de negocio.

**Decisión actual en la especificación**: El spec base **asume explícitamente**, como supuesto temporal, que el conteo de días para el MVP es en días calendario inclusivos, sin excluir fines de semana ni feriados — pero lo marca como pendiente de validación por el PO.

**Opciones**: (a) Días calendario (más simple, decisión actual asumida); (b) días hábiles excluyendo fines de semana; (c) días hábiles excluyendo fines de semana y feriados (requiere un calendario de feriados configurable).

**Decisión requerida del PO**: ¿Se confirma la política de días calendario para el MVP, o se requiere excluir fines de semana/feriados desde el inicio?

**Regla candidata (EARS)**: `El sistema deberá calcular la duración de una solicitud como [días calendario inclusivos / días hábiles excluyendo fines de semana y feriados].`

**Referencias cruzadas**: P (Sección 2, pregunta 2) · Assumptions del documento 2 · "Working calendar policy" en tabla cruzada original (P2)

---

**✅ RESUELTO — Decisión del PO**: Se excluyen sábados y domingos del cálculo de duración. (Feriados: no se mencionó explícitamente su manejo — **punto AÚN ABIERTO** si el spec principal ya asumía feriados; no se resuelve por cuenta propia).

---

### B.3 Referencia horaria oficial y tolerancia de validación

**Contexto**: No está definido qué hora se usa como referencia oficial para validar "fecha pasada" o cierre de día: la del servidor, la del empleado, o una zona horaria corporativa única. Tampoco si la validación debe ser exacta al segundo o con tolerancia.

**Por qué importa**: En un sistema con empleados en distintas zonas horarias, esto determina si una solicitud "de último minuto" se acepta o rechaza de forma inconsistente según dónde esté el usuario.

**Decisión actual en la especificación**: No definida. Relacionado con la propuesta de Grupo 01 de establecer una **zona horaria corporativa** estandarizada.

**Opciones**: (a) Hora del servidor (UTC); (b) zona horaria corporativa única configurada por HR; (c) zona horaria local de cada empleado.

**Decisión requerida del PO**: ¿Cuál es la referencia horaria oficial del sistema, y existe tolerancia (ej. minutos/segundos) al validar cierres de día?

**Referencias cruzadas**: P (Sección 2, preguntas 3 y 5) · Grupo 01 "Estandarización Organizacional"

---

**✅ RESUELTO — Decisión del PO**: Todos los empleados operan en la MISMA zona horaria (zona corporativa única). No se soportan empleados en zonas horarias distintas. (No se definió tolerancia de segundos; déjalo fuera si no está ya definido, no lo inventes).

---

### B.4 Solicitud que vence sin ser procesada por el jefe

**Contexto**: ¿Qué ocurre si una solicitud `Pending` llega a su fecha de inicio sin que el jefe la haya aprobado o rechazado?

**Por qué importa**: Sin una regla, el sistema podría quedar en un estado inconsistente (el empleado "sale" de permiso sin autorización formal).

**Decisión actual en la especificación**: Este caso se relaciona directamente con **D-004 / FR-026 (Auto-Escalación)** del documento 2, que propone escalar automáticamente al jefe de nivel superior si la solicitud queda sin resolver por más de `[ESCALATION_TIMEOUT]` días hábiles — pero esa propuesta aún no está validada por el PO, y no cubre específicamente el caso límite de que la fecha de inicio ya haya pasado.

**Opciones**: (a) Auto-escalar antes de que llegue la fecha de inicio (ligado a D-004); (b) auto-rechazar la solicitud si llega la fecha de inicio sin resolución; (c) mantenerla pendiente indefinidamente y solo notificar.

**Decisión requerida del PO**: ¿Qué debe pasar cuando una solicitud pendiente alcanza su fecha de inicio sin resolución? ¿Se adopta la auto-escalación (D-004) para prevenir este escenario?

**Referencias cruzadas**: P (Sección 2, pregunta 4) · D-004 / FR-026 (documento 2)

---

**✅ RESUELTO — Decisión del PO**: Si una solicitud `Pending` no es aprobada/rechazada en **X días** (el valor numérico de X queda como parámetro configurable pendiente — usar un placeholder `[N]` días en el FR), el sistema debe rechazarla automáticamente por vencimiento. Además: existen VARIOS aprobadores (no un único jefe directo por empleado), y CUALQUIER aprobador activo puede aprobar CUALQUIER solicitud. (Esto hace que la escalación a "jefe de nivel superior" no aplique — no hay jerarquía).

---

### B.5 Límite de duración máxima, horizonte futuro máximo y antelación mínima

**Contexto**: Este es el punto con **mayor duplicación entre los tres documentos**. El documento 1 lo agrupa en una sola pregunta ("límite máximo de duración y antelación mínima"). El documento 2 lo separa en **tres requisitos independientes**: FR-022 (duración máxima en días consecutivos), FR-023 (horizonte futuro máximo en meses) y FR-024 (antelación mínima en días). El documento 3 (Grupo 04) plantea exactamente FR-022 y FR-023 como sus dos únicas preguntas, mostrando que dos grupos distintos, de forma independiente, identificaron el mismo vacío como crítico.

**Por qué importa**: Sin estos tres límites, una solicitud podría abarcar un periodo irrazonablemente largo o programarse con años de anticipación, complicando la planificación de cobertura de equipo y el cálculo de saldos futuros.

**Decisión actual en la especificación**: Ninguna; los tres valores están marcados como placeholders sin definir (`[MAX_CONSECUTIVE_DAYS]`, `[MAX_FUTURE_HORIZON_MONTHS]`, `[MIN_NOTICE_DAYS]`).

**Opciones**: Definir valores numéricos concretos para cada parámetro, o decidir que alguno de ellos no aplica para el MVP (sin límite).

**Decisión requerida del PO**: ¿Cuáles son los valores de (1) duración máxima consecutiva, (2) horizonte futuro máximo para solicitar, y (3) antelación mínima requerida? ¿Aplican los tres desde el MVP o se posponen algunos?

**Regla candidata (EARS)**:
- `SI la duración de la solicitud excede [MAX_CONSECUTIVE_DAYS] días consecutivos, ENTONCES el sistema deberá rechazar la solicitud.`
- `SI la fecha de inicio es posterior a [MAX_FUTURE_HORIZON_MONTHS] meses desde la fecha actual, ENTONCES el sistema deberá rechazar la solicitud.`
- `SI la solicitud se envía con menos de [MIN_NOTICE_DAYS] días de anticipación respecto a la fecha de inicio, ENTONCES el sistema deberá rechazar la solicitud.`

**Referencias cruzadas**: P (Sección 1, pregunta 3) · FR-022, FR-023, FR-024 (documento 2) · G4 Pregunta 1 (anticipación) y Pregunta 2 (duración máxima)

---

**✅ RESUELTO — Decisión del PO**:
- No se puede solicitar para el día actual; la fecha de inicio mínima válida es el día siguiente al de la solicitud (antelación mínima = 1 día).
- La duración máxima solicitable = el saldo disponible del empleado (no hay un tope fijo de días independiente del saldo).
- La duración mínima solicitable = 1 día.
- **NO se definió un horizonte futuro máximo (cuántos meses a futuro se puede solicitar)** — déjalo explícitamente como punto AÚN ABIERTO, no lo asumas resuelto.

---

### B.6 Solicitudes para el mismo día ("same-day requests")

**Contexto**: ¿Una solicitud con fecha de inicio igual al día actual es válida, o debe ser estrictamente futura?

**Por qué importa**: Define un caso límite frecuente (ej. un permiso médico de emergencia el mismo día) que podría chocar con la regla de antelación mínima (B.5) si no se trata como excepción.

**Decisión actual en la especificación**: No definida; el documento 2 lo deja explícitamente como pregunta abierta y lo referencia como dependiente de la política de antelación mínima (FR-024).

**Opciones**: (a) Nunca permitir solicitudes del mismo día; (b) permitirlas solo para ciertos tipos de permiso (ej. licencias médicas de emergencia, ver A.1); (c) permitirlas siempre, sin restricción.

**Decisión requerida del PO**: ¿Se permiten solicitudes con fecha de inicio igual al día actual? Si sí, ¿para todos los tipos de permiso o solo algunos?

**Referencias cruzadas**: FR-024, Edge Cases "Same-day requests" (documento 2)

---

## C. Flujo de Aprobación y Jerarquías

### C.1 Niveles de aprobación

**Contexto**: No está definido si la aprobación es exclusiva del jefe directo, o si se contemplan niveles adicionales (ej. Manager + RRHH) para ciertos tipos de permiso o duración.

**Por qué importa**: Determina la complejidad del motor de aprobación (aprobación simple vs. cadena de aprobaciones).

**Decisión actual en la especificación**: El MVP base asume aprobación por el jefe directo únicamente; no hay flujo multinivel definido.

**Opciones**: (a) Solo jefe directo; (b) jefe directo + RRHH para ciertos umbrales (ej. más de N días); (c) cadena configurable.

**Decisión requerida del PO**: ¿Se necesita más de un nivel de aprobación en el MVP, y bajo qué condiciones se activaría?

**Referencias cruzadas**: P (Sección 3, pregunta 1)

---

**✅ RESUELTO — Decisión del PO**: NO existen niveles jerárquicos de jefe directo. El modelo es simple: dos roles, "empleado" (quien solicita) y "aprobador" (quien aprueba). Hay VARIOS aprobadores, y cualquiera de ellos puede aprobar y ver TODAS las solicitudes del sistema, sin jerarquía ni asignación 1-a-1 entre empleado y jefe. Por lo tanto, no hay niveles de aprobación múltiples.

---

### C.2 Aprobación de solicitudes de los jefes y de la máxima autoridad

**Contexto**: ¿Quién aprueba las solicitudes de un jefe? ¿Y qué ocurre con la Gerencia General, que no tiene un superior jerárquico?

**Por qué importa**: Sin esta regla, un jefe o el Gerente General quedarían sin forma de solicitar permisos dentro del sistema, o se necesitaría un caso especial no contemplado.

**Decisión actual en la especificación**: Relacionado conceptualmente con **D-005 / FR-025 (rol "Leave Administrator")** del documento 2, pensado originalmente para empleados sin jefe asignado — el PO debe decidir si la Gerencia General se modela como un caso de "empleado sin manager" que cae bajo esta misma figura, o si necesita un flujo propio.

**Opciones**: (a) El jefe del jefe aprueba (cadena jerárquica normal, con la Gerencia General como techo); (b) un rol especial "Leave Administrator" aprueba a todos los que no tienen jefe (incluida Gerencia General); (c) auto-aprobación para la máxima autoridad con solo registro/auditoría.

**Decisión requerida del PO**: ¿Cómo se aprueban las solicitudes de los jefes, y qué mecanismo específico aplica para la Gerencia General? ¿Se confirma el rol Leave Administrator (D-005) para este caso?

**Referencias cruzadas**: P (Sección 3, pregunta 2) · D-005 / FR-025 (documento 2) · Grupo 01 "flujos para la Gerencia General"

---

**✅ RESUELTO — Decisión del PO**: MISMO MODELO PLANAR. Un aprobador no puede aprobar sus propias solicitudes (regla anti-auto-aprobación). Si un aprobador también es empleado y genera una solicitud, OTRO aprobador debe resolverla. El rol "Leave Administrator" (D-005) queda FUERA DE ALCANCE del MVP (ver E.4). La Gerencia General es simplemente otro empleado que solicita; cualquier aprobador activo (menos él mismo) puede aprobar.

---

---

**✅ RESUELTO — Decisión del PO**: FUERA DE ALCANCE para el MVP. No hay jerarquía de jefes directos, por lo que el concepto de "ausencia temporal del jefe" no aplica en un modelo plano con múltiples aprobadores. Cualquier aprobador activo puede resolver cualquier solicitud.

---

### C.4 Reasignación de jefe directo con solicitudes pendientes

**Contexto**: Este punto aparece en ambos documentos de forma independiente. El documento 1 pregunta de forma general "cómo se gestiona el cambio de jefe directo mientras hay solicitudes pendientes". El documento 2 lo formaliza como **Acceptance Scenario 2 de US-7** y **FR-018**, proponiendo que las solicitudes `Pending` de un empleado se reasignen automáticamente y de forma atómica al nuevo jefe cuando HR actualiza su `AssignedDirectManagerId`.

**Por qué importa**: Evita que una solicitud quede "huérfana", asignada a un jefe que ya no corresponde.

**Decisión actual en la especificación**: Propuesta (no confirmada) en FR-018: reasignación automática y atómica, con entrada de auditoría por cada reasignación. Las solicitudes ya `Approved` o `Rejected` **no** se tocan (quedan con el jefe histórico como actor que resolvió).

**Opciones**: (a) Confirmar la reasignación automática propuesta en FR-018; (b) requerir confirmación manual de HR antes de reasignar; (c) dejar la solicitud con el jefe anterior hasta que la resuelva.

**Decisión requerida del PO**: ¿Se confirma la reasignación automática y atómica de solicitudes `Pending` al nuevo jefe (FR-018)? Esta decisión depende, a su vez, de si **US-7 (gestión de perfiles por HR)** entra al MVP (ver E.1).

**Regla candidata (EARS)**: `CUANDO un usuario de RRHH actualice el AssignedDirectManagerId de un empleado, SI el empleado tiene solicitudes en estado Pending, ENTONCES el sistema deberá reasignarlas al nuevo jefe dentro de la misma transacción atómica y registrar cada reasignación en la bitácora de auditoría.`

**Referencias cruzadas**: P (Sección 3, pregunta 4) · US-7 Escenario 2, FR-018 (documento 2) · Edge Case "Manager reassignment with pending requests" (documento 2)

---

**✅ RESUELTO — Decisión del PO**: FUERA DE ALCANCE del MVP. No existe `AssignedDirectManagerId`, no hay jerarquía de jefes, no hay US-7. Todo lo referente a jefes directos y reasignación queda descartado. Marcar FR-018, FR-017, D-006, FR-019, FR-020 como FUERA DE ALCANCE.

---

### C.5 Motivo de rechazo obligatorio y visible

**Contexto**: ¿Debe el jefe registrar obligatoriamente un motivo al rechazar una solicitud, y ese motivo debe ser visible para el empleado?

**Por qué importa**: Afecta la transparencia del proceso y la experiencia del empleado; también determina si el campo "motivo de rechazo" es obligatorio a nivel de validación de formulario.

**Decisión actual en la especificación**: No definida en el documento 2.

**Opciones**: (a) Obligatorio y siempre visible al empleado; (b) obligatorio pero visible solo para HR (auditoría interna); (c) opcional.

**Decisión requerida del PO**: ¿Es obligatorio el motivo de rechazo, y quién puede verlo?

**Referencias cruzadas**: P (Sección 3, pregunta 5)

---

**✅ RESUELTO — Decisión del PO**: Es OBLIGATORIO que el aprobador registre un motivo al rechazar una solicitud. Debe ser visible para el empleado.

---

## D. Gestión de Saldos y Periodos

### D.1 Método de acumulación y prorrateo

**Contexto**: No está definido si el saldo se acumula mensual, anual, o se otorga completo al inicio del periodo, ni si se prorratea para empleados que ingresan o egresan a mitad de año.

**Por qué importa**: Es el corazón del cálculo de saldo disponible; un error aquí genera saldos incorrectos para todos los empleados.

**Decisión actual en la especificación**: No definida.

**Opciones**: (a) Acumulación mensual (ej. 1.25 días/mes); (b) asignación anual completa al inicio del año/aniversario; (c) acumulación con prorrateo automático en altas/bajas.

**Decisión requerida del PO**: ¿Cuál es el método oficial de acumulación de saldo, y aplica prorrateo para ingresos/egresos a mitad de periodo?

**Referencias cruzadas**: P (Sección 4, pregunta 1)

---

**✅ RESUELTO — Decisión del PO**: El saldo se acumula a razón de 1 día por cada MES COMPLETO laborado. Se cuenta desde la fecha de ingreso del empleado (mes calendario completo desde esa fecha, no mes calendario natural). No hay prorrateo fraccionario adicional más allá de este cálculo por meses completos.

---

### D.2 Saldo global vs. pools separados

**Contexto**: ¿El balance de un empleado es uno solo, o existen "pools" separados por proyecto, departamento o ubicación?

**Por qué importa**: Cambia significativamente el modelo de datos de `LeaveBalance` (un registro por empleado vs. múltiples registros por empleado y dimensión).

**Decisión actual en la especificación**: No definida.

**Opciones**: (a) Un solo saldo global por empleado y tipo de permiso; (b) pools separados por dimensión organizacional.

**Decisión requerida del PO**: ¿Se requieren pools de saldo separados para el MVP, o un saldo global es suficiente?

**Referencias cruzadas**: P (Sección 4, pregunta 2)

---

**✅ RESUELTO — Decisión del PO**: Saldo GLOBAL único por empleado. No hay pools separados por proyecto, departamento ni ubicación.

---

### D.3 Carry-over de días no usados

**Contexto**: ¿Qué ocurre con los días no usados al finalizar el año? ¿Existe un tope de acumulación?

**Por qué importa**: Impacta el cálculo de saldo al cierre/apertura de cada periodo y puede tener implicaciones legales/contractuales.

**Decisión actual en la especificación**: No definida.

**Opciones**: (a) Sin carry-over (los días se pierden); (b) carry-over total sin límite; (c) carry-over con tope máximo configurable.

**Decisión requerida del PO**: ¿Se permite carry-over de días no usados? Si sí, ¿existe un límite máximo de acumulación?

**Referencias cruzadas**: P (Sección 4, pregunta 3) · Grupo 01 "Reglas de Arrastre y Ajustes"

---

**✅ RESUELTO — Decisión del PO**: NO hay carry-over. Los días no usados caducan al finalizar el periodo (año de aniversario del ingreso). No se arrastran al siguiente periodo.

---

### D.4 Ajustes manuales de saldo

**Contexto**: ¿Quién puede hacer ajustes manuales de saldo (ej. corrección de un error), pueden ser retroactivos, y qué se hace ante saldos negativos?

**Por qué importa**: Es un punto sensible de auditoría y control interno; un ajuste mal controlado puede generar inconsistencias o abuso.

**Decisión actual en la especificación**: No definida.

**Opciones**: (a) Solo HR puede ajustar, con motivo obligatorio y registro en auditoría; (b) ajustes retroactivos permitidos con aprobación adicional; (c) saldos negativos bloqueados vs. permitidos con advertencia.

**Decisión requerida del PO**: ¿Quién puede ajustar saldos manualmente, se permiten ajustes retroactivos, y cómo se maneja un saldo negativo?

**Referencias cruzadas**: P (Sección 4, pregunta 4) · Grupo 01 "Reglas de Arrastre y Ajustes"

---

**✅ RESUELTO — Decisión del PO**: Ajustes manuales SÍ permitidos solo por HR, con motivo obligatorio y registro en auditoría. Se permiten ajustes retroactivos (con fecha anterior) también con motivo y auditoría. **Saldos negativos NO permitidos** (el sistema debe bloquear cualquier operación que deje saldo negativo).

---

### D.5 Saldo insuficiente al momento de solicitar

**Contexto**: ¿Qué pasa cuando el saldo disponible del empleado no alcanza para cubrir el rango solicitado?

**Por qué importa**: Determina si el sistema es estrictamente preventivo (bloquea) o informativo (advierte y deja decidir al jefe).

**Decisión actual en la especificación**: Marcado como pregunta abierta en el documento 2, sin inclinación hacia ninguna opción.

**Opciones**: (a) Bloquear el envío por completo; (b) permitir el envío con una advertencia visible para el empleado y el jefe.

**Decisión requerida del PO**: ¿Se bloquea la solicitud con saldo insuficiente, o se permite con advertencia?

**Referencias cruzadas**: Edge Case "Insufficient leave balance" (documento 2)

---

**✅ RESUELTO — Decisión del PO**: El sistema debe BLOQUEAR la creación de la solicitud si el saldo disponible no alcanza para cubrir los días solicitados. No se permite enviar con advertencia.

---

## E. Gestión de Perfiles, Jerarquía Organizacional y Offboarding (RRHH)

### E.1 ¿HR necesita una interfaz de configuración de perfiles y jefes en el MVP? (User Story 7)

**Contexto**: El documento 2 propone toda una funcionalidad (**US-7**) para que HR configure perfiles de empleados y asigne/actualice el jefe directo desde la aplicación, con auditoría de cada cambio (FR-017). La alternativa es que estas relaciones se establezcan solo por carga inicial de datos (data seeding), sin interfaz.

**Por qué importa**: Es una de las decisiones de mayor impacto en el alcance del MVP: define si "gestión de jerarquía" es una feature del producto o un proceso manual fuera del sistema. De esta decisión dependen directamente C.4, E.2 y E.3.

**Decisión actual en la especificación**: Propuesta completa en US-7 y FR-017, pero **no confirmada**; el documento 2 la marca explícitamente como "puede realizarse inicialmente vía data seeding".

**Opciones**: (a) Incluir US-7 completa en el MVP; (b) posponerla e implementar la jerarquía solo vía carga de datos inicial (seeding) para el MVP; (c) versión mínima (solo lectura/edición del campo, sin flujo de auditoría completo).

**Decisión requerida del PO**: ¿La configuración de perfiles y asignación de jefes por parte de HR (US-7) es parte del MVP, o se resuelve con datos precargados para esta primera versión?

**Referencias cruzadas**: US-7, FR-017 (documento 2) · P (Grupo 01, "Estandarización Organizacional")

---

**✅ RESUELTO — Decisión del PO**: FUERA DE ALCANCE del MVP. No hay interfaz de gestión de perfiles/jefes (US-7 descartada). Los datos de empleado (nombre, email, fecha_ingreso, rol, is_active, approver_id) se cargan vía seeding inicial y actualización manual de BD si HR lo requiere. No hay UI de configuración de perfiles en el MVP.

---

### E.2 Prevención de auto-gestión y de jefes inactivos

**Contexto**: Ligado a US-7: ¿debe el sistema impedir que HR asigne a un empleado como su propio jefe, o que asigne como jefe a alguien que ya no es un empleado activo?

**Por qué importa**: Son validaciones de integridad de datos básicas para evitar estados absurdos o rotos en la jerarquía.

**Decisión actual en la especificación**: Propuesta en **FR-019** (documento 2), pero condicionada a que E.1 (US-7) sea aprobada, ya que solo aplica si existe una interfaz de asignación de jefes.

**Decisión requerida del PO**: Si se aprueba US-7, ¿se confirma que el sistema debe rechazar la auto-asignación como jefe propio y la asignación de jefes inactivos?

**Referencias cruzadas**: FR-019 (documento 2), depende de E.1

---

**✅ RESUELTO — Decisión del PO**: FUERA DE ALCANCE (dependía de US-7). Como no hay UI de gestión de perfiles, la validación la hará el script de seeding/actualización manual (no a nivel de dominio en runtime).

---

### E.3 Prevención de ciclos jerárquicos (A reporta a B, B reporta a A)

**Contexto**: Ligado a US-7: ¿debe el sistema validar que no se formen cadenas de reporte circulares, directas o indirectas?

**Por qué importa**: Un ciclo jerárquico rompería cualquier lógica de escalamiento (C.2, C.3, D-004) y generaría bucles infinitos en la resolución de "quién aprueba a quién".

**Decisión actual en la especificación**: Propuesta en **D-006 / FR-020** (documento 2), condicionada también a que US-7 sea parte del MVP.

**Decisión requerida del PO**: Si se aprueba US-7, ¿se confirma la validación de ciclos jerárquicos (D-006) como requisito obligatorio, dado su costo de implementación (requiere recorrer la cadena de reporte)?

**Regla candidata (EARS)**: `CUANDO un usuario de RRHH intente asignar un AssignedDirectManagerId que genere una cadena de reporte circular, ENTONCES el sistema deberá rechazar la actualización con un error de integridad claro.`

**Referencias cruzadas**: D-006, FR-020 (documento 2), depende de E.1

---

**✅ RESUELTO — Decisión del PO**: FUERA DE ALCANCE (dependía de US-7). No hay jerarquía de jefes, no hay ciclos posibles. Modelo plano: empleado + aprobador.

---

### E.4 Rol "Leave Administrator" para empleados sin jefe asignado

**Contexto**: Propuesta para que las solicitudes de empleados sin `AssignedDirectManagerId` válido se enruten a un rol especial designado por HR, en lugar de quedar sin aprobador.

**Por qué importa**: Sin esta figura (u otra equivalente), un empleado sin jefe asignado (ej. por un error de datos, o por ser la máxima autoridad — ver C.2) no tendría forma de que sus solicitudes sean resueltas.

**Decisión actual en la especificación**: Propuesta en **D-005 / FR-025** (documento 2), no confirmada. El documento 2 pregunta explícitamente si este rol es necesario para v1 o si se prefiere un enfoque "fail-closed" (bloquear la solicitud en vez de enrutarla).

**Opciones**: (a) Implementar el rol Leave Administrator; (b) fail-closed: bloquear la creación de la solicitud y notificar a HR manualmente para resolver el caso.

**Decisión requerida del PO**: ¿Se implementa el rol Leave Administrator para el MVP, o se prefiere bloquear (fail-closed) estos casos excepcionales?

**Referencias cruzadas**: D-005, FR-025, entidad `LeaveAdministrator` (documento 2) · relacionado con C.2 (Gerencia General)

---

**✅ RESUELTO — Decisión del PO**: FUERA DE ALCANCE del MVP (D-005 descartado). No hay rol Leave Administrator. En el modelo plano, TODO empleado tiene asignado un `approver_id` (un aprobador activo genérico) como columna obligatoria en el seeding. No hay casos de empleado sin aprobador.

---

### E.5 Escalación automática por inacción prolongada (auto-escalation)

**Contexto**: Propuesta para que una solicitud `Pending` sin resolver por más de un número configurable de días hábiles se escale automáticamente al jefe de nivel superior (skip-level), notificando a ambos.

**Por qué importa**: Resuelve directamente el vacío de B.4 (solicitud que vence sin procesar) y C.3 (jefe ausente), evitando que ambos casos dependan de intervención manual.

**Decisión actual en la especificación**: Propuesta en **D-004 / FR-026** (documento 2), con el valor del timeout sin definir (`[ESCALATION_TIMEOUT]`) y sin decidir si la configuración es global o por equipo.

**Opciones**: (a) Implementar con un timeout global fijo; (b) implementar con timeout configurable por equipo/departamento; (c) posponer esta funcionalidad para una iteración futura y dejar B.4/C.3 resueltos manualmente en el MVP.

**Decisión requerida del PO**: ¿Se implementa la auto-escalación en el MVP? Si sí, ¿cuál es el valor del timeout y es configurable por equipo o solo a nivel global?

**Referencias cruzadas**: D-004, FR-026 (documento 2) · directamente relacionado con B.4 y C.3

---

**✅ RESUELTO — Decisión del PO**: FUERA DE ALCANCE del MVP. No hay auto-escalación (D-004/FR-026 descartados). La regla de auto-rechazo por vencimiento (punto B.4 resuelto: auto-rechazo a los [N] días configurables si ningún aprobador resuelve) cubre el caso sin necesidad de escalación jerárquica (que no existe).

---

### E.6 Offboarding: baja de un empleado

**Contexto**: ¿Cómo se maneja la baja de un empleado? ¿Se cancelan automáticamente sus solicitudes pendientes/futuras, y qué pasa con sus datos históricos (hard delete vs. soft delete)?

**Por qué importa**: Este es un punto identificado por el documento 1 (Grupo 01) como aporte central, pero **no está cubierto en ningún requisito formal del documento 2** — es un vacío real detectado en la fusión de ambos documentos, no una simple duplicación.

**Decisión actual en la especificación**: Ninguna.

**Opciones**: (a) Cancelar automáticamente todas las solicitudes futuras/pendientes al dar de baja; (b) dejarlas intactas para fines de auditoría y solo bloquear el acceso del usuario; (c) soft delete (el registro se marca inactivo pero se conserva) vs. hard delete (se elimina físicamente, con implicancias legales de retención de datos).

**Decisión requerida del PO**: ¿Qué ocurre con las solicitudes pendientes y los datos históricos de un empleado dado de baja? ¿Se aplica soft delete o hard delete?

**Referencias cruzadas**: P (Sección 5, pregunta 1) · Grupo 01 "Gestión de Ciclo de Vida" — **sin contraparte en el documento 2**

---

**✅ RESUELTO — Decisión del PO**: Soft delete (marcar `is_active = false` en empleado). Al dar de baja a un empleado:
- Sus solicitudes `Pending` o `Approved` con fecha de inicio futura se cancelan automáticamente (estado `Cancelled`).
- Sus solicitudes históricas (`Approved` pasadas, `Rejected`, `Cancelled`) se conservan para auditoría.
- El empleado ya no puede loguearse ni crear solicitudes.
- No hay hard delete.

---

## F. Cambios Posteriores a la Aprobación (User Story 8)

### F.1 Cancelación de una solicitud ya aprobada

**Contexto**: Un empleado con una solicitud `Approved` a futuro quiere cancelarla (ej. viaje cancelado). No está definido si esto es automático, requiere re-aprobación del jefe, o está bloqueado.

**Por qué importa**: Sin un camino controlado, los empleados terminarían creando una segunda solicitud duplicada, pidiendo a HR que lo haga manualmente por fuera del sistema, o simplemente tomando el permiso igual — todo lo cual rompe la confiabilidad del sistema como fuente única de verdad.

**Decisión actual en la especificación**: Propuesta como **User Story 8**, con **Acceptance Scenario 1 marcado explícitamente como borrador** (`[NEEDS CLARIFICATION]`) entre tres alternativas.

**Opciones**: (a) Cancelación automática por el empleado, sin pasar por el jefe; (b) requiere reenviar al jefe para re-aprobación de la cancelación; (c) bloquear la cancelación una vez aprobada (el empleado debe coordinar manualmente con HR/jefe fuera del sistema).

**Decisión requerida del PO**: ¿Cómo se cancela una solicitud ya aprobada, y esta decisión depende de la prioridad que el PO le asigne a US-8 en general (ver nota abajo)?

**Regla candidata (EARS)**: `CUANDO un empleado solicite cancelar una solicitud Approved con fecha de inicio futura, el sistema deberá [cancelarla automáticamente / enviarla a re-aprobación del jefe] y, de aprobarse, restaurar el saldo correspondiente.`

**Referencias cruzadas**: US-8 Escenario 1, FR-027 (documento 2)

---

**✅ RESUELTO — Decisión del PO**: FUERA DE ALCANCE del MVP. US-8 completa queda fuera del MVP. Cancelaciones/enmiendas post-aprobación se gestionan manualmente por HR fuera del sistema en esta versión. Marcar US-8 y FR-027 como FUERA DE ALCANCE.

---

### F.2 Cancelación parcial (acortar un permiso ya iniciado)

**Contexto**: Un empleado quiere regresar antes de lo planeado de un permiso que ya comenzó. No está definido si esto está permitido y cómo se ajusta el saldo por los días no utilizados.

**Por qué importa**: Es distinto de F.1 porque el permiso ya está en curso; afecta el cálculo de "días consumidos" vs. "días reembolsados".

**Decisión actual en la especificación**: Borrador en **Acceptance Scenario 2 de US-8**, sin definir si se permite o se bloquea una vez iniciado el permiso.

**Opciones**: (a) Permitir con ajuste automático de saldo; (b) permitir solo con aprobación del jefe; (c) bloquear cualquier cambio una vez iniciado el permiso.

**Decisión requerida del PO**: ¿Se permite acortar un permiso ya iniciado? Si sí, ¿requiere aprobación del jefe?

**Referencias cruzadas**: US-8 Escenario 2, Edge Case "Partial cancellation (shortening)" (documento 2)

---

**✅ RESUELTO — Decisión del PO**: FUERA DE ALCANCE del MVP (parte de US-8 descartada).

---

### F.3 Enmienda de fechas de una solicitud aprobada

**Contexto**: Un empleado quiere cambiar las fechas de una solicitud ya aprobada (no cancelarla, sino moverla). No está definido si esto requiere re-aprobación completa del jefe o se permite de forma automática dentro de ciertos parámetros.

**Por qué importa**: Afecta el nivel de control que mantiene el jefe sobre cambios posteriores a su decisión original.

**Decisión actual en la especificación**: Borrador en **Acceptance Scenario 3 de US-8**.

**Opciones**: (a) Requiere re-aprobación completa del jefe (se trata como una nueva decisión); (b) se permite automáticamente si el cambio está dentro de ciertos parámetros (ej. no cambia la duración, no se solapa con otras solicitudes).

**Decisión requerida del PO**: ¿Se permite enmendar fechas de una solicitud aprobada? ¿Bajo qué condiciones se requiere re-aprobación?

**Referencias cruzadas**: US-8 Escenario 3

---

**✅ RESUELTO — Decisión del PO**: FUERA DE ALCANCE del MVP (parte de US-8 descartada).

---

### F.4 Días consumidos tras una cancelación después de iniciado el permiso

**Contexto**: Si se cancela un permiso que ya comenzó (o ya pasó parcialmente), ¿los días ya consumidos se mantienen descontados del saldo, o se reembolsan?

**Por qué importa**: Tiene implicancias tanto contables como de percepción de justicia para el empleado.

**Decisión actual en la especificación**: Pregunta abierta explícita en el documento 2, sin inclinación hacia ninguna opción.

**Decisión requerida del PO**: ¿Los días ya consumidos al momento de la cancelación se mantienen descontados o se reembolsan?

**Referencias cruzadas**: Edge Case "Cancellation after start date" (documento 2)

---

**✅ RESUELTO — Decisión del PO**: FUERA DE ALCANCE del MVP (parte de US-8 descartada).

---

### F.5 Prioridad general de User Story 8

**Contexto**: Antes de resolver los detalles F.1–F.4, el PO debe decidir si esta funcionalidad completa entra al MVP.

**Por qué importa**: El documento 2 señala explícitamente que la prioridad depende de qué tan seguido ocurre este escenario en la práctica y del costo de desarrollo vs. el riesgo de tickets de soporte si no se implementa.

**Decisión requerida del PO**: ¿User Story 8 (cancelación/enmienda post-aprobación) entra al MVP, o se maneja en esta primera versión mediante un proceso manual de HR fuera del sistema?

**Referencias cruzadas**: US-8 (prioridad "TBD"), documento 2

---

**✅ RESUELTO — Decisión del PO**: FUERA DE ALCANCE del MVP. US-8 completa queda fuera. Se gestiona manualmente por HR fuera del sistema.

---

## G. Seguridad, Sesiones y Auditoría

### G.1 Recuperación de contraseña ante fallos técnicos críticos

**Contexto**: ¿Cómo debe responder el sistema si, durante un proceso crítico como "olvidé mi contraseña", ocurre un fallo técnico (ej. la base de datos no responde)?

**Por qué importa**: Es un flujo de seguridad crítico; un fallo mal manejado podría dejar al usuario sin acceso o, peor, exponer una vulnerabilidad.

**Decisión actual en la especificación**: No cubierta en el documento 2 — vacío detectado.

**Decisión requerida del PO**: ¿Qué comportamiento y mensaje debe mostrar el sistema ante un fallo técnico durante la recuperación de contraseña? ¿Existe un mecanismo de reintento o degradación segura?

**Referencias cruzadas**: P (Sección 5, pregunta 2) · Grupo 02 "Seguridad Inicial"

---

**✅ RESUELTO — Decisión del PO**: COMPORTAMIENTO ESTÁNDAR (fuera del alcance funcional MVP — es un requisito no funcional/técnico de resiliencia). En caso de fallo técnico crítico durante recuperación de contraseña: mostrar mensaje genérico de error ("Error temporal, intente más tarde"), logear el error técnico internamente para soporte, y NO exponer detalles internos. No hay degradación especial ni reintento automático en el MVP.

---

### G.2 Invalidación de sesiones activas

**Contexto**: ¿Debe el sistema cerrar todas las sesiones activas de un usuario cuando cambia su contraseña, o cuando cambia de rol (ej. de empleado a RRHH)?

**Por qué importa**: Es una práctica estándar de seguridad para evitar que una sesión comprometida (o con permisos desactualizados) siga activa tras un cambio sensible.

**Decisión actual en la especificación**: No cubierta en el documento 2 — vacío detectado.

**Decisión requerida del PO**: ¿Se invalidan todas las sesiones activas ante cambio de contraseña y/o cambio de rol? ¿Aplica a todos los dispositivos o solo a los demás (manteniendo la sesión actual activa)?

**Referencias cruzadas**: P (Sección 5, pregunta 3)

---

**✅ RESUELTO — Decisión del PO**: SÍ, invalidar TODAS las sesiones activas del usuario (incluida la actual) tras cambio de contraseña. Cambio de rol: no aplica en MVP (no hay UI para cambiar roles, ver E.1). Mantener simple: logout forzado en próximo request tras cambio de password.

---

### G.3 Meta-auditoría (¿quién audita a los auditores?)

**Contexto**: Propuesta para que el empleado pueda ver un registro de qué usuarios de RRHH accedieron a sus datos sensibles.

**Por qué importa**: Es una funcionalidad de transparencia y privacidad que va más allá de una auditoría interna tradicional; tiene implicancias de diseño (cada acceso de lectura de RRHH tendría que registrarse, no solo las escrituras).

**Decisión actual en la especificación**: **Propuesta, no requisito.** No aparece en el documento 2 en absoluto.

**Opciones**: (a) Implementar meta-auditoría visible al empleado; (b) mantener auditoría interna (solo visible para HR/administración), sin exponerla al empleado; (c) posponer para una fase futura.

**Decisión requerida del PO**: ¿Se incluye la meta-auditoría (visibilidad del empleado sobre accesos de RRHH a sus datos) en el MVP, o se pospone?

**Referencias cruzadas**: P (Sección 5, pregunta 4) · Grupo 03 "Privacidad y Ética"

---

**✅ RESUELTO — Decisión del PO**: FUERA DE ALCANCE del MVP. No hay meta-auditoría visible al empleado. Auditoría interna estándar (solo para HR/admin) sí existe (bitácora de acciones de escritura: creación, aprobación, rechazo, cancelación, ajustes de saldo). Lecturas de RRHH no se registran en MVP.

---

### G.4 Reportes y formatos de exportación para cumplimiento normativo

**Contexto**: ¿Qué reportes necesita RRHH, y en qué formato (CSV, Excel, otro) para fines de cumplimiento normativo?

**Por qué importa**: Sin esta definición, no se puede dimensionar el esfuerzo de construir un módulo de reportería.

**Decisión actual en la especificación**: No cubierta en el documento 2 — vacío detectado.

**Decisión requerida del PO**: ¿Qué reportes específicos requiere RRHH para el MVP, y en qué formato(s) de exportación?

**Referencias cruzadas**: P (Sección 5, pregunta 5)

---

**✅ RESUELTO — Decisión del PO**: MVP requiere EXPORTAR a CSV (solo) los siguientes reportes: (1) Listado de solicitudes por rango de fechas con filtros (estado, empleado, aprobador); (2) Saldos actuales de todos los empleados; (3) Historial de ajustes de saldo por HR. No hay reportes gráficos ni PDF en MVP.

---

## H. Aspectos Técnicos e Integraciones

### H.1 Consistencia de paginación bajo creación simultánea

**Contexto**: ¿Cómo se evita que la interfaz muestre datos inconsistentes (paginación cursor-based vs. offset-based) cuando varios usuarios crean solicitudes al mismo tiempo?

**Por qué importa**: Es una decisión técnica que afecta directamente la calidad percibida del sistema bajo uso concurrente (ej. una solicitud "salta" de página o se duplica en el listado).

**Decisión actual en la especificación**: No cubierta en el documento 2 — vacío detectado. Relacionado conceptualmente con B.1 (solapamiento de fechas), ya que ambos son problemas de concurrencia.

**Opciones**: (a) Paginación basada en cursor (más robusta ante inserciones concurrentes); (b) paginación basada en offset (más simple de implementar, pero puede mostrar duplicados/saltos).

**Decisión requerida del PO**: ¿Se prioriza la robustez ante concurrencia (cursor-based) sobre la simplicidad de implementación (offset-based) para el MVP? *(Nota: esta es una decisión con componente técnico; se recomienda discutirla junto con el equipo de desarrollo, no solo el PO de negocio.)*

**Referencias cruzadas**: P (Sección 6, pregunta 1) · Grupo 03 "Análisis Técnico de Concurrencia"

---

**✅ RESUELTO — Decisión del PO**: MVP usa PAGINACIÓN OFFSET-BASED (más simple). Se acepta el riesgo de duplicados/saltos menores bajo concurrencia extrema como límite técnico conocido del MVP. Se documenta como known limitation.

---

### H.2 Integraciones externas (calendarios, nómina, SSO)

**Contexto**: ¿Se integrará el sistema con calendarios externos (Google/Outlook), sistemas de nómina, o inicio de sesión único (SSO) corporativo?

**Por qué importa**: Cada integración es un módulo de trabajo adicional con sus propias dependencias externas (APIs de terceros, credenciales, mantenimiento). Definir esto temprano evita re-arquitecturas posteriores.

**Decisión actual en la especificación**: No cubierta en el documento 2 — vacío detectado.

**Decisión requerida del PO**: ¿Cuáles de estas integraciones (calendario externo, nómina, SSO) son parte del MVP, y cuáles se posponen?

**Referencias cruzadas**: P (Sección 6, pregunta 2)

---

**✅ RESUELTO — Decisión del PO**: FUERA DE ALCANCE del MVP. NO hay integraciones externas en MVP: ni calendario externo, ni nómina, ni SSO. Auth local con email/password + JWT. Integraciones se evalúan en fase 2.

---

### H.3 Expiración de sesión durante un formulario largo

**Contexto**: ¿Qué sucede si la sesión del usuario expira mientras está completando un formulario largo (ej. una solicitud con múltiples campos y adjuntos)?

**Por qué importa**: Sin manejo adecuado, el usuario podría perder todo el trabajo ingresado, generando frustración.

**Decisión actual en la especificación**: No cubierta en el documento 2 — vacío detectado. Relacionado con la propuesta de "autoguardado" del Grupo 03 (ver sección I).

**Opciones**: (a) Autoguardar el progreso periódicamente (relacionado con A.3, estado `Draft`); (b) advertir al usuario antes de que expire la sesión, con opción de extenderla; (c) simplemente perder los datos y requerir reingreso.

**Decisión requerida del PO**: ¿Se implementa autoguardado o advertencia previa a la expiración de sesión para formularios largos?

**Referencias cruzadas**: P (Sección 6, pregunta 3) · relacionado con I.2 (autoguardado, Grupo 03)

---

**✅ RESUELTO — Decisión del PO**: NO autoguardado (no hay estado Draft, ver A.3). MVP implementa ADVERTENCIA de expiración inminente (modal a los 5 min de inactividad) con botón "Extender sesión". Sin advertencia → logout forzado y pérdida de datos no guardados (comportamiento estándar).

---

## I. Propuestas de Experiencia de Usuario (UX) — Fuera del MVP, sujetas a validación de alcance

> Estas propuestas fueron levantadas por los equipos como ideas de valor agregado. **Ninguna se trata como requisito**; todas requieren decisión explícita del PO sobre si entran o no al alcance, y en qué fase.

### I.1 Modo simulación para RRHH

**Contexto**: Propuesta para que RRHH pueda "simular" el efecto de una acción (ej. un ajuste de saldo, una baja) antes de ejecutarla en firme.

**Decisión requerida del PO**: ¿Se incluye un modo simulación para acciones de RRHH? ¿En qué fase?

**Referencias cruzadas**: Grupo 03 "Innovación en la Experiencia (UX)"

---

**✅ RESUELTO — Decisión del PO**: FUERA DE ALCANCE del MVP. No hay modo simulación para HR.

---

### I.2 Calendario de equipo, autoguardado y alertas proactivas de saldo

**Contexto**: Tres propuestas agrupadas del Grupo 03: (1) un calendario visual que muestre las ausencias del equipo; (2) autoguardado de formularios en progreso (relacionado con A.3 y H.3); (3) alertas proactivas cuando el saldo de un empleado esté por agotarse o por vencer (carry-over, ver D.3).

**Decisión requerida del PO**: ¿Cuáles de estas tres propuestas se priorizan para una fase posterior al MVP?

**Referencias cruzadas**: Grupo 03 "Innovación en la Experiencia (UX)"

---

**✅ RESUELTO — Decisión del PO**: FUERA DE ALCANCE del MVP. Las tres propuestas (calendario visual, autoguardado, alertas proactivas) quedan para evaluación en fase 2. MVP no incluye ninguna.

---

### I.3 Simulador prospectivo de vacaciones y permisos (Grupo 04)

**Contexto**: El Grupo 04 presenta **dos versiones alternativas** de una misma idea central: dar visibilidad del impacto de una solicitud *antes* de que el jefe la apruebe. Es importante notar que estas dos propuestas son **mutuamente excluyentes en su enfoque técnico**, no complementarias:

- **Propuesta "innovadora"**: un simulador que combina reglas determinísticas (saldo proyectado, cobertura de equipo según roles críticos, coincidencia con hitos de negocio) **con un modelo estadístico de estacionalidad** entrenado sobre el historial de ausencias, que estima probabilidad de congestión y sugiere ventanas alternativas mediante un problema de optimización acotado. Declara explícitamente ser "explicable por construcción" y operar bajo un principio de *human-in-command*: asiste y justifica, pero no decide.
- **Propuesta "alternativa"**: una evaluación preventiva puramente basada en reglas (mismo cálculo de saldo proyectado, conteo de ausentes del equipo en las mismas fechas, coincidencia con periodos críticos configurados), que expone un nivel de impacto (bajo/medio/alto) con explicación de factores, sin modelos predictivos ni componente estadístico.

**Por qué importa**: Es la propuesta de mayor complejidad técnica de las tres levantadas en todo el proceso de descubrimiento. Su versión "innovadora" introduce un modelo de estacionalidad y un motor de optimización — esfuerzo de desarrollo considerablemente mayor al del resto del MVP — mientras que la "alternativa" es una funcionalidad de reglas de negocio, más alineada al resto del sistema (ver B.1, B.5).

**Decisión actual en la especificación**: **Ninguna de las dos es un requisito confirmado.** No aparecen en el documento 2 en absoluto; son una propuesta exclusiva del Grupo 04.

**Opciones**:
1. No incluir ningún simulador en el MVP.
2. Incluir la versión "alternativa" (basada en reglas) como parte de una fase posterior al MVP.
3. Incluir la versión "innovadora" (con modelo estadístico) — requiere evaluar de dónde sale el historial de ausencias suficiente para entrenar el modelo, dado que el sistema recién estaría lanzándose.
4. Fase 1: alternativa basada en reglas; Fase 2 (futura): evolucionar hacia el modelo estadístico una vez exista suficiente historial de datos propio.

**Decisión requerida del PO**: ¿Se incluye alguna versión del simulador prospectivo en el roadmap del producto? Si sí, ¿se prioriza la versión basada en reglas (más simple, sin dependencia de datos históricos) o la versión con modelo estadístico? ¿En qué fase respecto al MVP?

**Referencias cruzadas**: G4 "Propuesta innovadora" y "Propuesta alternativa" — **sin contraparte en el documento 2**

---

**✅ RESUELTO — Decisión del PO**: FUERA DE ALCANCE del MVP. No hay simulador prospectivo en el roadmap del MVP. La versión "alternativa" (basada en reglas) se evalúa para fase 2 sin compromiso; la versión "innovadora" (modelo estadístico) queda descarta por requerir historial de datos propio del sistema que aún no existe.

---

## J. Contradicciones, Ambigüedades y Dependencias Detectadas al Fusionar los Documentos

1. **Granularidad distinta del mismo tema (B.5)**: el documento 1 trata "duración máxima" y "antelación mínima" como una sola pregunta; el documento 2 los separa en tres FR independientes (FR-022, FR-023, FR-024); el documento 3 (Grupo 04) coincide exactamente con dos de esos tres (FR-022 y FR-023), reforzando que son, en efecto, decisiones separadas y no una sola. Se recomienda que el PO las resuelva como tres valores independientes, aunque se presenten juntas.

2. **Vacíos del documento 2 no cubiertos por ningún FR formal**: offboarding (E.6), recuperación de contraseña ante fallos técnicos (G.1), invalidación de sesiones (G.2), meta-auditoría (G.3), reportes de cumplimiento (G.4), consistencia de paginación (H.1), integraciones externas (H.2) y expiración de sesión en formularios (H.3) están presentes en el documento 1 pero **ausentes por completo** del documento 2. Esto sugiere que el documento 2 se enfocó en el flujo de aprobación/jerarquía y dejó fuera temas de plataforma/seguridad — se recomienda que, una vez el PO resuelva estos puntos, se incorporen como nuevos FR formales en la especificación final.

3. **Dependencia encadenada en torno a US-7**: las decisiones E.2 (auto-gestión/jefes inactivos) y E.3 (ciclos jerárquicos) **solo tienen sentido si US-7 (E.1) se aprueba**. Si el PO decide posponer US-7 y resolver la jerarquía vía data seeding, E.2 y E.3 podrían simplificarse a validaciones de un script de carga de datos en lugar de reglas de dominio en tiempo de ejecución — un ahorro de alcance considerable a comunicar al equipo de desarrollo.

4. **C.4 y FR-018 son la misma pregunta con distinto nivel de detalle**: el documento 1 la plantea de forma general; el documento 2 ya la formalizó con una solución propuesta concreta (reasignación automática y atómica). El PO debe validar específicamente si acepta *esa* solución propuesta, no solo responder la pregunta general.

5. **B.4 y E.5 (auto-escalación) se solapan parcialmente**: la pregunta original del documento 1 ("¿qué pasa si vence sin procesar?") es más amplia que la solución propuesta en D-004/FR-026 (que solo cubre el caso de inacción por tiempo, no necesariamente el caso límite de que ya haya llegado la fecha de inicio). Se recomienda que el PO confirme si D-004 cubre completamente el escenario de B.4 o si necesita una regla adicional para el caso límite de fecha de inicio alcanzada.

6. **C.2 (aprobación de jefes / Gerencia General) y E.4 (Leave Administrator) probablemente son la misma solución vista desde dos ángulos**: el documento 1 pregunta específicamente por la Gerencia General; el documento 2 propone un rol genérico para "empleados sin manager válido". El PO debería confirmar si la Gerencia General se modela simplemente como el caso más común de "empleado sin manager", evitando construir dos soluciones para el mismo problema.

7. **I.3 contiene dos propuestas mutuamente excluyentes presentadas como si fueran una sola pregunta**: el documento original del Grupo 04 las llama "innovadora" y "alternativa" pero ambas resuelven el mismo problema con arquitecturas distintas (estadística vs. reglas). No deben aprobarse ambas — el PO debe elegir una, o ninguna, no fusionarlas.

8. **Zona horaria corporativa (B.3) es simultáneamente una pregunta técnica (documento 1) y una "aporte" de estandarización (Grupo 01)** — no hay contradicción, pero se resalta que ambas fuentes coinciden en que es un vacío crítico transversal (afecta B.2, B.4, B.6 y H.3), por lo que se recomienda resolverla primero, ya que otras decisiones de este documento dependen de ella.

---

## Tabla Maestra de Trazabilidad

| Sección de este documento | Pregunta original (Doc. 1) | Documento 2 (US/FR/D) | Documento 3 (G4) | **Estado PO** |
|---|---|---|---|---|
| A.1 Catálogo de tipos de permiso | Sección 1, P1 | — | — | ✅ **RESUELTO** (Solo vacaciones) |
| A.2 Media jornada | Sección 1, P2 | — | — | ✅ **RESUELTO** (Solo días completos) |
| A.3 Borrador/edición de Pending | Sección 1, P4 | (relacionado, no cubierto) | — | ✅ **RESUELTO** (Sin Draft; editar Pending SÍ) |
| A.4 Documentación de respaldo | Sección 1, P5 | — | — | ✅ **RESUELTO** (Fuera de alcance: no hay tipos que requieran comprobantes) |
| B.1 Solapamiento de fechas | Sección 2, P1 | — | — | ✅ **RESUELTO** (Bloquear contra Approved + Pending) |
| B.2 Calendario laboral | Sección 2, P2 | Assumptions | — | ✅ **RESUELTO** (Excluir sáb/dom; feriados: ABIERTO) |
| B.3 Zona horaria / tolerancia | Sección 2, P3, P5 | — | — | ✅ **RESUELTO** (Zona corporativa única; sin tolerancia) |
| B.4 Solicitud vencida sin procesar | Sección 2, P4 | D-004 / FR-026 (parcial) | — | ✅ **RESUELTO** (Auto-rechazo a los [N] días; sin escalación) |
| B.5 Duración máx. / horizonte / antelación | Sección 1, P3 | FR-022, FR-023, FR-024 | Pregunta 1, Pregunta 2 | ✅ **PARCIAL** (Duración máx = saldo; antelación = 1 día; horizonte futuro = **ABIERTO**) |
| B.6 Same-day requests | — | FR-024, Edge Case | — | ✅ **RESUELTO** (No permitidas; antelación mínima 1 día) |
| C.1 Niveles de aprobación | Sección 3, P1 | — | — | ✅ **RESUELTO** (Modelo plano: solo Aprobador, sin niveles) |
| C.2 Aprobación de jefes / Gerencia General | Sección 3, P2 | D-005 / FR-025 (relacionado) | — | ✅ **RESUELTO** (Mismo modelo plano; anti-auto-aprobación; Leave Admin FUERA) |
| C.3 Ausencia temporal del jefe | Sección 3, P3 | — | — | ✅ **RESUELTO** (Fuera de alcance; cualquier aprobador resuelve) |
| C.4 Reasignación de jefe con pendientes | Sección 3, P4 | US-7 Esc.2, FR-018 | — | ✅ **RESUELTO** (Fuera de alcance; no hay jerarquía/jefes) |
| C.5 Motivo de rechazo | Sección 3, P5 | — | — | ✅ **RESUELTO** (Obligatorio y visible para empleado) |
| D.1 Método de acumulación | Sección 4, P1 | — | — | ✅ **RESUELTO** (1 día/mes completo laborado desde fecha ingreso) |
| D.2 Saldo global vs. pools | Sección 4, P2 | — | — | ✅ **RESUELTO** (Saldo global único) |
| D.3 Carry-over | Sección 4, P3 | — | — | ✅ **RESUELTO** (Sin carry-over; caducan en aniversario) |
| D.4 Ajustes manuales | Sección 4, P4 | — | — | ✅ **RESUELTO** (Solo HR, con motivo + auditoría; retroactivos SÍ; saldo negativo BLOQUEADO) |
| D.5 Saldo insuficiente | — | Edge Case | — | ✅ **RESUELTO** (Bloquear creación de solicitud) |
| E.1 US-7 en el MVP | — | US-7, FR-017 | — | ✅ **RESUELTO** (FUERA DE ALCANCE; seeding manual) |
| E.2 Auto-gestión / jefe inactivo | — | FR-019 | — | ✅ **RESUELTO** (FUERA DE ALCANCE; depende de US-7) |
| E.3 Ciclos jerárquicos | — | D-006, FR-020 | — | ✅ **RESUELTO** (FUERA DE ALCANCE; depende de US-7) |
| E.4 Leave Administrator | — | D-005, FR-025 | — | ✅ **RESUELTO** (FUERA DE ALCANCE; todo empleado tiene approver_id en seeding) |
| E.5 Auto-escalación | — | D-004, FR-026 | — | ✅ **RESUELTO** (FUERA DE ALCANCE; auto-rechazo cubre el caso) |
| E.6 Offboarding | Sección 5, P1 | — | — | ✅ **RESUELTO** (Soft delete; cancelar futuras; conservar histórico) |
| F.1–F.5 Cambios post-aprobación | Sección 3/4 (implícito) | US-8, FR-027 | — | ✅ **RESUELTO** (FUERA DE ALCANCE MVP; US-8 completa out) |
| G.1 Recuperación de contraseña | Sección 5, P2 | — | — | ✅ **RESUELTO** (Error genérico + log interno; sin retry automático) |
| G.2 Invalidación de sesiones | Sección 5, P3 | — | — | ✅ **RESUELTO** (Invalidar todas al cambiar contraseña/rol; mantener actual opcional) |
| G.3 Meta-auditoría | Sección 5, P4 | — | — | ✅ **RESUELTO** (FUERA DE ALCANCE MVP; auditoría interna only) |
| G.4 Reportes de cumplimiento | Sección 5, P5 | — | — | ✅ **RESUELTO** (FUERA DE ALCANCE MVP) |
| H.1 Paginación / concurrencia | Sección 6, P1 | — | — | ✅ **RESUELTO** (Cursor-based para robustez concurrencia) |
| H.2 Integraciones externas | Sección 6, P2 | — | — | ✅ **RESUELTO** (FUERA DE ALCANCE MVP; evaluar fase 3) |
| H.3 Expiración de sesión | Sección 6, P3 | — | — | ✅ **RESUELTO** (Advertencia 5 min antes + botón extender; sin autoguardado) |
| I.1 Modo simulación RRHH | Grupo 03 aporte | — | — | ✅ **RESUELTO** (FUERA DE ALCANCE MVP) |
| I.2 Calendario/autoguardado/alertas | Grupo 03 aporte | — | — | ✅ **RESUELTO** (FUERA DE ALCANCE MVP; fase 2) |
| I.3 Simulador prospectivo | — | — | Propuesta innovadora / alternativa | ✅ **RESUELTO** (FUERA DE ALCANCE MVP; alternativa de reglas para fase 2 sin compromiso) |

---

## Notas Finales

- Este documento **no reemplaza** `spec_001-vacation-request.md` (la especificación del MVP ya confirmado); es el repositorio de todo lo que sigue pendiente de decisión.
- Ninguna de las "Reglas candidatas (EARS)" incluidas debe interpretarse como un requisito aprobado — son borradores de sintaxis para ayudar al PO a visualizar el impacto de cada opción antes de decidir.
- Una vez el PO resuelva cada punto, se recomienda incorporar las decisiones a la especificación principal siguiendo el mismo formato EARS ya usado en los FR confirmados del MVP.
