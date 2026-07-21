# Constitución: Sistema de Gestión de Solicitudes de Permisos/Vacaciones

## 1. Nombre y Propósito del Proyecto
**Nombre del Proyecto:** Sistema de Gestión de Solicitudes de Permisos/Vacaciones
**Propósito:** Un sistema centralizado para gestionar las solicitudes de permisos y vacaciones de los empleados dentro de una organización. Proporciona una forma confiable para que los empleados soliciten tiempo libre, los gerentes revisen esas solicitudes y RR.HH. controle los saldos y el historial.

## 2. Principios de Arquitectura

- **Clean Architecture (Monolito Modular):** El sistema debe seguir Clean Architecture, organizado como un monolito modular. Las dependencias siempre deben apuntar hacia adentro: las capas externas (Infraestructura, Presentación) dependen de las capas internas (Aplicación, Dominio), nunca al revés.

- **Separación de Capas:** Deben mantenerse cuatro capas claramente definidas:
  - **Dominio:** Entidades de negocio y reglas centrales, sin dependencias externas.
  - **Aplicación:** Casos de uso que orquestan la lógica de dominio, definiendo interfaces para lo que necesita de infraestructura (ej. IRequestRepository).
  - **Infraestructura:** Implementa esas interfaces (ej. acceso a base de datos mediante EF Core).
  - **Presentación:** ASP.NET Core MVC + Razor Views. Los Controllers solo reciben la entrada, llaman a los casos de uso de Aplicación, y retornan las vistas — sin lógica de negocio aquí.

- **Independencia del Framework:** Las capas de Dominio y Aplicación no deben hacer referencia directa a ASP.NET Core, Entity Framework, ni a ningún otro framework, de modo que las reglas de negocio principales puedan probarse y analizarse independientemente de la tecnología web.

- **Principios SOLID:** Todo el código, especialmente en las capas de Dominio y Aplicación, debe seguir SOLID (Responsabilidad Única, Abierto/Cerrado, Sustitución de Liskov, Segregación de Interfaces, Inversión de Dependencias). La Inversión de Dependencias se aplica mediante la Inyección de Dependencias nativa de ASP.NET Core.

- **Justificación:** Esto asegura que las reglas de negocio (Sección 4) permanezcan protegidas, comprobables y fáciles de mantener, y que cualquier desarrollador o generación de código asistida por IA coloque el nuevo código en la capa correcta.
- 
## 3. Actores y Roles del Sistema

El sistema opera con 3 actores distintos, cada uno con permisos de acceso claramente delimitados:

- **Empleado:** Solicita permisos (especificando fechas, tipo y motivo) y consulta el estado y el historial de sus propias solicitudes. No tiene acceso a las solicitudes de otros empleados.

- **Gerente Directo:** Revisa, aprueba o rechaza las solicitudes enviadas únicamente por los miembros de su propio equipo (relación jerárquica empleado-gerente). No puede aprobar ni ver solicitudes de equipos que no le pertenecen.

- **RR.HH. (Recursos Humanos):** Tiene acceso de **solo lectura** al historial completo de solicitudes de todos los empleados y a los saldos de días disponibles. No participa en el flujo de aprobación/rechazo.

**Nota:** Un mismo usuario puede tener más de un rol simultáneamente (por ejemplo, un Gerente Directo también es Empleado y puede solicitar sus propios permisos, los cuales serán aprobados por su propio superior).

## 4. Reglas de Negocio No Negociables

- **Una Solicitud Pendiente a la Vez:** Un empleado no puede tener más de una solicitud en estado Pendiente simultáneamente. Para solicitar otras fechas, debe primero cancelar su solicitud pendiente actual.

- **Límites de Días:** Un empleado no puede solicitar más días de los que tiene disponibles en su saldo acumulado. Esta validación de saldo aplica a todos los tipos de permiso, **excepto al tipo Médico**, el cual no requiere saldo disponible para ser solicitado.

- **Validación de Fechas:**
  - No se permiten solicitudes para fechas pasadas.
  - Las fechas de la solicitud deben ser válidas (fecha de inicio anterior o igual a la fecha de fin).

