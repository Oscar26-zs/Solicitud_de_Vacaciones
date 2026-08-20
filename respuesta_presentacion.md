# Presentación: Cómo desarrollamos "Solicitud de Vacaciones"

> Relato concreto y honesto del proceso real: no la teoría, sino cómo generamos, revisamos, corregimos y validamos este sistema durante la pasantía en Novacomp.

---

## 1. Cómo arrancó el proceso

El punto de partida no fue una idea, sino **una caja cerrada de especificación**. Antes de tocar una sola línea del sistema, ya existía el material completo al que debía ajustarme:

- El **spec** (`spec/spec.md`) con las 9 historias de usuario (HU-01 a HU-09), las ~36 reglas de negocio (RN-01 a RN-36) y los 47 requisitos funcionales (RF-001 a RF-047). Ahí estaba definido **qué** debía hacer el sistema.
- La **constitución** (`.specify/memory/constitution.md`): las reglas no funcionales y arquitectónicas (Clean Architecture en 4 capas, prohibido `DateTime.Now` en Domain/Application, prohibido `DELETE` físico, `TimeProvider` para el tiempo, cobertura de tests ≥ 80%, etc.). Es decir, el **cómo** técnico.
- La **guía de diseño** (`spec/DESIGN_TOKENS.md`): el "prototipo" visual. No era la parte de negocio, era mis **tokens**: paleta monocromática gris/casi-negro, radios, tipografía Geist, badges de estado (ámbar=pendiente, esmeralda=aprobada, rojo=rechazada, gris=cancelada/expirada), el modal destructivo reutilizable, etc.
- Los **casos de uso** (`docs/use-cases.md`, CU-01 a CU-19), el **plan** (`spec/plan.md`, con las decisiones del PO resueltas, las 4 capas, el contrato de API y la estructura objetivo) y el **plan de tareas** (`spec/tasks.md`, 68 tareas repartidas en 7 fases).

La instrucción concreta que di para la primera generación fue más o menos: **"implementá el sistema en orden según el plan.md: primero Fase 1 al 7, respetando las reglas de la constitution, las decisiones del PO y las tareas del documento de tareas, y dejá la UI fiel al DESIGN_TOKENS"**. O sea: no un "creá un sistema de vacaciones" suelto, sino "tomá estos 6 documentos como fuente única de verdad y convertí el plan en código".

---

## 2. Primera generación — qué resultó

La primera generación construyó **la base y el núcleo funcional con buena calidad**, no fue un esqueleto vacío:

- **La arquitectura salió bien de entrada.** Se generaron los 4 proyectos (`Vacations.Domain`, `Application`, `Infrastructure`, `Web`) con la dirección de dependencias correcta hacia adentro: Domain sin referencias a frameworks, Application que depende solo de Domain, Infrastructure que implementa repositorios, Web que presenta. Compilaba.
- **El dominio quedó muy cerca de la spec.** Entidades `Empleado`, `SolicitudVacaciones`, `SaldoEmpleado`, `HistorialSolicitud`; el Value Object `RangoFechas`; el enum de 5 estados; excepciones tipadas (`SaldoInsuficienteException`, `AutoAprobacionNoPermitidaException`, `TraslapeSolicitudesException`, `TransicionEstadoInvalidaException`). La lógica de saldo (acumulado − consumido − pendiente) y la de días hábiles excluyendo sábados/domingos aparecieron funcionales desde la primera vez.
- **Todo el flujo de casos de uso quedó mapeado a endpoints** (crear/editar/cancelar solicitud, bandeja, aprobar/rechazar, cancelar aprobada, auto-expiración vía background service, consultas RRHH).

**Qué no salió bien de entrada:**
- **La capa de persistencia y las migraciones**: el `DbContext`, las configuraciones de EF Core y la migración inicial necesitaron ajustes para que el esquema quedara consistente con las entidades (p. ej. cómo se mapea el enum a string y dónde se configura el `RowVersion`).
- **La capa de tests de integración quedó vacía desde el arranque** (esto lo detallo abajo).
- **En UI se generó una estructura completa** de vistas y parciales (`_Layout`, `_StatusBadge`, `site.css`) cumpliendo los tokens, **pero hubo desviaciones puntuales del prototipo en escenarios de interacción** (ver siguiente sección).

