# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

REST Web API for technical service orders, built with Clean Architecture and CQRS.

> Note: the codebase is greenfield — paths and commands below describe the intended structure. Verify layout as it materializes and keep this file in sync.

## Stack

- **.NET** REST Web API
- **Clean Architecture** — Domain → Application → Infrastructure → WebApi (dependencies point inward)
- **CQRS via MediatR** — commands and queries dispatched through `IMediator`
- **EF Core + SQLite** — persistence
- **xUnit** — testing

## Important paths

- `src/Domain` — entities, value objects, domain events, and domain interfaces. No dependencies on other layers or frameworks.
- `src/Application` — use cases as MediatR commands/queries + handlers, DTOs, validation, and abstractions (repository/service interfaces). Depends only on Domain.
- `src/Infrastructure` — EF Core `DbContext`, migrations, repository implementations, external service integrations. Implements Application/Domain interfaces.
- `src/WebApi` — controllers/endpoints, DI wiring, middleware, configuration. Composition root; the only project that references Infrastructure.
- `tests/Application.Tests` — xUnit tests for Application-layer handlers and use cases.

## CQRS naming conventions

- **Commands** (writes): `VerbNounCommand` + `VerbNounCommandHandler` (e.g. `CreateServiceOrderCommand`).
- **Queries** (reads): `GetNounQuery` / `GetNounByIdQuery` + matching `...QueryHandler`.
- **Results/DTOs**: `NounDto` or `VerbNounResult`; never expose Domain entities across the WebApi boundary.
- **Validators**: `VerbNounCommandValidator` (FluentValidation) colocated with the command.
- Group each command/query with its handler, validator, and result in a feature folder under `src/Application` (e.g. `Application/ServiceOrders/Commands/CreateServiceOrder/`).

## Working rules

- **Plan before editing.** Understand the affected layers and outline the change before writing code; respect the inward-only dependency rule.
- **TDD.** Write a failing xUnit test in `tests/Application.Tests` first, then implement the handler to make it pass.
- **Do not commit secrets.** No connection strings with credentials, API keys, or tokens — use configuration/user-secrets.
- **Do not commit `.db` files.** SQLite database files (`*.db`, `*.db-shm`, `*.db-wal`) stay out of git; ensure they are gitignored.

## Verification commands

```bash
# Build the whole solution
dotnet build

# Run all tests
dotnet test

# Run a single test project
dotnet test tests/Application.Tests

# Run a single test by name
dotnet test --filter "FullyQualifiedName~CreateServiceOrder"

# EF Core migrations (run from repo root; adjust project paths as needed)
dotnet ef migrations add <Name> --project src/Infrastructure --startup-project src/WebApi
dotnet ef database update --project src/Infrastructure --startup-project src/WebApi
```
