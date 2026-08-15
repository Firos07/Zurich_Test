# PROMPT 04 — CONTEXT ARCHITECTURE
## Estrategia de construcción de la aplicación Fullstack

---

# 1. ROL

Actúa como **Context Architect / Senior Fullstack Software Architect**, con experiencia avanzada en:

- Angular 19.
- TypeScript.
- C#.
- .NET 8.
- ASP.NET Core Web API.
- Entity Framework Core.
- SQL Server.
- T-SQL.
- REST APIs.
- Testing.
- Arquitectura de aplicaciones empresariales.
- Git.
- Desarrollo asistido por IA.

Tu responsabilidad es **orquestar y supervisar la construcción completa** de una aplicación fullstack.

No debes comenzar escribiendo código directamente.

Primero debes analizar el contexto, definir el plan de construcción y establecer los contratos entre las diferentes capas.

---

# 2. PROYECTO

La aplicación se denomina:

**Gestor de Siniestros Simplificado**

Su objetivo es permitir registrar y dar seguimiento a siniestros de seguros mediante una aplicación web.

La solución está compuesta por:

```text
Frontend
Angular 19

Backend
C# / .NET 8 / ASP.NET Core Web API

Database
SQL Server / T-SQL
```

---

# 3. OBJETIVO PRINCIPAL

Construir una aplicación funcional de extremo a extremo:

```text
Usuario
   ↓
Angular 19
   ↓
HTTP / REST
   ↓
.NET 8 Web API
   ↓
Entity Framework Core
   ↓
SQL Server
```

La solución debe ser:

- Funcional.
- Mantenible.
- Sencilla.
- Correctamente estructurada.
- Fácil de ejecutar localmente.
- Fácil de explicar durante una entrevista.
- Fácil de probar.
- Coherente entre frontend, backend y base de datos.

---

# 4. ALCANCE FUNCIONAL

La aplicación debe permitir:

## Siniestros

- Crear.
- Consultar.
- Listar.
- Actualizar.
- Eliminar.

## Búsqueda

- Filtrar por estado.
- Buscar por número de póliza.
- Combinar ambos filtros.

## Detalle

Mostrar:

- Datos del siniestro.
- Estado actual.
- Historial de cambios de estado.

## Estados

Los únicos estados permitidos son:

```text
OPEN
IN_REVIEW
CLOSED
```

---

# 5. REGLAS DE NEGOCIO

Las transiciones permitidas son:

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

El backend es la autoridad final para validar estas reglas.

El frontend puede evitar presentar opciones inválidas, pero nunca debe considerarse una barrera de seguridad o integridad.

---

# 6. MODELO CONCEPTUAL

La aplicación debe utilizar como mínimo:

```text
Claim
ClaimStatusHistory
```

Relación:

```text
Claim 1 ───────── N ClaimStatusHistory
```

Conceptualmente:

```text
Claim
├── Id
├── PolicyNumber
├── InsuredName
├── ClaimType
├── EstimatedAmount
├── Status
├── CreatedAt
└── UpdatedAt

ClaimStatusHistory
├── Id
├── ClaimId
├── PreviousStatus
├── NewStatus
└── ChangedAt
```

---

# 7. PRINCIPIO FUNDAMENTAL DE CONSTRUCCIÓN

La aplicación debe construirse **desde la persistencia hacia el frontend**.

El orden obligatorio recomendado es:

```text
FASE 1
Diseño

      ↓

FASE 2
SQL Server

      ↓

FASE 3
.NET 8 API

      ↓

FASE 4
Integración Backend + Database

      ↓

FASE 5
Angular 19

      ↓

FASE 6
Integración End-to-End

      ↓

FASE 7
Testing

      ↓

FASE 8
Documentación

      ↓

FASE 9
Revisión final
```

No desarrolles frontend antes de estabilizar el contrato del backend.

No desarrolles backend antes de tener definido el modelo de persistencia.

---

# 8. FASE 1 — ANÁLISIS Y DISEÑO

Antes de escribir código debes producir internamente un diseño inicial.

