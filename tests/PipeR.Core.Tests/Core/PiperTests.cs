namespace PipeR.Core.Tests.Core;

using Microsoft.Extensions.DependencyInjection;
using PipeR.Core.Core;
using System.Reflection;
using Xunit;

public class PiperTests
{
    [Fact]
    public async Task Send_Resolves_Handler_And_ReturnsResponse()
    {
        var services = new ServiceCollection();
        services.AddTransient<IRequestHandler<TestRequest, string>, TestRequestHandler>();
        var provider = services.BuildServiceProvider();

        var piper = new Piper(provider);
        var result = await piper.Send(new TestRequest());

        Assert.Equal("handled", result);
    }

    [Fact]
    public async Task Send_Uses_RuntimeRequestType()
    {
        var services = new ServiceCollection();
        services.AddTransient<IRequestHandler<DerivedRequest, string>, DerivedRequestHandler>();
        var provider = services.BuildServiceProvider();

        var piper = new Piper(provider);
        IRequest<string> req = new DerivedRequest();
        var result = await piper.Send(req);

        Assert.Equal("derived", result);
    }

    [Fact]
    public async Task Send_Throws_When_Handler_NotRegistered()
    {
        var services = new ServiceCollection();
        var provider = services.BuildServiceProvider();

        var piper = new Piper(provider);

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await piper.Send(new TestRequest());
        });
    }

    [Fact]
    public async Task Send_Populates_HandlerTypeCache()
    {
        var services = new ServiceCollection();
        services.AddTransient<IRequestHandler<TestRequest, string>, TestRequestHandler>();
        var provider = services.BuildServiceProvider();

        var piper = new Piper(provider);

        // Ensure cache is empty first (field is static)
        var field = typeof(Piper).GetField("HandlerTypeCache", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(field);

        // Clear dictionary if needed
        var dict = field.GetValue(null);
        // If possible, clear existing entries via reflection
        var clearMethod = dict?.GetType().GetMethod("Clear");
        clearMethod?.Invoke(dict, null);

        await piper.Send(new TestRequest());
        await piper.Send(new TestRequest());

        dynamic d = field.GetValue(null);
        int count = (int)d.Count;
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
