# Especificación Funcional: Sistema de Gestión de Solicitudes de Permisos y Vacaciones

Última actualización: 2026-07-17

## 0. Cambios recientes (2026-07-17)

Esta sección resume los cambios aplicados en la última actualización del documento:

1. **Estado EXPIRED para solicitudes**: Las solicitudes pendientes que no sean resueltas tras [N] días ahora cambian su estado a `EXPIRED` (expirada) en lugar de ser rechazadas. El sistema registra actor="SISTEMA_AUTO_EXPIRACION".

2. **No hay ciclos jerárquicos**: Confirmado explícitamente que el sistema NO maneja ni permite ciclos jerárquicos en la estructura organizacional.

3. **Restricción de cancelación de solicitudes aprobadas**: 
   - Una solicitud aprobada **NO puede ser cancelada** una vez que el periodo de vacaciones ha iniciado (fecha inicio <= hoy).
   - Solo un **aprobador** puede cancelar solicitudes aprobadas (no el empleado).
   - El saldo se restaura únicamente si la cancelación ocurre **antes** de que inicie el periodo.

4. **Cancelación parcial NO aplica**: El sistema no soporta cancelación parcial de solicitudes de vacaciones en esta versión del MVP.

5. **Offboarding no aplica**: La gestión de offboarding de empleados no se incluye. El estado activo/inactivo del usuario es suficiente para controlar accesos.

6. **Funcionalidades para versión futura** (fuera de MVP):
   - Recuperación de contraseñas
   - Integraciones externas (nómina, calendario, SSO, AD)
   - Reportes (exportación CSV, dashboards)
   - Calendario de equipo (se trabajará en prototipo)

7. **Auditoría limitada a trazabilidad de solicitudes**: La auditoría se realizará únicamente para registrar movimientos de solicitudes (creación, cambios de estado, ediciones). NO incluye logs de inicio de sesión, cambios de usuario o acciones administrativas.

8. **Gestión de sesiones con Identity Framework**: 
   - Se utilizará ASP.NET Core Identity Framework para autenticación y gestión de sesiones.
   - Las sesiones expirarán después de cierto tiempo de inactividad (configurable).
   - La expiración durante formularios se manejará a nivel de credenciales de la página.

9. **Paginación**: Pendiente de resolución técnica (estrategia a definir).

10. **Simplificación del sistema**: El sistema realizará las solicitudes y acciones necesarias de forma directa, sin complejidades adicionales de flujo.

## 1. Resumen ejecutivo

Objetivo: Proveer un sistema para gestionar el ciclo completo de solicitudes de **vacaciones** (único tipo de permiso en el MVP) dentro de la empresa, desde la creación por parte del empleado, revisión y decisión por un **aprobador** (rol plano, sin jerarquía), hasta la consulta y auditoría por RRHH.

Alcance: Automatizar el flujo de solicitudes de vacaciones, validación de saldos y fechas, notificación a aprobadores y consultas/filtrado por RRHH. No incluye integraciones externas, múltiples niveles de aprobación, jerarquías de jefes directos, ni gestión de perfiles/jefes por RRHH (ver sección Fuera de alcance).

Audiencia: Product Owner, analistas de negocio, RRHH, aprobadores y stakeholders operativos.

## 2. Actores

- **Empleado**: Solicita vacaciones, consulta saldo y estado de solicitudes, puede editar o cancelar solicitudes pendientes que no hayan iniciado. Puede cancelar solicitudes aprobadas solo si el periodo de vacaciones no ha iniciado y solo un usuario aprobador puede ejecutar esta acción.
- **Aprobador**: Revisa, aprueba o rechaza solicitudes de **cualquier** empleado del sistema; puede añadir comentario obligatorio al rechazar. No puede aprobar sus propias solicitudes. Un aprobador inactivo no puede aprobar. Puede cancelar solicitudes aprobadas si el periodo de vacaciones no ha iniciado (descuento de saldo se aplica solo si el periodo no ha comenzado).
- **RRHH**: Acceso de consulta a historial y saldos, con filtros básicos; rol sin permiso de aprobación ni creación/edición de solicitudes.

## 3. Alcance funcional (resumen)

- Crear solicitud de **vacaciones** (único tipo) con fechas y motivo.
- Validaciones de fecha (inicio ≥ mañana), saldo disponible y solapamiento con solicitudes **Approved o Pending** del mismo empleado.
- Flujo de notificación y bandeja de **aprobadores** (rol plano, sin jerarquía) con filtros.
- Aprobación/rechazo con **comentario obligatorio al rechazar**; descuento de saldo solo al aprobar.
- Auto-rechazo de solicitudes `Pending` tras **[N] días** sin resolver (parámetro configurable); estado de la solicitud pasa a `Expired` (expirada).
- Consulta y filtrado por RRHH (sin reportes en MVP).
- Edición y cancelación de solicitudes `Pending` por el empleado, y cancelación de solicitudes `Approved` solo antes de que inicie el periodo de vacaciones (requiere aprobador).
- Acumulación de saldo: 1 día por mes completo laborado desde fecha de ingreso; carry-over ilimitado entre periodos (sin tope definido — **abierto**).
- Cálculo de duración en **días calendario**, excluyendo **sábados y domingos** (feriados: **abierto**).
- Un aprobador **no puede aprobar sus propias solicitudes**; un aprobador **inactivo** no puede aprobar.

## 4. Historias de usuario

Nota: las siguientes historias mantienen el formato funcional original pero reformulan los criterios como sentencias EARS usando palabras clave como "Cuando", "Si", "Mientras" y "En caso de" para facilitar su transformación a requisitos formales.