- **Flujo de Aprobación:** Toda solicitud debe pasar por un flujo de aprobación explícito (Gerente Directo) antes de afectar oficialmente el saldo final del empleado, según corresponda a su tipo.

- **Prohibición de Auto-Aprobación (Separación de Intereses):** Ningún usuario puede aprobar ni rechazar su propia solicitud. Si un Gerente Directo solicita un permiso (actuando como Empleado), esta solicitud queda bloqueada para él mismo y solo puede ser gestionada (aprobada/rechazada) por su superior jerárquico.

- **Impacto en el Saldo según Estado y Tipo:**
  - Una solicitud **Aprobada** de tipo distinto a Médico descuenta los días del saldo definitivo del empleado.
  - Una solicitud **Aprobada** de tipo **Médico** no descuenta días del saldo, independientemente de su duración.
  - Una solicitud **Rechazada**, de cualquier tipo, no afecta el saldo.

- **Cancelación:** Un empleado puede cancelar su propia solicitud únicamente mientras se encuentre en estado **Pendiente**. Una vez aprobada o rechazada, la solicitud no puede cancelarse.

## 5. Estados y Transiciones Válidas

- **Estados posibles:** `Pendiente`, `Aprobada`, `Rechazada`, `Cancelada`.

- **Ciclo de Vida de la Solicitud:**
  - `Pendiente` → `Aprobada` (acción exclusiva del Gerente Directo).
  - `Pendiente` → `Rechazada` (acción exclusiva del Gerente Directo).
  - `Pendiente` → `Cancelada` (acción exclusiva del Empleado dueño de la solicitud).

- **Diagrama de transición:**
[Pendiente] --Aprobar (Gerente)--> [Aprobada]
[Pendiente] --Rechazar (Gerente)--> [Rechazada]
[Pendiente] --Cancelar (Empleado)--> [Cancelada]

- **Inmutabilidad Tras la Resolución:** Una vez que una solicitud transiciona a `Aprobada`, `Rechazada` o `Cancelada`, es un estado final: no puede volver a `Pendiente` ni cambiar a ningún otro estado. La resolución es definitiva.

- **RR.HH.:** No puede ejecutar ninguna transición de estado (ver Sección 3) — su rol es exclusivamente de consulta.

## 6. Estándares de Calidad y Pruebas

- **Estrategia por Capas (Clean Architecture):**

  - **Dominio:** Las pruebas unitarias deben validar el estado interno, invariantes (ej. fechas pasadas) y cálculos sin usar dependencias externas ni mocks (solo lógica pura).
  - **Aplicación (Casos de Uso):** Se deben probar los flujos, orquestación y validación de permisos (ej. que un empleado no apruebe su solicitud), haciendo mock de las interfaces de infraestructura (ej. repositorios).
  - **Infraestructura:** Pruebas de integración puntuales para validar queries de base de datos (Entity Framework Core) y el sistema de traza/auditoría.

- **Cobertura de Reglas Críticas:** Es obligatorio contar con pruebas unitarias para escenarios de las Reglas No Negociables (Sección 4), como: validaciones de saldo, la regla del permiso médico que no descuenta días, la restricción de una sola solicitud pendiente a la vez, y la estricta máquina de estados.

- **Abstracción del Tiempo:** Dado que hay reglas complejas sobre fechas (no permitir fechas pasadas), está prohibido usar `DateTime.Now` o `DateTime.UtcNow` de forma directa en el código de Dominio/Aplicación. Se debe inyectar una abstracción (ej. `TimeProvider` en .NET 8) que permita fijar el tiempo en las pruebas.

- **Controladores Delgados (Thin Controllers):** En ASP.NET Core MVC, los controladores no pueden contener lógica de negocio ni acceso directo a datos; su única responsabilidad es recibir peticiones HTTP, delegar a la capa de Aplicación y devolver vistas Razor.

## 7. Estándares de Seguridad y Permisos

- **Políticas de Autorización (Policy-Based Authorization):** En ASP.NET Core se usarán Políticas (`[Authorize(Policy = "...")]`) en lugar de roles estáticos quemados en código, manteniendo gran flexibilidad para segmentar permisos (Empleado, Gerente, RR.HH.).

