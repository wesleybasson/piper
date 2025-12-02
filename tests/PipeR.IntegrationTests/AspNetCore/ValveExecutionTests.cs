using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;

namespace PipeR.IntegrationTests.AspNetCore;

public class ValveExecutionTests : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory _factory;

    public ValveExecutionTests(TestApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task FullPipeline_Executes_Valves_And_Handler_InOrder()
    {
        // Arrange
        var client = _factory.CreateClient();
        var tracker = _factory.Services.GetRequiredService<List<string>>();
        tracker.Clear();

        // Act
        var result = await client.GetFromJsonAsync<TestResponse>(
            "/test?valueIn=10&originalString=abc"
        );

        // Assert: response correctness
        Assert.NotNull(result);
        Assert.Equal(11, result!.ValueOut); // handler adds +1
        Assert.Equal("Mutated abc", result.MutatedString);

        // Assert: pipeline execution order
        var expectedOrder = new[]
        {
            "before",
            "handler",
            "after"
        };

        Assert.Equal(expectedOrder, tracker);
    }
}