HU-01: Solicitar vacaciones con fechas y motivo
- Como empleado quiero solicitar vacaciones con fecha de inicio/fin y motivo para ausentarme.
- Criterios:
  - Cuando el empleado completa las fechas de inicio y fin, entonces el sistema debe calcular y mostrar los días solicitados (excluyendo sábados y domingos) antes de confirmar.
  - Cuando la fecha de inicio sea anterior al día siguiente a la fecha actual, entonces el sistema debe bloquear el envío y mostrar: "La fecha de inicio no puede ser anterior a mañana".
  - Cuando la fecha de fin sea anterior a la fecha de inicio, entonces el sistema debe bloquear el envío y mostrar: "La fecha de fin no puede ser anterior a la de inicio".
  - Si los días solicitados exceden el saldo disponible del empleado, entonces el sistema debe impedir el envío y mostrar: "Saldo insuficiente para esta solicitud".
  - Si el rango solicitado incluye días que ya están en solicitudes **Approved o Pending** del mismo empleado, entonces el sistema debe impedir la creación y mostrar: "La solicitud incluye días que ya están comprometidos en otra solicitud".
  - Cuando todas las validaciones anteriores sean correctas, entonces el sistema debe crear la solicitud en estado "PENDIENTE" y notificar la bandeja de aprobadores.

HU-02: Ver el estado de mis solicitudes
- Como empleado quiero ver mis solicitudes con estado, fechas y días.
- Criterios:
  - Cuando el empleado accede a "Mis solicitudes", entonces el sistema debe mostrar una lista paginada con columnas: ID, fecha inicio, fecha fin, días, estado, fecha de creación y comentario del aprobador si existe.
  - Si una solicitud está en estado RECHAZADA y contiene comentario del aprobador, entonces el comentario debe estar visible en la lista y en la vista de detalle.
  - Mientras exista historial de eventos para una solicitud, entonces el sistema debe permitir ver el rastro completo (creación, cambios de estado, usuario y timestamp).

HU-03: Editar o cancelar una solicitud pendiente o aprobada (con restricciones)
- Como empleado quiero editar o cancelar una solicitud mientras esté pendiente, o cancelar una solicitud aprobada antes de que inicie el periodo.
- Criterios:
  - Mientras una solicitud esté en estado PENDIENTE, el sistema debe permitir al empleado modificar fecha inicio, fecha fin y motivo; cada modificación debe registrarse en auditoría de trazabilidad de solicitudes (cambio de estado, campo, valor anterior, valor nuevo, actor, timestamp).
  - Si la solicitud está en estado PENDIENTE y el empleado confirma la cancelación en el diálogo de confirmación, entonces el sistema debe cambiar el estado a CANCELADA y registrar usuario y timestamp.
  - Si una solicitud está en estado APROBADA y la fecha de inicio no ha llegado (es futura), entonces un usuario aprobador puede cancelarla; el sistema debe restaurar el saldo del empleado solo si el periodo de vacaciones no había iniciado.
  - Si una solicitud está en estado APROBADA y la fecha de inicio ya pasó o es hoy, entonces el sistema debe bloquear la cancelación y mostrar: "No se puede cancelar: el periodo de vacaciones ya ha iniciado".
  - Cuando una solicitud es CANCELADA, entonces debe dejar de aparecer en la bandeja de pendientes de los aprobadores y no debe poder reabrirse desde la interfaz estándar.
  - **Cancelación parcial no aplica en esta versión del MVP** (fuera de alcance).

HU-04: Consultar mi saldo de días disponibles
- Como empleado quiero ver mi saldo total anual, saldo actual y histórico de descuentos por aprobadas.
- Criterios:
  - Cuando el empleado accede a "Mi saldo", entonces el sistema debe mostrar: Saldo inicial anual, Días consumidos (sumatoria de APROBADAS) y Saldo disponible = Saldo inicial - Días consumidos.
  - Si el usuario selecciona otro año (histórico), entonces el sistema debe recalcular y mostrar los valores correspondientes a ese período.
  - El saldo se acumula a razón de 1 día por cada mes completo laborado desde la fecha de ingreso (mes calendario completo desde esa fecha, no mes natural); los días no usados se acumulan de un periodo a otro sin tope máximo definido (**abierto**).

HU-05: Ver solicitudes pendientes (bandeja de aprobador)
- Como aprobador quiero ver solicitudes pendientes de **todos los empleados** con detalles y saldo del empleado; filtrar por empleado y fechas; advertir traslapes.
- Criterios:
  - Cuando el aprobador accede a su bandeja, entonces el sistema debe mostrar solicitudes PENDIENTES de **cualquier empleado** (no hay asignación 1-a-1).
  - Si una solicitud se solapa con otra PENDIENTE o APROBADA del mismo empleado, entonces el sistema debe indicar la advertencia "Existen otras solicitudes que se solapan".
  - Si una solicitud se solapa con una APROBADA, entonces la acción de aprobación debe estar deshabilitada y mostrar: "No se puede aprobar: existe solapamiento con solicitud aprobada".
  - El aprobador no debe ver sus propias solicitudes en la bandeja (no puede auto-aprobarse).

HU-06: Aprobar o rechazar una solicitud con comentario obligatorio al rechazar
- Como aprobador quiero aprobar/rechazar con comentario obligatorio al rechazar.
- Criterios:
  - Si el aprobador aprueba la solicitud, entonces el sistema debe cambiar el estado a APROBADA, registrar aprobador y timestamp, y descontar inmediatamente los días del saldo del empleado.
  - Si el aprobador rechaza la solicitud, entonces el sistema debe cambiar el estado a RECHAZADA, registrar usuario y timestamp, y **exigir** un comentario de rechazo (máx. 500 caracteres) visible para el empleado.
  - Si al momento de la aprobación el saldo actual no es suficiente (por concurrencia), entonces el sistema debe bloquear la aprobación y mostrar: "No se puede aprobar: saldo insuficiente al momento de la aprobación".
  - Un aprobador inactivo no puede aprobar ni rechazar solicitudes.

HU-07: Ver impacto en saldo al revisar solicitud
- Como aprobador quiero ver saldo antes de la solicitud, días solicitados y saldo estimado posterior a la aprobación.
- Criterios:
  - Cuando el aprobador abre el detalle de la solicitud, entonces el sistema debe mostrar: Saldo disponible actual, Días solicitados y Saldo estimado tras la aprobación.
  - Si el Saldo estimado es negativo, entonces el sistema debe resaltar la advertencia: "Esta aprobación excedería el saldo disponible".

