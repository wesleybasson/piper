using BenchmarkDotNet.Attributes;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PipeR.Benchmarks.Pipelines.DelegateCreation;

[MemoryDiagnoser]
[WarmupCount(3)]
[IterationCount(10)]
public class FinalHandlerDelegateBenchmarks
{
    private TestHandler _handler = null!;
    private Type _requestType = null!;
    private Type _responseType = null!;

    [GlobalSetup]
    public void Setup()
    {
        _handler = new TestHandler();
        _requestType = typeof(TestRequest);
        _responseType = typeof(TestResponse);
    }

    [Benchmark]
    public object Alpha()
    {
        return CreateFinalHandlerDelegateAlpha(_handler, _requestType, _responseType);
    }

    [Benchmark]
    public object Beta()
    {
        return CreateFinalHandlerDelegateBeta(_handler);
    }


    // ==== METHODS UNDER TEST ====

    private static object CreateFinalHandlerDelegateAlpha(
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

    private static object CreateFinalHandlerDelegateBeta(
        object handlerObj)
    {
        var method = handlerObj.GetType().GetMethod("Handle")!;

        return (Func<object, CancellationToken, Task>)((req, ct) =>
        {
            return (Task)method.Invoke(handlerObj, new[] { req, ct })!;
        });
    }


    // ==== TEST FIXTURES ====

    public sealed class TestRequest { }

    public sealed class TestResponse { }

    public sealed class TestHandler
    {
        public Task<TestResponse> Handle(TestRequest r, CancellationToken ct)
        {
            return Task.FromResult(new TestResponse());
        }
    }
}
