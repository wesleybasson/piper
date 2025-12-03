# PipeR.Core

## PipeR Intro

**PipeR** is a fast, modern, and fully open-source pipeline engine for .NET - designed as a drop-in alternative to commercialized middleware and mediator frameworks.
Born out of necessity, **PipeR** fills the growing gap left by libraries that have shifted behind paywalls or restrictive licensing.
Instead of locking critical infrastructure behind subscriptions, **PipeR** gives developers a lightweight, high-performance pipeline model that is free to use in development *and* production, with no limits.

PipeR focuses on:

- Performance-first design - minimal allocations, low overhead, and lean abstractions.
- Predictable architecture - request/response pipelines, valves (middleware), and handlers that feel familiar yet faster.
- Flexibility without bloat — simple enough for small projects, powerful enough for enterprise workloads.
- Full OSS freedom — MIT licensed, transparent, community-driven.

Whether you use `PipeR.Core` in your backend services or `PipeR.Extensions.AspNetCore` for effortless request pipelines in web APIs, the goal is the same:
a modern pipeline framework that stays open, fast, and in your control.

## PipeR.Core

**PipeR** is a lightweight, bare-bones, publicly available alternative to other mediator frameworks. `PipeR.Core` contains the core, framework-agnostic implementation of the in-process mediator used across the repository.

## What it is
- A minimal mediator implementation for sending requests and publishing notifications within a single process.
- Designed to be small, easy to understand and to integrate into vanilla .NET applications without extra runtime dependencies.

## Key scenarios
- Request/response handling (in-process commands/queries)
- Notification/event publishing to zero-or-more handlers
- Easy dependency injection registration and handler discovery

## Supported frameworks
- .NET 9 (.NET 9.0)
- .NET 8 (.NET 8.0)

## Quick build
- Build: `dotnet build` (from repo root) or `dotnet build ./PipeR.Core/`
- Test: `dotnet test` (if this project or solution contains tests)
- Pack (create NuGet): `dotnet pack ./PipeR.Core/ -c Release`

## Getting started
- Add a project reference: `dotnet add <YourProject> reference ./PipeR.Core/PipeR.Core.csproj`
- Register the core services in your dependency injection container and inject the mediator into your classes. Use the mediator to send requests or publish notifications from application code.

## Examples and API details
- See the source code for concrete types and examples. If you want a short usage snippet or starter example added here, tell me which APIs you prefer and I will add it.

## Contributing and license
- See the repository root for `CONTRIBUTING.md` and `LICENSE`.
