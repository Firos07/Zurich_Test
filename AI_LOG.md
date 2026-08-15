# AI_LOG.md

Registro de uso crítico de la IA durante la prueba técnica **Fullstack Developer — Gestor de
Siniestros Simplificado**.

El log se organiza en dos partes, correspondientes a las dos conversaciones con IA que
participaron en el proceso:

1. **Parte 1 — Definición y preparación**: conversación con un asistente de IA previo a la
   implementación, donde se analizaron requerimientos, stack, alcance, modelo de datos,
   arquitectura, estrategia de pruebas, git e integración con OpenCode.
2. **Parte 2 — Implementación**: conversación con el asistente de desarrollo (OpenCode)
   durante la construcción real de la solución, con propuestas, decisiones, correcciones y
   resultados.

La IA no se considera la autoridad técnica final. Las decisiones se revisaron y se establecieron
buscando mantener el alcance de la prueba, evitar sobrearquitectura y producir una solución fácil
de probar y explicar.

---

# Parte 1 — Definición y preparación

## 1. Propósito

Este documento registra el uso de IA durante la definición y preparación de la prueba técnica
**Fullstack Developer — Gestor de Siniestros Simplificado**.

El objetivo es dejar evidencia de que la IA se utilizó como apoyo para:

- Analizar los requerimientos.
- Evaluar decisiones de arquitectura.
- Definir el modelo de datos.
- Diseñar la API.
- Definir la estructura frontend.
- Establecer el flujo de construcción.
- Preparar la estrategia de uso de IA dentro del proyecto.

La IA no se considera la autoridad técnica final. Las decisiones se revisaron y se establecieron
buscando mantener el alcance de la prueba, evitar sobrearquitectura y producir una solución fácil
de probar y explicar.

---

## 2. Contexto de la prueba

La prueba solicita construir una mini-aplicación fullstack para registrar y dar seguimiento a
siniestros de seguros.

Requisitos funcionales identificados:

- CRUD de siniestros.
- Número de póliza.
- Asegurado.
- Tipo de siniestro.
- Monto estimado.
- Estado: abierto / en revisión / cerrado.
- Listado con filtro por estado.
- Búsqueda por número de póliza.
- Vista de detalle.
- Historial de cambios de estado.
- Validaciones básicas.
- Persistencia real.
- Repositorio Git con commits incrementales.
- Evidencia de uso crítico de IA.

La prueba asigna 25% de la evaluación al uso crítico de IA, por lo que el proceso de revisión y
las decisiones tomadas son parte importante del resultado.

---

## 3. Decisión de stack tecnológico

### Propuesta inicial

La prueba permitía elegir entre varios stacks para backend y frontend.

### Decisión

Se estableció como stack definitivo:

```text
Frontend: Angular 19
Backend: C# / .NET 8 / ASP.NET Core Web API
Database: SQL Server / T-SQL
ORM: Entity Framework Core
Testing backend: xUnit
```

### Motivo

Se decidió trabajar con Angular 19, .NET 8 y SQL Server para centrar la solución en las
tecnologías objetivo y evitar introducir tecnologías que no aportaran valor a esta prueba.

---

## 4. Análisis del alcance

### Decisión

La aplicación debe ser pequeña, funcional y profesional, sin incorporar infraestructura o
patrones que no sean necesarios para cumplir los requisitos.

### Se descartó explícitamente introducir

- Microservicios.
- CQRS.
- Event Sourcing.
- Redis.
- Message Brokers.
- Autenticación.
- JWT.
- Usuarios.
- Roles.
- Pagos.
- Notificaciones.
- Cloud infrastructure.
- DevOps avanzado.
- NgRx.
- Microfrontends.
- Otras abstracciones sin beneficio claro.

### Motivo

El tiempo sugerido de la prueba es de 4 a 6 horas. Introducir estas capacidades elevaría la
complejidad y reduciría el tiempo disponible para cumplir correctamente los requisitos evaluados.

Principio adoptado:

> **NO construir más. CONSTRUIR mejor.**

---

## 5. Modelo de datos

### Problema analizado

La aplicación requiere mostrar el estado actual de cada siniestro y además conservar el historial
de cambios de estado.

### Decisión

Se separó el estado actual del historial mediante dos entidades:

```text
Claims
ClaimStatusHistory
```

Relación:

```text
Claims 1 ───────── N ClaimStatusHistory
```

### Claims

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

