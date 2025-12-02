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
        // Build the final handler delegate
        var key = (request.GetType(), typeof(TResponse));
        var handlerType = HandlerTypeCache.GetOrAdd(key, k => typeof(IRequestHandler<,>).MakeGenericType(k.requestType, k.responseType));
        dynamic handler = _serviceProvider.GetRequiredService(handlerType);

        // Use the builder to compose the pipeline
        Type concreteRequestType = request.GetType();

        var builderType = typeof(PipelineBuilder<,>)
            .MakeGenericType(concreteRequestType, typeof(TResponse));

        var builder = (dynamic)Activator.CreateInstance(
            builderType,
            (Func<object, CancellationToken, Task<TResponse>>)
                (async (reqObj, ct) => await handler.Handle((dynamic)reqObj, ct)));

        // Add all endpoint-specific valves
        var valves = GetMiddlewareForRequest<TResponse>(request, concreteRequestType);
        foreach (var valve in valves)
        {
            builder.Use((dynamic)valve);
        }

        // Execute
        return await builder.ExecuteAsync((dynamic)request, cancellationToken);
    }


    private IEnumerable<object> GetMiddlewareForRequest<TResponse>(IRequest<TResponse> request, Type concreteRequestType)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context?.GetEndpoint() is not Endpoint endpoint)
            return Array.Empty<object>();

        var valvesAttribute = endpoint.Metadata.GetMetadata<ValvesAttribute>();
        if (valvesAttribute == null)
            return Array.Empty<object>();

        var adapters = new List<object>();
        foreach (var middlewareType in valvesAttribute.ValveTypes)
        {
            var service = _serviceProvider.GetService(middlewareType);
            if (service == null)
            {
                throw new InvalidOperationException(
                    $"Middleware of type {middlewareType.FullName} is not registered.");
            }

            // Ensure the resolved service implements IValve<IRequest<TResponse>, TResponse>
            var expectedInterface = typeof(IValve<,>).MakeGenericType(typeof(IRequest<>).MakeGenericType(typeof(TResponse)), typeof(TResponse));
            if (!expectedInterface.IsAssignableFrom(service.GetType()))
            {
                throw new InvalidOperationException(
                    $"Middleware of type {middlewareType.FullName} does not implement IValve<IRequest<{typeof(TResponse).FullName}>, {typeof(TResponse).FullName}.");
            }

            // Create an adapter that implements IValve<ConcreteRequest, TResponse>
            var adapterType = typeof(ValveAdapter<,>).MakeGenericType(concreteRequestType, typeof(TResponse));
            var adapter = Activator.CreateInstance(adapterType, service);
            adapters.Add(adapter!);
        }
        return adapters;
    }

    private class ValveAdapter<TReq, TRes> : IValve<TReq, TRes> where TReq : IRequest<TRes>
    {
        private readonly IValve<IRequest<TRes>, TRes> _inner;

        public ValveAdapter(IValve<IRequest<TRes>, TRes> inner)
        {
            _inner = inner;
        }

        public Task<TRes> Handle(TReq request, RequestHandlerDelegate<TRes> next, CancellationToken cancellationToken)
            => _inner.Handle(request, next, cancellationToken);
    }
}
