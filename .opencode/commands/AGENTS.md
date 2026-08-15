# PROJECT DEVELOPMENT INSTRUCTIONS

## Master Architecture Context

The file:

`prompts/04-context-architecture.md`

is the **MASTER PROMPT AND AUTHORITATIVE ARCHITECTURE CONTEXT** for this project.

All development decisions MUST comply with the rules, architecture,
scope, conventions, development order, and acceptance criteria defined
in that document.

The master context has precedence over the specialized prompts.

---

## Specialized Contexts

The project contains three specialized implementation prompts:

- `prompts/01-database.md`
  - SQL Server / T-SQL
  - Database schema
  - Constraints
  - Indexes
  - Seed data

- `prompts/02-backend.md`
  - C#
  - .NET 8
  - ASP.NET Core
  - Entity Framework Core
  - REST API
  - Business rules
  - Testing

- `prompts/03-frontend.md`
  - Angular 19
  - TypeScript
  - RxJS
  - Reactive Forms
  - UI
  - Frontend testing

These files are NOT independent architectural authorities.

They are specialized implementation contexts subordinate to:

`prompts/04-context-architecture.md`

---

# HIERARCHY

Use the following hierarchy when resolving conflicts:

```text
04-context-architecture.md
        │
        ├── 01-database.md
        │
        ├── 02-backend.md
        │
        └── 03-frontend.md


## ARCHITECTURAL CHANGE CONTROL

The AI MUST NOT change the architecture defined in
`prompts/04-context-architecture.md` without explicit user approval.

If implementation reveals that the architecture is insufficient:

1. Do not silently modify it.
2. Explain the problem.
3. Propose the architectural change.
4. Explain the impact.
5. Wait for explicit approval before changing the architecture.