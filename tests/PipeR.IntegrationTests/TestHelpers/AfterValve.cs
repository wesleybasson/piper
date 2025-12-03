using PipeR.Core.Core;
using PipeR.Core.Middleware;
using PipeR.IntegrationTests.AspNetCore;

namespace PipeR.IntegrationTests.TestHelpers;

public class AfterValve(List<string> tracker) : IValve<IRequest<TestResponse>, TestResponse>
{
    private readonly List<string> _tracker = tracker;

    public async Task<TestResponse> Handle(IRequest<TestResponse> request, RequestHandlerDelegate<IRequest<TestResponse>, TestResponse> next, CancellationToken cancellationToken)
    {
        var result = await next(request, cancellationToken);
        _tracker.Add("after");
        return result;
    }
}
