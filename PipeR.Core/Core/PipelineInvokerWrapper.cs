using static PipeR.Core.Core.PipelineDelegates;

namespace PipeR.Core.Core;

public class PipelineInvokerWrapper<TRequest, TResponse>(PipelineInvoker<TRequest, TResponse> invoker) : IPipelineInvoker
    where TRequest : IRequest<TResponse>
{
    private readonly PipelineInvoker<TRequest, TResponse> _invoker = invoker;

    public async Task<object?> Invoke(object request, CancellationToken ct)
    {
        return await _invoker((TRequest)request, ct);
    }
}
