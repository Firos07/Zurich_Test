# Gestor de Siniestros Simplificado

Aplicación fullstack para registrar y dar seguimiento a siniestros de seguros, con un
flujo de trabajo de estados `OPEN → IN_REVIEW → CLOSED`.

## Arquitectura

```text
Usuario
   ↓
Angular 19 (frontend/claims-app)      →  http://localhost:4200
   ↓  HTTP / REST  (JSON)
.NET 8 Web API (src/Claims.Api)       →  http://localhost:5000
   ↓  Entity Framework Core 8
SQL Server (ClaimsDb)                 →  localhost\SQLEXPRESS
```

El backend se estructura en capas dentro de la solución `Prueba_Zurich.slnx`:

- `Claims.Domain` — entidades, enum de estados y regla de transiciones.
- `Claims.Application` — DTOs, excepciones, contratos y servicio con las reglas de negocio.
- `Claims.Infrastructure` — EF Core 8 (`ClaimsDbContext`), configuraciones y repositorio.
- `Claims.Api` — controladores REST, middleware de errores (ProblemDetails), CORS y Swagger.
- `Claims.Tests` — pruebas de las reglas de negocio y del repositorio.

La base de datos se versiona como proyecto SQL Server Data Tools
(`database/ClaimsDatabase.sqlproj`) con scripts T-SQL del 01 al 05.

## Tecnologías

| Capa      | Tecnología                                            |
|-----------|-------------------------------------------------------|
| Frontend  | Angular 19, TypeScript, SCSS, standalone components, signals, Reactive Forms, RxJS, Karma |
| Backend   | C# / .NET 8, ASP.NET Core Web API, Entity Framework Core 8 |
| Database  | SQL Server 2022, T-SQL, SQL Server Data Tools (`Microsoft.Build.Sql`) |

## Prerequisitos

- [.NET SDK 10.x](https://dotnet.microsoft.com/) (permite compilar proyectos `net8.0` con runtime 8.0 instalado).
- Runtime .NET 8.0.
- [Node.js 22](https://nodejs.org/) y Angular CLI (`npm install -g @angular/cli@19`).
- [SQL Server 2022 Express](https://www.microsoft.com/sql-server/sql-server-downloads) (instancia `localhost\SQLEXPRESS`, autenticación de Windows).
- Git.

> El archivo `global.json` fija el SDK 10.0.302 con `rollForward: latestMajor`.

## Configuración de SQL Server

1. Verificar la instancia:

```powershell
sqlcmd -S localhost\SQLEXPRESS -E -Q "SELECT @@VERSION"
```

2. Crear la base de datos y las tablas. Existen dos vías:

**Opción A — scripts (recomendada):**

```powershell
sqlcmd -S localhost\SQLEXPRESS -E -i database/scripts/01_create_database.sql
sqlcmd -S localhost\SQLEXPRESS -E -i database/scripts/02_create_schema.sql
sqlcmd -S localhost\SQLEXPRESS -E -i database/scripts/03_seed_statuses.sql
sqlcmd -S localhost\SQLEXPRESS -E -i database/scripts/04_create_indexes.sql
sqlcmd -S localhost\SQLEXPRESS -E -i database/scripts/05_seed_data.sql
```

**Opción B — dacpac:**

```powershell
dotnet build database/ClaimsDatabase.sqlproj
# Publicar database/bin/Debug/ClaimsDatabase.dacpac contra localhost\SQLEXPRESS
```

La seed data es idempotente: inserta 10 siniestros de ejemplo (POL-1001 a POL-1010)
junto con su historial de estados inicial. Si una póliza ya existe, la deja intacta.

## Configuración del Backend

1. La cadena de conexión vive en `src/Claims.Api/appsettings.Development.json`:

```json
"ConnectionStrings": {
  "Default": "Server=localhost\\SQLEXPRESS;Database=ClaimsDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

2. Ejecutar la API:

```powershell
dotnet run --project src/Claims.Api --urls http://localhost:5000
```

- Swagger disponible en `http://localhost:5000/swagger`.
- La API habilita CORS únicamente para `http://localhost:4200`.

## Configuración del Frontend

```powershell
cd frontend/claims-app
npm install
ng serve
```

- Aplicación en `http://localhost:4200`.
- La URL de la API se define en `src/environments/environment.development.ts`
  (`apiUrl = 'http://localhost:5000/api'`) y se sustituye en la configuración
  `development` de `angular.json` mediante `fileReplacements`.

## Cómo ejecutar los tests

**Backend:**

```powershell
dotnet test Prueba_Zurich.slnx
```

**Frontend (Chrome headless):**

```powershell
cd frontend/claims-app
ng test --watch=false --browsers=ChromeHeadless
```

**Verificación de la solución completa:**

```powershell
dotnet build Prueba_Zurich.slnx
```

## Endpoints principales

| Método | Ruta                          | Descripción                              |
|--------|-------------------------------|------------------------------------------|
| GET    | `/api/claims`                 | Lista con filtros `?status=&policyNumber=` |
| GET    | `/api/claims/{id}`            | Detalle de un siniestro                  |
| POST   | `/api/claims`                 | Crea un siniestro (estado inicial `OPEN`) |
| PUT    | `/api/claims/{id}`            | Actualiza los datos del siniestro        |
| DELETE | `/api/claims/{id}`            | Elimina un siniestro (y su historial)    |
| PATCH  | `/api/claims/{id}/status`     | Cambia el estado `{ "newStatus": "IN_REVIEW" }` |
| GET    | `/api/claims/{id}/history`    | Historial de cambios de estado           |

Errores: validación → `400` ProblemDetails con `detail` por campo;
no encontrado → `404`; transición inválida o conflicto de concurrencia → `409`;
error interno → `500`.

## Decisiones relevantes

- **Transiciones de estado**: la regla `OPEN → IN_REVIEW → CLOSED` está centralizada en
  `ClaimStatusTransitions` (Domain) y se revalida en el backend, autoridad final.
- **Historial**: `ClaimStatusHistory` es una tabla 1:N con `ON DELETE CASCADE`; el
  registro inicial (null → OPEN) se crea con la póliza.
- **Concurrencia en el cambio de estado**: el repositorio relee la fila con `WITH (UPDLOCK)`
  dentro de una transacción y valida el estado esperado; ante un conflicto devuelve `409`.
- **Enum como strings**: la API registra `JsonStringEnumConverter` para aceptar
  `newStatus: "IN_REVIEW"` en el JSON.
- **`PolicyNumber` sin `UNIQUE`**: permite que una misma póliza tenga varios siniestros;
  la búsqueda por póliza devuelve todas las coincidencias.
- **Frontend sin librería UI**: SCSS personalizado con un design system en `styles.scss`
  (variables de color, botones, cards, tablas y formularios).
- **Formato de solución `.slnx`**: generado por el SDK 10, compatible con Visual Studio 2026.