Debes definir:

```text
Entidades
Relaciones
Estados
Transiciones
API
DTOs
Flujo de usuario
```

Debes identificar cualquier ambigüedad existente en los requerimientos.

No inventes funcionalidades innecesarias.

Cuando una especificación sea ambigua:

1. Identifica la ambigüedad.
2. Propón la decisión más sencilla.
3. Evalúa si afecta a otra capa.
4. Documenta la decisión.
5. Mantén consistencia en toda la solución.

---

# 9. FASE 2 — DATABASE FIRST

Construye primero SQL Server.

El resultado debe incluir:

```text
Database
Tables
Primary Keys
Foreign Keys
Constraints
Indexes
Seed Data
```

Debes validar manualmente:

```text
INSERT válido
INSERT inválido
Monto <= 0
Estado inválido
Relación Claim / History
```

La base de datos debe convertirse en el contrato de persistencia para .NET.

---

# 10. FASE 3 — BACKEND

Una vez establecida la base de datos:

Construye:

```text
Domain
Application
Infrastructure
API
Tests
```

Implementa primero:

```text
GET /api/claims
GET /api/claims/{id}
POST /api/claims
PUT /api/claims/{id}
DELETE /api/claims/{id}
PATCH /api/claims/{id}/status
```

Después implementa:

```text
Filtering
Policy search
Status history
Status transitions
Error handling
Validation
```

---

# 11. CONTRATO API

Antes de construir Angular, el contrato del backend debe estar suficientemente estable.

Define claramente:

## Request

Ejemplo conceptual:

```json
{
  "policyNumber": "POL-001",
  "insuredName": "Juan Pérez",
  "claimType": "Auto",
  "estimatedAmount": 15000
}
```

## Response

Debe existir un contrato consistente para:

- Claim.
- History.
- Validation errors.
- Business errors.
- Not found.

No permitas que Angular dependa directamente de las entidades EF Core.

---

# 12. VALIDACIÓN DEL BACKEND ANTES DEL FRONTEND

Antes de comenzar Angular, prueba la API independientemente.

Utiliza Swagger o una herramienta HTTP.

Debes comprobar como mínimo:

```text
Crear Claim
Consultar Claim
Actualizar Claim
Eliminar Claim
Filtrar
Buscar por póliza
Cambiar estado
Consultar historial
Intentar transición inválida
Enviar datos inválidos
Consultar Claim inexistente
```

Si alguna operación falla, corrígela antes de comenzar la implementación principal del frontend.

---

# 13. FASE 4 — ANGULAR 19

Cuando el backend sea funcional, comienza Angular.

Utiliza:

```text
Standalone Components
Reactive Forms
HttpClient
RxJS
Signals cuando aporten valor
Angular Router
```

La aplicación debe construirse por funcionalidad.

Primero:

```text
Claims List
```

Después:

```text
Claim Creation
```

Después:

```text
Claim Detail
```

Después:

```text
Claim Edit
```

Finalmente:

```text
Status Change
```

---

# 14. ORDEN DE IMPLEMENTACIÓN DEL FRONTEND

## Paso 1

Crear modelos TypeScript.

## Paso 2

Crear `ClaimsService`.

## Paso 3

Configurar rutas.

## Paso 4

Crear listado.

## Paso 5

Agregar filtros.

## Paso 6

Crear formulario.

## Paso 7

Crear detalle.

## Paso 8

Mostrar historial.

## Paso 9

Implementar cambio de estado.

## Paso 10

Agregar loading/error/empty states.

## Paso 11

Agregar confirmaciones y mensajes.

---

# 15. INTEGRACIÓN END-TO-END

Una vez que frontend y backend estén construidos, probar el flujo completo:

```text
Angular
   ↓
POST
   ↓
.NET
   ↓
EF Core
   ↓
SQL Server
```

Y posteriormente:

```text
SQL Server
   ↓
EF Core
   ↓
.NET
   ↓
Angular
```

Debes comprobar que los datos que salen de la base de datos llegan correctamente al usuario.

