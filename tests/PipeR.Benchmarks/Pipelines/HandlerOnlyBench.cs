using BenchmarkDotNet.Attributes;
using PipeR.Core.Core;
using PipeR.IntegrationTests.AspNetCore;
using System.Threading.Tasks;

namespace PipeR.Benchmarks.Pipelines;

public class HandlerOnlyBench
{
    private IPiper _piper = null!;
    private TestQuery _query = null!;

    [GlobalSetup]
    public void Setup()
    {
        _piper = new PipelineBuilder()
            .AddRequestHandler(new TestQueryHandler())
            .Build();

        _query = new TestQuery { ValueIn = 10, OriginalString = "abc" };
    }

    [Benchmark(Baseline = true)]
    public Task<TestResponse> Execute_HandlerOnly() =>
        _piper.Send(_query);
}