En definitiva: lo de negocio y arquitectura quedó fuerte y alineado desde el primer intento; **la fricción real se concentró en la UI fina y en la cobertura de pruebas**, que es donde más vueltas dimos.

---

## 3. Errores de diseño y cómo se corrigieron

El diseño del DESIGN_TOKENS era de **máxima claridad: "el modal establo muestra accionable en el prototipo concreto".** Y justo ahí se generó el error más ilustrativo del proyecto.

**Error 1 — el `confirm()` nativo en vez del modal propio del prototipo.**

- El prototipo (DESIGN_TOKENS) define el **modal destructivo reutilizable** `_ConfirmDialog.cshtml`: overlay oscuro con `backdrop-filter: blur(4px)`, título ("¿Cancelar esta solicitud?"), y dos botones centrados "Volver" (outline) + el de acción en rojo destructivo. Y deja una regla explícita: **"Nunca usar `confirm()` del navegador"**.
- El sistema generado, en cambio, puso `data-confirm="¿Aprobar esta solicitud?"` directamente en el botón `Aprobar` (`BandejaAprobador/Detalle.cshtml`) y `data-confirm="¿Está seguro de cancelar esta solicitud?"` en los botones de cancelar (`SolicitudVacaciones/Detalle.cshtml`). Eso dispara el `confirm()` feo del navegador, fuera de la paleta, sin blur, sin el footer del prototipo.
- **Cómo lo corregimos (varias vueltas):**
  1. **Detecté** en la revisión que el patrón no coincidía con el doc. El incluso el propio DESIGN_TOKENS lo marcaba como "MEJORA PENDIENTE" citando las líneas exactas donde estaba mal.
  2. **Indiqué de forma precisa**, apuntando al componente del prototip existente (la sección "Modal de confirmación destructiva") y nombrando el resultado esperado: *"reemplazá el `confirm()` nativo por el partial `_ConfirmDialog.cshtml` ya definido en DESIGN_TOKENS, con Overlay de blur, y el botón destructivo"*.
  3. El sistema **creó** el partial `_ConfirmDialog.cshtml` y lo cargó en `_Layout` (`@await Html.PartialAsync("_ConfirmDialog")`), e interceptó el submit con JS para abrir el modal en vez del `confirm()`.
  4. **Pero la migración quedó incompleta**: los botones de la bandeja (`Detalle.cshtml`) siguen con el `data-confirm` y en `Index.cshtml` quedó un `TODO: Reemplazar confirm() nativo … según DESIGN_TOKENS.md`. O sea: el componente nuevo existe, el patrón de uso no se migró al 100%. **Ajusté esta corrección llevó ~2 iteraciones y quedó a medias** — es el ejemplo más honesto de que no todo cierra solo.

**Error 2 — el motivo de la solicitud y el modal de cancelación del aprobador.**

Hubo retrabajo directo por cambios del PO en el flujo, que en git se ven nítidamente:
- Se movió el **motivo del rechazo/cancelación `dentro del modal de cancelación` del aprobador** (antes vivía en otro lugar del detalle).
- El **motivo** de la solicitud se cambió de obligatorio a **opcional**.
- Los commits "Cambio modal de cancelar solicitud" y "Quitar el motivo como obligatorio y ponerlo en el modal del aprobador" son exactamente esa corrección. Tomamos !aproximadamente una iteración por pantalla afectada (la del empleado y la del aprobador).

**Error 3 — codificación de acentos en textos UI.**
En los botones de detalle quedaron con signos inválidos ("¿Est� seguro de cancelar?"). Es un detalle menor de encoding, pero una vista llegó con el texto roto y hubo que normalizarlo.

**Lo que aprendimos de este círculo:** las correcciones de diseño **funcionaron mejor cuando apunté a la sección/línea exacta del doc de referencia y nombré el componente que debía reutilizarse** (por nombre: `_ConfirmDialog`), en vez de decir "mejora el diálogo". Cuanto más específico el caso, menos vueltas.

---

## 4. Errores y trabajo en la parte de tests

Esta fue la zona donde más nos costó llegar a un resultado razonable.

