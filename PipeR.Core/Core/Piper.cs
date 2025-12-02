using Microsoft.Extensions.DependencyInjection;
using PipeR.Core.Middleware;
using System.Collections.Concurrent;

namespace PipeR.Core.Core;

public class Piper : IPiper
{
    private readonly IServiceProvider _serviceProvider;
    private static readonly ConcurrentDictionary<(Type requestType, Type responseType), Type> HandlerTypeCache = new();

    public Piper(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        // No valves here — just go straight to the handler
        RequestHandlerDelegate<TResponse> handlerDelegate = async () =>
        {
            var key = (request.GetType(), typeof(TResponse));
            var handlerType = HandlerTypeCache.GetOrAdd(key, k => typeof(IRequestHandler<,>).MakeGenericType(k.requestType, k.responseType));
            dynamic handler = _serviceProvider.GetRequiredService(handlerType);
            return await handler.Handle((dynamic)request, cancellationToken);
        };

        return handlerDelegate();
    }
}