- **Autorización Basada en Recursos (Resource-based Authorization):** Para garantizar el aislamiento de datos (ej. un Gerente solo aprueba a los empleados de su propio equipo, el empleado solo ve sus propias solicitudes), la seguridad verificará en la capa de aplicación el ID del usuario logueado contra el propietario de los datos solicitados.

- **Uso de Reclamaciones (Claims):** La identidad del usuario (ID de Empleado, ID de su Jefe Directo, y su Rol) debe almacenarse en los `Claims` de la cookie de autenticación durante el login, para evitar consultas repetitivas a la base de datos por cada petición HTTP.

- **Protección Anti-CSRF:** Todas las mutaciones de estado que se realicen a través de POST en las vistas Razor (crear solicitud, cancelar, aprobar, rechazar) deben incluir obligatoriamente el atributo `[ValidateAntiForgeryToken]` en el controlador y el tag helper correspondiente en el formulario.

- **Prevención de Abuso (Rate Limiting):** Se debe configurar el middleware nativo de Rate Limiting de .NET para rutas críticas (como el Login o el envío de nuevas solicitudes), con el fin de mitigar ataques de fuerza bruta o de denegación de servicio (DoS) por parte de bots.

- **Protección OWASP (Prevención de XSS):** El sistema debe prevenir estrictamente la inyección de scripts maliciosos (Cross-Site Scripting). Se validarán y sanitizarán todas las entradas del usuario (ej. comentarios o motivos de las solicitudes) rechazando cualquier input de texto que contenga etiquetas HTML o scripts no permitidos antes de tocar la base de datos.

## 8. Estándares de Datos y Persistencia

- **Sin Eliminación Física (Soft Delete):** Está terminantemente prohibido hacer `DELETE` físico de la tabla de solicitudes. Se aplicará un patrón de *eliminación lógica* (ej. propiedad `IsDeleted = true`) respaldado por un **Global Query Filter** en Entity Framework Core para ocultarlos automáticamente de las consultas.

- **Auditoría Automática (EF Core Interceptors):** El registro de auditoría no debe dejarse a la memoria del programador. Se debe implementar sobreescribiendo el método `SaveChangesAsync` en EF Core para registrar automáticamente el usuario (vía *Claims*) y un sello de tiempo exacto (*TimeProvider*).

- **Control de Concurrencia Optimista:** Para evitar condiciones de carrera (ej. un Empleado intentando cancelar exactamente cuando su Gerente aprueba), la base de datos usará tokens de concurrencia (`RowVersion` o similar) para atrapar y controlar el conflicto sin corrupción de estados.

## 9. Restricciones Tecnológicas

- **Core Framework:** ASP.NET Core MVC puro. Queda **estrictamente prohibido** agregar frameworks SPA de JS (React, Angular, Vue) para mantener las vistas simples (Razor) y la infraestructura ligera.

- **ORM:** Se empleará el estándar de Microsoft, Entity Framework Core.

- **Baja Fricción:** Está prohibida la dependencia en servicios de terceros (Redis, RabbitMQ, o APIs externas). El sistema debe poder correrse localmente al 100% usando solo SQL Server LocalDB o SQLite.

## 10. Convenciones de Código y Nomenclatura (Naming Conventions)

Para mantener un código predecible y uniforme, se aplicarán estrictamente los estándares de la comunidad de C#/.NET y los sufijos de Clean Architecture:

- **Capitalización Básica:** `PascalCase` para Nombres de Clases, Métricas, Propiedades y Métodos. `camelCase` para variables locales y parámetros.

- **Interfaces:** Toda interfaz debe estar precedida por la letra mayúscula **I** (ej. `ILeaveRequestRepository`, `ITimeProvider`).

- **Sufijos Arquitectónicos Obligatorios:** 

  - **Dominio:** Entidades sin sufijo (ej. `LeaveRequest`, `Employee`). Las excepciones personalizadas usarán el sufijo `Exception` (ej. `InsufficientBalanceException`).
  - **Aplicación (Casos de Uso):** Dependiendo del patrón (Ej. CQRS clásico o Servicios), deben terminar en `Service`, `UseCase`, `Command` o `Query`. (ej. `ApproveLeaveRequestCommandHandler`).
  - **Presentación:** Modelos de vista terminarán en `ViewModel` (ej. `LeaveRequestDetailViewModel`). Los controladores terminan obligatoriamente en `Controller`.
  - **Infraestructura:** Clases que implementan persistencia terminarán en `Repository` o `DbContext` (ej. `SqlLeaveRequestRepository`).

