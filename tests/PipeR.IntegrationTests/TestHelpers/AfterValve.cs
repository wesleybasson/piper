using PipeR.Core.Core;
using PipeR.Core.Middleware;

namespace PipeR.IntegrationTests.AspNetCore;

public class AfterValve : IValve<IRequest<TestResponse>, TestResponse>
{
    private readonly List<string> _tracker;

    public AfterValve(List<string> tracker) => _tracker = tracker;

    public async Task<TestResponse> Handle(IRequest<TestResponse> request, RequestHandlerDelegate<TestResponse> next, CancellationToken cancellationToken)
    {
        var result = await next();
        _tracker.Add("after");
        return result;
    }
}
