# TDD Evidence — "Assign Technician" use case

- **Date:** 2026-07-07
- **Branch:** `main`  ·  **Commit:** `28dc3ef`
- **SDK:** .NET 10.0.301  ·  **Test framework:** xUnit
- **Related:** [plan-assign-technician.md](plan-assign-technician.md) · GitHub issue #1

## What is under test

`AssignTechnicianCommandHandler` and `AssignTechnicianCommandValidator` (Application layer, CQRS via
MediatR). The handler loads an existing `ServiceOrder` through `IServiceOrderRepository.GetByIdAsync`,
invokes the domain method `ServiceOrder.AssignTechnician(technicianId)` (which assigns the technician
and transitions the order to `InProgress`, guarding empty ids and closed orders), persists via
`UpdateAsync`, and returns an `AssignTechnicianResult(Id, TechnicianId, Status)`. When the order is
not found it throws `KeyNotFoundException`.

Tests run against a hand-written in-memory fake (`InMemoryServiceOrderRepository`) — no database, no
mocking library. The fake stores orders by reference, so `UpdateAsync` is a no-op and mutations made
by the domain method are observable directly on the stored entity.

## Test files

- `tests/Application.Tests/ServiceOrders/Commands/AssignTechnician/AssignTechnicianCommandHandlerTests.cs`
- `tests/Application.Tests/ServiceOrders/Commands/AssignTechnician/AssignTechnicianCommandValidatorTests.cs`
- `tests/Application.Tests/Fakes/InMemoryServiceOrderRepository.cs`

## Behaviors covered

| Test | Asserts |
| --- | --- |
| `Handle_assigns_technician_and_moves_order_to_InProgress` | Result `Id`/`TechnicianId` match the request and `Status == "InProgress"`; the stored order's `TechnicianId` is set and `Status` is `InProgress`. |
| `Handle_throws_when_order_does_not_exist` | Against an empty repository, `KeyNotFoundException` is thrown. |
| `Handle_throws_when_order_is_closed` | For an order closed via `Close()`, the domain guard surfaces as `InvalidOperationException`. |
| `Valid_command_passes` | A command with non-empty `ServiceOrderId` and `TechnicianId` passes validation. |
| `Empty_service_order_id_fails` | `ServiceOrderId == Guid.Empty` fails validation with an error on `ServiceOrderId`. |
| `Empty_technician_id_fails` | `TechnicianId == Guid.Empty` fails validation with an error on `TechnicianId`. |

## Reproduce

```bash
dotnet test tests/Application.Tests/Application.Tests.csproj \
  --filter "FullyQualifiedName~AssignTechnician" \
  --logger "console;verbosity=detailed"
```

## Result — all green (6 passed, 0 failed)

```
Passed!  - Failed:     0, Passed:     6, Skipped:     0, Total:     6, Duration: 650 ms - Application.Tests.dll (net10.0)
```

## Note on TDD sequencing

This evidence document was authored **retrospectively**. The tests and the implementation were
committed together in `28dc3ef`, so a distinct failing "red" run was not captured separately at the
time. The tests nonetheless pin the specified behavior (state transition, not-found, closed-order
guard, and the two validation rules) and pass against the current implementation, as shown above. A
genuine red→green capture would require stubbing the handler to `NotImplementedException` first —
not done here since the feature was already implemented.
