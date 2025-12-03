using PipeR.Core.Middleware;

namespace PipeR.Core.Core;

public class PiperBuilder
{
    private readonly Dictionary<(Type requestType, Type responseType), object> _handlers = [];
    private readonly Dictionary<(Type requestType, Type responseType), List<object>> _valves = [];

    public PiperBuilder AddRequestHandler<TRequest, TResponse>(
        IRequestHandler<TRequest, TResponse> handler
    )
        where TRequest : IRequest<TResponse>
    {
        ArgumentNullException.ThrowIfNull(handler);

        _handlers[(typeof(TRequest), typeof(TResponse))] = handler;
        return this;
    }

    public PiperBuilder AddValve<TRequest, TResponse>(
        IValve<TRequest, TResponse> valve
    )
        where TRequest : IRequest<TResponse>
    {
        var key = (typeof(TRequest), typeof(TResponse));

        if (!_valves.TryGetValue(key, out var list))
        {
            list = [];
            _valves[key] = list;
        }

        list.Add(valve);
        return this;
    }

    public IPiper Build() => new Piper(_handlers, _valves);
}

