using BenchmarkDotNet.Attributes;
using PipeR.Core.Core;
using PipeR.IntegrationTests.AspNetCore;
using PipeR.IntegrationTests.TestHelpers;
using System.Threading.Tasks;

namespace PipeR.Benchmarks.Pipelines;

public class ValvePipelineBench
{
    private IPiper _pipeline1 = null!;
    private IPiper _pipeline3 = null!;
    private TestQuery _query = null!;

    [GlobalSetup]
    public void Setup()
    {
        var handler = new TestQueryHandler();
        _query = new TestQuery { ValueIn = 10, OriginalString = "abc" };

        _pipeline1 = new PiperBuilder()
            .AddValve(new LoggingValve())
            .AddRequestHandler(handler)
            .Build();

        _pipeline3 = new PiperBuilder()
            .AddValve(new LoggingValve())
            .AddValve(new ValidationValve())
            .AddValve(new AuthorizationValve())
            .AddRequestHandler(handler)
            .Build();
    }

    [Benchmark]
    public Task<TestResponse> Execute_1Valve() => _pipeline1.Send(_query);

    [Benchmark]
    public Task<TestResponse> Execute_3Valves() => _pipeline3.Send(_query);
}

