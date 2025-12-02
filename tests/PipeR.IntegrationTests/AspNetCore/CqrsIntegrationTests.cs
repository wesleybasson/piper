using System.Net.Http.Json;

namespace PipeR.IntegrationTests.AspNetCore;

public class CqrsIntegrationTests
{
    [Fact]
    public async Task TestQuery_Flow_Through_Controller_And_Piper_Works()
    {
        using var app = new TestApplicationFactory();  // inherits from WebApplicationFactory
        var client = app.CreateClient();

        var response = await client.GetAsync("/test?valueIn=10&originalString=hello");
        var body = await response.Content.ReadFromJsonAsync<TestResponse>();

        Assert.Equal(11, body.ValueOut);
        Assert.Equal("Mutated hello", body.MutatedString);
    }
}