### ClaimStatusHistory

```text
Id
ClaimId
PreviousStatus
NewStatus
ChangedAt
```

### Motivo

El historial representa una relación 1:N y debe poder consultarse, ordenarse y mantenerse de
forma estructurada. Se consideró más adecuado mantenerlo como entidad relacionada en SQL Server
que almacenarlo como una estructura JSON dentro del siniestro.

---

## 6. Estados y transiciones

### Ambigüedad detectada

La especificación define los estados, pero no define explícitamente las transiciones permitidas
entre ellos.

### Decisión

Se estableció el siguiente flujo simplificado:

```text
OPEN
  ↓
IN_REVIEW
  ↓
CLOSED
```

No se permiten:

```text
CLOSED → OPEN
CLOSED → IN_REVIEW
IN_REVIEW → OPEN
```

### Motivo

Se adoptó un flujo lineal sencillo, coherente con el escenario de una prueba técnica, sin
introducir un workflow complejo que no estuviera solicitado.

### Responsabilidad

El backend será la autoridad final para validar las transiciones. El frontend podrá limitar las
opciones mostradas al usuario, pero no se confiará en él para garantizar las reglas de negocio.

---

## 7. API REST

### Decisión

Se definieron inicialmente los siguientes endpoints:

```http
GET    /api/claims
GET    /api/claims/{id}
POST   /api/claims
PUT    /api/claims/{id}
DELETE /api/claims/{id}
PATCH  /api/claims/{id}/status
```

Para el listado se definió un filtro combinado como:

```http
GET /api/claims?status=OPEN&policyNumber=POL-001
```

### Motivo

Se separó el cambio de estado del CRUD general porque el cambio de estado implica reglas de
negocio y generación de historial.

---

## 8. Historial y cambio de estado

### Decisión

Cambiar el estado debe implicar dos operaciones relacionadas:

```text
Actualizar Claims.Status
+
Insertar ClaimStatusHistory
```

Estas operaciones deben ser consistentes y ejecutarse de forma atómica desde el backend.

### Flujo definido

```text
1. Verificar que exista el Claim.
2. Validar la transición.
3. Obtener el estado actual.
4. Actualizar el estado.
5. Crear el registro histórico.
6. Persistir ambas operaciones de forma atómica.
```

### Motivo

Se debe evitar un estado actual sin historial correspondiente o un historial que no represente el
estado realmente aplicado.

---

## 9. Responsabilidad de las validaciones

### Decisión

Las validaciones se implementarán en más de una capa, con responsabilidades distintas.

### Angular

Responsable de:

- Campos requeridos.
- Monto positivo.
- Feedback inmediato.
- Experiencia de usuario.

### Backend

Responsable de:

- Revalidar datos.
- Reglas de negocio.
- Transiciones de estado.
- Integridad de la operación independientemente del cliente.

### SQL Server

Responsable principalmente de la integridad persistente mediante constraints y relaciones.

### Motivo

El frontend no es una frontera de confianza. La API debe seguir siendo correcta aunque el cliente
sea manipulado o sustituido.

---

## 10. Arquitectura backend

### Decisión

Se propuso una arquitectura limpia y contenida:

```text
src/
 ├── Claims.Api
 ├── Claims.Application
 ├── Claims.Domain
 └── Claims.Infrastructure

tests/
 └── Claims.Tests
```

### Responsabilidades

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
- Reglas de dominio necesarias.

### Claims.Infrastructure

- DbContext.
- Configuración EF Core.
- Persistencia.
- SQL Server.

### Motivo

Se buscó separar responsabilidades sin convertir una aplicación pequeña en una arquitectura
sobredimensionada.

---

## 11. DTOs

### Decisión

No exponer directamente las entidades de Entity Framework Core como contratos HTTP.

Se definieron como mínimo:

```text
CreateClaimRequest
UpdateClaimRequest
ChangeClaimStatusRequest
ClaimResponse
ClaimStatusHistoryResponse
```

### Motivo

Desacoplar la persistencia del contrato público de la API y permitir que el modelo interno
evolucione sin forzar cambios directos en el consumidor.

---

## 12. Manejo de errores

### Decisión

Utilizar respuestas HTTP coherentes:

```text
200 OK
201 Created
204 No Content
400 Bad Request
404 Not Found
409 Conflict
500 Internal Server Error
```

Se propuso utilizar `ProblemDetails` cuando corresponda.

