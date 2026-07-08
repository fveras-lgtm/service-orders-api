# Plan — "Assign Technician" use case

_This plan was authored retrospectively to document the design of an already-implemented feature
(commit `28dc3ef`); it was not written before the code._

## Context

With the first use case in place (an order can be created in the `Pending` state with no technician
— see [plan-crear-orden.md](plan-crear-orden.md)), the next step in the order lifecycle is
assignment. This use case takes an existing order, assigns a technician to it, and moves it from
`Pending` into `InProgress`. It is the second CQRS command on the `ServiceOrder` aggregate and the
first that **loads and mutates** an existing aggregate (create only added new ones).

The state transition itself already lives on the Domain entity: `ServiceOrder.AssignTechnician(Guid)`
sets `TechnicianId` and flips `Status` to `InProgress`, guarding against an empty id and against
assigning to a `Closed` order. The Application layer only orchestrates load → invoke → persist.

### Decisions (confirmed with user)
- **Inputs:** `ServiceOrderId` (which order) and `TechnicianId` (who to assign). Both required.
- **State change:** delegate entirely to the existing `ServiceOrder.AssignTechnician` domain method;
  do not re-implement the transition or the guards in the handler.
- **Not found:** if the order does not exist, the handler throws `KeyNotFoundException`; the WebApi
  controller maps that to `404`.
- **Return value:** an informative result — the order id, the assigned technician id, and the new
  status as a string (`AssignTechnicianResult`), not just the id.
- **Persistence:** the create path only needed `AddAsync`; this use case needs to **load** and
  **update**, so add `GetByIdAsync` and `UpdateAsync` to `IServiceOrderRepository`.
- **Endpoint:** `PUT api/service-orders/{id}/technician`, with the technician id in the body.

## Scope / non-goals
- No new domain behavior — `AssignTechnician` already exists on the entity.
- No reassignment/unassignment or technician-existence validation (a `TechnicianId` is accepted as
  given; there is no lookup against a technicians table).
- No `ValidationBehavior` pipeline wiring (validators are registered but not yet executed at
  request time — a known project-wide gap, out of scope here).

## Files to create

**Application abstraction** — extend `src/Application/Abstractions/Persistence/IServiceOrderRepository.cs`
- `Task<ServiceOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);`
- `Task UpdateAsync(ServiceOrder order, CancellationToken cancellationToken = default);`
- (Infrastructure implements these in `src/Infrastructure/Persistence/Repositories/ServiceOrderRepository.cs`:
  `GetByIdAsync` → `FirstOrDefaultAsync`; `UpdateAsync` → `Update` + `SaveChangesAsync`.)

**Feature folder** `src/Application/ServiceOrders/Commands/AssignTechnician/`
- `AssignTechnicianCommand.cs`
  - `public record AssignTechnicianCommand(Guid ServiceOrderId, Guid TechnicianId) : IRequest<AssignTechnicianResult>;`
- `AssignTechnicianResult.cs`
  - `public record AssignTechnicianResult(Guid Id, Guid TechnicianId, string Status);`
- `AssignTechnicianCommandValidator.cs` (FluentValidation, colocated per CLAUDE.md)
  - `ServiceOrderId` `NotEmpty()`
  - `TechnicianId` `NotEmpty()`
- `AssignTechnicianCommandHandler.cs`
  - `IRequestHandler<AssignTechnicianCommand, AssignTechnicianResult>`
  - Loads via `GetByIdAsync`; throws `KeyNotFoundException` if null; calls
    `order.AssignTechnician(request.TechnicianId)`; `await _repository.UpdateAsync(order, ct)`;
    returns `new AssignTechnicianResult(order.Id, order.TechnicianId!.Value, order.Status.ToString())`.

**WebApi** — `src/WebApi/Controllers/ServiceOrdersController.cs`
- `PUT api/service-orders/{id:guid}/technician` action `AssignTechnician(Guid id, [FromBody] AssignTechnicianRequest request, CancellationToken)`.
- `public record AssignTechnicianRequest(Guid TechnicianId);` for the body; sends
  `new AssignTechnicianCommand(id, request.TechnicianId)`; `Ok(result)`; catches
  `KeyNotFoundException` → `NotFound()`. Declares `200`/`400`/`404`.

**Tests** `tests/Application.Tests/`
- Extend `Fakes/InMemoryServiceOrderRepository.cs` with `GetByIdAsync` (`SingleOrDefault`) and
  `UpdateAsync` (no-op — the list holds references).
- `ServiceOrders/Commands/AssignTechnician/AssignTechnicianCommandHandlerTests.cs`
  - assigns technician and moves the order to `InProgress`
  - throws `KeyNotFoundException` when the order does not exist
  - throws `InvalidOperationException` when the order is `Closed`
- `ServiceOrders/Commands/AssignTechnician/AssignTechnicianCommandValidatorTests.cs`
  - a valid command passes
  - empty `ServiceOrderId` fails
  - empty `TechnicianId` fails

## Implementation order (TDD per CLAUDE.md)
1. Add `GetByIdAsync`/`UpdateAsync` to `IServiceOrderRepository` and the command/result types
   (so tests compile).
2. Extend the in-memory fake with the two new methods.
3. Write the handler + validator tests; add skeleton handler/validator so the suite compiles —
   `dotnet test` → **red**.
4. Implement the handler and validator → `dotnet test` → **green**.
5. Implement `GetByIdAsync`/`UpdateAsync` in Infrastructure's `ServiceOrderRepository`.
6. Add the WebApi endpoint; `dotnet build` the solution.

## Verification
- `dotnet test tests/Application.Tests --filter "FullyQualifiedName~AssignTechnician"` — handler +
  validator tests pass.
- `dotnet build` — whole solution compiles, 0 errors.
- End-to-end (Development): `PUT /api/service-orders/{id}/technician` with `{ "technicianId": "<guid>" }`
  returns `200` with the updated order, and `404` for an unknown id. Exercisable via the Scalar UI
  at `/scalar`.
