using PipeR.Core.Middleware;
using static PipeR.Core.Core.PipelineDelegates;

namespace PipeR.Core.Core;

public class PipelineBuilder<TRequest, TResponse>(
    Func<object, CancellationToken, Task<TResponse>> finalHandler
) where TRequest : IRequest<TResponse>
{
    private readonly List<IValve<TRequest, TResponse>> _valves = [];
    private readonly Func<object, CancellationToken, Task<TResponse>> _finalHandler =
        finalHandler ?? throw new ArgumentNullException(nameof(finalHandler));

    public PipelineBuilder<TRequest, TResponse> Use(IValve<TRequest, TResponse> valve)
    {
        _valves.Add(valve);
        return this;
    }

    public PipelineBuilder<TRequest, TResponse> Use(
        Func<TRequest, RequestHandlerDelegate<TRequest, TResponse>, CancellationToken, Task<TResponse>> inlineValve)
    {
        _valves.Add(new InlineValve<TRequest, TResponse>(inlineValve));
        return this;
    }

    public PipelineInvoker<TRequest, TResponse> Build()
    {
        PipelineInvoker<TRequest, TResponse> current =
            (req, ct) => _finalHandler(req!, ct);

        for (int i = _valves.Count - 1; i >= 0; i--)
        {
            var valve = _valves[i];
            var next = current;

            Task<TResponse> nextDelegate(TRequest request, CancellationToken cancellationToken) => next(request, cancellationToken);

            current = (req, ct) =>
                valve.Handle(req, nextDelegate, ct);
        }

        return current;
    }

    public Task<TResponse> ExecuteAsync(
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        var pipeline = Build();
        return pipeline(request, cancellationToken);
    }

    private class InlineValve<TReq, TRes>(Func<TReq,
            RequestHandlerDelegate<TReq, TRes>,
            CancellationToken, Task<TRes>> func) : IValve<TReq, TRes>
        where TReq : IRequest<TRes>
    {
        private readonly Func<TReq,
            RequestHandlerDelegate<TReq, TRes>,
            CancellationToken, Task<TRes>> _func = func;

        public Task<TRes> Handle(
            TReq request,
            RequestHandlerDelegate<TReq, TRes> next,
            CancellationToken cancellationToken)
            => _func(request, next, cancellationToken);
    }
}