HU-08: Consultar historial y saldo de cualquier empleado (RRHH)
- Como RRHH quiero consultar el historial y saldo de días de cualquier empleado.
- Criterios:
  - Cuando RRHH busca un empleado, entonces el sistema debe mostrar todo el historial de solicitudes para el período seleccionado y el saldo correspondiente.
  - RRHH no debe ver ningún botón de aprobación/rechazo en la interfaz (solo lectura).

HU-09: Filtrar solicitudes por estado, empleado o rango de fechas (RRHH)
- Como RRHH quiero filtrar las solicitudes por estado, empleado o rango de fechas para consultar historial.
- Criterios:
  - Cuando RRHH aplica filtros (estado, empleado, rango), entonces el sistema debe devolver los resultados que coincidan.
  - Si no hay coincidencias con los filtros aplicados, entonces el sistema debe mostrar: "No se encontraron solicitudes que coincidan con los filtros aplicados".
  - **Exportación y generación de reportes no están incluidos en esta versión del MVP** (fuera de alcance).

## 5. Reglas de negocio (resumen y criterios de validación)

- RN-01 Saldo anual disponible: Cada empleado **inicia con saldo 0** (cero) al crearse; el saldo se acumula a razón de **1 día por cada mes completo laborado** desde la fecha de ingreso. El valor "días por mes laborado" es configurable (por defecto 1 día/mes). **Se permite crear empleado sin saldo inicial** ya que debe laborar al menos 1 mes completo para ganar su primer día de saldo.
- RN-02 No solicitar más días que los disponibles: El sistema impide crear solicitud que exceda saldo y muestra "Saldo insuficiente para esta solicitud".
- RN-03 Descuento solo en aprobación: El saldo se actualiza únicamente cuando la solicitud pasa a "aprobada". Si una solicitud aprobada se cancela antes de que inicie el periodo, el saldo se restaura.
- RN-04 Restaurar saldo al cancelar solicitud aprobada: Si se cancela una solicitud aprobada **antes de que inicie el periodo de vacaciones**, los días se reintegran al saldo. Solo un aprobador puede cancelar solicitudes aprobadas. No se puede cancelar una solicitud aprobada una vez que haya iniciado el periodo (fecha de inicio <= hoy).
- RN-05 No permitir solicitudes retroactivas: Fecha de inicio debe ser >= mañana (no se puede solicitar para hoy).
- RN-06 Fecha de fin no puede ser anterior a inicio: Validación y mensaje "La fecha de fin no puede ser anterior a la de inicio".
- RN-07 No traslapar con solicitudes aprobadas ni pendientes: Nuevas solicitudes no pueden incluir días ya comprometidos en otras solicitudes del mismo empleado en estado Approved o Pending.
- RN-08 Advertencia de traslape en solicitudes pendientes/confirmadas: Señalar al aprobador si hay traslapes con otras solicitudes del mismo empleado.
- RN-09 Tipo de permiso: Solo existe un tipo — vacaciones. Se elimina catálogo múltiple; no hay permisos médicos, personales, luto ni "otro".
- RN-10 Descripción obligatoria: Campo motivo obligatorio (mín. 10 caracteres) al crear solicitud.
- RN-11 Solicitud nueva en estado "pendiente".
- RN-12 Cambio de estado solo por aprobador (excepto cancelación/edición por empleado en Pending).
- RN-13 Estados finales no se pueden cambiar (Approved, Rejected, Cancelled, Expired son finales salvo cancelación de Approved antes del inicio del periodo por un aprobador). Una solicitud aprobada no puede tener cambios después de haber sido aprobada, a menos que sea un usuario aprobador quien la cancele y solo si el periodo de vacaciones no ha iniciado.
- RN-14 Cualquier aprobador activo puede aprobar/rechazar cualquier solicitud (rol plano, sin asignación 1-a-1).
- RN-15 No se requiere jefe directo asignado para crear solicitud (modelo plano sin jerarquía).
- RN-16 Comentario obligatorio en rechazo: El aprobador debe registrar un motivo al rechazar; visible para el empleado.
- RN-17 Acceso completo a historial para RRHH: lectura ilimitada por antigüedad y estados.
- RN-18 Filtrado por RRHH: filtros por estado/empleado/rango funcionales y precisos.
- RN-19 RRHH sin permiso de aprobación: RRHH no ve botones para decidir, solo consultar.
- RN-20 Edición de solicitudes PENDIENTES: Mientras una solicitud esté en estado PENDIENTE el empleado podrá modificar fecha inicio, fecha fin y motivo; después de APROBADA/RECHAZADA/CANCELADA/EXPIRED la solicitud no podrá editarse. Una solicitud aprobada **no puede tener cambios después de haber sido aprobada**, a menos que sea cancelada por un aprobador (y solo si el periodo no ha iniciado). Todas las ediciones deberán registrarse en auditoría de trazabilidad.
- RN-21 Solicitudes por días completos: La aplicación no soportará solicitudes por horas, medio día ni fracciones; todas las solicitudes serán por días completos.
- RN-22 RRHH sin permiso para crear/editar solicitudes: RRHH tendrá únicamente permisos de consulta. RRHH no podrá crear, modificar ni registrar solicitudes en nombre de terceros bajo ninguna circunstancia.
- RN-23 Acumulación de saldo: 1 día por mes completo laborado desde fecha de ingreso (mes calendario completo desde esa fecha, no mes natural). No hay prorrateo fraccionario adicional.
- RN-24 Carry-over ilimitado: Los días no usados se acumulan de un periodo a otro (no se pierden al cierre de año). Tope máximo de acumulación: **abierto (pendiente de definición)**.
- RN-25 Cálculo de duración en días hábiles: Se excluyen sábados y domingos del cómputo. Feriados: **abierto (pendiente de definición)**.
- RN-26 Auto-rechazo por inacción: Solicitud Pending sin resolver tras **[N] días** (parámetro configurable) cambia su estado a **Expired** (expirada) automáticamente por vencimiento. El sistema registra el cambio con actor="SISTEMA_AUTO_EXPIRACION" y timestamp.
- RN-27 Zona horaria única: Todos los empleados operan en la misma zona horaria corporativa. No se soportan zonas horarias distintas.
- RN-28 Antelación mínima: No se puede solicitar para el día actual; fecha de inicio mínima válida = mañana (1 día de antelación).
- RN-29 Duración máxima: Igual al saldo disponible del empleado (no hay tope fijo independiente del saldo).
- RN-30 Duración mínima: 1 día completo.
- RN-31 Horizonte futuro máximo: **Abierto (pendiente de definición - cuántos meses a futuro se puede solicitar)**.
- RN-32 Aprobador no puede auto-aprobarse: Un aprobador que también sea empleado no puede aprobar sus propias solicitudes; debe resolverlas otro aprobador.
- RN-33 Aprobador inactivo bloqueado: Un usuario/aprobador inactivo no puede aprobar ninguna solicitud.
- RN-34 Sin US-7, FR-017, FR-018, D-006, FR-019, FR-020: Fuera de alcance todo lo referente a gestión de perfiles/jefes por HR, AssignedDirectManagerId, ciclos jerárquicos, auto-gestión, reasignación de jefe, rol Leave Administrator, auto-escalación. **No hay ciclos jerárquicos en el sistema**.
- RN-35 Cancelación parcial no aplica: El sistema no soporta cancelación parcial de solicitudes de vacaciones en esta versión del MVP.
- RN-36 Offboarding no aplica: La gestión de offboarding de empleados no se incluye en esta versión; el estado activo/inactivo del usuario es suficiente para controlar accesos.

