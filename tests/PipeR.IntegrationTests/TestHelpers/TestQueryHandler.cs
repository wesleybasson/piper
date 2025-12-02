using PipeR.Core.Core;

namespace PipeR.IntegrationTests.AspNetCore;

public class TestQueryHandler : IRequestHandler<TestQuery, TestResponse>
{
    private readonly List<string>? _tracker;

    public TestQueryHandler(List<string>? tracker = null)
    {
        _tracker = tracker;
    }

    public Task<TestResponse> Handle(TestQuery request, CancellationToken cancellationToken)
    {
        _tracker?.Add("handler");

        var response = new TestResponse
        {
            ValueOut = request.ValueIn + 1,
            MutatedString = $"Mutated {request.OriginalString}"
        };

        return Task.FromResult(response);
    }
}
