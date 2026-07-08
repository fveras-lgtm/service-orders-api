# Evidence

Evidence log for the **service-orders-api** project — a REST Web API for technical service
orders built with Clean Architecture and CQRS. Each section below documents one topic: what was
done, the artifacts it produced, the supporting screenshots in [`evidence/`](evidence/), and the
relevant GitHub links.

- **Repository:** https://github.com/fveras-lgtm/service-orders-api
- **Issue #1 (setup):** https://github.com/fveras-lgtm/service-orders-api/issues/1
- **PR #2 (docs):** https://github.com/fveras-lgtm/service-orders-api/pull/2

**Key commits**

| Commit | Description |
|--------|-------------|
| `2710e03` | Initial structure — Clean Architecture |
| `9c1583d` | Create Service Order |
| `28dc3ef` | Features: Assign Technician and Get Orders By Status |
| `46698d2` | docs: README, security review, and ServiceOrder XML docs |

> **Notes on the `evidence/` folder.**
> - The topic numbering below is reconstructed from the project's activity; **Topic 5** is fixed
>   (the security review). Adjust if the canonical list differs.
> - Screenshots dated **2026-06-22** (`112707`, `112900`, `113115`, `152116`) belong to a
>   *different* project (CalSystem / Quartz / SQL Server) and are **not** evidence for this work.
> - ⚠️ `Screenshot 2026-07-07 144034.png` and `Screenshot 2026-07-07 144054.png` contain a
>   **plaintext GitHub token** and are deliberately **not** cited here. Do not commit them to a
>   public repository.

---

## Topic 1 — Project initialization & Clean Architecture scaffolding

**What was done.** Ran `/init` to analyze the (near-empty) repository and author `CLAUDE.md`, then
scaffolded the solution with the inward-only layer structure (Domain → Application → Infrastructure
→ WebApi) plus the `Application.Tests` xUnit project, EF Core + SQLite, and an initial migration.

**Evidence.**
- Source: [`CLAUDE.md`](CLAUDE.md), [`ServiceOrders.slnx`](ServiceOrders.slnx), `src/Domain`,
  `src/Application`, `src/Infrastructure`, `src/WebApi`, `tests/Application.Tests`.
- Operations log: [`evidence/claude-operations.log`](evidence/claude-operations.log) (PreToolUse
  hook recording every Bash command).
