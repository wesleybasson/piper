using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using PipeR.AspNetCore.Attributes;
using PipeR.Core.Core;
using PipeR.Core.Middleware;
using System.Collections.Concurrent;

namespace PipeR.AspNetCore.Core;

public class AspNetCorePiper : IPiper
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private static readonly ConcurrentDictionary<(Type requestType, Type responseType), Type> HandlerTypeCache = new();

    public AspNetCorePiper(IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor)
    {
        _serviceProvider = serviceProvider;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var valves = GetMiddlewareForRequest(request);

        // Build the final handler delegate
        var key = (request.GetType(), typeof(TResponse));
        var handlerType = HandlerTypeCache.GetOrAdd(key, k => typeof(IRequestHandler<,>).MakeGenericType(k.requestType, k.responseType));
        dynamic handler = _serviceProvider.GetRequiredService(handlerType);

        // Use the builder to compose the pipeline
        var builder = new PipelineBuilder<IRequest<TResponse>, TResponse>(
            async (req, ct) => await handler.Handle((dynamic)req, ct)
        );

        // Add all endpoint-specific valves
        foreach (var valve in valves)
        {
            builder.Use(valve);
        }

        // Execute
        return await builder.ExecuteAsync(request, cancellationToken);
    }


    private IEnumerable<IValve<IRequest<TResponse>, TResponse>> GetMiddlewareForRequest<TResponse>(IRequest<TResponse> request)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context?.GetEndpoint() is not Endpoint endpoint)
            return [];

        var valvesAttribute = endpoint.Metadata.GetMetadata<ValvesAttribute>();
        if (valvesAttribute == null)
            return [];

        var valves = new List<IValve<IRequest<TResponse>, TResponse>>();
        foreach (var middlewareType in valvesAttribute.ValveTypes)
        {
            if (_serviceProvider.GetService(middlewareType) is IValve<IRequest<TResponse>, TResponse> valve)
            {
                valves.Add(valve);
            }
            else
            {
                throw new InvalidOperationException(
                    $"Middleware of type {middlewareType.FullName} does not implement IValve<IRequest<TResponse>, TResponse>.");
            }
        }
        return valves;
    }
}
