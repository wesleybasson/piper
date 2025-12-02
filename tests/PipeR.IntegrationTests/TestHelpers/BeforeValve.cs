using PipeR.Core.Core;
using PipeR.Core.Middleware;

namespace PipeR.IntegrationTests.AspNetCore;

public class BeforeValve : IValve<IRequest<TestResponse>, TestResponse>
{
    private readonly List<string> _tracker;

    public BeforeValve(List<string> tracker) => _tracker = tracker;

    public async Task<TestResponse> Handle(IRequest<TestResponse> request, RequestHandlerDelegate<TestResponse> next, CancellationToken cancellationToken)
    {
        _tracker.Add("before");
        return await next();
    }
}
