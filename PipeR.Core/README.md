# PipeR.Core

PipeR is a lightweight, bare?bones, publicly available alternative to MediatR. `PipeR.Core` contains the core, framework-agnostic implementation of the in?process mediator used across the repository.

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
