# Security Evidence

## Topic 5 — Security review: Create Service Order endpoint

**Scope:** `POST /api/service-orders` and everything on its request path —
`ServiceOrdersController.Create` → `CreateServiceOrderCommand`/`Validator`/`Handler` →
`ServiceOrder`/`Customer`/`Equipment` domain types → `ServiceOrderRepository` (EF Core) →
`AppDbContext` / `ServiceOrderConfiguration`, plus `Program.cs` and both `DependencyInjection`
composition roots.

**Overall:** The core data-handling is sound (no SQL injection, no mass-assignment exposure,
domain invariants enforced), but the endpoint is **unauthenticated** and its **input validation
does not actually run at request time**, which are the two findings that need attention.

---

### Findings

| # | Severity | Area | Finding |
|---|----------|------|---------|
| 5.1 | **High** | Authorization | Endpoint is fully anonymous — no authN/authZ anywhere in the app. |
| 5.2 | **High** | Input validation | `CreateServiceOrderCommandValidator` is registered but never executed; business validation is effectively bypassed. |
| 5.3 | **Medium** | Info leakage / errors | No centralized exception handling or ProblemDetails; domain guard exceptions surface as HTTP 500, with full stack traces in Development. |
| 5.4 | **Medium** | Input validation (DoS) | String fields are effectively unbounded at runtime — the validator's `MaximumLength` never runs and SQLite does not enforce `HasMaxLength`. |
| 5.5 | **Low** | Transport | HTTPS redirection is enabled but HSTS is not. |
| 5.6 | **Low** | Robustness | `CreatedAtAction(nameof(Create), …)` points at the POST action (no GET-by-id exists); Location-header generation is fragile. |

---

#### 5.1 — No authentication or authorization (High)

**Evidence:** `Program.cs` contains no `AddAuthentication`/`AddAuthorization`,
no `UseAuthentication`/`UseAuthorization`; the controller and action carry no `[Authorize]`
attribute. A grep for `UseAuthentication|UseAuthorization|Authorize` returns nothing.

**Impact:** Any anonymous caller can create service orders without limit — data pollution,
spam, and a resource-exhaustion vector (compounded by 5.4). If this API is not strictly internal
behind another gate, this is the highest-priority issue.

**Recommendation:** Add an authentication scheme (JWT bearer / cookie as appropriate), call
`app.UseAuthentication()` + `app.UseAuthorization()`, and protect writes with `[Authorize]`
(optionally a policy/role for order creation). Consider rate limiting
(`AddRateLimiter`) on the write endpoint regardless of auth.

#### 5.2 — FluentValidation validators are never invoked (High)

**Evidence:** `Application/DependencyInjection.cs` calls `AddValidatorsFromAssembly(assembly)`
(registers validators in DI) and `AddMediatR(...)`, but **no** `IPipelineBehavior`
(`ValidationBehavior<,>`) is registered and **no** `AddFluentValidationAutoValidation()` is wired
into MVC. `[ApiController]` auto-validates only `ModelState` (DataAnnotations), and
`CreateServiceOrderCommand` has no DataAnnotations. Net effect: the rules in
`CreateServiceOrderCommandValidator` (e.g. `CustomerEmail().EmailAddress()`, `MaximumLength`)
**do not run** for any request.

**Impact:** Malformed data is accepted — e.g. a syntactically invalid `CustomerEmail` is persisted.
Where the domain does *not* guard a field (phone, email, brand/model/serial), invalid values pass
straight through. Where the domain *does* guard (empty `CustomerName`/`EquipmentType`/
`ProblemDescription`), the failure arrives as an unhandled `ArgumentException` → HTTP 500 (see 5.3)
instead of a clean 400.

**Recommendation:** Add a MediatR `ValidationBehavior<TRequest,TResponse>` that resolves
`IValidator<TRequest>` and throws a `ValidationException` (mapped to 400) before the handler runs —
or register `AddFluentValidationAutoValidation()`. This is the same gap flagged when the earlier
use cases were generated; closing it fixes validation for all three endpoints at once.

#### 5.3 — No centralized error handling; exceptions leak as 500s (Medium)

