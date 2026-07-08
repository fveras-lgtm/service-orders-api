# TDD Evidence — "Get Orders By Status" use case

- **Date:** 2026-07-07
- **Branch:** `main`  ·  **Commit:** `28dc3ef`
- **SDK:** .NET 10.0.301  ·  **Test framework:** xUnit
- **Related:** [plan-get-orders-by-status.md](plan-get-orders-by-status.md) · GitHub issue #1

## What is under test

`GetOrdersByStatusQueryHandler` (Application layer, CQRS via MediatR). The handler calls
`IServiceOrderRepository.ListByStatusAsync(status)` and projects each `ServiceOrder` onto a flat
`ServiceOrderDto` (`Id`, `CustomerName`, `EquipmentType`, `ProblemDescription`, `TechnicianId`,
`Status`), returning an `IReadOnlyList<ServiceOrderDto>` — never a Domain entity across the boundary.

There is no validator: the sole input is the `OrderStatus` enum, and queries carry no business
rules. Tests run against a hand-written in-memory fake (`InMemoryServiceOrderRepository`) whose
`ListByStatusAsync` filters the stored orders by status — no database, no mocking library.

## Test files

- `tests/Application.Tests/ServiceOrders/Queries/GetOrdersByStatus/GetOrdersByStatusQueryHandlerTests.cs`
- `tests/Application.Tests/Fakes/InMemoryServiceOrderRepository.cs`

## Behaviors covered

| Test | Asserts |
| --- | --- |
| `Handle_returns_only_orders_in_the_requested_status` | With one `Pending` and one `InProgress` order stored, querying `InProgress` returns a single DTO whose `Id` and `Status` match the `InProgress` order. |
| `Handle_maps_domain_fields_onto_the_dto` | The DTO's `Id`, `CustomerName`, `EquipmentType`, `ProblemDescription`, and `TechnicianId` are correctly mapped from the domain entity. |
| `Handle_returns_empty_when_no_orders_match` | With only a `Pending` order stored, querying `Closed` returns an empty list. |

## Reproduce

```bash
dotnet test tests/Application.Tests/Application.Tests.csproj \
  --filter "FullyQualifiedName~GetOrdersByStatus" \
  --logger "console;verbosity=detailed"
```

## Result — all green (3 passed, 0 failed)

```
Passed!  - Failed:     0, Passed:     3, Skipped:     0, Total:     3, Duration: 24 ms - Application.Tests.dll (net10.0)
```

## Note on TDD sequencing

This evidence document was authored **retrospectively**. The tests and the implementation were
committed together in `28dc3ef`, so a distinct failing "red" run was not captured separately at the
time. The tests nonetheless pin the specified behavior (status filtering, field mapping, and the
empty-result case) and pass against the current implementation, as shown above. A genuine red→green
capture would require stubbing the handler to `NotImplementedException` first — not done here since
the feature was already implemented.