Cada regla incluye criterios de éxito y caso de error (ver anexo si se requiere trazabilidad a pruebas).

## 6. Flujos principales (alto nivel)

1. Crear solicitud
- Empleado completa formulario -> sistema valida saldo, fechas (inicio >= mañana) y traslapes con Approved/Pending -> crea solicitud en "pendiente" -> notifica bandeja de aprobadores.

2. Revisión y decisión del aprobador
- Aprobador accede a solicitudes pendientes (de cualquier empleado) -> ve detalles y saldo estimado -> puede aprobar (descontar días y cerrar) o rechazar (no descontar, **comentario obligatorio**).

3. Cancelación
- Empleado cancela solicitud pendiente (confirmación) -> estado "cancelada". Si un aprobador cancela una solicitud aprobada **antes de que inicie el periodo de vacaciones** (fecha inicio > hoy), saldo se restaura. No se puede cancelar una vez iniciado el periodo.

4. Edición de solicitudes pendientes
- Mientras una solicitud esté en estado PENDIENTE, el empleado podrá editar fechas (inicio/fin) y motivo. Los cambios deberán registrarse en auditoría. Una vez la solicitud cambie a APROBADA/RECHAZADA/CANCELADA, no podrá editarse.

5. Auto-expiración por vencimiento
- Solicitud `Pending` sin resolver tras **[N] días** (parámetro configurable) -> sistema cambia el estado a `Expired` automáticamente por vencimiento.

6. Consulta RRHH
- RRHH busca empleado o aplica filtros -> obtiene listado histórico y saldo (exportación y reportes no incluidos en MVP).

## 7. Reglas de presentación

- Listados ordenados por fecha más reciente por defecto.
- Filtros combinables y persistentes por sesión.
- Paginación: **pendiente de resolución técnica** (definir estrategia de paginación del lado del servidor).
- Bandeja de aprobadores: muestra solicitudes de TODOS los empleados (sin filtro de equipo/jefe), con columnas: empleado, ID solicitud, fechas, días, motivo, saldo disponible actual.
- **Reportes no se incluyen en esta versión del MVP** (fuera de alcance).
- **Calendario de equipo se trabajará en prototipo** (fuera de alcance para MVP).

## 8. Auditoría y trazabilidad

- **Auditoría se hará únicamente a nivel de trazabilidad de movimientos de solicitudes**: creación, cambio de estado (Pending -> Approved/Rejected/Cancelled/Expired), cancelación de aprobadas, edición de Pending (campo, valor anterior, valor nuevo, actor, timestamp).
- La auditoría registra: usuario que realizó la acción, marca temporal y comentario (si aplica).
- RRHH podrá consultar historial de trazabilidad de solicitudes.
- Auto-expiración por vencimiento: registrar actor="SISTEMA_AUTO_EXPIRACION" con timestamp y motivo "Expiración tras [N] días sin resolución".
- **Auditoría a nivel de inicio de sesión, cambios de usuario o acciones administrativas NO está incluida en esta versión** (fuera de alcance).

## 9. Seguridad, permisos y gestión de sesiones

- Acceso por rol: Empleado (propias solicitudes y saldo), Aprobador (todas las solicitudes pendientes para decisión y cancelación de aprobadas antes del inicio), RRHH (lectura de cualquier empleado).
- Cualquier aprobador activo puede aprobar/rechazar cualquier solicitud.
- RRHH es rol de solo lectura para efectos de aprobación y no puede crear/editar solicitudes.
- Un aprobador inactivo no tiene permisos de aprobación.
- Un aprobador no puede aprobar sus propias solicitudes.
- **Gestión de sesiones de usuario**: Se manejará con ASP.NET Core Identity Framework.
- **Expiración de sesiones**: La sesión debe expirar después de cierto tiempo de inactividad (configurable); durante un formulario se manejará a nivel de credenciales de la página.
- **Recuperación de contraseña**: No se incluye en esta versión del MVP (versión futura).
- **Invalidación de sesiones activas**: **SÍ soportado por Identity Framework** (ej. actualizando SecurityStamp del usuario, revocando tokens, o SignOutAsync). Se incluirá como funcionalidad básica de administración de usuarios.
- **Offboarding del empleado**: No aplica en esta versión; el estado activo/inactivo del usuario controla el acceso al sistema.

## 10. Validaciones y mensajes de usuario (UX)

- Mensajes claros y específicos: "Saldo insuficiente para esta solicitud", "La fecha de inicio no puede ser anterior a mañana", "La fecha de fin no puede ser anterior a la de inicio", "La solicitud incluye días que ya están comprometidos en otra solicitud".
- Confirmaciones para acciones destructivas (p. ej. cancelar solicitud).
- Mensaje obligatorio de rechazo visible al empleado: "Motivo de rechazo: [comentario del aprobador]".

## 11. Casos borde y excepciones

