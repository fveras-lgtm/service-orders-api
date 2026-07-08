# Service Orders API

REST Web API for managing technical service orders, built with **Clean Architecture** and **CQRS**.

An order captures a customer, the equipment brought in, and the reported problem. It starts in the
`Pending` state, moves to `InProgress` once a technician is assigned, and ends as `Closed`.

## Architecture

The solution follows Clean Architecture with dependencies pointing inward
(Domain ← Application ← Infrastructure ← WebApi):

| Project | Responsibility |
| --- | --- |
| `src/Domain` | Entities, value objects, enums. No dependencies on other layers or frameworks. |
| `src/Application` | Use cases as MediatR commands/queries + handlers, DTOs, FluentValidation validators, and persistence abstractions. Depends only on Domain. |
| `src/Infrastructure` | EF Core `DbContext`, migrations, and repository implementations. |
| `src/WebApi` | Controllers, DI wiring, configuration. Composition root. |
| `tests/Application.Tests` | xUnit tests for Application-layer handlers and validators. |

**Stack:** .NET 10 · ASP.NET Core · MediatR (CQRS) · EF Core + SQLite · FluentValidation · xUnit.

## Requirements

- [.NET SDK 10.0](https://dotnet.microsoft.com/download) or later
- (Optional) the EF Core CLI tools for managing migrations manually:

  ```bash
  dotnet tool install --global dotnet-ef
  ```

## Build

```bash
dotnet build
```

## Database & migrations

The API uses SQLite; the database file (`serviceorders.db`) is created next to the WebApi project.

**Migrations are applied automatically at startup** — `Program.cs` runs `db.Database.Migrate()`,
so a fresh checkout needs no manual step before running.

To manage migrations manually (run from the repo root):

```bash
# Add a new migration
dotnet ef migrations add <Name> --project src/Infrastructure --startup-project src/WebApi

# Apply pending migrations to the database
dotnet ef database update --project src/Infrastructure --startup-project src/WebApi
```

## Run the API

```bash
dotnet run --project src/WebApi
```

The API listens on:

- HTTP — `http://localhost:5204`
- HTTPS — `https://localhost:7170`

In the `Development` environment the OpenAPI document is served at `/openapi/v1.json`.

## Run the tests

```bash
# All tests
dotnet test

# A single test project
dotnet test tests/Application.Tests

# Filter by name
dotnet test --filter "FullyQualifiedName~GetOrdersByStatus"
```

## Endpoints

Base route: `/api/service-orders`

| Method & route | Description | Request | Responses |
| --- | --- | --- | --- |
| `POST /api/service-orders` | Create a new service order (starts in `Pending`). | JSON body with customer, equipment, and problem description. | `201 Created` with the new order id · `400 Bad Request` |
| `PUT /api/service-orders/{id}/technician` | Assign a technician and move the order to `InProgress`. | JSON body `{ "technicianId": "<guid>" }` | `200 OK` with the updated order · `400 Bad Request` · `404 Not Found` |
| `GET /api/service-orders?status={status}` | List service orders filtered by status. | `status` query value: `Pending`, `InProgress`, or `Closed`. | `200 OK` with the matching orders · `400 Bad Request` |

### Examples

```bash
# Create an order
curl -X POST http://localhost:5204/api/service-orders \
  -H "Content-Type: application/json" \
  -d '{
        "customerName": "Ada Lovelace",
        "customerPhone": "809-555-0101",
        "customerEmail": "ada@example.com",
        "equipmentType": "Laptop",
        "equipmentBrand": "Dell",
        "equipmentModel": "XPS 13",
        "equipmentSerialNumber": "SN-12345",
        "problemDescription": "Does not power on."
      }'

# Assign a technician
curl -X PUT http://localhost:5204/api/service-orders/<order-id>/technician \
  -H "Content-Type: application/json" \
  -d '{ "technicianId": "6f9619ff-8b86-d011-b42d-00c04fc964ff" }'

# List orders that are in progress
curl "http://localhost:5204/api/service-orders?status=InProgress"
```
