using PipeR.Core.Core;
using System.Reflection;

namespace PipeR.Core.Utilities;

public static class AssemblyScanner
{
    public static IEnumerable<Type> ScanForHandlers(Assembly assembly)
    {
        return assembly.GetTypes()
            .Where(t => !t.IsAbstract &&
                        t.Name.EndsWith("Handler", StringComparison.OrdinalIgnoreCase) &&
                        t.GetInterfaces().Any(i =>
                            i.IsGenericType &&
                            i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)));
    }
}
