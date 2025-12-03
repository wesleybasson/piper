# PipeR

![NuGet Core](https://img.shields.io/nuget/v/PipeR.Core?label=PipeR.Core&logo=nuget)
![NuGet AspNetCore](https://img.shields.io/nuget/v/PipeR.Extensions.AspNetCore?label=PipeR.Extensions.AspNetCore&logo=nuget)
![Build](https://img.shields.io/github/actions/workflow/status/wesleybasson/PipeR/publish-nuget.yml)
![License](https://img.shields.io/github/license/wesleybasson/PipeR)

## PipeR Intro

**PipeR** is a fast, modern, and fully open-source pipeline engine for .NET - designed as a drop-in alternative to commercialized middleware and mediator frameworks.
Born out of necessity, **PipeR** fills the growing gap left by libraries that have shifted behind paywalls or restrictive licensing.
Instead of locking critical infrastructure behind subscriptions, **PipeR** gives developers a lightweight, high-performance pipeline model that is free to use in development *and* production, with no limits.

PipeR focuses on:

- Performance-first design - minimal allocations, low overhead, and lean abstractions.
- Predictable architecture - request/response pipelines, valves (middleware), and handlers that feel familiar yet faster.
- Flexibility without bloat - simple enough for small projects, powerful enough for enterprise workloads.
- Full OSS freedom - MIT licensed, transparent, community-driven.

Whether you use `PipeR.Core` in your backend services or `PipeR.Extensions.AspNetCore` for effortless request pipelines in web APIs, the goal is the same:
a modern pipeline framework that stays open, fast, and in your control.

## How PipeR works

### Code Example: CQRS with PipeR

A simple end-to-end example demonstrating how **PipeR** integrates cleanly into an ASP.NET Core application using a CQRS-style request/response pipeline.

1. **Controller Usage**

```csharp
[Route("api/[controller]")]
[ApiController]
public class ExampleController : ControllerBase
{
    private readonly IPiper _piper;

    public ExampleController(IPiper piper) => _piper = piper;

    [HttpGet]
    public async Task<ActionResult<ExampleResponse>> GetExample([FromQuery] ExampleQuery query)
    {
        var response = await _piper.Send(query);

        // Optional: If you include validation valves, validation errors
        // can short-circuit the pipeline before your handler executes.

        return Ok(response);
    }
}
```

2. **Request and Response Models**

```csharp
public class ExampleRequest
{
    // Define your incoming parameters here.
}

public class ExampleResponse
{
    // Define your outgoing response fields here.
}
```

3. **Query Definition**

```csharp
public class ExampleQuery : ExampleRequest, IRequest<ExampleResponse>
{
}
```

4. **Handler Definition**

```csharp
public class ExampleQueryHandler : IRequestHandler<ExampleQuery, ExampleResponse>
{
    public async Task<ExampleResponse> Handle(ExampleQuery request, CancellationToken ct)
    {
        // Your business logic here
        await Task.Delay(10, ct); // simulate async work

        return new ExampleResponse
        {
            // Set properties...
        };
    }
}
```

   Note:
   PipeR supports dependency injection into handlers.
   Example
   ```csharp
   private readonly IExampleRepository _repo;
   public ExampleQueryHandler(IExampleRepository repo) => _repo = repo;
   ```

5. **Registration**

**PipeR** integrates naturally with the .NET dependency injection system.
A typical setup looks like this:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Register PipeR core services and specify assembly to scan
    services.AddPipeR(options =>
    {
        options.AssemblyToScan = typeof(Program).Assembly; // or Assembly.GetExecutingAssembly()
    });

    services.AddControllers();
}
```
Or using the new minimal hosting model:
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPipeR(options =>
{
    options.AssemblyToScan = typeof(Program).Assembly; // or Assembly.GetExecutingAssembly()
});

var app = builder.Build();

app.MapControllers();
app.Run();
```

## Want to Contribute?

### Getting Started

**Quick links**
- License: `LICENSE`
- Contributing: `CONTRIBUTING.md`

**Prerequisites**
- .NET SDK 8.0+ (or the SDK version used by the solution). The libraries target `net8.0` and `net9.0`; tests and benchmarks target `net10.0`. Install an SDK that covers the highest framework you intend to build (for example, .NET 10 SDK to run all tests/benchmarks).

**Build**
```pwsh
dotnet build PipeR.sln
```

**Run tests**
```pwsh
dotnet test
```

**Benchmarks**
Navigate to the Benchmarks project folder `tests/PipeR.Benchmarks/` and run:
```pwsh
dotnet run -c Release
```

**Where to look**
- Core library: `PipeR.Core/`
- ASP.NET Core integration: `PipeR.AspNetCore/`

**Questions or issues**
- Open an issue or a pull request. See `CONTRIBUTING.md` for guidelines.

**License**
- This project is available under the terms of the `MIT` license. See `LICENSE`.
