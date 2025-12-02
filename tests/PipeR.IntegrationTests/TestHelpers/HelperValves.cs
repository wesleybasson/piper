using PipeR.Core.Core;
using PipeR.Core.Middleware;

namespace PipeR.IntegrationTests.AspNetCore;

public class LoggingValve : IValve<TestQuery, TestResponse>
{
    public Task<TestResponse> Handle(TestQuery request, RequestHandlerDelegate<TestResponse> next, CancellationToken cancellationToken)
    {
        // Simple logging simulation: mutate the request string to indicate logging occurred
        request.OriginalString = request.OriginalString + ":logged";
        return next();
    }
}

public class ValidationValve : IValve<TestQuery, TestResponse>
{
    public Task<TestResponse> Handle(TestQuery request, RequestHandlerDelegate<TestResponse> next, CancellationToken cancellationToken)
    {
        if (request.ValueIn < 0)
        {
            throw new InvalidOperationException("ValueIn must be non-negative");
        }

        return next();
    }
}

public class AuthorizationValve : IValve<TestQuery, TestResponse>
{
    public Task<TestResponse> Handle(TestQuery request, RequestHandlerDelegate<TestResponse> next, CancellationToken cancellationToken)
    {
        // Simple authorization simulation
        if (string.Equals(request.OriginalString, "forbidden", StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Request is not authorized");
        }

        return next();
    }
}