**Evidence:** `Program.cs` has no `UseExceptionHandler`, no `AddProblemDetails`, no
`IExceptionHandler`. Domain constructors throw `ArgumentException`/`ArgumentNullException`
(`ServiceOrder`, `Customer`, `Equipment`). With `ASPNETCORE_ENVIRONMENT=Development`
(see `launchSettings.json`), `WebApplication` enables the Developer Exception Page, so unhandled
exceptions return **full stack traces** (type names, file paths, framework internals).

**Impact:** In Development, internal implementation details are exposed to any caller. In all
environments, validation-shaped failures return 500 rather than a structured 400, which is both
a poor contract and an availability signal (every bad input is an exception).

**Recommendation:** Add `builder.Services.AddProblemDetails()` and a global
`app.UseExceptionHandler()` (or an `IExceptionHandler`) that maps `ValidationException` → 400 and
unexpected exceptions → a generic 500 ProblemDetails with no internals. Ensure the Developer
Exception Page is only ever active in Development (it is, by default) and that the deployed
environment is not Development.

#### 5.4 — Effectively unbounded input length (Medium)

**Evidence:** `ServiceOrderConfiguration` declares `HasMaxLength(...)` on all string columns, but
the SQLite provider does **not** enforce column length at runtime, and the validator's
`MaximumLength` rules do not run (5.2). The only ceiling is Kestrel's default max request body
size (~28.6 MB).

**Impact:** A single request can persist multi-megabyte strings (e.g. `ProblemDescription`),
enabling storage abuse / memory pressure — amplified by the lack of auth (5.1).

**Recommendation:** Fixing 5.2 restores the `MaximumLength` enforcement. Additionally consider an
explicit per-endpoint request-size limit (`[RequestSizeLimit]`) for defense in depth.

#### 5.5 — HSTS not configured (Low)

**Evidence:** `app.UseHttpsRedirection()` is present; `app.UseHsts()` is not.

**Recommendation:** Add `app.UseHsts()` for non-Development environments so browsers pin HTTPS.

#### 5.6 — Fragile Location header (Low, robustness)

**Evidence:** `Create` returns `CreatedAtAction(nameof(Create), new { id = result.Id }, result)`,
but `Create` is the POST action and takes no `id` route value; there is no GET-by-id action.
Link generation to the wrong action can yield an incorrect Location or throw.

**Recommendation:** Once a `GET /api/service-orders/{id}` exists, point `CreatedAtAction` at it;
until then return `Created(string.Empty, result)` or a `201` without a synthesized Location.

---

### Confirmed well-implemented

- **No mass assignment / over-posting.** The bound contract is
  `CreateServiceOrderCommand`, which exposes **only** client-supplied fields (customer + equipment
  details, problem description). Server-controlled state — `Id`, `Status`, `TechnicianId` — is set
  inside the `ServiceOrder` constructor (`Id = Guid.NewGuid()`, `Status = Pending`,
  `TechnicianId = null`) and is **not** bindable from the request body. The command DTO acts as a
  proper allow-list.
- **No SQL injection via EF Core.** The write path uses only `DbSet.AddAsync` +
  `SaveChangesAsync`; a source grep found **no** `FromSqlRaw`/`ExecuteSqlRaw`/`SqlQuery` or string-
  concatenated SQL anywhere in `src/`. All persistence is parameterized by EF Core.
- **Domain invariants as defense in depth.** `ServiceOrder`, `Customer`, and `Equipment` reject
  null/whitespace required values in their constructors and trim inputs, so even with 5.2 unfixed
  the required-field invariants hold (they just surface as 500 instead of 400).
- **Transport security baseline.** `UseHttpsRedirection()` is enabled.
- **Schema hygiene.** Value objects are mapped as owned types with explicit column names and
  declared max lengths; `Status` is persisted as a constrained string. (Note the SQLite
  non-enforcement caveat in 5.4 — this is a modeling nicety, not a runtime guard.)

---

### Priority order

1. **5.1** — add authentication/authorization (+ rate limiting) to the write endpoint.
2. **5.2** — wire the FluentValidation pipeline so validation actually runs.
3. **5.3 / 5.4** — add ProblemDetails + global exception handling; restore length limits.
4. **5.5 / 5.6** — HSTS and Location-header cleanup.
