# PROMPT 01 — BASE DE DATOS SQL SERVER / T-SQL

## Rol

Actúa como **Senior Database Developer / Database Architect**, especializado en Microsoft SQL Server y T-SQL, con amplia experiencia en diseño relacional, integridad de datos, transacciones, índices, restricciones y modelado para aplicaciones empresariales.

Tu responsabilidad es diseñar e implementar exclusivamente la **capa de persistencia** de una aplicación fullstack denominada:

**Gestor de Siniestros Simplificado**

El frontend será desarrollado posteriormente con **Angular 19** y el backend con **C# / .NET 8 / ASP.NET Core Web API**.

---

# 1. Objetivo funcional del sistema

La aplicación permitirá registrar y dar seguimiento a siniestros de seguros.

Cada siniestro debe contener como mínimo:

- Número de póliza.
- Nombre del asegurado.
- Tipo de siniestro.
- Monto estimado.
- Estado.
- Fecha de creación.
- Fecha de última modificación.

Los estados permitidos son:

- `OPEN` — Abierto.
- `IN_REVIEW` — En revisión.
- `CLOSED` — Cerrado.

Además, el sistema debe conservar un historial de todos los cambios de estado.

---

# 2. Modelo relacional

Diseña como mínimo las siguientes entidades:

## Claims

Representa el siniestro actual.

Campos conceptuales:

- Id
- PolicyNumber
- InsuredName
- ClaimType
- EstimatedAmount
- Status
- CreatedAt
- UpdatedAt

## ClaimStatusHistory

Representa cada cambio de estado ocurrido sobre un siniestro.

Campos conceptuales:

- Id
- ClaimId
- PreviousStatus
- NewStatus
- ChangedAt

La relación debe ser:

```text
Claims 1 ───────── N ClaimStatusHistory
```

Un siniestro puede tener múltiples registros de historial.

---

# 3. Reglas de integridad

Implementa las restricciones necesarias para garantizar la integridad de los datos.

Como mínimo:

- `PolicyNumber` obligatorio.
- `InsuredName` obligatorio.
- `ClaimType` obligatorio.
- `EstimatedAmount` obligatorio.
- `EstimatedAmount` debe ser mayor que cero.
- `Status` obligatorio.
- `Status` solamente puede contener:
  - `OPEN`
  - `IN_REVIEW`
  - `CLOSED`
- `ClaimStatusHistory.ClaimId` debe tener una FK hacia `Claims.Id`.
- Las fechas deben almacenarse de forma consistente.

Define longitudes razonables para campos de texto.

No utilices tipos de datos excesivamente grandes sin justificación.

---

# 4. Historial de estados

El historial debe permitir conocer cómo evolucionó un siniestro.

Ejemplo:

```text
OPEN
↓
IN_REVIEW
↓
CLOSED
```

El sistema debe conservar:

```text
Estado anterior
Estado nuevo
Fecha del cambio
Siniestro afectado
```

El historial no debe reemplazar el estado actual almacenado en `Claims`.

El estado actual debe estar disponible directamente en `Claims.Status`.

---

# 5. Transiciones de estado

La especificación funcional no define explícitamente las transiciones permitidas.

Para esta prueba técnica adopta la siguiente regla:

```text
OPEN → IN_REVIEW
IN_REVIEW → CLOSED
```

No debe permitirse:

```text
CLOSED → OPEN
CLOSED → IN_REVIEW
IN_REVIEW → OPEN
```

No agregues estados adicionales.

No agregues workflows complejos.

La validación principal de estas transiciones será responsabilidad del backend, pero la base de datos debe estar diseñada de manera que no contradiga estas reglas.

---

# 6. Scripts

Entrega scripts T-SQL independientes y ejecutables en SQL Server.

Como mínimo:

```text
01_create_database.sql
02_create_tables.sql
03_create_constraints.sql
04_create_indexes.sql
05_seed_data.sql
```

Si consideras que algunos scripts pueden combinarse sin perder claridad, puedes hacerlo, pero mantén una organización fácilmente ejecutable.

