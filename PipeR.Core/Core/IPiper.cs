namespace PipeR.Core.Core;

public interface IPiper
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}
