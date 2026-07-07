# TDD Evidence — "Create Service Order" use case

- **Date:** 2026-07-07
- **Branch:** `main`  ·  **Commit:** `2710e03`
- **SDK:** .NET 10.0.301  ·  **Test framework:** xUnit
- **Related:** [plan-crear-orden.md](plan-crear-orden.md) · GitHub issue #1

## What is under test

`CreateServiceOrderCommandHandler` (Application layer, CQRS via MediatR). The handler builds the `Customer` and `Equipment` value objects from the command, constructs a `ServiceOrder` domain entity, persists it through `IServiceOrderRepository`, and returns the new order id.

Tests run against a hand-written in-memory fake (`InMemoryServiceOrderRepository`) — no database, no mocking library.

## Test files

- `tests/Application.Tests/ServiceOrders/Commands/CreateServiceOrder/CreateServiceOrderCommandHandlerTests.cs`
- `tests/Application.Tests/ServiceOrders/Commands/CreateServiceOrder/CreateServiceOrderCommandValidatorTests.cs`
- `tests/Application.Tests/Fakes/InMemoryServiceOrderRepository.cs`

## Behaviors covered

| Test | Asserts |
| --- | --- |
| `Handle_persists_one_order_and_returns_its_id` | Exactly one order is persisted; returned `Id` is non-empty and matches the stored order. |
| `Handle_creates_order_in_Pending_state_with_no_technician` | A valid command yields an order in `OrderStatus.Pending` with `TechnicianId == null`. |
| `Handle_maps_command_fields_onto_the_created_order` | Customer, equipment, and problem-description fields (incl. optional) are mapped from the command. |
| `Valid_command_passes` | A fully valid command passes validation. |
| `Empty_customer_name_fails` (`""`, `"   "`) | Blank `CustomerName` fails validation. |
| `Empty_equipment_type_fails` (`""`, `"   "`) | Blank `EquipmentType` fails validation. |
| `Empty_problem_description_fails` (`""`, `"   "`) | Blank `ProblemDescription` fails validation. |
| `Malformed_email_fails` | Non-empty invalid `CustomerEmail` fails validation. |
| `Null_email_passes` | Null `CustomerEmail` is accepted (optional field). |

## Reproduce

```bash
dotnet test tests/Application.Tests/Application.Tests.csproj \
  --filter "FullyQualifiedName~CreateServiceOrder" \
  --logger "console;verbosity=detailed"
```

## Result — all green (12 passed, 0 failed)

```
Passed  CreateServiceOrderCommandHandlerTests.Handle_persists_one_order_and_returns_its_id [10 ms]
Passed  CreateServiceOrderCommandHandlerTests.Handle_creates_order_in_Pending_state_with_no_technician [2 ms]
Passed  CreateServiceOrderCommandHandlerTests.Handle_maps_command_fields_onto_the_created_order [1 ms]
Passed  CreateServiceOrderCommandValidatorTests.Empty_problem_description_fails(description: "") [21 ms]
Passed  CreateServiceOrderCommandValidatorTests.Empty_problem_description_fails(description: "   ") [< 1 ms]
Passed  CreateServiceOrderCommandValidatorTests.Empty_equipment_type_fails(type: "   ") [< 1 ms]
Passed  CreateServiceOrderCommandValidatorTests.Empty_equipment_type_fails(type: "") [< 1 ms]
Passed  CreateServiceOrderCommandValidatorTests.Empty_customer_name_fails(name: "   ") [< 1 ms]
Passed  CreateServiceOrderCommandValidatorTests.Empty_customer_name_fails(name: "") [< 1 ms]
Passed  CreateServiceOrderCommandValidatorTests.Malformed_email_fails [< 1 ms]
Passed  CreateServiceOrderCommandValidatorTests.Null_email_passes [< 1 ms]
Passed  CreateServiceOrderCommandValidatorTests.Valid_command_passes [< 1 ms]

Passed!  - Failed: 0, Passed: 12, Skipped: 0, Total: 12
```

## Note on TDD sequencing

The handler was implemented in the same step as the tests (per the approved plan), so a distinct failing "red" run was not captured separately. The tests nonetheless pin the specified behavior and pass against the current implementation. A genuine red→green capture would require stubbing the handler to `NotImplementedException` first — not done here since the user opted to leave the implementation as-is.