- Solicitudes solapadas con Approved: bloquear creación.
- Solicitudes solapadas con Pending: bloquear creación (no solo advertir).
- Intento de aprobar que dejaría saldo negativo: impedir aprobación y mostrar advertencia.
- Solicitud Pending que vence sin resolución tras [N] días: auto-expiración; estado cambia a **Expired**.
- Aprobador intenta auto-aprobar: bloqueado, debe ser otro aprobador.
- Aprobador inactivo intenta aprobar: bloqueado.
- Intento de cancelar solicitud aprobada después de que inició el periodo: bloqueado con mensaje "No se puede cancelar: el periodo de vacaciones ya ha iniciado".
- Horizonte futuro máximo (cuántos meses a futuro): **ABIERTO**.
- Manejo de feriados en cálculo de días: **ABIERTO**.
- Tope máximo de acumulación carry-over: **ABIERTO**.
- Valor numérico exacto de [N] días para auto-expiración: **ABIERTO (parámetro configurable pendiente)**.

- Offboarding / Baja: **No aplica en esta versión del MVP**. El estado activo/inactivo del usuario es suficiente para controlar accesos y permisos.

## 12. Supuestos y dependencias

- Cálculo de días: **días calendario excluidos sábados y domingos** (feriados: **abierto**).
- **Integraciones externas no se incluyen en esta versión del MVP**: No hay integración con calendarios corporativos, nómina, SSO, AD, ni sistemas externos.
- Los saldos iniciales por empleado se deben cargar al crear el usuario o por importación masiva previa.
- Las validaciones de fecha se realizarán usando la zona horaria corporativa única y las fechas se tratarán como fechas puras (sin componente hora).
- Todas las solicitudes serán por días completos; no se soportarán medios días ni fracciones en esta versión.
- **No hay jerarquía de jefes directos ni ciclos jerárquicos**; el modelo es plano con rol "aprobador".
- La acumulación de saldo es: 1 día por mes completo laborado desde fecha de ingreso (mes calendario completo desde esa fecha, no mes natural); carry-over ilimitado entre periodos (**tope: abierto**).
- No se definió tolerancia de segundos/hora para validaciones de fecha; se usa fecha pura en zona corporativa.
- No hay auto-escalación, ni rol Leave Administrator, ni gestión de perfiles/jefes por HR (fuera de alcance MVP).
- El sistema realizará las solicitudes y las acciones necesarias de forma directa (sin complejidades adicionales de flujo).

## 13. Fuera de alcance (MVP)

- **Integraciones externas**: nómina, calendario corporativo, SSO, Active Directory, APIs externas.
- Múltiples niveles de aprobación y escalación (auto-escalación, skip-level).
- Cálculo automático de días hábiles o festivos (feriados: **abierto**).
- Notificaciones automáticas por correo/push (posible mejora futura).
- Delegación temporal de aprobaciones (suplencia).
- Solicitudes por horas/medias jornadas/fracciones.
- **Gestión de perfiles y asignación de jefes por RRHH (User Story 7 completa)**.
- **Prevención de auto-gestión y validación de jefes inactivos (FR-019)**.
- **Prevención de ciclos jerárquicos (D-006 / FR-020)** — confirmado: **no existen ciclos jerárquicos en el sistema**.
- **Rol "Leave Administrator" para empleados sin jefe (D-005 / FR-025)**.
- **Reasignación automática de solicitudes pendientes al cambiar de jefe (FR-018 / D-006)**.
- **Auto-escalación por inacción prolongada (D-004 / FR-026)**.
- **Todo lo referente a AssignedDirectManagerId, ciclos jerárquicos, auto-gestión de jefes, reasignación de jefe**.
- Tipos de permiso múltiples (licencia médica, permiso personal, luto, "otro"); solo existe **vacaciones** en MVP.
- **Cancelación parcial de solicitudes** (fuera de alcance).
- **Reportes** (exportación a CSV, dashboards, análisis de ausentismo — fuera del MVP).
- **Calendario de equipo** (se trabajará en prototipo, no en MVP).
- Simulador prospectivo (opciones "innovadora" o "alternativa" del Grupo 04).
- Modo simulación para RRHH, autoguardado, alertas proactivas de saldo.
- **Recuperación de contraseña** (no se manejará en el sistema MVP).
- **Offboarding automatizado de empleados** (estado activo/inactivo es suficiente en MVP).

## 14. Criterios de aceptación generales

- Todas las reglas de negocio (RN-01 a RN-34) deben cumplirse y estar cubiertas por casos de prueba.
- Historias de usuario HU-01 a HU-09 deben tener pruebas de extremo a extremo que verifiquen los criterios de aceptación listados.
- Auditoría: cada cambio de estado, edición de Pending y acción relevante debe quedar registrado con usuario y fecha.

## 15. Glosario

- Saldo inicial: días asignados al inicio del período anual (configurable por empleado).
- Saldo disponible: saldo inicial menos días consumidos por solicitudes aprobadas.
- Solicitud: registro que contiene fechas, motivo, estado y comentarios.
- Estados de solicitud:
  - **PENDING** (Pendiente): Solicitud creada, esperando decisión de aprobador.
  - **APPROVED** (Aprobada): Solicitud aprobada por un aprobador; saldo descontado.
  - **REJECTED** (Rechazada): Solicitud rechazada por un aprobador con comentario obligatorio.
  - **CANCELLED** (Cancelada): Solicitud cancelada por empleado (si estaba Pending) o por aprobador (si estaba Approved antes del inicio).
  - **EXPIRED** (Expirada): Solicitud pendiente que no fue resuelta tras [N] días; sistema la expira automáticamente.
- Día hábil (para cálculo de duración): día calendario que no es sábado ni domingo (**feriados: abierto**).
- Mes completo laborado: mes calendario completo desde la fecha de ingreso del empleado (no mes natural).
- Carry-over: arrastre de días no usados de un periodo al siguiente sin tope máximo definido (**abierto**).
- Aprobador: rol plano que puede aprobar/rechazar solicitudes de cualquier empleado (sin asignación 1-a-1); puede cancelar solicitudes aprobadas si el periodo no ha iniciado.
- Zona horaria corporativa: única zona horaria para todos los empleados.
- [N]: parámetro configurable de días para auto-expiración de solicitudes Pending sin resolver (**pendiente de valor numérico**).
- Paginación: **Pendiente de resolución técnica** (estrategia pendiente de definir).
- Identity Framework: ASP.NET Core Identity Framework para gestión de autenticación y sesiones de usuario.