- **Lo que la specpedía (constitution §9):** pirámide de pruebas con xUnit y **cobertura ≥ 80%** en Domain y Application, con 4 proyectos de test (Domain, Application, Infrastructure y Web, los últimos con WebApplicationFactory).
- **Qué se generó primero:** los tests de **Domain** (5 archivos: `SolicitudVacacionesTests`, `SaldoEmpleadoTests`, `RangoFechasTests`, `EmpleadoTests`, `HistorialSolicitudTests`) y los de **Application** (varios handlers y validators). Eso salió bien.
- **Qué falló / quedó vacío:**
  - **Los proyectos de tests de Infrastructure.Tests y Web.Tests quedaron como "cáscaras":** el `.csproj` existe y se crean, pero **no contenía ni un solo archivo de teste de prueba de integración** (0 archivos `.cs` de prueba en cada uno, solo el scaffolding de obj). Es decir, la prueba de integración real de la infraestructura (contra BD) y la E2E con WebApplicationFactory **nunca se escribieron**, pese a estar en las tareas (TASK-068) y en la constitution.
  - **La cobertura no llegó sola:** la medí en las 4 corridas de test guardadas en `TestResults/` y la evoluciónCount es muy ilustrativa:
    - corrida → **46,6%** (223/478 líneas)
    - 2ª → **72,8%**
    - 3ª y 4ª → **80,1%** (383/478)
  - O sea que para llegar a la meta del 80% en Application **tuvimos que iterar 3 veces agregando casos de prueba**, no salió en el primer intent.: de menos de la mitad, hasta el 80%.
- **Qué se hizo para la versión final de los tests:** se priorizaron las capas de negocio (Domain y Application), que fueron las que cubrieron los casos de uso críticos de la spec (crear, saldo insuficiente, traslape, aprobar/rechazar, auto-aprobación bloqueada, cancelación de aprobada antes del inicio). Y la validación de algunos criterios E2E de las historias (HU-01..HU-09) se terminó de verificar **manualmente en la UI** en vez de con tests de integración automatizados, porque esa capa quedó pendiente.

---

## 5. Cómo se trabajaron las tareas del plan

El plan de tareas venía bien organizado: **68 tareas en 7 fases**, con dependencias explícitas, prioridad (Alta/Media/Baja), etiqueta de historia de usuario y un orden "MVP primero".