### Motivo

Mantener una API consistente y facilitar el manejo de errores desde Angular.

Se identificó `409 Conflict` como código apropiado para conflictos de negocio, por ejemplo una
transición de estado no permitida.

---

## 13. Frontend Angular

### Decisión

Se estableció Angular 19 utilizando:

- Standalone Components.
- TypeScript.
- RxJS.
- Reactive Forms.
- HttpClient.
- Angular Router.
- Signals cuando aporten valor.

### Estado

Se decidió no utilizar NgRx.

### Motivo

El alcance de la aplicación no justifica un gestor de estado global. Se prioriza una solución
simple y mantenible.

---

## 14. Organización del frontend

Se propuso una organización por funcionalidad:

```text
src/app/
├── core/
│   ├── services/
│   ├── interceptors/
│   └── models/
├── shared/
│   ├── components/
│   └── pipes/
└── features/
    └── claims/
        ├── pages/
        │   ├── claim-list/
        │   ├── claim-form/
        │   └── claim-detail/
        ├── components/
        │   ├── claim-filters/
        │   ├── claim-table/
        │   └── claim-status-history/
        ├── services/
        │   └── claims.service.ts
        └── models/
```

### Motivo

Mantener la funcionalidad de Claims agrupada y evitar componentes monolíticos sin fragmentar
innecesariamente la aplicación.

---

## 15. Flujo de construcción de la solución

### Decisión

Se estableció un orden de implementación para evitar dependencias y contratos inestables:

```text
FASE 1 — Diseño
        ↓
FASE 2 — SQL Server
        ↓
FASE 3 — .NET 8 API
        ↓
FASE 4 — Integración Backend + DB
        ↓
FASE 5 — Angular 19
        ↓
FASE 6 — Integración E2E
        ↓
FASE 7 — Testing
        ↓
FASE 8 — Documentación
        ↓
FASE 9 — Revisión final
```

### Motivo

La base de datos define el contrato de persistencia. Posteriormente el backend define el contrato
HTTP. Finalmente Angular consume ese contrato estable.

---

## 16. Arquitectura maestra

### Decisión

Se generaron cuatro prompts especializados:

```text
01-database.md
02-backend.md
03-frontend.md
04-context-architecture.md
```

Se estableció que:

```text
04-context-architecture.md
```

es el **prompt maestro / contexto arquitectónico principal**.

Jerarquía:

```text
04-context-architecture.md
        │
        ├── 01-database.md
        ├── 02-backend.md
        └── 03-frontend.md
```

### Motivo

Los prompts especializados deben servir como contextos de implementación y no convertirse en
autoridades independientes capaces de cambiar la arquitectura global.

---

## 17. Integración con OpenCode

### Decisión propuesta

Se recomendó utilizar un archivo:

```text
AGENTS.md
```

en la raíz del proyecto.

### Responsabilidad de AGENTS.md

El archivo debe indicar a OpenCode que:

- `04-context-architecture.md` es el contexto maestro.
- `01-database.md` es el contexto especializado de SQL Server.
- `02-backend.md` es el contexto especializado de .NET 8.
- `03-frontend.md` es el contexto especializado de Angular 19.

### Jerarquía

```text
AGENTS.md
    ↓
04-context-architecture.md
    ↓
01-database.md
02-backend.md
03-frontend.md
```

También se propuso que los cambios arquitectónicos no se realicen silenciosamente desde un prompt
especializado.

---

## 18. Uso crítico de IA

### Decisión

La IA se utilizará como herramienta de apoyo, no como autoridad técnica.

Flujo esperado:

```text
Prompt
  ↓
IA propone
  ↓
Analizar
  ↓
Validar contra requisitos
  ↓
Aceptar / Modificar / Rechazar
  ↓
Implementar
  ↓
Probar
```

Nunca se debe asumir que una propuesta es correcta únicamente porque:

- Compila.
- Parece sofisticada.
- Utiliza un patrón conocido.
- La IA la presenta como buena práctica.

---

## 19. Control de alcance durante el uso de IA

### Decisión

Las sugerencias de IA deben evaluarse considerando:

```text
Correctness
Maintainability
Simplicity
Security
Testability
Performance
Consistency
```

Ante dos soluciones válidas se priorizará la que sea:

1. Más sencilla.
2. Más mantenible.
3. Más fácil de probar.
4. Suficiente para los requisitos.
5. Más fácil de explicar en entrevista.