- Screenshot: [`/init` session](<evidence/Screenshot 2026-07-07 135819.png>).
- Commit [`2710e03`](https://github.com/fveras-lgtm/service-orders-api/commit/2710e03) — "Inicial
  structure Clean Architecture".

## Topic 2 — GitHub MCP setup & issue tracking

**What was done.** Registered the GitHub MCP server in the Claude Code user config and used it to
create the project's tracking issue with a checklist covering the Clean Architecture setup and the
planned use cases.

**Evidence.**
- Screenshots: [GitHub MCP auth status](<evidence/Screenshot 2026-07-07 144252.png>),
  [`/mcp` reconnected to github](<evidence/Screenshot 2026-07-07 144752.png>),
  [Issue #1 created](<evidence/Screenshot 2026-07-07 150047.png>).
- GitHub: **Issue #1 — "Initial setup and Clean Architecture"**
  https://github.com/fveras-lgtm/service-orders-api/issues/1
- ⚠️ The MCP-registration screenshots (`144034`, `144054`) are omitted — they expose a token.

## Topic 3 — Create Service Order use case (plan-first + TDD)

**What was done.** Planned the first CQRS write use case before coding, then implemented it
test-first: `CreateServiceOrderCommand` + handler + FluentValidation validator + result DTO, with
xUnit tests driven against a hand-written in-memory repository fake.

**Evidence.**
- Planning doc: [`evidence/plan-crear-orden.md`](evidence/plan-crear-orden.md).
- TDD doc: [`evidence/tdd-crear-orden.md`](evidence/tdd-crear-orden.md) — 12 tests, all green.
- Source: `src/Application/ServiceOrders/Commands/CreateServiceOrder/`,
  `tests/Application.Tests/ServiceOrders/Commands/CreateServiceOrder/`,
  `tests/Application.Tests/Fakes/InMemoryServiceOrderRepository.cs`.
- Screenshot: [plan doc + `git commit "Create Service Order"`](<evidence/Screenshot 2026-07-07 152831.png>).
- Commit [`9c1583d`](https://github.com/fveras-lgtm/service-orders-api/commit/9c1583d) — "Create
  Service Order".

## Topic 4 — Custom `cqrs-handler` skill: AssignTechnician & GetOrdersByStatus

**What was done.** Authored a reusable `cqrs-handler` skill that scaffolds a complete vertical CQRS
slice (command/query + handler + validator + result/DTO + test) following the repo conventions, and
used it to generate two use cases:
- **AssignTechnician** (command) — loads the order, assigns a `TechnicianId`, transitions to
  `InProgress`, `PUT /api/service-orders/{id}/technician`.
- **GetOrdersByStatus** (query) — lists orders filtered by `OrderStatus`,
  `GET /api/service-orders?status={status}`.

**Evidence.**
- Skill: `.claude/skills/cqrs-handler/SKILL.md`.
- Source: `src/Application/ServiceOrders/Commands/AssignTechnician/`,
  `src/Application/ServiceOrders/Queries/GetOrdersByStatus/`,
  `src/WebApi/Controllers/ServiceOrdersController.cs`, and mirrored tests under
  `tests/Application.Tests/`.
- Screenshots: [`SKILL.md` (cqrs-handler)](<evidence/Screenshot 2026-07-07 152831.png>),
  [AssignTechnician prompt](<evidence/Screenshot 2026-07-07 213811.png>),
  [AssignTechnician result summary](<evidence/Screenshot 2026-07-07 212814.png>).
- Commit [`28dc3ef`](https://github.com/fveras-lgtm/service-orders-api/commit/28dc3ef) — "Features
  Assign Technician and Get Order By Status".

## Topic 5 — Security review: Create Service Order endpoint

**Scope:** `POST /api/service-orders` and its full request path — `ServiceOrdersController.Create`
→ `CreateServiceOrderCommand`/`Validator`/`Handler` → `ServiceOrder`/`Customer`/`Equipment` domain
types → `ServiceOrderRepository` (EF Core) → `AppDbContext`/`ServiceOrderConfiguration`, plus
`Program.cs` and both `DependencyInjection` composition roots.

**Overall:** Core data-handling is sound (no SQL injection, no mass-assignment exposure, domain
invariants enforced), but the endpoint is **unauthenticated** and its **input validation does not
actually run at request time**.

**Evidence.** This document; source paths above; PR [#2](https://github.com/fveras-lgtm/service-orders-api/pull/2).

### Findings

| # | Severity | Area | Finding |
|---|----------|------|---------|
| 5.1 | **High** | Authorization | Endpoint is fully anonymous — no authN/authZ anywhere in the app. |
| 5.2 | **High** | Input validation | `CreateServiceOrderCommandValidator` is registered but never executed; business validation is effectively bypassed. |
| 5.3 | **Medium** | Info leakage / errors | No centralized exception handling or ProblemDetails; domain guard exceptions surface as HTTP 500, with full stack traces in Development. |
| 5.4 | **Medium** | Input validation (DoS) | String fields are effectively unbounded at runtime — the validator's `MaximumLength` never runs and SQLite does not enforce `HasMaxLength`. |
| 5.5 | **Low** | Transport | HTTPS redirection is enabled but HSTS is not. |
| 5.6 | **Low** | Robustness | `CreatedAtAction(nameof(Create), …)` points at the POST action (no GET-by-id exists); Location-header generation is fragile. |

#### 5.1 — No authentication or authorization (High)
`Program.cs` has no `AddAuthentication`/`AddAuthorization` or `UseAuthentication`/`UseAuthorization`;
no `[Authorize]` anywhere. Any anonymous caller can create orders without limit (data pollution /
resource exhaustion, compounding 5.4). **Recommendation:** add an auth scheme, the auth middleware,
and `[Authorize]` on writes; consider rate limiting.

#### 5.2 — FluentValidation validators are never invoked (High)
`Application/DependencyInjection.cs` calls `AddValidatorsFromAssembly` + `AddMediatR`, but there is
**no** `IPipelineBehavior` (`ValidationBehavior<,>`) and **no** `AddFluentValidationAutoValidation()`.
`[ApiController]` only validates DataAnnotations, and the command has none — so the validator rules
never run. Invalid emails persist; empty required fields arrive as unhandled `ArgumentException` →
HTTP 500 (see 5.3) instead of 400. **Recommendation:** add a MediatR `ValidationBehavior` (or
auto-validation) so validation runs before handlers — fixes all endpoints at once.

#### 5.3 — No centralized error handling; exceptions leak as 500s (Medium)
No `UseExceptionHandler`/`AddProblemDetails`/`IExceptionHandler`. Domain constructors throw
`ArgumentException`/`ArgumentNullException`; in Development the Developer Exception Page returns full
stack traces. **Recommendation:** `AddProblemDetails()` + global `UseExceptionHandler()` mapping
`ValidationException` → 400 and unexpected errors → generic 500 with no internals.

#### 5.4 — Effectively unbounded input length (Medium)
`HasMaxLength(...)` is declared but SQLite does not enforce it at runtime, and the validator's
`MaximumLength` does not run (5.2); only Kestrel's ~28.6 MB body cap applies. **Recommendation:**
fixing 5.2 restores length enforcement; add `[RequestSizeLimit]` for defense in depth.

#### 5.5 — HSTS not configured (Low)
`UseHttpsRedirection()` present; `UseHsts()` absent. **Recommendation:** add `UseHsts()` for
non-Development.

#### 5.6 — Fragile Location header (Low)
`Create` returns `CreatedAtAction(nameof(Create), …)` but `Create` is the POST action and has no
`id` route; there is no GET-by-id action. **Recommendation:** point at a real GET-by-id once it
exists, or return `Created(string.Empty, result)`.

### Confirmed well-implemented
- **No mass assignment / over-posting.** `CreateServiceOrderCommand` exposes only client fields;
  `Id`, `Status`, `TechnicianId` are set server-side in the `ServiceOrder` constructor and are not
  bindable. The command DTO is a proper allow-list.
- **No SQL injection.** Write path uses only `AddAsync` + `SaveChangesAsync`; a source grep found no
  `FromSqlRaw`/`ExecuteSqlRaw`/`SqlQuery` or string-concatenated SQL. All persistence is
  parameterized by EF Core.
- **Domain invariants as defense in depth.** `ServiceOrder`/`Customer`/`Equipment` reject
  null/whitespace required values and trim input.
- **Transport baseline.** `UseHttpsRedirection()` is enabled; value objects mapped as owned types
  with declared column max lengths.

## Topic 6 — Project README

**What was done.** Generated the root `README.md`: project description and the
`Pending → InProgress → Closed` lifecycle, architecture/layer table, requirements (.NET 10, optional
`dotnet-ef`), build / migrate / run / test commands (actual ports HTTP `5204` / HTTPS `7170`), and a
summary of the three endpoints with `curl` examples.

**Evidence.**
- Source: [`README.md`](README.md).
- Delivered in PR [#2](https://github.com/fveras-lgtm/service-orders-api/pull/2); commit
  [`46698d2`](https://github.com/fveras-lgtm/service-orders-api/commit/46698d2).

## Topic 7 — XML documentation comments

**What was done.** Added XML documentation (`<summary>`, `<param>`, `<exception>`) to the
`ServiceOrder` entity and all of its public members — properties, the public constructor, and the
`AssignTechnician`/`Close` methods. (`<returns>` does not apply: no public member returns a value.)

**Evidence.**
- Source: [`src/Domain/Entities/ServiceOrder.cs`](src/Domain/Entities/ServiceOrder.cs).
- Screenshots: [XML-docs prompt + rationale](<evidence/Screenshot 2026-07-07 213824.png>),
  [`ServiceOrder.cs` doc-comment diff](<evidence/Screenshot 2026-07-07 224349.png>).
- Delivered in PR [#2](https://github.com/fveras-lgtm/service-orders-api/pull/2); commit
  [`46698d2`](https://github.com/fveras-lgtm/service-orders-api/commit/46698d2).

## Topic 8 — Branch, commit & pull request via GitHub MCP

**What was done.** Established the remote `main` base, created a feature branch, committed the
pending documentation work (isolated from build artifacts), pushed it, and opened a pull request to
`main` using the GitHub MCP.

**Evidence.**
- Branch: `docs/readme-security-review-xml-docs` (head `46698d2`) → base `main` (`28dc3ef`).
- GitHub: **PR #2 — "docs: add README, security review, and ServiceOrder XML docs"**
  https://github.com/fveras-lgtm/service-orders-api/pull/2
- Operations log: [`evidence/claude-operations.log`](evidence/claude-operations.log).
