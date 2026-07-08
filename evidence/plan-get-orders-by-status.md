# Plan — "Get Orders By Status" use case

_This plan was authored retrospectively to document the design of an already-implemented feature
(commit `28dc3ef`); it was not written before the code._

## Context

With orders being created ([plan-crear-orden.md](plan-crear-orden.md)) and assigned to technicians
([plan-assign-technician.md](plan-assign-technician.md)), operators need to see orders by where they
are in the lifecycle — e.g. all `Pending` orders waiting for a technician, or all `InProgress` work.
This is the first CQRS **query** on the `ServiceOrder` aggregate: given an `OrderStatus`, return the
matching orders as a flat read model.

Per the boundary rule in CLAUDE.md, Domain entities must not cross the WebApi boundary, so the query
returns a dedicated `ServiceOrderDto` rather than the `ServiceOrder` entity.

### Decisions (confirmed with user)
- **Input:** a single `OrderStatus` enum value (`Pending`, `InProgress`, `Closed`).
- **Output:** `IReadOnlyList<ServiceOrderDto>` — a flat projection (no Domain types), empty when
  nothing matches (not an error).
- **DTO shape:** `Id`, `CustomerName`, `EquipmentType`, `ProblemDescription`, `TechnicianId`
  (nullable), and `Status` as a string.
- **No validator:** the only input is the `OrderStatus` enum, which model binding already
  constrains; there are no business rules to validate (queries are read-only).
- **Persistence:** add a read method `ListByStatusAsync(OrderStatus, …)` to `IServiceOrderRepository`.
- **Endpoint:** `GET api/service-orders?status={status}`, binding the enum from the query string.

## Scope / non-goals
- No paging, sorting, or free-text filtering — status is the only filter.
- No separate read model / CQRS read store; the query reads through the same repository abstraction.
- Reuse the existing `IServiceOrderRepository`; do not introduce a separate read-repository interface.

## Files to create

**Application abstraction** — extend `src/Application/Abstractions/Persistence/IServiceOrderRepository.cs`
- `Task<IReadOnlyList<ServiceOrder>> ListByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default);`
- (Infrastructure implements this in `ServiceOrderRepository.cs` with
  `AsNoTracking().Where(o => o.Status == status).ToListAsync(...)` — no tracking needed for a read.)

**Feature folder** `src/Application/ServiceOrders/Queries/GetOrdersByStatus/`
- `GetOrdersByStatusQuery.cs`
  - `public record GetOrdersByStatusQuery(OrderStatus Status) : IRequest<IReadOnlyList<ServiceOrderDto>>;`
- `ServiceOrderDto.cs`
  - `public record ServiceOrderDto(Guid Id, string CustomerName, string EquipmentType, string ProblemDescription, Guid? TechnicianId, string Status);`
- `GetOrdersByStatusQueryHandler.cs`
  - `IRequestHandler<GetOrdersByStatusQuery, IReadOnlyList<ServiceOrderDto>>`
  - `await _repository.ListByStatusAsync(request.Status, ct)`, then map each order →
    `new ServiceOrderDto(order.Id, order.Customer.Name, order.Equipment.Type, order.ProblemDescription, order.TechnicianId, order.Status.ToString())`, returned as a `List`.

**WebApi** — `src/WebApi/Controllers/ServiceOrdersController.cs`
- `GET api/service-orders?status={status}` action `GetByStatus([FromQuery] OrderStatus status, CancellationToken)`;
  sends `new GetOrdersByStatusQuery(status)`; `Ok(result)`. Declares `200`/`400`.

**Tests** `tests/Application.Tests/`
- Extend `Fakes/InMemoryServiceOrderRepository.cs` with `ListByStatusAsync`
  (`Orders.Where(o => o.Status == status).ToList()`).
- `ServiceOrders/Queries/GetOrdersByStatus/GetOrdersByStatusQueryHandlerTests.cs`
  - returns only orders in the requested status
  - maps domain fields onto the DTO
  - returns empty when no orders match

## Implementation order (TDD per CLAUDE.md)
1. Add `ListByStatusAsync` to `IServiceOrderRepository` and the query/DTO types (so tests compile).
2. Extend the in-memory fake with `ListByStatusAsync`.
3. Write the handler tests; add a skeleton handler so the suite compiles — `dotnet test` → **red**.
4. Implement the handler → `dotnet test` → **green**.
5. Implement `ListByStatusAsync` in Infrastructure's `ServiceOrderRepository`.
6. Add the WebApi endpoint; `dotnet build` the solution.

## Verification
- `dotnet test tests/Application.Tests --filter "FullyQualifiedName~GetOrdersByStatus"` — handler
  tests pass.
- `dotnet build` — whole solution compiles, 0 errors.
- End-to-end (Development): `GET /api/service-orders?status=InProgress` returns the matching orders
  as JSON (`200`), and an unknown value like `?status=Foo` returns `400`. Exercisable via the Scalar
  UI at `/scalar`.
