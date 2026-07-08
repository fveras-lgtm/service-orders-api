---
name: cqrs-handler
description: >-
  Scaffold a complete CQRS use case (command OR query) in the Application layer of this
  .NET Clean Architecture service-orders API. Use when the user asks to "add a use case",
  "create a command/query", "add a handler", or otherwise implement a new write or read
  operation (e.g. "add an AssignTechnician command", "add a GetServiceOrderById query").
  Generates the Command/Query, its Handler, a FluentValidation validator (commands), a
  result DTO, and a matching xUnit test — respecting the inward-only dependency rule and
  the naming conventions in CLAUDE.md.
---

# CQRS use-case generator

Generates one complete vertical slice in the **Application** layer, plus its test in
`tests/Application.Tests`. Never touches Domain, Infrastructure, or WebApi except to add a
persistence-abstraction method when the use case needs one (see step 4).

## When to use

- The user wants a new **command** (write): create/update/delete/state-change an aggregate.
- The user wants a new **query** (read): fetch one or many DTOs.

If the request is ambiguous about command vs query, ask. Writes are commands; reads are queries.

## Inputs to determine first

Before generating, settle these (ask the user only if not inferable):

1. **Command or query?**
2. **Entity / aggregate** it belongs to (e.g. `ServiceOrder`). The folder uses the **plural**
   grouping (e.g. `ServiceOrders`) to match the existing layout.
3. **Verb** (e.g. `Create`, `AssignTechnician`, `Close`, `Get`, `GetById`).
4. **Fields**: inputs on the command/query and which are required vs optional.
5. **Return value**: for commands, usually the new/affected id (`...Result`); for queries, a DTO.
6. **Persistence needs**: which repository method the handler calls (may need a new one — step 4).

## Target layout

Create the slice under:

```
src/Application/Features/{Entity}/{Verb}{Entity}/
```

- `{Entity}` is the **plural** aggregate group, e.g. `ServiceOrders`.
- `{Verb}{Entity}` is the operation folder, e.g. `CreateServiceOrder`, `GetServiceOrderById`.

> **Convention note.** CLAUDE.md's example and the first slice used
> `src/Application/{Entity}/Commands|Queries/{Verb}{Entity}/`. This skill standardizes on the
> `Features/{Entity}/{Verb}{Entity}/` root requested for the project. Keep new slices consistent
> with whichever layout already dominates the repo; if both exist, prefer `Features/` and mention
> the inconsistency to the user.

Files in that folder:

| File | Command | Query |
| --- | --- | --- |
| `{Verb}{Entity}Command.cs` / `{Verb}{Entity}Query.cs` | ✅ | ✅ |
| `{Verb}{Entity}Result.cs` (or `{Entity}Dto.cs`) | ✅ | ✅ |
| `{Verb}{Entity}CommandHandler.cs` / `...QueryHandler.cs` | ✅ | ✅ |
| `{Verb}{Entity}CommandValidator.cs` | ✅ | optional (only if the query has constrained inputs) |

Test file:

```
tests/Application.Tests/Features/{Entity}/{Verb}{Entity}/{Verb}{Entity}{Command|Query}HandlerTests.cs
```

(Mirror the source layout under the test project. A validator test
`{Verb}{Entity}CommandValidatorTests.cs` is added for commands with validation rules.)

## Rules to honor

- **Dependency rule.** Application depends only on Domain. Never reference Infrastructure or
  WebApi. Persist through an interface in `src/Application/Abstractions/Persistence/`, never a
  concrete `DbContext`.
- **Naming (CLAUDE.md).** Commands: `VerbNounCommand` + `VerbNounCommandHandler`. Queries:
  `GetNounQuery` / `GetNounByIdQuery` + `...QueryHandler`. Results: `NounDto` or `VerbNounResult`.
  Validators: `VerbNounCommandValidator`. Colocate command/query + handler + validator + result.
- **Never expose Domain entities** across a boundary — return a `...Result`/`Dto`, not the entity.
- **MediatR**: commands/queries implement `IRequest<TResult>`; handlers implement
  `IRequestHandler<TRequest, TResult>`.
- **Validation**: FluentValidation `AbstractValidator<T>`, required fields `NotEmpty()`, sensible
  `MaximumLength(...)`, optional-but-formatted fields guarded with `.When(...)`.
- **TDD**: write the handler/validator test first; run `dotnet test` red, implement, run green.
- Validators are registered via `AddValidatorsFromAssembly` and MediatR via
  `RegisterServicesFromAssembly` in `src/Application/DependencyInjection.cs` — no per-slice DI edits
  needed. (Note: a `ValidationBehavior` pipeline may not yet be wired; if the use case must return
  400s, flag that to the user.)

## Steps

1. Confirm the inputs above.
2. Create the source folder and files from the templates below.
3. Create the mirrored test file(s); use a hand-written in-memory fake for the repository
   (see `tests/Application.Tests/Fakes/InMemoryServiceOrderRepository.cs`) — the project does not
   use a mocking library.
