# PROMPT 02 — BACKEND C# / .NET 8

## Rol

Actúa como **Senior Backend Developer / Software Architect**, especializado en C#, .NET 8, ASP.NET Core Web API, Entity Framework Core, REST, validación, manejo de errores, testing y diseño de aplicaciones empresariales.

Debes desarrollar exclusivamente el **backend** de una aplicación fullstack denominada:

**Gestor de Siniestros Simplificado**

El frontend será desarrollado posteriormente con **Angular 19**.

La persistencia utilizará:

**Microsoft SQL Server + T-SQL**

---

# 1. Objetivo

Construir una API REST que permita:

- Crear siniestros.
- Consultar siniestros.
- Consultar un siniestro por Id.
- Actualizar información de un siniestro.
- Eliminar un siniestro.
- Filtrar por estado.
- Buscar por número de póliza.
- Consultar historial de cambios de estado.
- Cambiar el estado de un siniestro.
- Validar reglas de negocio.

---

# 2. Stack obligatorio

Utiliza exclusivamente:

```text
C#
.NET 8
ASP.NET Core Web API
Entity Framework Core
SQL Server
```

Para testing:

```text
xUnit
```

Puedes utilizar otras librerías pequeñas si aportan un valor claro, pero no agregues dependencias innecesarias.

---

# 3. Arquitectura

Utiliza una arquitectura limpia y sencilla.

Una estructura recomendada es:

```text
src/
 ├── Claims.Api
 ├── Claims.Application
 ├── Claims.Domain
 └── Claims.Infrastructure

tests/
 └── Claims.Tests
```

Responsabilidades:

### Claims.Api

- Controllers.
- Configuración HTTP.
- Middleware.
- Dependency Injection.
- Swagger.

### Claims.Application

- Casos de uso.
- Servicios.
- DTOs.
- Validaciones.
- Reglas de negocio.

### Claims.Domain

- Entidades.
- Enumeraciones.
- Reglas propias del dominio cuando corresponda.

### Claims.Infrastructure

- DbContext.
- Configuración EF Core.
- Persistencia.
- Implementaciones relacionadas con SQL Server.

No crees capas adicionales sin una razón concreta.

---

# 4. Entidades

Debe existir como mínimo:

```text
Claim
ClaimStatusHistory
```

Claim:

```text
Id
PolicyNumber
InsuredName
ClaimType
EstimatedAmount
Status
CreatedAt
UpdatedAt
```

ClaimStatusHistory:

```text
Id
ClaimId
PreviousStatus
NewStatus
ChangedAt
```

---

# 5. Estados

Utiliza únicamente:

```text
OPEN
IN_REVIEW
CLOSED
```

Representa los estados de manera type-safe.

Evita utilizar strings arbitrarios por todo el código.

---

# 6. Transiciones

Implementa estas transiciones:

```text
OPEN → IN_REVIEW
IN_REVIEW → CLOSED
```

No permitas:

```text
CLOSED → OPEN
CLOSED → IN_REVIEW
IN_REVIEW → OPEN
```

La regla debe estar centralizada.

No la repitas en múltiples Controllers.

---

# 7. API REST

Implementa:

```http
GET    /api/claims
GET    /api/claims/{id}
POST   /api/claims
PUT    /api/claims/{id}
DELETE /api/claims/{id}
PATCH  /api/claims/{id}/status
```

El listado debe aceptar filtros:

```http
GET /api/claims?status=OPEN&policyNumber=POL-001
```

Los filtros deben poder utilizarse individualmente o combinados.

La búsqueda por póliza debe ser apropiadamente manejada.

---

# 8. Historial

Cuando se cambie el estado:

```text
1. Validar que el Claim exista.
2. Validar que la transición sea válida.
3. Obtener el estado actual.
4. Actualizar el estado.
5. Crear ClaimStatusHistory.
6. Persistir ambas operaciones de manera atómica.
```

No debe existir un cambio de estado sin su correspondiente registro de historial.

Utiliza una transacción cuando sea necesaria para garantizar esta consistencia.

---

# 9. DTOs

No expongas directamente las entidades EF Core como contratos de API.

Define DTOs.

Como mínimo:

```text
CreateClaimRequest
UpdateClaimRequest
ChangeClaimStatusRequest
ClaimResponse
ClaimStatusHistoryResponse
```

El contrato de API debe estar desacoplado de las entidades de persistencia.

---

# 10. Validaciones

Implementa validaciones para:

### PolicyNumber

Obligatorio.

### InsuredName