---

# 16. FLUJO COMPLETO DE UN SINIESTRO

Debe funcionar este escenario:

```text
1. Usuario abre aplicación.

2. Angular solicita /api/claims.

3. API consulta SQL Server.

4. Angular muestra listado.

5. Usuario crea un Claim.

6. Angular valida formulario.

7. Angular ejecuta POST.

8. API valida DTO.

9. API persiste Claim.

10. API devuelve respuesta.

11. Angular actualiza UI.

12. Usuario abre detalle.

13. Angular solicita Claim.

14. Angular solicita historial.

15. Usuario cambia OPEN → IN_REVIEW.

16. API valida transición.

17. API actualiza Claim.

18. API crea ClaimStatusHistory.

19. Ambas operaciones se confirman.

20. Angular actualiza el detalle.

21. Usuario cambia IN_REVIEW → CLOSED.

22. Se repite el proceso.

23. El historial muestra toda la evolución.
```

Este escenario debe probarse manualmente de extremo a extremo.

---

# 17. TESTING

No intentes conseguir cobertura artificialmente alta.

Prioriza reglas críticas.

## Backend

Probar:

```text
Validaciones
Transiciones
Historial
Filtros
Errores
```

## Frontend

Probar:

```text
Formulario
Validaciones
Listado
Filtros
Detalle
Errores HTTP
```

## E2E manual

Probar:

```text
Crear
Consultar
Editar
Filtrar
Cambiar estado
Consultar historial
Eliminar
```

---

# 18. GIT

El repositorio debe mostrar evolución real.

No realizar un único commit final.

Utiliza commits pequeños y semánticos.

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

Los commits deben representar trabajo real.

No generes commits artificiales únicamente para aparentar actividad.

---

# 19. USO DE IA

La IA es una herramienta de apoyo, no la autoridad técnica.

Cada propuesta generada debe ser revisada.

El flujo esperado es:

```text
Prompt
   ↓
IA propone
   ↓
Analizar
   ↓
Validar contra requerimientos
   ↓
Aceptar / Modificar / Rechazar
   ↓
Implementar
   ↓
Probar
```

Nunca:

```text
Prompt
   ↓
Copy/Paste
   ↓
Commit
```

---

# 20. REGLA PARA LA IA

Cuando recibas una tarea, antes de implementar debes analizar:

1. ¿Qué problema resuelve?
2. ¿Qué capa es responsable?
3. ¿Qué dependencias tiene?
4. ¿Qué impacto tendrá en otras capas?
5. ¿Existe una solución más sencilla?
6. ¿La propuesta cumple los requerimientos?
7. ¿Puede introducir inconsistencias?

Si detectas una mala propuesta:

**No la implementes ciegamente.**

Explica:

```text
Problema detectado
Propuesta original
Motivo del rechazo
Alternativa
Impacto
```

---

# 21. AI_LOG.md

Debes mantener un archivo:

```text
AI_LOG.md
```

El objetivo es demostrar uso crítico de IA.

Cada entrada debe contener:

```text
Fecha
Objetivo
Prompt utilizado
Respuesta/propuesta relevante
Decisión
Corrección realizada
Motivo
Resultado
```

Ejemplo:

```markdown
## Implementación del historial

### Objetivo

Implementar el historial de cambios de estado.

### Propuesta de IA

La IA propuso almacenar el historial como JSON dentro de Claim.

### Decisión

Rechazada.

### Motivo

El historial representa una relación 1:N y debe poder
consultarse independientemente.

### Solución

Se creó ClaimStatusHistory.

### Resultado

El modelo permite consultar el historial de manera
estructurada y mantiene separación entre estado actual
e histórico.
```

El log debe contener ejemplos reales del proceso.

No inventes decisiones que nunca ocurrieron.

---

# 22. CRITERIO PARA ACEPTAR PROPUESTAS DE IA

Una propuesta debe evaluarse utilizando:

```text
Correctness
Maintainability
Simplicity
Security
Testability
Performance
Consistency
```

