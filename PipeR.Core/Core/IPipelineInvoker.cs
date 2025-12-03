namespace PipeR.Core.Core;

public interface IPipelineInvoker
{
    Task<object?> Invoke(object request, CancellationToken ct);
}