Los scripts deben poder ejecutarse desde una base de datos limpia.

---

# 7. Seed data

Incluye datos iniciales suficientes para probar la aplicación.

Debe existir información que permita visualizar:

- Siniestros abiertos.
- Siniestros en revisión.
- Siniestros cerrados.
- Siniestros con historial de estados.

Incluye suficientes registros para probar filtros y búsquedas.

No utilices datos reales ni información personal real.

---

# 8. Índices

Analiza las consultas principales esperadas por la aplicación.

La aplicación necesitará:

```text
Filtrar por Status
Buscar por PolicyNumber
Consultar un Claim por Id
Consultar el historial de un Claim
```

Crea únicamente los índices que tengan una justificación clara.

No crees índices indiscriminadamente.

Explica brevemente la razón de cada índice.

---

# 9. Transacciones

El cambio de estado de un siniestro conceptualmente implica:

```text
Actualizar Claims.Status
        +
Insertar ClaimStatusHistory
```

Estas operaciones deben poder ejecutarse de forma atómica desde el backend.

No implementes triggers para solucionar esto salvo que exista una justificación técnica excepcional.

La lógica de negocio debe permanecer principalmente en .NET.

---

# 10. Triggers

No utilices triggers para implementar la lógica principal del sistema.

Especialmente no utilices triggers para:

- Cambiar estados.
- Implementar workflows.
- Generar automáticamente lógica de negocio compleja.

La aplicación utilizará .NET 8 + EF Core como capa principal de acceso a datos.

---

# 11. Stored Procedures

No es obligatorio utilizar Stored Procedures.

Para esta prueba se priorizará un diseño sencillo y mantenible compatible con Entity Framework Core.

No generes Stored Procedures solamente para demostrar conocimiento de T-SQL.

Si decides crear alguna, debe existir una razón concreta y documentada.

---

# 12. EF Core

El modelo debe ser fácilmente consumible por:

```text
C#
.NET 8
ASP.NET Core
Entity Framework Core
```

Evita diseños que dificulten innecesariamente el mapeo mediante EF Core.

---

# 13. Convenciones

Utiliza convenciones claras y consistentes.

Preferentemente:

- Tablas en plural.
- PK `Id`.
- FK con formato `{Entity}Id`.
- `datetime2` para timestamps.
- `decimal` apropiado para montos monetarios.
- Restricciones explícitas.
- Nombres descriptivos.

Define una precisión adecuada para `EstimatedAmount`.

---

# 14. Seguridad e integridad

No almacenes información sensible innecesaria.

No agregues:

- Usuarios.
- Passwords.
- Roles.
- Autenticación.
- Tokens.
- Información bancaria.

No forman parte de la prueba.

---

# 15. Entregables

Genera:

1. Scripts T-SQL completos.
2. Estructura final de tablas.
3. Constraints.
4. Foreign keys.
5. Índices.
6. Seed data.
7. Breve documentación de decisiones técnicas.

---

# 16. Criterios de aceptación

La solución se considera correcta si:

- La base de datos puede crearse desde cero.
- Las tablas se crean correctamente.
- Las relaciones son correctas.
- Los campos obligatorios están protegidos.
- Los montos negativos o cero son rechazados.
- Los estados inválidos son rechazados.
- La relación Claim → StatusHistory funciona correctamente.
- El historial puede consultarse por ClaimId.
- Existen datos suficientes para probar la aplicación.
- Los índices están justificados.
- El diseño puede ser consumido correctamente desde EF Core.

---

# 17. Restricciones

NO implementes:

- Frontend.
- API.
- C#.
- Angular.
- Autenticación.
- Microservicios.
- CQRS.
- Event Sourcing.
- Triggers innecesarios.
- Stored Procedures innecesarios.

Tu responsabilidad termina en la **capa SQL Server / T-SQL**.

Antes de generar código, analiza el diseño y detecta posibles inconsistencias.

Si existe una decisión ambigua, elige la alternativa más sencilla, mantenible y coherente con una prueba técnica de 4–6 horas y documenta la decisión.

No agregues funcionalidades no solicitadas.