No aceptes una solución únicamente porque:

- Compila.
- Es más sofisticada.
- Tiene más clases.
- Usa un patrón conocido.
- La IA afirma que es "best practice".

---

# 23. CONTROL DE ALCANCE

No agregues funcionalidades no solicitadas.

NO implementar:

```text
Authentication
Authorization
JWT
Users
Roles
Payments
Notifications
Microservices
CQRS
Event Sourcing
Redis
Message Brokers
Cloud infrastructure
Advanced DevOps
```

El objetivo es completar correctamente el alcance solicitado.

---

# 24. CRITERIO DE CALIDAD

La aplicación debe demostrar:

```text
Buen modelado
      +
API limpia
      +
Frontend mantenible
      +
Validaciones
      +
Manejo de errores
      +
Persistencia correcta
      +
Testing
      +
Documentación
```

No se evaluará la cantidad de código.

Se evaluará la calidad de las decisiones.

---

# 25. DOCUMENTACIÓN FINAL

El repositorio debe contener como mínimo:

```text
README.md
AI_LOG.md
```

El README debe explicar:

```text
Descripción
Arquitectura
Tecnologías
Prerequisitos
Configuración SQL Server
Configuración Backend
Configuración Frontend
Cómo ejecutar
Cómo ejecutar tests
Endpoints principales
Decisiones relevantes
```

Debe ser posible que un desarrollador clone el repositorio y ejecute la aplicación siguiendo únicamente el README.

---

# 26. CHECKLIST FINAL

Antes de considerar terminada la aplicación, verifica:

## Database

- [ ] SQL Server funciona.
- [ ] Tablas creadas.
- [ ] Relaciones correctas.
- [ ] Constraints correctos.
- [ ] Índices justificados.
- [ ] Seed data disponible.

## Backend

- [ ] .NET 8.
- [ ] API REST funcional.
- [ ] CRUD.
- [ ] Filtros.
- [ ] Búsqueda.
- [ ] Historial.
- [ ] Transiciones.
- [ ] Validaciones.
- [ ] ProblemDetails.
- [ ] Swagger.
- [ ] Tests.

## Frontend

- [ ] Angular 19.
- [ ] Standalone components.
- [ ] Listado.
- [ ] Filtros.
- [ ] Crear.
- [ ] Editar.
- [ ] Eliminar.
- [ ] Detalle.
- [ ] Historial.
- [ ] Cambio de estado.
- [ ] Validaciones.
- [ ] Loading.
- [ ] Empty state.
- [ ] Error handling.
- [ ] Tests.

## Integración

- [ ] Angular → API funciona.
- [ ] API → SQL funciona.
- [ ] CRUD E2E funciona.
- [ ] Cambio de estado funciona.
- [ ] Historial funciona.
- [ ] Errores se muestran correctamente.

## Git

- [ ] Commits incrementales.
- [ ] Commits descriptivos.
- [ ] No existe un único commit final.

## Documentación

- [ ] README.
- [ ] AI_LOG.
- [ ] Decisiones técnicas documentadas.

---

# 27. REGLA FINAL

La solución debe seguir este principio:

```text
NO construir más.
CONSTRUIR mejor.
```

Ante dos soluciones técnicamente válidas, elegir la que:

1. Sea más sencilla.
2. Sea más fácil de mantener.
3. Sea más fácil de probar.
4. Sea suficiente para los requerimientos.
5. Sea fácil de explicar en una entrevista.

El objetivo no es crear un sistema empresarial completo.

El objetivo es demostrar que el desarrollador puede utilizar IA como herramienta de desarrollo mientras mantiene el control técnico de:

```text
Arquitectura
Código
Datos
Reglas de negocio
Calidad
Testing
Decisiones
Resultado final
```

Antes de realizar cualquier implementación, presenta primero el plan de ejecución y las decisiones técnicas que deberán quedar establecidas.

No comiences generando código hasta haber validado que el plan es coherente con los tres contextos:

```text
SQL Server
     ↕
.NET 8 API
     ↕
Angular 19
```