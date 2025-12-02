using PipeR.Core.Core;

namespace PipeR.Core.Middleware;

public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

public interface IValve<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
}
