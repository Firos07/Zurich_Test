# PROMPT 03 — FRONTEND ANGULAR 19

## Rol

Actúa como **Senior Frontend Developer / Angular Architect**, especializado en Angular 19, TypeScript, RxJS, formularios reactivos, consumo de APIs REST, manejo de estado y diseño de interfaces empresariales.

Debes desarrollar exclusivamente el **frontend** de una aplicación denominada:

**Gestor de Siniestros Simplificado**

El backend será:

```text
C#
.NET 8
ASP.NET Core Web API
```

La persistencia será:

```text
SQL Server
```

Tu aplicación Angular debe consumir exclusivamente la API REST.

---

# 1. Objetivo

Construir una interfaz web que permita:

- Listar siniestros.
- Filtrar por estado.
- Buscar por número de póliza.
- Crear siniestros.
- Editar siniestros.
- Eliminar siniestros.
- Consultar detalle.
- Consultar historial de estados.
- Cambiar el estado de un siniestro.

---

# 2. Stack obligatorio

Utiliza:

```text
Angular 19
TypeScript
RxJS
Angular Router
Reactive Forms
HttpClient
```

Utiliza componentes standalone.

No utilices una arquitectura basada en NgModules salvo que Angular lo requiera explícitamente.

---

# 3. Estructura

Utiliza una estructura organizada por funcionalidad.

Una estructura sugerida:

```text
src/app/

├── core/
│   ├── services/
│   ├── interceptors/
│   └── models/
│
├── shared/
│   ├── components/
│   └── pipes/
│
└── features/
    └── claims/
        ├── pages/
        │   ├── claim-list/
        │   ├── claim-form/
        │   └── claim-detail/
        │
        ├── components/
        │   ├── claim-filters/
        │   ├── claim-table/
        │   └── claim-status-history/
        │
        ├── services/
        │   └── claims.service.ts
        │
        └── models/
```

No generes carpetas vacías o capas que no tengan responsabilidad real.

---

# 4. Rutas

Implementa como mínimo:

```text
/claims
/claims/new
/claims/:id
/claims/:id/edit
```

La ruta `/claims` debe ser la pantalla principal.

---

# 5. Listado

La pantalla principal debe mostrar:

```text
Número de póliza
Asegurado
Tipo de siniestro
Monto estimado
Estado
Fecha de creación
Acciones
```

Debe permitir:

- Ver detalle.
- Editar.
- Eliminar.
- Cambiar estado cuando corresponda.

---

# 6. Filtros

Implementa:

```text
Estado
Número de póliza
```

El usuario debe poder:

```text
Filtrar solamente por estado.

Buscar solamente por póliza.

Utilizar ambos filtros simultáneamente.

Limpiar filtros.
```

Evita llamadas innecesarias a la API cuando los valores de búsqueda no hayan cambiado.

---

# 7. Formulario

El formulario debe permitir:

```text
Número de póliza
Asegurado
Tipo de siniestro
Monto estimado
```

El estado inicial de un nuevo siniestro debe ser:

```text
OPEN
```

El usuario no debe poder establecer arbitrariamente un estado inicial inválido.

---

# 8. Validaciones Angular

Implementa Reactive Forms.

Validaciones mínimas:

```text
PolicyNumber → required

InsuredName → required

ClaimType → required

EstimatedAmount → required + > 0
```

Muestra mensajes de validación claros.

No permitas enviar formularios inválidos.

---

# 9. Detalle

La pantalla de detalle debe mostrar:

```text
Información del siniestro
────────────────────────────

Número de póliza
Asegurado
Tipo
Monto
Estado
Fecha de creación
Última modificación
```

Y debajo:

```text
Historial de estados
────────────────────────────

OPEN
   ↓
IN_REVIEW
   ↓
CLOSED
```

Debe mostrar al menos:

```text
Estado anterior
Estado nuevo
Fecha
```

---

# 10. Cambio de estado

El frontend debe respetar las reglas:

```text
OPEN → IN_REVIEW
IN_REVIEW → CLOSED
```

No debe presentar opciones inválidas al usuario.

Sin embargo, el frontend nunca debe considerarse responsable de garantizar la regla de negocio.

El backend es la autoridad final.

Si el backend devuelve:

```text
409 Conflict
```

debe mostrarse un mensaje apropiado al usuario.

---

# 11. Servicio HTTP

Centraliza el acceso a la API en:

```text
ClaimsService
```

No hagas llamadas HTTP directamente desde los componentes.

El servicio debe encapsular:

```text
getClaims()
getClaim(id)
createClaim()
updateClaim()
deleteClaim()
changeStatus()
getStatusHistory()
```