---

Anexo: Las historias y reglas aquí listadas contienen los casos de éxito y error que actúan como criterios de aceptación para pruebas funcionales.
## Requisitos Funcionales

El siguiente conjunto extrae y transforma los requisitos funcionales de la especificación al formato. Cada requisito incluye: identificador, nombre corto, requisito en sintaxis EARS, historia(s) de origen y regla(s) de negocio relacionadas cuando aplique.

RF-001 — Saldo inicial en cero y acumulación por mes laborado
- Requisito (Ubiquitous): El sistema deberá crear cada empleado con saldo inicial de **cero (0) días**; el saldo se acumula a razón de **1 día por cada mes completo laborado** desde la fecha de ingreso (valor "días por mes laborado" configurable, por defecto 1). Se permite crear empleado sin saldo inicial ya que debe laborar al menos 1 mes completo para ganar su primer día de saldo.
- Origen: RN-01, RN-23
- Reglas relacionadas: RN-01, RN-23

RF-002 — Calcular días solicitados (días hábiles: excluir sábados y domingos)
- Requisito (Ubiquitous): El sistema deberá calcular el número de días solicitados como el conteo inclusivo de días calendario entre la fecha de inicio y la fecha de fin, EXCLUYENDO sábados y domingos del cómputo. (Feriados: abierto/pendiente de definición).
- Origen: HU-01
- Reglas relacionadas: RN-05, RN-25

RF-003 — Impedir fecha de inicio retroactiva (mínimo: mañana)
- Requisito (Unwanted behavior): Si la fecha de inicio solicitada es anterior al día siguiente a la fecha actual del sistema, el sistema deberá bloquear la creación de la solicitud y mostrar el mensaje "La fecha de inicio no puede ser anterior a mañana".
- Origen: HU-01
- Reglas relacionadas: RN-05, RN-28

RF-004 — Impedir fecha de fin anterior a inicio
- Requisito (Unwanted behavior): Si la fecha de fin solicitada es anterior a la fecha de inicio solicitada, el sistema deberá bloquear la creación de la solicitud y mostrar el mensaje "La fecha de fin no puede ser anterior a la de inicio".
- Origen: HU-01
- Reglas relacionadas: RN-06

RF-005 — Validar saldo disponible antes de crear
- Requisito (Event-driven): Cuando un empleado intente crear una solicitud, el sistema deberá verificar el saldo disponible y, si los días solicitados > saldo disponible, deberá bloquear la creación y mostrar: "Saldo insuficiente para esta solicitud".
- Origen: HU-01, HU-04
- Reglas relacionadas: RN-02, RN-03

RF-006 — Evitar traslape con solicitudes Approved o Pending (al crear)
- Requisito (Unwanted behavior): Si el rango de fechas solicitado traslapa días incluidos en solicitudes APROBADAS o PENDIENTES existentes del mismo empleado, el sistema deberá impedir la creación y mostrar: "La solicitud incluye días que ya están comprometidos en otra solicitud".
- Origen: HU-01
- Reglas relacionadas: RN-07

RF-007 — Crear solicitud en estado PENDIENTE
- Requisito (Ubiquitous): Cuando todas las validaciones sean correctas, el sistema deberá crear la solicitud con estado = PENDIENTE y registrar la fecha/hora de creación y el autor.
- Origen: HU-01
- Reglas relacionadas: RN-11

RF-008 — Notificar a aprobadores al crear solicitud pendiente
- Requisito (Event-driven): Cuando una solicitud se cree en estado PENDIENTE, el sistema deberá añadirla a la bandeja de pendientes de los aprobadores (rol plano, sin asignación 1-a-1).
- Origen: HU-01
- Reglas relacionadas: RN-11

RF-009 — Mostrar resumen antes de confirmar
- Requisito (Event-driven): Cuando el empleado complete el formulario (fechas y motivo), el sistema deberá mostrar un resumen con los días calculados (excluyendo sábados y domingos) y el efecto estimado sobre el saldo antes de confirmar.
- Origen: HU-01, HU-04

RF-010 — Listar solicitudes del empleado con columnas requeridas
- Requisito (Ubiquitous): El sistema deberá presentar al empleado una lista paginada de sus solicitudes con: ID, fecha inicio, fecha fin, días solicitados, estado, fecha de creación y comentario del aprobador (si existe).
- Origen: HU-02

RF-011 — Orden y rastro de auditoría en detalle
- Requisito (Ubiquitous): El sistema deberá ordenar por fecha de última modificación por defecto, permitir ordenar por fecha de inicio o estado y deberá mostrar un rastro de auditoría por solicitud (creación, cambios de estado, actor, timestamp).
- Origen: HU-02, RN-17
- Reglas relacionadas: RN-17

RF-012 — Mostrar acción Cancelar solo para PENDIENTE
- Requisito (State-driven): Mientras una solicitud esté en estado PENDIENTE, el sistema deberá presentar al empleado la acción "Cancelar"; en otros estados la acción no deberá mostrarse.
- Origen: HU-03
- Reglas relacionadas: RN-12

RF-013 — Confirmación al cancelar
- Requisito (Event-driven): Cuando un empleado inicie la acción "Cancelar" en una solicitud PENDIENTE, el sistema deberá mostrar un diálogo de confirmación y, si el empleado confirma, cambiar el estado a CANCELADA y registrar actor y timestamp.
- Origen: HU-03

RF-014 — Canceladas no reabribles desde UI estándar
- Requisito (Ubiquitous): Cuando una solicitud esté en estado CANCELADA, el sistema deberá impedir su reapertura o cambio de estado desde la interfaz estándar y deberá registrar la cancelación en el historial.
- Origen: HU-03
- Reglas relacionadas: RN-13

