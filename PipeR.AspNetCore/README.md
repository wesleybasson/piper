# PipeR.AspNetCore

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

## PipeR.Extensions.AspNetCore

`PipeR.Extensions.AspNetCore` builds on top of `PipeR.Core` to provide ASP.NET Core specific integrations and helpers for using the lightweight **PipeR** mediator inside web API applications.

## What it is
- Middleware, extensions and DI helper methods that make it straightforward to register and use `PipeR.Core` in ASP.NET Core applications.
- Provides conventional wiring for handlers, scoped dependencies and optional request/response pipeline integrations useful in HTTP APIs.

## Key scenarios
- Register the mediator and handler discovery during application startup
- Integrate request/response handling into controller actions or minimal APIs
- Publish domain notifications or application events from web requests

## Supported frameworks
- .NET 9 (.NET 9.0)
- .NET 8 (.NET 8.0)

## Quick build
- Build: `dotnet build` (from repo root) or `dotnet build ./PipeR.AspNetCore/`
- Test: `dotnet test` (if this project or solution contains tests)
- Pack (create NuGet): `dotnet pack ./PipeR.AspNetCore/ -c Release`

## Getting started
- Add a project reference: `dotnet add <YourProject> reference ./PipeR.AspNetCore/PipeR.AspNetCore.csproj`
- From `Program.cs` call the provided extension methods to register services and handlers. Inject the mediator into controllers, services, or minimal API delegates.

## Examples and API details
- See the source code for the available extension methods and sample integrations. If you want a short usage snippet or starter example added here, tell me which APIs you prefer and I will add it.

## Contributing and license
- See the repository root for `CONTRIBUTING.md` and `LICENSE`.