- **Idioma del Código:** Aunque los requerimientos estén en español, **todo el código fuente (clases, variables, métodos) debe ser escrito en Inglés** para garantizar uniformidad e internacionalidad, salvo el texto mostrado en la Interfaz (UI).

## 11. Diagramas y Especificaciones Visuales (Docs as Code)

Para garantizar que la documentación de arquitectura nunca quede obsoleta o "huérfana" en el disco de algún desarrollador, se aplicará el paradigma de **Diagramas como Código**.

- **Prohibición de Imágenes Estáticas:** Queda estrictamente prohibido el uso de imágenes estáticas (PNG, JPG o PDFs) generadas por herramientas de dibujo externas (como Visio, Lucidchart o Draw.io) para documentar flujos. Todo el esquema visual debe ser generado escribiendo texto plano usando la sintaxis de **Mermaid.js** dentro de bloques de código en archivos Markdown.

- **Ubicación Centralizada y Trazabilidad:** Todo archivo con diagramas vivirá en la carpeta designada de documentación física (ej. `/docs/diagrams`). Al guardar los diagramas en puro texto junto con el código fuente en Git, cualquier modificación arroja un histórico rastreable (`git diff`). En un Pull Request, el equipo podrá comparar qué línea del diagrama de negocio cambió, y el sistema (GitHub/GitLab/DevOps) lo dibujará automáticamente en la interfaz web.

- **Diagramas Obligatorios Mínimos:**

  1. **Casos de Uso (Use Cases):** Representación de "Quién hace Qué". Delimita los flujos entre los 3 Actores:
     - *Empleado:* `Solicitar Permiso`, `Cancelar Permiso`, `Consultar Balance/Historial`.
     - *Gerente Directo:* `Ver Bandeja de Equipo`, `Aprobar Permiso`, `Rechazar Permiso`.
     - *RR.HH.:* `Auditar Historial General`, `Reporte de Saldos`.

  2. **Máquina de Estados (State Diagram):** Representación inquebrantable de los estados y transiciones de la Sección 5 (Pendiente -> Aprobada/Rechazada/Cancelada), demostrando visualmente cuáles son "estados finales" inmutables.

  3. **Flujo de Ejecución / Secuencia (Sequence Diagram):** Representación técnica que muestre la orquestación de CQRS en la Capa de Aplicación (Cómo entra un Command desde el MVC Controller, pasa por Validaciones, el Value Object valida fechas y la base de datos lo guarda con el Soft Delete y el Audit Interceptor).


## 12. Gobernanza y Evolución del Sistema

- **La Constitución es la Ley Máxima:** Este documento tiene prioridad sobre cualquier otro artefacto del proyecto (spec, plan, código). Ante cualquier conflicto, prevalece lo definido aquí.

- **Rechazo Automático de Violaciones Arquitectónicas:** Cualquier intento de romper las reglas de Clean Architecture será rechazado inmediatamente, por ejemplo:
  
  - Un Controller de MVC llamando directamente a Entity Framework (en lugar de pasar por la capa de Aplicación).
  - Lógica de negocio implementada dentro de una Vista Razor (.cshtml).
  - La capa de Dominio referenciando directamente a la capa de Infraestructura.

- **Proceso de Cambio:** Las reglas de negocio o restricciones core (Secciones 2 a 9) solo pueden modificarse si el cambio se revisa, se justifica y se aprueba primero, antes de reflejarse en el spec, el plan o el código.

- **Extensión vs. Violación:** Agregar nuevas reglas (extender la constitution) es válido y bienvenido conforme el proyecto evoluciona, pero sigue el mismo proceso de revisión que modificar una regla existente — ninguna regla se cambia "sobre la marcha" durante el desarrollo.

- **Control de Versiones:** Cada cambio a este documento debe quedar registrado con número de versión, fecha y motivo del cambio, para mantener trazabilidad de cómo evolucionaron las reglas del proyecto.