4. If the handler needs a repository operation that doesn't exist, add the method to the relevant
   interface in `src/Application/Abstractions/Persistence/` (and note that Infrastructure must
   implement it — that's a separate step, outside this skill).
5. Run `dotnet test --filter "FullyQualifiedName~{Verb}{Entity}"`; iterate to green.
6. Report the files created and the test result.

## Templates

### Command

```csharp
using MediatR;

namespace Application.Features.{Entity}.{Verb}{Entity};

public record {Verb}{Entity}Command(
    // required first, then optional (nullable) fields
    string SomeRequiredField,
    string? SomeOptionalField) : IRequest<{Verb}{Entity}Result>;
```

### Result / DTO

```csharp
namespace Application.Features.{Entity}.{Verb}{Entity};

// Command result: usually the affected id.
public record {Verb}{Entity}Result(Guid Id);

// Query DTO: flat, no Domain types.
// public record {Entity}Dto(Guid Id, string SomeField, string Status);
```

### Validator (commands)

```csharp
using FluentValidation;

namespace Application.Features.{Entity}.{Verb}{Entity};

public class {Verb}{Entity}CommandValidator : AbstractValidator<{Verb}{Entity}Command>
{
    public {Verb}{Entity}CommandValidator()
    {
        RuleFor(x => x.SomeRequiredField)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.SomeOptionalField)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.SomeOptionalField));
    }
}
```

### Command handler

```csharp
using Application.Abstractions.Persistence;
using Domain.Entities;
using MediatR;

namespace Application.Features.{Entity}.{Verb}{Entity};

public class {Verb}{Entity}CommandHandler
    : IRequestHandler<{Verb}{Entity}Command, {Verb}{Entity}Result>
{
    private readonly I{Entity}Repository _repository;

    public {Verb}{Entity}CommandHandler(I{Entity}Repository repository)
    {
        _repository = repository;
    }

    public async Task<{Verb}{Entity}Result> Handle(
        {Verb}{Entity}Command request,
        CancellationToken cancellationToken)
    {
        // Build/load the aggregate, invoke domain behavior, persist.
        var entity = new {EntitySingular}(/* map fields / value objects */);

        await _repository.AddAsync(entity, cancellationToken);

        return new {Verb}{Entity}Result(entity.Id);
    }
}
```

### Query handler (read)

```csharp
using Application.Abstractions.Persistence;
using MediatR;

namespace Application.Features.{Entity}.{Verb}{Entity};

public class {Verb}{Entity}QueryHandler
    : IRequestHandler<{Verb}{Entity}Query, {Entity}Dto?>
{
    private readonly I{Entity}ReadRepository _repository; // or I{Entity}Repository

    public {Verb}{Entity}QueryHandler(I{Entity}ReadRepository repository)
    {
        _repository = repository;
    }

    public async Task<{Entity}Dto?> Handle(
        {Verb}{Entity}Query request,
        CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
        return entity is null
            ? null
            : new {Entity}Dto(entity.Id /*, ...map fields... */);
    }
}
```

### Handler test (xUnit, in-memory fake)

```csharp
using Application.Features.{Entity}.{Verb}{Entity};
using Application.Tests.Fakes;

namespace Application.Tests.Features.{Entity}.{Verb}{Entity};

public class {Verb}{Entity}CommandHandlerTests
{
    private static {Verb}{Entity}Command ValidCommand() => new(
        SomeRequiredField: "value",
        SomeOptionalField: null);

    [Fact]
    public async Task Handle_persists_and_returns_id()
    {
        var repository = new InMemory{Entity}Repository();
        var handler = new {Verb}{Entity}CommandHandler(repository);

        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.Id);
        var entity = Assert.Single(repository.Orders);
        Assert.Equal(entity.Id, result.Id);
    }

    // Add behavior-specific asserts (state transitions, mapping, guards).
}
```

### Validator test (xUnit)

```csharp
using Application.Features.{Entity}.{Verb}{Entity};

namespace Application.Tests.Features.{Entity}.{Verb}{Entity};

public class {Verb}{Entity}CommandValidatorTests
{
    private readonly {Verb}{Entity}CommandValidator _validator = new();

    [Fact]
    public void Valid_command_passes() =>
        Assert.True(_validator.Validate(new {Verb}{Entity}Command("value", null)).IsValid);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Empty_required_field_fails(string value)
    {
        var result = _validator.Validate(new {Verb}{Entity}Command(value, null));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            e => e.PropertyName == nameof({Verb}{Entity}Command.SomeRequiredField));
    }
}
```

## Verify

```bash
dotnet test tests/Application.Tests/Application.Tests.csproj \
  --filter "FullyQualifiedName~{Verb}{Entity}"
dotnet build
```

Report the created files and the pass/fail result. If a new repository method was added, remind
the user that its Infrastructure implementation + any WebApi endpoint are follow-up steps.