---

## 20. AI_LOG como parte de la entrega

La prueba técnica exige un log de uso de IA donde se muestre:

- Prompts utilizados.
- Qué propuso la IA.
- Qué se corrigió.
- Qué se rechazó.
- Por qué se tomó cada decisión.

Por ese motivo, este documento se diseñó para servir como registro del proceso de definición
asistida por IA.

La **Parte 2** de este documento contiene las entradas de la implementación real, con evidencia
de las interacciones reales utilizadas para producir el código.

**Importante:** la Parte 1 registra las decisiones ocurridas durante la conversación de
definición. No se inventó historial de prompts de una herramienta que todavía no se había
utilizado; las interacciones reales quedan registradas en la Parte 2.

---

## 21. Git

### Decisión

Se recomendó un historial de commits incremental y significativo.

Ejemplo:

```text
chore: initialize solution
feat: create database schema
feat: add claim persistence
feat: implement claims api
feat: add claim validation
feat: implement status history
test: add claim business rules
feat: initialize angular application
feat: implement claims list
feat: add claim filters
feat: implement claim form
feat: implement claim detail
feat: implement status transition
test: add frontend tests
fix: handle invalid status transition
docs: add README
docs: add AI usage log
```

### Motivo

El historial de commits es parte de la evaluación de la prueba y debe mostrar evolución real del
proyecto.

---

## 22. Testing

### Backend

Se recomendaron pruebas para reglas importantes como:

```text
CreateClaim_WithInvalidAmount_ShouldFail
CreateClaim_WithMissingPolicyNumber_ShouldFail
ChangeStatus_OpenToInReview_ShouldSucceed
ChangeStatus_InReviewToClosed_ShouldSucceed
ChangeStatus_ClosedToOpen_ShouldFail
ChangeStatus_ShouldCreateHistory
GetClaims_WithStatusFilter_ShouldReturnFilteredResults
```

### Frontend

Se recomendaron pruebas para:

```text
ClaimList renders claims
Filter by status
Filter by policy number
Claim form validates required fields
Claim form rejects amount <= 0
Create claim
Display claim detail
Display status history
Handle API error
```

### Motivo

Priorizar pruebas de comportamiento y reglas de negocio por encima de una cobertura
artificialmente alta.

---

## 23. Documentación final

Se definió que el repositorio debe contener como mínimo:

```text
README.md
AI_LOG.md
```

El README debe cubrir:

- Descripción.
- Arquitectura.
- Tecnologías.
- Prerrequisitos.
- Configuración de SQL Server.
- Configuración del backend.
- Configuración del frontend.
- Ejecución.
- Tests.
- Endpoints.
- Decisiones relevantes.

---

## 24. Estado del proceso documentado

Hasta este punto, la conversación de definición ha revisado:

- Stack tecnológico.
- Interpretación de la prueba.
- Alcance funcional.
- Modelo relacional.
- Reglas de transición.
- Diseño API.
- Responsabilidades por capa.
- Arquitectura backend.
- Arquitectura frontend.
- Orden de construcción.
- Estrategia de testing.
- Estrategia Git.
- Uso crítico de IA.
- Estructura de prompts.
- Prompt maestro.
- Integración conceptual con OpenCode mediante `AGENTS.md`.

La implementación efectiva del código registra sus propias interacciones reales con la IA en la
Parte 2.

---

## 25. Regla final del proceso

La solución debe mantenerse alineada con el principio:

> **Construir lo necesario, revisar lo generado y mantener el control técnico humano.**

Cualquier futura modificación de arquitectura debe quedar registrada aquí cuando represente una
decisión relevante, indicando:

- Problema.
- Propuesta de IA.
- Evaluación.
- Decisión.
- Motivo.
- Resultado.

---

# Parte 2 — Implementación con OpenCode

Contexto: desarrollo real de la solución asistido por el agente de desarrollo OpenCode. Cada
entrada refleja una propuesta, decisión o corrección real del proceso.

## 1. Formato de la solución: `.slnx`

### Objetivo
Crear la solución con Visual Studio 2026 manteniendo el formato moderno.

### Propuesta de IA
`dotnet new sln` con el SDK 10 genera el formato XML `.slnx` (nuevo).

### Decisión
Aceptada.

### Motivo
El formato `.slnx` es el nativo del SDK 10 y es compatible con Visual Studio 2026;
no se fuerza la conversión al `.sln` clásico.

