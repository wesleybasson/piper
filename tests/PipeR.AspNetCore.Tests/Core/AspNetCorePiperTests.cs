using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using PipeR.AspNetCore.Attributes;
using PipeR.AspNetCore.Core;
using PipeR.Core.Core;
using PipeR.Core.Middleware;

namespace PipeR.AspNetCore.Tests.Core;

public class AspNetCorePiperTests
{
    [Fact]
    public async Task Piper_NoEndpoint_InvokesHandlerOnly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<TestHandler>();
        services.AddSingleton<IRequestHandler<TestReq, string>>(sp => sp.GetRequiredService<TestHandler>());
        var provider = services.BuildServiceProvider();

        var http = new DefaultHttpContext();
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(http);

        var piper = new AspNetCorePiper(provider, accessor);

        var req = new TestReq("X");

        // Act
        var response = await piper.Send(req);

        // Assert
        Assert.Equal("OK_X", response);
        Assert.Equal(["handler"], provider.GetRequiredService<TestHandler>().Log);
    }

    [Fact]
    public async Task Piper_EndpointHasValves_ExecutesValvesInOrder()
    {
        // Arrange
        var log = new List<string>();

        var services = new ServiceCollection();
        var handler = new TestHandler(log);
        services.AddSingleton<TestHandler>(handler);
        services.AddSingleton<IRequestHandler<TestReq, string>>(sp => sp.GetRequiredService<TestHandler>());
        services.AddTransient(typeof(TestValve), sp => new TestValve(log, "1"));
        services.AddTransient(typeof(TestValve2), sp => new TestValve2(log)); // example second valve
        var provider = services.BuildServiceProvider();

        var endpoint = new Endpoint(
            requestDelegate: async ctx => await Task.CompletedTask,
            metadata: new EndpointMetadataCollection(
                new ValvesAttribute(typeof(TestValve), typeof(TestValve2))
            ),
            displayName: "test"
        );

        var http = new DefaultHttpContext();
        http.SetEndpoint(endpoint);

        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(http);

        var piper = new AspNetCorePiper(provider, accessor);

        // Act
        var result = await piper.Send(new TestReq("A"));

        // Assert
        Assert.Equal("OK_A", result);

        var actualLog = provider.GetRequiredService<TestHandler>().Log;
        Assert.Collection(actualLog,
            e => Assert.Equal("valve_1_before", e),
            e => Assert.Equal("valve_2_before", e),
            e => Assert.Equal("handler", e),
            e => Assert.Equal("valve_2_after", e),
            e => Assert.Equal("valve_1_after", e)
        );
    }

    [Fact]
    public async Task Piper_ValveNotRegistered_Throws()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IRequestHandler<TestReq, string>, TestHandler>();
        var provider = services.BuildServiceProvider();

        var endpoint = new Endpoint(
            _ => Task.CompletedTask,
            new EndpointMetadataCollection(new ValvesAttribute(typeof(TestValve))),
            "test"
        );

        var http = new DefaultHttpContext();
        http.SetEndpoint(endpoint);

        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(http);

        var piper = new AspNetCorePiper(provider, accessor);

        // Act + Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            piper.Send(new TestReq("fail")));
    }

    [Fact]
    public async Task Piper_WrongValveType_Throws()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IRequestHandler<TestReq, string>, TestHandler>();
        services.AddSingleton<NotAValve>();
        var provider = services.BuildServiceProvider();

        var endpoint = new Endpoint(
            _ => Task.CompletedTask,
            new EndpointMetadataCollection(new ValvesAttribute(typeof(NotAValve))),
            "test"
        );

        var http = new DefaultHttpContext();
        http.SetEndpoint(endpoint);

        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(http);

        var piper = new AspNetCorePiper(provider, accessor);

        // Act + Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            piper.Send(new TestReq("bad")));
    }

    [Fact]
    public async Task Piper_CachesHandlerType()
    {
        // Arrange
        var callCount = 0;

        var services = new ServiceCollection();
        services.AddTransient<IRequestHandler<TestReq, string>>(sp =>
        {
            callCount++;
            return new TestHandler();
        });
        var provider = services.BuildServiceProvider();

        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(new DefaultHttpContext());

        var piper = new AspNetCorePiper(provider, accessor);

        // Act
        await piper.Send(new TestReq("1"));
        await piper.Send(new TestReq("2"));

        // Assert
        Assert.Equal(2, callCount);
    }
}

public record TestReq(string Value) : IRequest<string>;

public class TestHandler : IRequestHandler<TestReq, string>
{
    public List<string> Log = new();

    public TestHandler() { }

    public TestHandler(List<string> log)
    {
        Log = log;
    }

    public Task<string> Handle(TestReq request, CancellationToken cancellationToken)
    {
        Log.Add("handler");
        return Task.FromResult("OK_" + request.Value);
    }
}

public class TestValve : IValve<IRequest<string>, string>
{
    private readonly List<string> _log;
    private readonly string _id;

    public TestValve(List<string> log, string id)
    {
        _log = log;
        _id = id;
    }

    public async Task<string> Handle(IRequest<string> request, RequestHandlerDelegate<IRequest<string>, string> next, CancellationToken cancellationToken)
    {
        _log.Add("valve_" + _id + "_before");
        var result = await next(request, cancellationToken);
        _log.Add("valve_" + _id + "_after");
        return result;
    }
}

public class TestValve2 : IValve<IRequest<string>, string>
{
    private readonly List<string> _log;
    private readonly string _id;

    public TestValve2(List<string> log)
    {
        _log = log;
        _id = "2";
    }

    public async Task<string> Handle(IRequest<string> request, RequestHandlerDelegate<IRequest<string>, string> next, CancellationToken cancellationToken)
    {
        _log.Add("valve_2_before");
        var result = await next(request, cancellationToken);
        _log.Add("valve_2_after");
        return result;
    }
}


public class NotAValve { }