---

# 12. Modelos

Define interfaces/types para los contratos de API.

Por ejemplo conceptualmente:

```text
Claim
CreateClaimRequest
UpdateClaimRequest
ChangeClaimStatusRequest
ClaimStatusHistory
```

No utilices `any` salvo casos realmente justificados.

---

# 13. Estado

No agregues NgRx para esta aplicación.

El alcance no lo justifica.

Utiliza:

- Signals cuando simplifiquen el estado local.
- RxJS para operaciones HTTP y streams.
- Servicios para lógica compartida.

Mantén el manejo de estado sencillo.

---

# 14. UX

La interfaz debe comunicar claramente:

```text
Loading
Success
Validation errors
API errors
Empty results
Delete confirmation
Status changes
```

Ejemplos:

```text
Cargando siniestros...

No se encontraron siniestros.

El siniestro fue creado correctamente.

No fue posible eliminar el siniestro.

La transición de estado no es válida.
```

---

# 15. Eliminación

Antes de eliminar un siniestro solicita confirmación.

Después de eliminar correctamente:

- Actualiza el listado.
- Muestra confirmación.
- Maneja errores.

No elimines silenciosamente.

---

# 16. Manejo de errores HTTP

Maneja como mínimo:

```text
400 Bad Request
404 Not Found
409 Conflict
500 Internal Server Error
```

No muestres mensajes técnicos directamente al usuario.

Por ejemplo, evita mostrar:

```text
SqlException: Violation of FK...
```

En su lugar:

```text
No fue posible completar la operación.
```

o un mensaje específico cuando sea seguro hacerlo.

---

# 17. Interceptor

Puedes implementar un interceptor HTTP para responsabilidades globales.

Por ejemplo:

- Manejo común de errores.
- Logging en desarrollo.

No implementes autenticación porque no forma parte de la prueba.

---

# 18. Diseño visual

La UI debe ser:

- Clara.
- Profesional.
- Simple.
- Consistente.
- Responsive a nivel razonable.

Puedes utilizar Angular Material.

Prioriza:

```text
Usabilidad
Legibilidad
Consistencia
```

No dediques tiempo excesivo a animaciones o efectos visuales.

---

# 19. Componentización

Evita crear un único componente gigante.

El listado, filtros e historial deben estar razonablemente separados.

Pero tampoco fragmentes excesivamente la aplicación.

La regla es:

> Crear un componente cuando tenga una responsabilidad clara y reutilizable o cuando mejore significativamente la legibilidad.

---

# 20. Seguridad

No implementes:

- Login.
- Registro.
- JWT.
- Roles.
- Permisos.

No forman parte de esta prueba.

Aun así:

- No confíes en validaciones frontend.
- No almacenes secretos.
- No hardcodees credenciales.
- No expongas información sensible.

---

# 21. Testing

Implementa pruebas para los comportamientos principales.

Como mínimo:

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

No es necesario conseguir una cobertura artificialmente alta.

Prioriza comportamiento relevante.

---

# 22. Configuración de API

No hardcodees la URL de producción dentro de los servicios.

Utiliza configuración por environment.

Por ejemplo:

```text
environment.ts
environment.development.ts
```

La URL de la API debe poder cambiarse sin modificar los servicios.

---

# 23. No implementar

NO agregues:

- NgRx.
- Microfrontends.
- Autenticación.
- JWT.
- Roles.
- WebSockets.
- PWA.
- Internacionalización compleja.
- Librerías innecesarias.
- Arquitecturas excesivamente complejas.

---

# 24. Criterios de aceptación

El frontend debe:

- Ejecutarse correctamente con Angular 19.
- Consumir la API .NET 8.
- Mostrar listado.
- Filtrar por estado.
- Buscar por póliza.
- Crear siniestros.
- Editar siniestros.
- Eliminar siniestros.
- Mostrar detalle.
- Mostrar historial.
- Cambiar estados válidos.
- Mostrar errores.
- Validar formularios.
- Manejar loading/empty/error states.
- Mantener una estructura mantenible.
- Utilizar TypeScript fuertemente tipado.

---

# 25. Principio de diseño

La prioridad es:

```text
Usabilidad
↓
Correctness
↓
Maintainability
↓
Type Safety
↓
Performance
↓
Simplicity
```

No agregues complejidad únicamente para demostrar conocimiento de Angular.

Antes de implementar, analiza el contrato de API y detecta inconsistencias.

Si el backend y el frontend presentan una discrepancia, identifica claramente el problema en lugar de ocultarlo mediante hacks en el frontend.

No agregues funcionalidades fuera del alcance.