RF-015 — Restaurar saldo al cancelar solicitud aprobada (con restricción temporal)
- Requisito (Event-driven): Cuando un aprobador cancele una solicitud que está en estado APROBADA **y la fecha de inicio es futura** (no ha iniciado el periodo de vacaciones), el sistema deberá restaurar los días previamente descontados al saldo disponible del empleado y registrar la reversión en el historial. Si el periodo ya inició (fecha inicio <= hoy), la cancelación debe estar bloqueada.
- Origen: RN-04, HU-03
- Reglas relacionadas: RN-04, RN-35
- Nota: Solo un aprobador puede cancelar solicitudes aprobadas; empleados no tienen este permiso.

RF-016 — Mostrar saldo actual e historial al empleado
- Requisito (Ubiquitous): El sistema deberá mostrar al empleado su saldo inicial anual, los días consumidos (sumatoria de APROBADAS) y el saldo disponible, así como el historial de solicitudes que afectaron el saldo.
- Origen: HU-04
- Reglas relacionadas: RN-01, RN-03

RF-017 — Bandeja de aprobadores: solicitudes pendientes de todos los empleados
- Requisito (Ubiquitous): El sistema deberá mostrar a cualquier aprobador activo las solicitudes PENDIENTES de TODOS los empleados, sin filtro de jerarquía ni asignación 1-a-1.
- Origen: HU-05
- Reglas relacionadas: RN-14

RF-018 — Fila de la bandeja: empleado, fechas, días, motivo y saldo
- Requisito (Ubiquitous): El sistema deberá mostrar en la lista del aprobador: nombre del empleado, ID solicitud, fechas, días solicitados, resumen del motivo y saldo disponible actual del empleado.
- Origen: HU-05

RF-019 — Filtros en la bandeja del aprobador
- Requisito (Ubiquitous): El sistema deberá permitir al aprobador filtrar por empleado, rango de fechas de inicio/fin; los filtros podrán combinarse.
- Origen: HU-05

RF-020 — Advertir sobre traslapes con pendientes (salvaguarda ante condiciones de carrera)
- Requisito (Ubiquitous): Si una solicitud mostrada se solapa con otras PENDIENTES del mismo empleado, el sistema deberá mostrar una advertencia visual: "Existen otras solicitudes pendientes que se solapan".
- Origen: RN-08, HU-05

RF-021 — Bloquear aprobación si traslapa con aprobadas (salvaguarda ante condiciones de carrera)
- Requisito (Unwanted behavior): Si una solicitud traslapa con una solicitud APROBADA del mismo empleado, el sistema deberá deshabilitar la acción Aprobar y mostrar: "No se puede aprobar: existe solapamiento con solicitud aprobada".
- Origen: RN-07, HU-05

RF-022 — Aprobar: cambio de estado y descuento de saldo
- Requisito (Event-driven): Cuando CUALQUIER aprobador activo apruebe una solicitud PENDIENTE, el sistema deberá cambiar el estado a APROBADA, registrar aprobador y timestamp, y descontar inmediatamente los días del saldo del empleado.
- Origen: HU-06
- Reglas relacionadas: RN-03, RN-14

RF-023 — Rechazar: cambio de estado y comentario OBLIGATORIO
- Requisito (Event-driven): Cuando un aprobador rechace una solicitud PENDIENTE, el sistema deberá cambiar el estado a RECHAZADA, registrar usuario y timestamp, y EXIGIR un comentario de rechazo (máx. 500 caracteres) visible para el empleado.
- Origen: HU-06
- Reglas relacionadas: RN-16

RF-024 — Impedir auto-aprobación y decisiones de aprobadores inactivos
- Requisito (Unwanted behavior): El sistema deberá bloquear la acción de aprobar/rechazar si (a) el aprobador actuante es el mismo empleado que creó la solicitud (auto-aprobación), O (b) el aprobador está inactivo.
- Origen: RN-32, RN-33, HU-06

RF-025 — Evitar aprobación con saldo insuficiente por concurrencia
- Requisito (Unwanted behavior): Si al momento de la aprobación el saldo disponible del empleado es menor que los días solicitados (por cambios concurrentes), el sistema deberá bloquear la aprobación y mostrar: "No se puede aprobar: saldo insuficiente al momento de la aprobación".
- Origen: HU-06, HU-07
- Reglas relacionadas: RN-02, RN-03

RF-026 — Mostrar impacto en saldo en detalle de solicitud
- Requisito (Event-driven): Cuando el aprobador abra el detalle de la solicitud, el sistema deberá mostrar: saldo disponible actual, días solicitados y saldo estimado tras la aprobación (actual - solicitados).
- Origen: HU-07

RF-027 — Resaltar saldo estimado negativo
- Requisito (State-driven): Si el saldo estimado tras la aprobación fuera negativo, el sistema deberá resaltar la advertencia: "Esta aprobación excedería el saldo disponible".
- Origen: HU-07

RF-028 — RRHH: acceso completo de solo lectura al historial
- Requisito (Ubiquitous): El sistema deberá permitir a usuarios de RRHH buscar cualquier empleado y ver su historial completo de solicitudes (todos los estados y timestamps) y su saldo actual, sin permitir aprobar o rechazar.
- Origen: HU-08
- Reglas relacionadas: RN-17, RN-19

RF-029 — Filtrado para RRHH (sin exportación ni reportes en MVP)
- Requisito (Ubiquitous): Cuando RRHH acceda a consultas, el sistema deberá permitir filtrar por estado(s), empleado (autocompletar), rango de fechas. **Exportación y reportes no están incluidos en esta versión del MVP**.
- Origen: HU-09
- Reglas relacionadas: RN-18

RF-030 — Tiempo de respuesta objetivo para filtros
- Requisito (Ubiquitous): El sistema deberá actualizar los resultados filtrados de forma interactiva; para volúmenes razonables, la respuesta ante cambios de filtro deberá ser <= 2 segundos.
- Origen: HU-09

RF-031 — Visibilidad de acciones basada en roles
- Requisito (Ubiquitous): El sistema deberá mostrar las acciones Aprobar/Rechazar solo a usuarios con rol de aprobador activo (excepto si el aprobador es el autor de la solicitud) y no mostrarlas al rol RRHH.
- Origen: RN-12, RN-19

