using PipeR.Core.Middleware;
using System.Collections.Concurrent;

namespace PipeR.Core.Core;

public class PipelineBuilder<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly List<IValve<TRequest, TResponse>> _valves = new();
    private readonly Func<object, CancellationToken, Task<TResponse>> _finalHandler;

    public PipelineBuilder(Func<object, CancellationToken, Task<TResponse>> finalHandler)
    {
        _finalHandler = finalHandler ?? throw new ArgumentNullException(nameof(finalHandler));
    }

    public PipelineBuilder<TRequest, TResponse> Use(IValve<TRequest, TResponse> valve)
    {
        _valves.Add(valve);
        return this;
    }

    public PipelineBuilder<TRequest, TResponse> Use(Func<TRequest, RequestHandlerDelegate<TResponse>, CancellationToken, Task<TResponse>> inlineValve)
    {
        _valves.Add(new InlineValve<TRequest, TResponse>(inlineValve));
        return this;
    }

    public RequestHandlerDelegate<TResponse> Build()
    {
        RequestHandlerDelegate<TResponse> handlerDelegate = () => _finalHandler(RequestInstance!, CancellationToken);

        foreach (var valve in _valves.AsEnumerable().Reverse())
        {
            var next = handlerDelegate;
            handlerDelegate = () => valve.Handle((TRequest)RequestInstance!, next, CancellationToken);
        }

        return handlerDelegate;
    }

    // To execute we need request & cancellationToken (provided at runtime)
    private object? RequestInstance;
    private CancellationToken CancellationToken;

    public async Task<TResponse> ExecuteAsync(TRequest request, CancellationToken cancellationToken = default)
    {
        RequestInstance = request;
        CancellationToken = cancellationToken;
        var pipeline = Build();
        return await pipeline();
    }

    // Inline valve adapter
    private class InlineValve<TReq, TRes> : IValve<TReq, TRes> where TReq : IRequest<TRes>
    {
        private readonly Func<TReq, RequestHandlerDelegate<TRes>, CancellationToken, Task<TRes>> _func;

        public InlineValve(Func<TReq, RequestHandlerDelegate<TRes>, CancellationToken, Task<TRes>> func)
        {
            _func = func;
        }

        public Task<TRes> Handle(TReq request, RequestHandlerDelegate<TRes> next, CancellationToken cancellationToken)
            => _func(request, next, cancellationToken);
    }
}

// Non-generic builder used in tests to register concrete handler instances
public class PipelineBuilder
{
    private readonly ConcurrentDictionary<(Type requestType, Type responseType), object> _handlers = new();
    private readonly ConcurrentDictionary<(Type requestType, Type responseType), List<object>> _valves = new();

    public PipelineBuilder()
    {
    }

    public PipelineBuilder AddRequestHandler<TRequest, TResponse>(IRequestHandler<TRequest, TResponse> handler)
        where TRequest : IRequest<TResponse>
    {
        if (handler == null) throw new ArgumentNullException(nameof(handler));
        _handlers[(typeof(TRequest), typeof(TResponse))] = handler;
        return this;
    }

    public PipelineBuilder AddValve<TRequest, TResponse>(IValve<TRequest, TResponse> valve)
        where TRequest : IRequest<TResponse>
    {
        var key = (typeof(TRequest), typeof(TResponse));
        var list = _valves.GetOrAdd(key, _ => new List<object>());
        list.Add(valve!);
        return this;
    }

    public IPiper Build()
    {
        return new BuiltPiper(_handlers, _valves);
    }

    private class BuiltPiper : IPiper
    {
        private readonly ConcurrentDictionary<(Type requestType, Type responseType), object> _handlers;
        private readonly ConcurrentDictionary<(Type requestType, Type responseType), List<object>> _valves;

        public BuiltPiper(ConcurrentDictionary<(Type requestType, Type responseType), object> handlers,
            ConcurrentDictionary<(Type requestType, Type responseType), List<object>> valves)
        {
            _handlers = handlers;
            _valves = valves;
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            var key = (request.GetType(), typeof(TResponse));
            if (!_handlers.TryGetValue(key, out var handlerObj))
            {
                throw new InvalidOperationException($"No handler registered for request type {key.Item1.FullName} and response type {key.Item2.FullName}.");
            }

            dynamic handler = handlerObj;

            // If there are no valves registered for this request/response pair, call the handler directly
            if (!_valves.TryGetValue(key, out var valvesList) || valvesList == null || valvesList.Count == 0)
            {
                return handler.Handle((dynamic)request, cancellationToken);
            }

            // Otherwise build a runtime pipeline using the generic builder via reflection/dynamic
            var requestType = request.GetType();
            var builderType = typeof(PipelineBuilder<,>).MakeGenericType(requestType, typeof(TResponse));

            var finalHandlerDelegate = (Func<object, CancellationToken, Task<TResponse>>)
                (async (reqObj, ct) => await handler.Handle((dynamic)reqObj, ct));

            var builder = Activator.CreateInstance(builderType, finalHandlerDelegate);

            // Add valves to the builder (dynamic invocation will bind to Use(IValve<,>))
            foreach (var valve in valvesList)
            {
                // Use dynamic to call the strongly-typed Use
                ((dynamic)builder).Use((dynamic)valve);
            }

            // Execute the pipeline
            return ((dynamic)builder).ExecuteAsync((dynamic)request, cancellationToken);
        }
    }
}
