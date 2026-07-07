# Plan — "Create Service Order" use case

## Context

The solution is greenfield: the Domain layer is fully implemented (entities, value objects, `OrderStatus` enum), but `src/Application` and `src/Infrastructure` are empty shells with only NuGet packages pre-referenced (MediatR + FluentValidation in Application; EF Core + SQLite in Infrastructure). This is the first CQRS use case (issue #1): accept the data needed to open a service order, validate it, create a `ServiceOrder` domain entity (which starts `Pending`), persist it, and return the new order's id.

### Decisions (confirmed with user)
- **Fields:** match the Domain as-is. Required: `CustomerName`, `EquipmentType`, `ProblemDescription`. Optional: `CustomerPhone`, `CustomerEmail`, `EquipmentBrand`, `EquipmentModel`, `EquipmentSerialNumber`.
- **Technician:** none at creation. Every order is created `Pending` with `TechnicianId == null` (assignment is a separate future use case).
- **Return value:** the new `Guid` id only (`CreateServiceOrderResult(Guid Id)`).
- **Persistence:** define an `IServiceOrderRepository` abstraction in Application now and TDD the handler against a hand-written in-memory fake. The EF Core `DbContext` + repository implementation + WebApi endpoint are **deferred** to a later step.

## Scope / non-goals
- No EF Core `DbContext`, no migration, no repository implementation in Infrastructure.
- No WebApi controller/endpoint, no HTTP wiring.
- No technician assignment logic.
These come in follow-up steps once this use case is green.

## Files to create

**Application abstraction**
- `src/Application/Abstractions/Persistence/IServiceOrderRepository.cs`
  - `Task AddAsync(ServiceOrder order, CancellationToken cancellationToken = default);`
  - (Add persists the aggregate; a separate `SaveChangesAsync`/unit-of-work is intentionally omitted to keep the create path lean — revisit when transactions across aggregates are needed.)

**Feature folder** `src/Application/ServiceOrders/Commands/CreateServiceOrder/`
- `CreateServiceOrderCommand.cs`
  - `public record CreateServiceOrderCommand(string CustomerName, string? CustomerPhone, string? CustomerEmail, string EquipmentType, string? EquipmentBrand, string? EquipmentModel, string? EquipmentSerialNumber, string ProblemDescription) : IRequest<CreateServiceOrderResult>;`
- `CreateServiceOrderResult.cs`
  - `public record CreateServiceOrderResult(Guid Id);`
- `CreateServiceOrderCommandValidator.cs` (FluentValidation, colocated per CLAUDE.md)
  - `CustomerName` NotEmpty (+ MaxLength, e.g. 200)
  - `EquipmentType` NotEmpty (+ MaxLength)
  - `ProblemDescription` NotEmpty (+ MaxLength, e.g. 2000)
  - `CustomerEmail` `EmailAddress()` **when** provided (optional-but-valid)
- `CreateServiceOrderCommandHandler.cs`
  - `IRequestHandler<CreateServiceOrderCommand, CreateServiceOrderResult>`
  - Constructs `Customer` and `Equipment` value objects from the command, `new ServiceOrder(customer, equipment, problemDescription)`, `await _repository.AddAsync(order, ct)`, returns `new CreateServiceOrderResult(order.Id)`.

**Application DI**
- `src/Application/DependencyInjection.cs`
  - `public static IServiceCollection AddApplication(this IServiceCollection services)` — registers MediatR (`RegisterServicesFromAssembly` of the Application assembly) and FluentValidation validators (`AddValidatorsFromAssembly`). Requires adding `Microsoft.Extensions.DependencyInjection.Abstractions` (transitively already available via FluentValidation.DI, verify at build; add explicitly if missing).

**Tests** `tests/Application.Tests/`
- `Fakes/InMemoryServiceOrderRepository.cs` — implements `IServiceOrderRepository`, stores added orders in a `List<ServiceOrder>` exposed for assertions (no Moq needed).
- `ServiceOrders/Commands/CreateServiceOrder/CreateServiceOrderCommandHandlerTests.cs`
  - persists exactly one order and returns a non-empty `Id`
  - created order is `OrderStatus.Pending` with `TechnicianId == null`
  - `Customer`, `Equipment`, and `ProblemDescription` are mapped from the command (incl. optional fields)
- `ServiceOrders/Commands/CreateServiceOrder/CreateServiceOrderCommandValidatorTests.cs`
  - empty `CustomerName` / `EquipmentType` / `ProblemDescription` each fail
  - malformed `CustomerEmail` fails; null email passes
  - a fully valid command passes
- Delete the default `tests/Application.Tests/UnitTest1.cs`.
- Assertions use xUnit `Assert` (no FluentAssertions added). Validator tests use `Validate(...)`/`TestValidate(...)` from FluentValidation.

## Implementation order (TDD per CLAUDE.md)
1. Write this plan to `evidence/plan-crear-orden.md`.
2. Add `IServiceOrderRepository`, `CreateServiceOrderCommand`, `CreateServiceOrderResult` (types the tests compile against).
3. Add the in-memory fake repository in the test project.
4. Write the handler + validator tests. Add skeleton `CreateServiceOrderCommandHandler`/`Validator` so the suite compiles — run `dotnet test` → **red**.
5. Implement the handler and validator → run `dotnet test` → **green**.
6. Add `AddApplication()` DI extension; `dotnet build` the solution.

## Verification
- `dotnet test tests/Application.Tests` — all new handler + validator tests pass (red→green demonstrated during step 4→5).
- `dotnet test --filter "FullyQualifiedName~CreateServiceOrder"` — targeted run.
- `dotnet build` — whole solution compiles, 0 errors.
- **Not** verifiable end-to-end via HTTP yet (persistence + endpoint deferred). End-to-end API verification is called out as the next step: EF Core `DbContext` + `IServiceOrderRepository` implementation + WebApi endpoint + `AddApplication`/`AddInfrastructure` wiring in `Program.cs`.