RF-032 — Registro de auditoría para trazabilidad de movimientos de solicitudes
- Requisito (Ubiquitous): El sistema deberá registrar en el historial de trazabilidad de solicitudes cada creación, transición de estado (aprobar/rechazar/cancelar/expirar), edición de solicitud PENDIENTE, el usuario que realizó la acción, timestamp y cualquier comentario del aprobador. **La auditoría se limita a movimientos de solicitudes** (no incluye logs de sesión ni acciones administrativas).
- Origen: HU-02, HU-06, RN-17

RF-033 — Persistir timestamps y actor para reportes
- Requisito (Ubiquitous): El sistema deberá persistir timestamps de creación y decisión y los identificadores de los actores para que los reportes muestren fechas de creación y decisión por solicitud.
- Origen: RN-17

RF-034 — Rechazar creación de empleado sin saldo por importación masiva
- Requisito (Unwanted behavior): Si una importación masiva o creación de empleado omite el campo de saldo inicial, el sistema deberá rechazar la importación/creación y reportar el campo obligatorio faltante.
- Origen: RN-01

RF-035 — Mensajes claros y localizados para validaciones
- Requisito (Ubiquitous): El sistema deberá mostrar mensajes de error claros y localizados para fallas de validación (ejemplos: "Saldo insuficiente para esta solicitud", "La fecha de inicio no puede ser anterior a mañana", "La fecha de fin no puede ser anterior a la de inicio", "La solicitud incluye días que ya están comprometidos en otra solicitud").
- Origen: Validaciones agrupadas (HU-01, RN-05, RN-06, RN-02, RN-07)

RF-036 — Edición de solicitudes PENDIENTES
- Requisito (Event-driven): Mientras una solicitud esté en estado PENDIENTE, el sistema deberá permitir al empleado modificar fecha inicio, fecha fin y motivo; cada modificación deberá registrarse en auditoría (campo modificado, valor anterior, valor nuevo, actor, timestamp).
- Origen: RN-20

RF-037 — Imposibilidad de editar solicitudes en estados finales
- Requisito (State-driven): Mientras la solicitud esté en estado APROBADA, RECHAZADA, CANCELADA o EXPIRED, el sistema deberá impedir la edición desde la interfaz estándar. Una solicitud aprobada solo puede ser cancelada por un aprobador y únicamente si el periodo de vacaciones no ha iniciado.
- Origen: RN-20, RN-13

RF-038 — Rechazo de solicitudes por fracciones/hours
- Requisito (Unwanted behavior): Si un usuario intenta crear una solicitud con medios días, horas o fracciones, el sistema deberá impedir la creación y mostrar: "No se permiten solicitudes por horas o fracciones en esta versión".
- Origen: RN-21

RF-039 — RRHH no puede crear ni modificar solicitudes
- Requisito (Unwanted behavior): Si un usuario con rol RRHH intenta crear, modificar o registrar una solicitud para un tercero, el sistema deberá impedir la acción y mostrar: "RRHH tiene acceso de solo lectura para solicitudes".
- Origen: RN-22

RF-040 — Cálculo de días solicitados excluyendo sábados y domingos
- Requisito (Ubiquitous): El sistema deberá calcular el número de días solicitados como el conteo inclusivo de días calendario entre la fecha de inicio y la fecha de fin, EXCLUYENDO sábados y domingos del cómputo. (Feriados: abierto/pendiente de definición).
- Origen: HU-01
- Reglas relacionadas: RN-25

RF-041 — Acumulación de saldo: 1 día por mes completo laborado con carry-over ilimitado
- Requisito (Ubiquitous): El sistema deberá acumular saldo a razón de 1 día por cada mes completo laborado, contado desde la fecha de ingreso del empleado (mes calendario completo desde esa fecha, no mes natural); los días no usados se acumulan de un periodo a otro sin tope máximo definido (tope: abierto/pendiente de definición).
- Origen: RN-23, RN-24

RF-042 — Validaciones de fecha usan zona horaria corporativa única
- Requisito (Ubiquitous): Todas las validaciones de fecha (inicio >= mañana, fin >= inicio, auto-rechazo a los [N] días) se realizarán usando la zona horaria corporativa única; todas las fechas se tratan como fechas puras sin componente hora; no se soportan empleados en zonas horarias distintas.
- Origen: RN-27

RF-043 — Auto-expiración de solicitudes Pending tras [N] días sin resolución
- Requisito (Event-driven): Cuando una solicitud permanezca en estado PENDIENTE sin ser aprobada ni rechazada durante [N] días (parámetro configurable, valor numérico exacto: abierto), el sistema deberá cambiar su estado a **EXPIRED** (expirada) automáticamente por vencimiento, registrar el cambio de estado con actor "SISTEMA_AUTO_EXPIRACION" y timestamp, y notificar al empleado.
- Origen: RN-26

RF-044 — Bloqueo explícito de auto-aprobación (aprobador == autor)
- Requisito (Unwanted behavior): Si el aprobador que intenta aprobar o rechazar una solicitud es el mismo empleado que la creó, el sistema deberá bloquear la acción y mostrar: "No puedes aprobar ni rechazar tu propia solicitud; otro aprobador debe resolverla".
- Origen: RN-32

RF-045 — Bloqueo explícito de aprobador inactivo
- Requisito (Unwanted behavior): Si un usuario con rol de aprobador está marcado como inactivo, el sistema no le permitirá aprobar ni rechazar ninguna solicitud.
- Origen: RN-33

RF-046 — Aprobador no ve sus propias solicitudes en la bandeja de pendientes
- Requisito (State-driven): Mientras un aprobador acceda a su bandeja de pendientes, el sistema NO debe mostrar las solicitudes que él mismo haya creado como empleado; solo mostrará solicitudes de otros empleados.
- Origen: HU-05 criterio

RF-047 — Bloquear cancelación de solicitudes aprobadas una vez iniciado el periodo
- Requisito (Unwanted behavior): Si una solicitud está en estado APROBADA y la fecha de inicio ya pasó o es hoy (periodo ya iniciado), el sistema deberá bloquear cualquier intento de cancelación y mostrar: "No se puede cancelar: el periodo de vacaciones ya ha iniciado".
- Origen: RN-04, HU-03
- Reglas relacionadas: RN-04, RN-35

---