- **El orden, en general, se siguió:** Fase 1 (setup) → Fase 2 (Foundational, en el curso bloqueante: enums, entidades, versiones, repos, DbContext, políticas) → Fase 3 (HU-01/02/04, el MVP de empleado) → Fase 4 (editar/cancelar) → Fase 5 (bandeja del aprobador) → Fase 6 (RRHH) → Fase 7 (polish/auth/seed/integración). La fase 2 se **MÁS rebloqueo las historias, y se respetó ese parar y validar**.
- **El plan en sí no estuvo estable todo el tiempo.** En el historial de la rama se ven que el plan se corrigió sobre la marcha (por ej. un commit de "Plan creado referente al nuevo constitution" y otro "correcciones en el plan"). Además, el **plan-checklist** le marcaba al propio plan una agenda de días que quedó si coherent point: rate limiting aparece como **5 y luego como 10/min** en distintos lugares — una inconsistencia interna que el propio checklist señaló (#6.2).
- **La tarea que más fricción generó fue la de los tests de integración (TASK-068)** y, en general, la Fase 7 (integraciones y testing E2E). Fragment de gran producto: porque dependía de casi todo el sistema (Fase 5-6 terminadas) y porque demandaba infraestructura extra (WebApplicationFactory, BD). Es ahí donde quedaron tareas botadas no completadas.
- **También hubo retrabajo por decisiones del PO que llegaron después:** el cambio del motivo opcional, el modal de cancelación del aprobador y los ajustes de diseño ("Cambios diseño MEJORAS"). Se re-ejecutaron pantallas en vez de seguir el plan de plano, literalmente.

En resumen: las **tareas de dominio y de los flujos de negocio** se ejecutaron en orden y sin dolor; las de **UI interactiva** y las de **traje de integración** fueron las que hubo que reorganizar y repetir.

---

## 6. Patrón general de iteración

El ciclo real fue: **generar → revisar → corregir → validar**, repetido hasta que la pantalla/regla se veía y se comportaba como decía la documentación fuente.

Tuve la figura del **corrector humano (vos)** volviendo sobre lo generado, y el sistema integrando las correcciones a veces de una, a veces después de dos o tres vueltas.

**Ejemplo concreto, de ida y vuelta completo (el modal de cancelar solicitud del aprobador):**

1. **Generar:** el sistema puso el motivo del rechazo en una vista y el botón cancelar con un `data-confirm`.
2. **Revisar (el humano):** "En el flujo del aprobador, cuando decido cancelar una solicitud ya aprobada, el motivo del rechazo debería pedirse acá, dentro del modal de cancelación, no en otro lado. Y el motivo de la solicitud primera, es opcional, no obligatorio."
3. **Corregir:** el sistema reubicó el campo de motivo del rechazo al modal de cancelación del aprobador, y ajustó la validación del motio a opcional por del lado del cliente.
4. **Validar:** se probó la pantalla del aprobador → al cancelar, aparece el modal con el campo de motivo, y al cré una solicitud el motivo deja de ser obligatorio.

La instrucción que **más rápido** resolvió fue la que especificaba: **el caso concreto (pantalla y acción), el componente a reutilizar (el modal definido), y el resultado observable esperado (el motivo está fuera; el motivo es opcional)**. Las instrucciones vagas del tipo "arreglá el orden" jugaron a favor de más idas y vueltas.

Otra lección repetida: **no siempre una corrección se termina en una sola vuelta**. El caso del modal `_ConfirmDialog` quedó a medias en la generación inicial (created partial, cargado en layout, pero los botones seguían con `data-confirm` y un `TODO` pendiente), y hay justo documentado eso como línea evolutiva del trabajo de revisión.

---

## 7. Balance final

Querendo ser honesto, el sistema quedó **muy cerca de la spec en el plano funcional y de dominio, y más lejos del prototipo exacto en la UI y en una de las capas de tests.**

- **Qué quedó bien/al alineado:**
    - Todo el flujo de negocio definido en la spec: crear (con saldo/traslape/validación de fechas), aprobar/rechazar con comentario, cancelar, editar pendiente; auto-expiración; roles (Empleado/Aprobador/RRHH); auditoría de do/esnasciejundú. Las palabras rt state machine de 5 estados y las restricciones (anti-auto-aprobación, aprobador inactivo, cancelación de aprobada solo antes de la fecha) están implementadas.
  - La arquitectura y el dominio, una alta fidelidad documentada: casi todas las decisiones del PO quedaron reflejadas (carry-over sin tope, `pendingBalance`, días excluyendo fines de semana, Horizonte de 2 meses, etc.).
  - La UI respetó en gran medida los tokens (paleta monocromática, badges de estado, tarjetas Stat-Card, paginación), con `tokens.css`/`components.css`/`utilities.css` y JS reales.
- **Qué tuvo que ajustarse manualmente al final:**
  - Migrar el `confirm()` nativo al modal destructivo reutilizable (quedó pendiente con `TODO`).
  - La codificación de acentos en algunos botones.
  - Los cambios de PO sobre el motivo (opcional) y el modal de cancelación del aprobador.
  - Completar la capa de tests de integración, que quedó vacía (Infrastructure.Tests y Web.Tests como cáscaras).
- **Nivel de intervención humana total:** fue **moderado y concentrado en dos frentes** *(UI interactiva y tests de integración)*. El dominio, la aplicación y el flujo de negocio salieron con intervención mínima (arranca bien, con ajustes menores). No fue "aceptar el primer resultado y listo", tampoco "había que corregirlo todo": el sistema hizo la parte pesada del descubrimiento y la estructura, y el humano y los PO pusieron los detalles finos y las historias de integración donde la máquina sola no llegaba.

**Números de cierre:** ~68 tareas planificadas; **Domain (5 arch. de tests) y Application (cobertura llegó a 80,1%) bien cubiertas**; **Infrastructure y Web sin tests reales**; cobertura de Application que trepó de 46,7% → 80,1 % con 3 iteraciones de corrección; y **varias iteraciones de diseño/UI** en el modal de cancelación y en el plan (maquetada cada una), que son parte natural del ciclo porque los PO y el humano iban aterrizando requisitos a medida que veían pantallas reales.