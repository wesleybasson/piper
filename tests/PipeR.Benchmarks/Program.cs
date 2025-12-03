using BenchmarkDotNet.Running;
using PipeR.Benchmarks.Pipelines;
using PipeR.Benchmarks.Pipelines.DelegateCreation;

namespace PipeR.Benchmarks;

internal class Program
{
    static void Main(string[] args)
    {
        var _ = BenchmarkRunner.Run([
            typeof(HandlerOnlyBench),
            typeof(ValvePipelineBench),
            typeof(EndToEndHttpBench),
            typeof(FinalHandlerDelegateBenchmarks)
        ]);
    }
}