### Resultado
`Prueba_Zurich.slnx` con carpetas Backend/Database/Frontend y los cinco proyectos .NET
referenciados.

---

## 2. Proveedor de esquema del proyecto de base de datos

### Objetivo
Que `ClaimsDatabase.sqlproj` (SDK `Microsoft.Build.Sql`) compile correctamente.

### Propuesta de IA
Usar el DSP genérico `SqlDatabaseSchemaProvider` que genera la plantilla por defecto.

### Corrección realizada
Cambiar a `Microsoft.Data.Tools.Schema.Sql.Sql160DatabaseSchemaProvider`.

### Motivo
El DSP genérico no es válido para el modelo de compilación del SDK `Microsoft.Build.Sql`;
el proveedor 160 es el que corresponde a SQL Server 2022.

### Resultado
El proyecto genera `ClaimsDatabase.dacpac` sin errores.

---

## 3. Scripts T-SQL y construcción del dacpac

### Objetivo
Vincular los scripts 01-05 al proyecto para que el dacpac contenga todo el esquema.

### Propuesta de IA
Marcar los cinco scripts como `Build` con DDL condicional (`IF NOT EXISTS`) y sin separadores `GO`.

### Corrección realizada
- Quitar el DDL condicional (debe crearse una vez, con lógica idempotente en el seed).
- Añadir separadores `GO`.
- `01_create_database.sql` excluido del `Build` (la creación de la base no pertenece al dacpac).
- `05_seed_data.sql` como `PostDeploy` para sembrar al publicar.

### Motivo
El SDK `Microsoft.Build.Sql` no tolera `CREATE` condicional ni ejecuta bien el script sin `GO`.

### Resultado
`dotnet build database/ClaimsDatabase.sqlproj` compila y el dacpac despliega `ClaimsDb`
con 10 siniestros sembrados.

---

## 4. Historial de estados como tabla, no como JSON

### Objetivo
Persistir el historial de cambios de estado del siniestro.

### Propuesta de IA (evaluada)
Almacenar el historial como columna JSON dentro de `Claims`.

### Decisión
Rechazada.

### Motivo
El historial es una relación 1:N que debe consultarse de forma independiente; guardarlo
como JSON dificultaría las consultas y rompería la integridad referencial.

### Solución
Tabla `ClaimStatusHistory` (`ClaimId` FK con `ON DELETE CASCADE`) y registro inicial
(`PreviousStatus = NULL`, `NewStatus = OPEN`) creado junto con la póliza.

### Resultado
`GET /api/claims/{id}/history` devuelve la evolución completa del siniestro.

---

## 5. Cambio de estado atómico ante concurrencia

### Objetivo
Evitar transiciones inválidas cuando dos peticiones cambian el estado a la vez.

### Propuesta de IA
Validar la transición únicamente en el servicio con el valor recibido.

### Corrección realizada
En `ClaimsRepository.ChangeStatusAsync` se abre una transacción, se relee la fila con
`WITH (UPDLOCK)` dentro de la misma y se revalida el estado actual contra el esperado.

### Motivo
La validación en memoria no protege contra escrituras concurrentes; con `UPDLOCK` la fila
queda bloqueada hasta confirmar, garantizando atomicidad.

### Resultado
Transiciones concurrentes inválidas devuelven `409 Conflict`; el flujo normal sigue
funcionando.

---

## 6. Aceptar valores de enum como strings en la API

### Objetivo
Que el frontend envíe `newStatus: "IN_REVIEW"` y el modelo en minúsculas sea rechazado por validación.

### Síntoma
El `PATCH /api/claims/{id}/status` devolvía `400` con `newStatus` serializado como número.

### Corrección realizada
Registrar `JsonStringEnumConverter` en la configuración JSON de la API.

### Motivo
Por defecto .NET serializa los enums como números; el frontend envía textos.

### Resultado
`PATCH { "newStatus": "IN_REVIEW" }` devuelve `200`; estados inválidos devuelven `400` con detalle.

---

## 7. DTOs como records

### Objetivo
Simplificar las pruebas y evitar DTOs mutables innecesarios.

### Propuesta de IA
DTOs como `record` en `Claims.Application/DTOs`.

### Decisión
Aceptada (ajuste realizado por revisión de las pruebas).

### Motivo
Los records permiten comparar instancias por valor y usar la sintaxis `with { }` en los tests.

