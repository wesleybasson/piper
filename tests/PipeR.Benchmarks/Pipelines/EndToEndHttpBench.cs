using BenchmarkDotNet.Attributes;
using PipeR.IntegrationTests.AspNetCore;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PipeR.Benchmarks.Pipelines;

public class EndToEndHttpBench
{
    private TestApplicationFactory? _app;
    private HttpClient _client = null!;

    [GlobalSetup]
    public void Setup()
    {
        _app = new TestApplicationFactory();  // inherits from WebApplicationFactory
        _client = _app.CreateClient();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _client?.Dispose();
        _app?.Dispose();
    }

    [Benchmark]
    public async Task<TestResponse?> HttpRequest_PipelineExecution()
    {
        return await _client.GetFromJsonAsync<TestResponse>(
            "/test?valueIn=10&originalString=abc");
    }
}
