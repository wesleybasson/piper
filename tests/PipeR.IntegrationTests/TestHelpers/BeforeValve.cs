using PipeR.Core.Core;
using PipeR.Core.Middleware;
using PipeR.IntegrationTests.AspNetCore;

namespace PipeR.IntegrationTests.TestHelpers;

public class BeforeValve(List<string> tracker) : IValve<IRequest<TestResponse>, TestResponse>
{
    private readonly List<string> _tracker = tracker;

    public async Task<TestResponse> Handle(IRequest<TestResponse> request, RequestHandlerDelegate<IRequest<TestResponse>, TestResponse> next, CancellationToken cancellationToken)
    {
        _tracker.Add("before");
        return await next(request, cancellationToken);
    }
}
