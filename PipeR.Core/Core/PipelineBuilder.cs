using PipeR.Core.Middleware;

namespace PipeR.Core.Core;

public class PipelineBuilder<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly List<IValve<TRequest, TResponse>> _valves = new();
    private readonly Func<TRequest, CancellationToken, Task<TResponse>> _finalHandler;

    public PipelineBuilder(Func<TRequest, CancellationToken, Task<TResponse>> finalHandler)
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
        RequestHandlerDelegate<TResponse> handlerDelegate = () => _finalHandler((TRequest)RequestInstance!, CancellationToken);

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
