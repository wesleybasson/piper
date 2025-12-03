using PipeR.Core.Core;
using System.Reflection;

namespace PipeR.Core.Tests.Core;

public class PiperTests
{
    [Fact]
    public async Task Send_Resolves_Handler_And_ReturnsResponse()
    {
        var handlers = new Dictionary<(Type, Type), object>
        {
            {(typeof(TestRequest), typeof(string)), new TestRequestHandler() }
        };
        var valves = new Dictionary<(Type, Type), List<object>>();

        var piper = new Piper(handlers, valves);
        var result = await piper.Send(new TestRequest());

        Assert.Equal("handled", result);
    }

    [Fact]
    public async Task Send_Uses_RuntimeRequestType()
    {
        var handlers = new Dictionary<(Type, Type), object>
        {
            {(typeof(DerivedRequest), typeof(string)), new DerivedRequestHandler() }
        };
        var valves = new Dictionary<(Type, Type), List<object>>();

        var piper = new Piper(handlers, valves);
        IRequest<string> req = new DerivedRequest();
        var result = await piper.Send(req);

        Assert.Equal("derived", result);
    }

    [Fact]
    public async Task Send_Throws_When_Handler_NotRegistered()
    {
        var handlers = new Dictionary<(Type, Type), object>();
        var valves = new Dictionary<(Type, Type), List<object>>();

        var piper = new Piper(handlers, valves);

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await piper.Send(new TestRequest());
        });
    }

    [Fact]
    public async Task Send_Populates_HandlerTypeCache()
    {
        var handlers = new Dictionary<(Type, Type), object>
        {
            {(typeof(TestRequest), typeof(string)), new TestRequestHandler() }
        };
        var valves = new Dictionary<(Type, Type), List<object>>();

        var piper = new Piper(handlers, valves);

        // Ensure invokers dictionary is accessible (private instance field)
        var field = typeof(Piper).GetField("_invokers", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);

        dynamic dict = field.GetValue(piper);
        int initialCount = (int)dict.Count;
        Assert.Equal(1, initialCount);

        await piper.Send(new TestRequest());
        await piper.Send(new TestRequest());

        int count = (int)dict.Count;
        Assert.Equal(1, count);
    }
}

// --- Test types ---
public class TestRequest : IRequest<string> { }

public class TestRequestHandler : IRequestHandler<TestRequest, string>
{
    public Task<string> Handle(TestRequest request, CancellationToken cancellationToken) => Task.FromResult("handled");
}

public class DerivedRequest : IRequest<string> { }

public class DerivedRequestHandler : IRequestHandler<DerivedRequest, string>
{
    public Task<string> Handle(DerivedRequest request, CancellationToken cancellationToken) => Task.FromResult("derived");
}