### Resultado
`ClaimServiceTests` y `ClaimsRepositoryTests` (SQLite) compilan y pasan: 22/22.

---

## 8. CSS personalizado en lugar de Angular Material

### Objetivo
Definir la UI del frontend.

### Propuesta de IA
Instalar Angular Material para acelerar los componentes.

### Decisión
Rechazada (por decisión del usuario).

### Motivo
Se prefirió SCSS personalizado: menos dependencias, control total del estilo y una base
más sencilla de explicar.

### Resultado
Design system en `styles.scss` (variables, botones, cards, tablas, formularios, alertas)
usado por todos los componentes.

---

## 9. `fileReplacements` del entorno en la configuración correcta

### Objetivo
Que el frontend apunte a `http://localhost:5000/api` al ejecutar `ng serve`.

### Síntoma
`environment.apiUrl` usaba el valor por defecto de producción; los tests HTTP fallaban al
buscar la URL correcta.

### Corrección realizada
Añadir `fileReplacements` en la configuración `development` de `angular.json` (no en production).

### Motivo
`ng serve` usa la configuración `development`; sin el reemplazo, el archivo
`environment.development.ts` nunca se aplica.

### Resultado
`ng build`/`ng serve` en development usan `apiUrl = http://localhost:5000/api`.

---

## 10. `as` solo en el bloque primario de `@if`

### Objetivo
Renderizar estados de carga, error y vacío en listado y detalle.

### Síntoma
El build fallaba con `NG5002: "as" expression is only allowed on the primary @if block`
y `NG9: Property 'err' does not exist`.

### Corrección realizada
Eliminar los alias `as` de los bloques `@else if`; usar la expresión directamente
(`{{ error() }}`) o anidar un `@if` primario con alias.

### Motivo
Angular 19 solo permite alias de variable en el bloque `@if` principal de la estructura.

### Resultado
`ng build` compila sin errores.

---

## 11. Tests del frontend con `RouterTestingHarness`

### Objetivo
Probar las páginas enrutadas (listado, formulario, detalle) contra `HttpTestingController`.

### Síntoma
`ng test` fallaba al compilar: `Property 'navigateTo' does not exist on type 'RouterTestingHarness'`.

### Corrección realizada
Reemplazar `harness.navigateTo(...)` por `await harness.navigateByUrl(url, Componente)`.

### Motivo
El método `navigateTo` no existe en Angular 19; la API vigente es `navigateByUrl`.

### Resultado
Los tests de páginas compilan y pasan.

---

## 12. Falta de `NgFor` en `ClaimFiltersComponent`

### Objetivo
Renderizar las opciones de estado del selector de filtros.

### Síntoma
`ng test` fallaba en runtime con `NG0303: Can't bind to 'ngForOf' since it isn't a known property`.

### Corrección realizada
Importar `NgFor` de `@angular/common` en `imports` del componente.

### Motivo
El template usa `*ngFor` sobre las opciones, pero el componente solo importaba `FormsModule`.

### Resultado
Los 33 tests del frontend pasan en verde (Chrome headless).

---

## 13. Commit inicial y semántica de git

### Objetivo
Mostrar evolución real del repositorio con commits pequeños y semánticos.

### Propuesta de IA
Un único commit final con todo el código.

### Decisión
Rechazada.

### Motivo
El contexto arquitectónico exige commits incrementales que representen trabajo real
(`chore:`, `feat:`, `test:`, `fix:`, `docs:`).

### Resultado
Historial con commits por fase: solución, esquema, seed, dominio, persistencia, API,
tests, correcciones de enum, frontend y documentación.

---

## 14. Compilar el proyecto de base de datos con dotnet y Visual Studio 2026

### Síntoma
`dotnet build database/ClaimsDatabase.sqlproj` fallaba con
`NETSDK1005: Assets file ... doesn't have a target for 'net472'` al alternar el build entre
`dotnet` y Visual Studio 2026.

### Causa
`dotnet` (MSBuild Core) restaura el sqlproj con `netstandard2.1`, mientras que Visual Studio
2026 (MSBuild .NET Framework) lo hace con `net472`; cada herramienta sobreescribía
`database/obj/project.assets.json`. El SDK `Microsoft.Build.Sql` vacía `TargetFrameworks`,
por lo que no hay multi-targeting que lo resuelva.

### Corrección realizada
Fijar `<TargetFramework>netstandard2.1</TargetFramework>` en `ClaimsDatabase.sqlproj`.

