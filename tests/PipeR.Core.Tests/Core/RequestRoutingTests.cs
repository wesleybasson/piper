using NSubstitute;
using PipeR.Core.Core;

namespace PipeR.Core.Tests.Core;

public class RequestRoutingTests
{
    [Fact]
    public async Task Piper_Send_ForQuery_InvokesHandler_AndReturnsResponse()
    {
        // Arrange
        var handler = Substitute.For<IRequestHandler<RoutingTestQuery, RoutingTestResponse>>();
        handler.Handle(Arg.Any<RoutingTestQuery>(), Arg.Any<CancellationToken>())
               .Returns(new RoutingTestResponse { ValueOut = 42, MutatedString = "Mutated abc" });

        var piper = new PiperBuilder()
           .AddRequestHandler(handler)
           .Build();

        var query = new RoutingTestQuery { ValueIn = 40, OriginalString = "abc" };

        // Act
        var result = await piper.Send(query);

        // Assert
        Assert.Equal(42, result.ValueOut);
        Assert.Equal("Mutated abc", result.MutatedString);
        await handler.Received(1).Handle(query, Arg.Any<CancellationToken>());
    }
}

public class RoutingTestRequest
{
    public int ValueIn { get; set; }
    public string OriginalString { get; set; } = string.Empty;
}

public class RoutingTestResponse
{
    public int ValueOut { get; set; }
    public string MutatedString { get; set; } = string.Empty;
}

public class RoutingTestQuery : RoutingTestRequest, IRequest<RoutingTestResponse>
{
    public int ValueIn { get; set; }
    public string OriginalString { get; set; } = string.Empty;
}