Obligatorio.

### ClaimType

Obligatorio.

### EstimatedAmount

Debe ser mayor que cero.

### Status

Debe ser un estado permitido.

Las validaciones deben existir en backend aunque Angular también las implemente.

Nunca confíes en la validación del frontend.

---

# 11. Manejo de errores

Utiliza respuestas HTTP apropiadas.

Como mínimo:

```text
200 OK
201 Created
204 No Content
400 Bad Request
404 Not Found
409 Conflict
500 Internal Server Error
```

Utiliza `ProblemDetails` cuando sea apropiado.

No devuelvas excepciones internas directamente al cliente.

No expongas:

- Stack traces.
- Connection strings.
- Información interna de SQL.
- Información sensible.

---

# 12. Casos de negocio

El backend debe poder responder correctamente a escenarios como:

```text
Crear Claim válido
Crear Claim inválido
Consultar Claim existente
Consultar Claim inexistente
Actualizar Claim
Eliminar Claim
Filtrar por estado
Buscar por póliza
Cambiar OPEN → IN_REVIEW
Cambiar IN_REVIEW → CLOSED
Intentar CLOSED → OPEN
Consultar historial
```

---

# 13. Concurrencia

Considera el escenario donde dos solicitudes intentan cambiar simultáneamente el estado de un mismo Claim.

No necesitas implementar un sistema complejo de concurrencia distribuida.

Sin embargo, evita una implementación donde sea trivial terminar con un historial inconsistente.

Explica brevemente la estrategia utilizada.

---

# 14. EF Core

Configura explícitamente:

- Relaciones.
- Longitudes.
- Precisión decimal.
- Required fields.
- Enumeraciones.
- Índices importantes.

No dependas únicamente de convenciones si una regla de persistencia requiere configuración explícita.

---

# 15. Connection String

Utiliza configuración estándar de ASP.NET Core.

No hardcodees:

- Passwords.
- Connection strings.
- Secretos.

Utiliza `appsettings.json` únicamente para configuración no sensible y mecanismos estándar de configuración para valores locales.

Incluye un ejemplo de configuración:

```text
appsettings.Development.json
```

sin credenciales reales.

---

# 16. Swagger

La API debe exponer Swagger/OpenAPI en desarrollo.

Documenta correctamente los endpoints principales.

---

# 17. Logging

Agrega logging únicamente donde aporte valor:

- Errores inesperados.
- Operaciones importantes.
- Problemas de persistencia.

No llenes el código de logs innecesarios.

---

# 18. Testing

Crea pruebas unitarias para las reglas de negocio más importantes.

Como mínimo:

```text
CreateClaim_WithInvalidAmount_ShouldFail

CreateClaim_WithMissingPolicyNumber_ShouldFail

ChangeStatus_OpenToInReview_ShouldSucceed

ChangeStatus_InReviewToClosed_ShouldSucceed

ChangeStatus_ClosedToOpen_ShouldFail

ChangeStatus_ShouldCreateHistory

GetClaims_WithStatusFilter_ShouldReturnFilteredResults
```

Prioriza las reglas de negocio sobre pruebas triviales de getters/setters.

---

# 19. No implementar

NO implementes:

- Angular.
- UI.
- Autenticación.
- JWT.
- Usuarios.
- Roles.
- Microservicios.
- CQRS.
- Event Sourcing.
- Mensajería.
- Redis.
- Cache distribuido.
- Arquitecturas innecesariamente complejas.

La aplicación debe mantenerse dentro del alcance de una prueba técnica de 4–6 horas.

---

# 20. Criterios de aceptación

El backend debe:

- Compilar correctamente.
- Ejecutarse en .NET 8.
- Conectarse a SQL Server.
- Implementar el CRUD.
- Implementar filtros.
- Implementar búsqueda por póliza.
- Implementar historial.
- Validar datos.
- Validar transiciones.
- Manejar errores.
- Utilizar DTOs.
- Utilizar EF Core correctamente.
- Tener pruebas unitarias.
- Exponer Swagger.
- Ser consumible desde Angular 19.

---

# 21. Principio de diseño

No intentes demostrar conocimiento mediante complejidad.

La prioridad es:

```text
Correctness
↓
Maintainability
↓
Testability
↓
Performance
↓
Simplicity
```

Si una abstracción no aporta un beneficio real, no la agregues.

Antes de implementar, analiza el modelo y las reglas.

Si detectas ambigüedades, toma la decisión más sencilla y documenta por qué.

No agregues funcionalidades fuera del alcance.