### Resultado
El dacpac se genera correctamente desde `dotnet build` y desde Visual Studio 2026.

---

## 15. Compilar el sqlproj desde los targets SSDT de Visual Studio

### Síntoma
Compilando el sqlproj desde la IDE de Visual Studio 2026 fallaba `MSB4063/MSB4064`:
`AutomaticIndexCompaction` no soportado por la DAC instalada por VS (v170.4.53.0) frente a la
del paquete NuGet (v170.4.83.3).

### Causa
La IDE fija `SSDTPath` a su DAC; los targets `net472` del paquete recomputan
`SqlServerRedistPath = SSDTPath` y pisaban el `SqlServerRedistPath` configurado en el proyecto.

### Corrección realizada
`<BuildFromSSDT>true</BuildFromSSDT>` en el sqlproj: en builds full-framework se usan los
targets SSDT de Visual Studio, consistentes con su DAC. `dotnet build` no se ve afectado.

### Resultado
Matriz verificada (dotnet, VS por defecto, VS con `SSDTPath`, VS con `BuildFromSSDT=true`):
el dacpac se genera en todos los casos.

---

## 16. La API corriendo en segundo plano bloqueaba el build

### Síntoma
Al compilar `Claims.Api`, el copy de las DLL fallaba:
`Exceeded retry count of 10. The file is locked by "Claims.Api"`.

### Corrección realizada
Detener el proceso de la API antes de compilar (`Stop-Process` de `Claims.Api.exe`/`dotnet.exe`)
y relanzarla tras el build.

### Motivo
La API en ejecución mantiene bloqueadas las DLL de salida en `bin\Debug\net8.0\`.

### Resultado
Build sin errores; se deja la API detenida mientras se compila para evitar bloqueos.

---

## 17. CORS para cualquier puerto de localhost

### Síntoma
La SPA Angular en `http://localhost:52905` era bloqueada por CORS:
`No 'Access-Control-Allow-Origin' header is present`.

### Corrección realizada
En `Program.cs`, la política `Frontend` pasó de `WithOrigins("http://localhost:4200")` a
`SetIsOriginAllowed`, permitiendo cualquier origen `http://localhost:*` o `http://127.0.0.1:*`.

### Motivo
El puerto de `ng serve` no es fijo; restringirlo a uno solo rompía la SPA cuando Angular se
servía en otro puerto.

### Resultado
Verificado: `Access-Control-Allow-Origin: http://localhost:52905`, status 200. README
actualizado.

---

## 18. Estrategia de ramas y push a GitHub

### Objetivo
Publicar el repositorio en GitHub con una cadena de ramas por entorno y llevar los cambios de la
rama de trabajo hasta `main`.

### Decisión
- Remoto `origin` apuntando a `https://github.com/Firos07/Zurich_Test.git`.
- Ramas en cadena: `preprod` (de `main`), `qa` (de `preprod`), `dev` (de `qa`).
- Rama local `local-branch-Zurich` (de `dev`) donde se commitearon los cambios pendientes.
- Propagación fast-forward por la cadena `dev → qa → preprod → main`.

### Resultado
Las 5 ramas en `origin` apuntan al mismo commit final; working tree limpio.

---

## 19. Autenticación contra GitHub con Personal Access Token

### Síntoma
El push fallaba: `Invalid username or token. Password authentication is not supported`.

### Corrección realizada
Generar un PAT (fine-grained) con permiso `Contents: Read and write` sobre el repositorio y
guardarlo en el credential manager de Windows (`git credential approve`). El primer PAT carecía
de permiso de escritura (`403 Permission denied`) y se reemplazó por uno con el alcance correcto.

### Resultado
Pushes exitosos; el token queda cacheado para futuras operaciones.

---

## 20. Push protection: secreto en la configuración

### Síntoma
GitHub rechazó el push de `local-branch-Zurich` por *push protection*: el commit contenía una
**GCP API key** en `opencode.json` (cabecera `X-Goog-Api-Key`).

### Corrección realizada
- Reemplazar la key por `{env:GOOGLE_API_KEY}`.
- Enmendar el commit local (aún no pusheado).
- Ignorar `.opencode/` y `opencode.json` en `.gitignore` y des-trackearlos con `git rm --cached`.

### Resultado
Push exitoso; el secreto nunca llegó al remoto y quedó fuera del control de versiones.