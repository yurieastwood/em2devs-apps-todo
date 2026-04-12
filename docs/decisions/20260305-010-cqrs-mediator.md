# CQRS/Mediator — Custom Lightweight Mediator + Domain Events

- Status: Accepted
- Date: 2026-03-05

## Context and Problem Statement

The application needs a clean architectural separation between operations that change state (commands) and operations that retrieve data (queries). This CQRS pattern reduces coupling between the write model and read model, and makes each handler independently testable. Alongside CQRS, a mediator is needed to dispatch commands and queries to their handlers without direct coupling between callers and handlers. Additionally, domain events are required to propagate side effects — particularly gamification triggers such as XP awards and achievement checks — without polluting command handlers with cross-cutting concerns. Which mediator library or approach should implement this pattern?

## Decision Drivers

- Licensing: must be usable commercially without per-deployment fees
- Simplicity: the mediator pattern itself is not complex; the solution should match that simplicity
- Dependency count: avoid pulling in a heavy framework for a thin abstraction
- Testability: handlers must be easily unit-testable in isolation
- Extensibility: support for pipeline behaviours (validation, logging, error handling) as cross-cutting concerns
- Domain event support: ability to publish notifications that fan out to multiple handlers

## Considered Options

- MediatR (commercial license as of v12+)
- Custom lightweight mediator
- Wolverine
- Immediate.Handlers

## Decision Outcome

Chosen option: "Custom lightweight mediator", because the mediator pattern is trivially implementable in approximately 30–50 lines of C# using .NET's built-in dependency injection. MediatR became commercial in v12+, making it unsuitable for unrestricted use. The core interfaces — `IRequest<TResponse>`, `IRequestHandler<TRequest, TResponse>`, `INotification`, `INotificationHandler<T>` — and a dispatcher that resolves handlers from the DI container cover all requirements without external dependencies.

Domain events follow the `INotification` pattern and are published after `SaveChanges` to ensure consistency. The gamification chain — `TodoCompleted` -> `XpAwarded` -> `AchievementChecked` — is implemented as a sequence of notification handlers. Pipeline behaviours are registered as decorators in DI to handle validation, logging, and error normalisation as cross-cutting concerns.

### Positive Consequences

- Zero licensing cost or risk — we own the implementation entirely
- Interfaces are identical in shape to MediatR v11 (pre-commercial), making future migration trivial if needed
- Each command and query handler is a plain class with a single dependency — straightforward to unit test without mocking a mediator framework
- Pipeline behaviours are explicit decorator registrations in DI, visible and debuggable without framework magic
- Domain event fan-out via `INotification` cleanly decouples gamification side effects from core command logic
- No transitive NuGet dependencies introduced

### Negative Consequences

- The implementation must be written and owned by the team; bugs are ours to fix
- Community resources (blog posts, Stack Overflow answers) will reference MediatR rather than our custom implementation
- Advanced MediatR features (streams via `IStreamRequest`, `RequestPreProcessor` / `RequestPostProcessor` chains) are not available out of the box and must be implemented if required

### Neutral

- The ~30–50 line implementation lives in a `Mediator/` folder in the application layer and is immediately readable by any developer familiar with the pattern
- If requirements grow significantly (distributed messaging, sagas, outbox), Wolverine remains a viable upgrade path without requiring a full architecture change

## Pros and Cons of the Options

### MediatR (commercial license as of v12+)

The de facto .NET mediator library, widely adopted and well-documented. From v12 onward, commercial use requires a paid license.

- Good, because extensive community documentation and examples
- Good, because full-featured: pipeline behaviours, streaming requests, pre/post processors
- Good, because familiar to most .NET developers
- Bad, because v12+ requires a commercial license for production use — unacceptable ongoing cost for a thin abstraction we can own ourselves
- Bad, because locking in a commercial dependency for ~50 lines of infrastructure code is disproportionate

### Custom lightweight mediator

A hand-written implementation of the mediator pattern using interfaces resolved from .NET's built-in DI container.

- Good, because zero licensing cost or risk
- Good, because the team owns the code — no upstream breaking changes
- Good, because the implementation is small enough for any team member to read and understand in minutes
- Good, because interface shape can mirror MediatR v11 for community familiarity
- Bad, because the team is responsible for any bugs or gaps in the implementation
- Bad, because advanced features must be built when needed rather than pulled from a library

### Wolverine

A full-featured .NET messaging and command-handling framework with built-in support for outbox patterns, sagas, and distributed messaging.

- Good, because comprehensive feature set covering messaging, scheduling, and saga coordination
- Good, because built-in outbox support ensures reliable message delivery
- Good, because open-source and actively maintained
- Bad, because significantly more than needed for an in-process mediator — brings distributed messaging infrastructure into scope
- Bad, because heavier framework increases onboarding complexity and startup configuration
- Bad, because coupling application architecture to Wolverine's conventions is a larger commitment than a simple mediator

### Immediate.Handlers

A source-generator-based mediator for .NET that generates handler dispatch code at compile time.

- Good, because compile-time dispatch eliminates reflection overhead at runtime
- Good, because open-source with no licensing restrictions
- Bad, because smaller community and less battle-tested than MediatR or Wolverine
- Bad, because source generator tooling can complicate debugging and IDE integration
- Bad, because the compile-time generation model is less flexible for dynamic pipeline behaviour composition

## More Information

- [CQRS pattern overview — Microsoft Architecture Guides](https://learn.microsoft.com/en-us/azure/architecture/patterns/cqrs)
- [MediatR licensing change announcement (v12)](https://github.com/jbogard/MediatR/discussions/969)
- [Wolverine documentation](https://wolverinefx.net/)
- [Immediate.Handlers GitHub](https://github.com/ImmediateActions/Immediate.Handlers)
- Related: [ADR-009](20260305-009-data-access.md) — EF Core (writes) + Dapper (reads), split along the command/query boundary established here
- Related: [ADR-018](20260305-018-error-handling.md) — Error handling implemented as a pipeline behaviour
- Related: [ADR-004](20260305-004-api-style.md) — SignalR real-time updates triggered via domain events published through this mediator
