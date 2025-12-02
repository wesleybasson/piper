using BenchmarkDotNet.Running;
using PipeR.Benchmarks.Pipelines;

namespace PipeR.Benchmarks;

internal class Program
{
    static void Main(string[] args)
    {
        var _ = BenchmarkRunner.Run([
            typeof(HandlerOnlyBench),
            typeof(ValvePipelineBench),
            typeof(EndToEndHttpBench)
        ]);
    }
}
