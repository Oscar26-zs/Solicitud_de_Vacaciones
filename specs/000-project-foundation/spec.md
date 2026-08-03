# Feature 000: Fundación del Proyecto (Infraestructura)

Este feature NO es funcional. Agrupa la infraestructura transversal necesaria para
soportar la implementación de los features funcionales (001..005) y las pruebas.
Trazabilidad: constitution.md secciones 3, 6, 7 y 9.

Resumen
- Objetivo: definir y entregar la base técnica del proyecto: estructura de
  solución, paquetes, dominios base, abstracciones, infraestructura y esqueleto
  web, además de la base para pruebas automatizadas.

Alcance
- Estructura de solución (Clean Architecture):
  - Vacations.Domain
  - Vacations.Application
  - Vacations.Infrastructure
  - Vacations.Web
  - Proyectos de tests por capa (xUnit)
- Paquetes NuGet por capa: EF Core + herramientas infra, AutoMapper, MediatR,
  FluentValidation, Serilog/Logger, Identity/Authentication.

Dominio base (entidades y tipos comunes)
- Enums: EstadoSolicitud, RolUsuario
- Value Objects: RangoFechas, DiasHabiles
- Entidades: Empleado, SaldoEmpleado, SolicitudVacaciones, HistorialSolicitud
- Excepciones de dominio: DomainException y derivadas (BusinessRuleViolation)

Abstracciones
- Repositorios (IEmployeeRepository, IRequestRepository, IReadOnlyRepository<T>)
- IUnitOfWork
- ITimeProvider (abstracción de tiempo para tests)

Infraestructura
- DbContext (VacationsDbContext) con configuración EF Core, mapeos y RowVersion
  para concurrencia optimista.
- Identity y configuración de usuarios/roles.
- Seed de datos inicial (roles, usuario admin, datos demo).
- Implementación concreta de IUnitOfWork y repositorios.
- Interceptor de auditoría para SaveChanges (InterceptorAuditoriaSaveChanges).

Esqueleto Web
- Autenticación y autorización por políticas (roles y claims).
- Layout base y CSS tokens (design tokens) compartidos.
- Endpoints básicos y estructura de carpetas MVC/API.

Base de pruebas
- Proyecto de pruebas por capa (Vacations.Domain.Tests, Vacations.Infrastructure.Tests,
  Vacations.Application.Tests, Vacations.Web.Tests).
- Uso de xUnit y fixtures para ITimeProvider, DB in-memory o SQL Lite para integración.

Orden de implementación (priorizado)
000 → 001 → 002 → 003 → 004 → 005

Notas
- Las implementaciones críticas: TimeProvider, InterceptorAuditoria y Servicio de
  Expiración Automática deben diseñarse pensando en testabilidad y observabilidad.
- TASK-026 (servicio de expiración) se implementa en este feature como
  infraestructura, pero su verificación funcional será responsabilidad del
  Feature 004.

Última actualización: 2026-08-03
