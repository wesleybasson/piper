namespace PipeR.Core.Core;

public class PipelineDelegates
{
    public delegate Task<TResponse> PipelineInvoker<TRequest, TResponse>(
        TRequest request,
        CancellationToken ct
        );
}
