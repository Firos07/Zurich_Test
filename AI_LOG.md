# AI_LOG.md

Registro de uso crítico de la IA durante el desarrollo del **Gestor de Siniestros
Simplificado**. Cada entrada refleja una propuesta, decisión o corrección real del proceso.

---

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