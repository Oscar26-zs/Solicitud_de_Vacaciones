# TAREAS.md — Exponer API para el agente de IA (Oro-Agente)

## Estado actual

La API `/api/vacaciones/...` para el agente de IA está implementada y verificada
en vivo: build limpio, 11/11 tests nuevos pasan, y se probó contra una base de
datos real (LocalDB) levantando el servidor y golpeando los endpoints con curl
— creación de solicitud, consulta de estado, traslape de fechas, saldo,
autenticación por API key, y validación de `empleadoId` — todos los casos
respondieron con el status code y el JSON esperados.

## Contrato con el otro proyecto (Oro-Agente, Python) — ⚠️ cambió

```
POST /api/vacaciones/solicitar
Body:     { "empleadoId": string (Guid), "destino": string, "fechaInicio": "YYYY-MM-DD", "fechaFin": "YYYY-MM-DD" }
Response: { "solicitudId": string (Guid), "estado": "pendiente" }

GET /api/vacaciones/{solicitudId}/estado
Response: { "solicitudId": string (Guid), "estado": "pendiente" | "aprobada" | "rechazada" }
```

**`empleadoId` y `solicitudId` ahora son Guid (string), no `int`** — ver
decisión #1 más abajo. El proyecto Oro-Agente (Python) todavía asume `int` en
`VacacionesApiClient`, en las tools de `agente_solicitudes`, y en el `empleadoId`
del endpoint `POST /chat`. **Eso requiere un cambio en Oro-Agente que no se
hizo en esta sesión** porque el pedido de esta conversación fue completar
las tareas de este proyecto (C# MVC) — queda como seguimiento pendiente.

## Decisiones (antes "pendientes de PO", ahora resueltas para poder implementar)

Siguiendo el mismo estilo que `docs/Preguntas_Pendientes.md` — documentadas,
no asumidas en silencio:

1. **✅ RESUELTO — Tipo de `empleadoId`/`solicitudId`**: se usa `Guid` (serializado
   como string en JSON), no un ID numérico paralelo. Es lo que ya usa todo el
   dominio (`Empleado.Id`, `CrearSolicitudCommand.EmpleadoId`, el claim
   `"EmpleadoId"`); introducir un ID numérico adicional habría significado una
   columna nueva, una invariante de unicidad nueva, y código de mapeo extra,
   solo para servir a un cliente. Consecuencia: Oro-Agente debe tratar estos
   campos como string, no como int (ver más arriba).
2. **✅ RESUELTO — Mapeo de `EstadoSolicitud` (5 valores) → 3 valores del chat**:
   `Pending`→`"pendiente"`, `Approved`→`"aprobada"`, `Rejected`→`"rechazada"`,
   `Cancelled`→`"rechazada"`, `Expired`→`"rechazada"`. Cancelled y Expired se
   agrupan como "rechazada" porque, desde la perspectiva del empleado en el
   chat, en ambos casos el viaje no va a suceder — es una decisión de producto
   razonable pero no la única posible; si el otro equipo necesita distinguirlos
   más adelante, el contrato tendría que crecer a un cuarto valor.
   Implementado en `EstadoSolicitudMapeador.AEstadoApi()`.
3. **✅ RESUELTO — Autenticación servidor-a-servidor**: header `X-Api-Key`
   comparado contra `AgenteIA:ApiKey` (`ApiKeyAuthFilter`). Sin `[Authorize]`
   de Identity — es un endpoint `ControllerBase` aparte, no cubierto por
   cookies de sesión. Si el servidor no tiene la key configurada, el endpoint
   responde `503` en vez de quedar abierto sin autenticación.
4. **✅ RESUELTO — `destino` no existe en el dominio**: se guarda dentro de
   `Motivo` con el prefijo `"Viaje a {destino}"` (para cumplir el mínimo de 10
   caracteres que exige `CrearSolicitudCommandValidator` — un destino corto
   como "Perú" por sí solo no lo cumpliría). Es una solución pragmática, no la
   ideal: si el destino necesita ser una propiedad consultable por separado
   más adelante (reportes, filtros), requeriría una migración de EF Core para
   agregar una columna `Destino` real a `SolicitudVacaciones`. Fuera de
   alcance de este pedido.
5. **✅ RESUELTO — Ownership check en `GET .../estado`**: el contrato no
   incluye `empleadoId` en esta consulta, así que no se puede validar
   pertenencia a nivel de recurso. Se confía en la autenticación por API key
   (transporte) como único control: cualquier llamador con la key correcta
   puede consultar cualquier solicitud. Se reutiliza
   `ObtenerSolicitudDetalleQueryHandler` pasando `EsAprobador: true, EsRRHH: true`
   para saltar su chequeo de dueño sin duplicar esa lógica de acceso.
6. **⚠️ Puerto real, no resuelto — discrepancia detectada**: el contrato
   original asumía `localhost:5000`, pero `launchSettings.json` de este
   proyecto usa `http://localhost:5051` (perfil `http`) /
   `https://localhost:7023` (perfil `https`) en desarrollo. **No se cambió el
   puerto del proyecto** (podría romper otra configuración/tooling que ya
   dependa de 5051). En su lugar: Oro-Agente debe apuntar
   `VACACIONES_API_URL` al puerto real (`http://localhost:5051`), o hay que
   decidir explícitamente fijar el puerto a 5000 en ambos lados. Pendiente de
   decidir con el otro proyecto.

---

## Fase 0: Resolver decisiones de contrato y seguridad

- [x] Tipo de `empleadoId`/`solicitudId` → decisión #1
- [x] Mapeo de `EstadoSolicitud` → decisión #2
- [x] Mecanismo de autenticación → decisión #3
- [ ] Puerto real → decisión #6, **no resuelta**, requiere coordinación con
      Oro-Agente

## Fase 1: Estructura para exponer API JSON

- [x] `src/Vacations.Web/Controllers/Api/VacacionesApiController.cs` — separado
      de `SolicitudVacacionesController`, hereda `ControllerBase` (no `Controller`),
      así que estructuralmente no puede devolver una vista HTML
- [x] `src/Vacations.Web/Controllers/Api/VacacionesApiModels.cs` — DTOs propios
      (`SolicitarVacacionesRequest/Response`, `EstadoSolicitudResponse`, `ErrorResponse`)
- [x] Ruta base `[Route("api/vacaciones")]`

## Fase 2: `POST /api/vacaciones/solicitar`

- [x] Implementado, mapea `destino`→`Motivo` (ver decisión #4)
- [x] Reutiliza `CrearSolicitudCommandHandler` y `CrearSolicitudCommandValidator`
      existentes, sin duplicar lógica de negocio
- [x] `SaldoInsuficienteException`/`TraslapeSolicitudesException` → 409,
      `InvalidOperationException` (empleado/saldo no encontrado) → 400,
      cualquier otra excepción → 500, todo como JSON
- [x] `empleadoId` ausente → 400 vía `FluentValidation` (`NotEmpty`, mensaje
      "El Id del empleado es requerido."); `empleadoId` con formato inválido
      (no-Guid) → 400 automático de `[ApiController]` — **ambos casos
      verificados en vivo con curl contra el servidor real**

## Fase 3: `GET /api/vacaciones/{solicitudId}/estado`

- [x] Reutiliza `ObtenerSolicitudDetalleQueryHandler`
- [x] Mapeo de estados aplicado antes de responder (decisión #2)
- [x] `SolicitudNoEncontradaException` → 404 — verificado en vivo
- [x] Ownership check → decisión #5 (no se exige, por diseño del contrato)

## Fase 4: Autenticación del endpoint

- [x] `ApiKeyAuthFilter` (`IAsyncActionFilter`), aplicado vía
      `[ServiceFilter(typeof(ApiKeyAuthFilter))]` en el controller
- [x] No requiere `[Authorize]`/cookie de Identity
- [x] `AgenteIA:ApiKey` agregado a `appsettings.Development.json` **vacío por
      defecto** (el endpoint responde 503 hasta que se configure un valor real
      — nunca queda abierto por accidente). Valor real va en
      `appsettings.Development.local.json` (ya estaba en `.gitignore` pero no
      se cargaba: se agregó `builder.Configuration.AddJsonFile(...)` en
      `Program.cs` para que esa convención funcione de verdad) o en la
      variable de entorno `AgenteIA__ApiKey`
- [x] Verificado en vivo: sin key → 401, key incorrecta → 401, key correcta →
      pasa; sin `AgenteIA:ApiKey` configurada en el servidor → 503

## Fase 5: Manejo de errores y logging

- [x] Todas las acciones envueltas en try/catch con fallback JSON (nunca HTML)
- [x] `ILogger<VacacionesApiController>` registra cada llamada entrante
      (`empleadoId`/`solicitudId`, nunca el body completo) y cualquier excepción
      no esperada con `LogError`

## Fase 6: Pruebas

- [x] `tests/Vacations.Web.Tests/Controllers/Api/VacacionesApiControllerTests.cs`
      — 7 tests: creación exitosa, saldo insuficiente, traslape, fecha en el
      pasado (validación), estado aprobada, estado pendiente, estado 404.
      Usa handlers **reales** (no mockeados — `CrearSolicitudCommandHandler` y
      `ObtenerSolicitudDetalleQueryHandler` son clases `sealed` sin interfaz,
      así que no se pueden mockear con NSubstitute) con repositorios falsos,
      igual que ya hacía `CrearSolicitudCommandHandlerTests`
- [x] `tests/Vacations.Web.Tests/Controllers/Api/ApiKeyAuthFilterTests.cs` — 4
      tests: key válida continúa, key ausente/incorrecta → 401, sin key
      configurada en servidor → 503
- [x] Paquetes `NSubstitute`/`FluentAssertions` agregados a
      `Vacations.Web.Tests.csproj` (antes solo tenía xUnit)
- [x] **11/11 tests pasan** (`dotnet test tests/Vacations.Web.Tests`)
- [x] Casos adicionales verificados en vivo contra el servidor real (no solo
      con mocks): creación exitosa con UTF-8 real, traslape real, 401/503 del
      filtro, 404, `empleadoId` ausente/inválido — ver "Estado actual"
- [ ] **No cubierto**: tests de integración HTTP completos con
      `WebApplicationFactory` (el proyecto de tests no tenía ese paquete ni
      esa infraestructura); se optó por tests a nivel de controller + repos
      falsos, que es el mismo nivel de profundidad que ya usa
      `Vacations.Application.Tests` en este repo

> **Hallazgo no relacionado con este trabajo**: al correr toda la solución
> (`dotnet test Solicitud_de_Vacaiones.slnx`) aparecen 4 fallos preexistentes
> — `SolicitudVacacionesTests.Cancelar_SolicitudAprobada_LanzaTransicionEstadoInvalidaException`
> y 3 en `CrearSolicitudCommandValidatorTests` (validaciones de rango de
> fechas). Ninguno de esos archivos fue tocado en esta sesión (confirmado con
> `git status`); parecen tests sensibles a la fecha real del sistema (hoy
> 2026-08-20) comparada contra fechas fijas en el código de prueba. No se
> tocaron porque no son parte de este pedido — quedan anotados para que el
> equipo los revise.

## Fase 7: Documentación e integración end-to-end

- [x] `README.md` actualizado con cómo levantar el sistema, el puerto real, y
      cómo configurar la API key
- [ ] Coordinar con Oro-Agente: **requiere que ese proyecto actualice su
      `VacacionesApiClient` para usar Guid (string) en vez de int, y apunte al
      puerto real** (decisión #6) — sin eso, la integración real entre ambos
      sistemas no puede probarse todavía, aunque cada lado ya funciona
      verificado por separado
