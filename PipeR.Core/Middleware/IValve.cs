using PipeR.Core.Core;

namespace PipeR.Core.Middleware;

public delegate Task<TResponse> RequestHandlerDelegate<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken);

public interface IValve<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken);
}
