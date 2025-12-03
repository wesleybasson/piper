using PipeR.Core.Core;

namespace PipeR.Core.Tests.Core;

public class PipelineBuilderTests
{
    private static readonly string[] expected = ["A", "B", "final", "B-end", "A-end"];

    [Fact]
    public async Task Pipeline_Executes_Valves_In_Correct_Order()
    {
        var log = new List<string>();

        var builder = new PipelineBuilder<DummyRequest, string>(
            (req, ct) => { log.Add("final"); return Task.FromResult("done"); });

        builder.Use(async (req, next, ct) =>
        {
            log.Add("A");
            var result = await next(req, ct);
            log.Add("A-end");
            return result;
        });

        builder.Use(async (req, next, ct) =>
        {
            log.Add("B");
            var result = await next(req, ct);
            log.Add("B-end");
            return result;
        });

        await builder.ExecuteAsync(new DummyRequest());

        Assert.Equal(expected, log);
    }

    [Fact]
    public async Task Pipeline_Allows_ShortCircuit()
    {
        bool finalCalled = false;

        var builder = new PipelineBuilder<DummyRequest, string>(
            (req, ct) => { finalCalled = true; return Task.FromResult("final"); });

        builder.Use((req, next, ct) => Task.FromResult("stop"));

        var result = await builder.ExecuteAsync(new DummyRequest());

        Assert.Equal("stop", result);
        Assert.False(finalCalled);
    }

    [Fact]
    public async Task Pipeline_InlineValve_Works_As_Normal_Valve()
    {
        bool valveCalled = false;

        var builder = new PipelineBuilder<DummyRequest, string>(
            (req, ct) => Task.FromResult("ok"));

        builder.Use((req, next, ct) =>
        {
            valveCalled = true;
            return next(req, ct);
        });

        await builder.ExecuteAsync(new DummyRequest());

        Assert.True(valveCalled);
    }

    [Fact]
    public async Task Pipeline_Passes_CancellationToken_To_Valves()
    {
        CancellationToken received = default;

        var builder = new PipelineBuilder<DummyRequest, string>(
            (req, ct) => Task.FromResult("ok"));

        builder.Use((req, next, ct) =>
        {
            received = ct;
            return next(req, ct);
        });

        using var cts = new CancellationTokenSource();
        await builder.ExecuteAsync(new DummyRequest(), cts.Token);

        Assert.Equal(cts.Token, received);
    }
}

public class DummyRequest : IRequest<string>
{
}
