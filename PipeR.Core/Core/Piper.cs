namespace PipeR.Core.Core;

using System.Linq.Expressions;

public class Piper : IPiper
{
    private readonly Dictionary<(Type requestType, Type responseType), IPipelineInvoker> _invokers = [];

    public Piper(
        Dictionary<(Type, Type), object> handlers,
        Dictionary<(Type, Type), List<object>> valves)
    {
        foreach (var handler in handlers)
        {
            var (requestType, responseType) = handler.Key;
            var handlerObj = handler.Value;

            BuildAndCachePipeline(
                handlerObj,
                valves,
                requestType,
                responseType);
        }
    }

    private void BuildAndCachePipeline(
        object handlerObj,
        Dictionary<(Type, Type), List<object>> valves,
        Type requestType,
        Type responseType)
    {
        var key = (requestType, responseType);
        var builderType = typeof(PipelineBuilder<,>)
            .MakeGenericType(requestType, responseType);

        var finalHandlerDelegate = CreateFinalHandlerDelegate(
            handlerObj,
            requestType,
            responseType);

        dynamic builder = Activator.CreateInstance(builderType, finalHandlerDelegate)!;

        if (valves.TryGetValue(key, out var valveObjs))
        {
            foreach (var valveObj in valveObjs)
            {
                builder = builder.Use((dynamic)valveObj);
            }
        }

        var compiled = (object)builder.Build();
        var wrapperType = typeof(PipelineInvokerWrapper<,>).MakeGenericType(requestType, responseType);
        var wrapper = (IPipelineInvoker)Activator.CreateInstance(wrapperType, compiled)!;

        _invokers[key] = wrapper;
    }

    private static object CreateFinalHandlerDelegate(
        object handlerObj,
        Type requestType,
        Type responseType)
    {
        var methodInfo = handlerObj.GetType().GetMethod("Handle")!;
        var responseTaskType = typeof(Task<>).MakeGenericType(responseType);

        // Create a delegate of type Func<object, CancellationToken, Task<TResponse>>
        var funcType = typeof(Func<,,>).MakeGenericType(typeof(object), typeof(CancellationToken), responseTaskType);

        var reqParam = Expression.Parameter(typeof(object), "req");
        var ctParam = Expression.Parameter(typeof(CancellationToken), "ct");
        var castReq = Expression.Convert(reqParam, requestType);
        var call = Expression.Call(Expression.Constant(handlerObj), methodInfo, castReq, ctParam);

        var lambda = Expression.Lambda(funcType, call, reqParam, ctParam);
        return lambda.Compile();
    }

    public async Task<TResponse> Send<TResponse>(
        IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var key = (request.GetType(), typeof(TResponse));

        if (!_invokers.TryGetValue(key, out var invoker))
        {
            throw new InvalidOperationException($"No pipeline invoker registered for request type {key.Item1.FullName} and response type {key.Item2.FullName}.");
        }

        var result = await invoker.Invoke(request, cancellationToken);
        return result is null
            ? throw new InvalidOperationException($"Pipeline invoker returned null for request type {key.Item1.FullName} and response type {key.Item2.FullName}.")
            : (TResponse)result;
    }
}

