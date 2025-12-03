using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PipeR.AspNetCore.Attributes;
using PipeR.AspNetCore.Extensions;
using PipeR.Core.Core;
using PipeR.IntegrationTests.TestHelpers;
using System.Reflection;
using System.Text.Json;

namespace PipeR.IntegrationTests.AspNetCore;

public class TestApplicationFactory : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        var hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseTestServer();

                webBuilder.ConfigureServices(services =>
                {
                    // Register PipeR services scanning current assembly
                    services.AddPipeR(options => options.AssemblyToScan = Assembly.GetExecutingAssembly());

                    // Register our test handler and valves
                    services.AddTransient<IRequestHandler<TestQuery, TestResponse>, TestQueryHandler>();
                    services.AddTransient<BeforeValve>();
                    services.AddTransient<AfterValve>();

                    // Tracker used to assert valve execution order
                    services.AddSingleton(new List<string>());
                });

                webBuilder.Configure(app =>
                {
                    app.UseRouting();

                    app.UseEndpoints(endpoints =>
                    {
                        // Apply ValvesAttribute on endpoint so AspNetCorePiper will pick them up
                        var metadata = new ValvesAttribute(typeof(BeforeValve), typeof(AfterValve));

                        endpoints.MapGet("/test", async (HttpContext context) =>
                        {
                            var qs = context.Request.Query;
                            if (!int.TryParse(qs["valueIn"], out var valueIn))
                            {
                                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                                await context.Response.WriteAsync("Missing or invalid 'valueIn'");
                                return;
                            }

                            var originalString = qs["originalString"].ToString();

                            var piper = context.RequestServices.GetRequiredService<IPiper>();
                            var query = new TestQuery { ValueIn = valueIn, OriginalString = originalString };
                            var result = await piper.Send(query);

                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsync(JsonSerializer.Serialize(result));
                        }).WithMetadata(metadata);
                    });
                });
            });

        var host = hostBuilder.Start();
        return host;
    }
}

public class TestQuery : IRequest<TestResponse>
{
    public int ValueIn { get; set; }
    public string OriginalString { get; set; } = string.Empty;
}

// Dummy Program type so WebApplicationFactory<Program> can target this assembly
public class Program { }
