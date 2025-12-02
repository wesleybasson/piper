namespace PipeR.Core.Tests.Utilities;

using System.Reflection;
using PipeR.Core.Core;
using PipeR.Core.Utilities;
using Xunit;

public class AssemblyScannerTests
{
    [Fact]
    public void ScanForHandlers_Finds_ValidHandlers()
    {
        var types = AssemblyScanner.ScanForHandlers(Assembly.GetExecutingAssembly()).ToList();

        Assert.Contains(typeof(ValidHandler), types);
        Assert.Contains(typeof(AnotherValidHandler), types);
    }

    [Fact]
    public void ScanForHandlers_Excludes_AbstractHandler()
    {
        var types = AssemblyScanner.ScanForHandlers(Assembly.GetExecutingAssembly()).ToList();

        Assert.DoesNotContain(typeof(AbstractHandler), types);
    }

    [Fact]
    public void ScanForHandlers_Excludes_NonHandlerNameEvenIfImplementsInterface()
    {
        var types = AssemblyScanner.ScanForHandlers(Assembly.GetExecutingAssembly()).ToList();

        Assert.DoesNotContain(typeof(NotNamedProperly), types);
    }

    [Fact]
    public void ScanForHandlers_Excludes_HandlerNameWithoutInterface()
    {
        var types = AssemblyScanner.ScanForHandlers(Assembly.GetExecutingAssembly()).ToList();

        Assert.DoesNotContain(typeof(HandlerWithoutInterface), types);
    }

    // --- Test types used by the tests ---

    private class TestRequest : IRequest<string> { }

    private class ValidHandler : IRequestHandler<TestRequest, string>
    {
        public Task<string> Handle(TestRequest request, CancellationToken cancellationToken)
            => Task.FromResult("ok");
    }

    private class AnotherValidHandler : IRequestHandler<TestRequest, string>
    {
        public Task<string> Handle(TestRequest request, CancellationToken cancellationToken)
            => Task.FromResult("ok2");
    }

    private abstract class AbstractHandler : IRequestHandler<TestRequest, string>
    {
        public abstract Task<string> Handle(TestRequest request, CancellationToken cancellationToken);
    }

    // Implements the interface but name does not end with "Handler" - should be excluded
    private class NotNamedProperly : IRequestHandler<TestRequest, string>
    {
        public Task<string> Handle(TestRequest request, CancellationToken cancellationToken)
            => Task.FromResult("nope");
    }

    // Name ends with Handler but does not implement IRequestHandler<,> - should be excluded
    private class HandlerWithoutInterface { }
}
