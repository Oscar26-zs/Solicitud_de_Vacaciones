Plan (extracto de specs/plan.md — secciones 2,5,7,8 y 10)

Contexto técnico
- Proyecto orientado a Clean Architecture. Separación de responsabilidades para
  facilitar el mantenimiento, testeo y despliegue.
- Decisiones clave: EF Core para persistencia, repositorios + UnitOfWork,
  ITimeProvider para controlar tiempo en tests, InterceptorAuditoria para
  mantener trazabilidad de cambios.

Módulos y estructura
- Vacations.Domain: modelos, value objects, reglas de negocio.
- Vacations.Application: casos de uso, DTOs, validaciones, handlers.
- Vacations.Infrastructure: EF DbContext, repositorios, implementaciones de
  servicios externos, configuración de Identity.
- Vacations.Web: API/Frontend skeleton, autenticación y políticas.

Complejidades conocidas
- ServicioExpiracionAutomatica: job periódico que debe ser idempotente y
  tolerante a fallos; requiere pruebas de integración y control de tiempo.
- ITimeProvider: imprescindible para pruebas deterministas de expiración y
  cálculos temporales.
- InterceptorAuditoriaSaveChanges: debe interceptar SaveChangesAsync y
  rellenar metadatos de auditoría sin afectar rendimiento.

Plan de trabajo (fases relevantes)
- Setup inicial: solución y proyectos, paquetes NuGet, CI básica.
- Implementación del dominio base: enums, entidades y value objects (RangoFechas,
  DiasHabiles).
- Abstracciones: interfaces de repositorios, IUnitOfWork, ITimeProvider.
- Infraestructura: DbContext, migraciones, Identity y seed.
- Esqueleto Web: autenticación, layout y endpoints mínimos.
- Tests: proyectos de pruebas y fixtures compartidos.

Riesgos y mitigaciones
- Migraciones y esquemas: diseñar migraciones incrementales y usar pruebas de
  integración antes de desplegar.
- Concurrencia: usar RowVersion y pruebas que simulen conflictos.

Dependencias
- Este feature es la base; los demás features dependen de su entrega parcial.

Última actualización: 2026-08-03
