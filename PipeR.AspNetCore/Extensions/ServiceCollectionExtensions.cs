using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using PipeR.AspNetCore.Core;
using PipeR.Core.Core;
using PipeR.Core.Utilities;
using System.Reflection;

namespace PipeR.AspNetCore.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPipeR(this IServiceCollection services, Action<PiperOptions> configure)
    {
        var options = new PiperOptions();
        configure(options);

        // Build a temporary provider to log during registration
        var tempProvider = services.BuildServiceProvider();
        var logger = tempProvider.GetService<ILoggerFactory>()?.CreateLogger("PipeR")
                     ?? new LoggerFactory().CreateLogger("PipeR");

        var handlers = AssemblyScanner.ScanForHandlers(options.AssemblyToScan).ToList();

        foreach (var handlerType in handlers)
        {
            var handlerInterface = handlerType.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));
            services.AddTransient(handlerInterface, handlerType);

            logger.LogInformation("[PipeR] Registered handler {HandlerType} for {RequestType} → {ResponseType}",
                handlerType.Name,
                handlerInterface.GenericTypeArguments[0].Name,
                handlerInterface.GenericTypeArguments[1].Name);
        }

        // Defensive: Remove accidental IRequest<> registrations
        services.RemoveAll(typeof(IRequest<>));

        services.AddHttpContextAccessor();
        services.AddTransient<IPiper, AspNetCorePiper>();

        logger.LogInformation("[PipeR] Registered {HandlerCount} handlers from assembly {AssemblyName}.",
            handlers.Count, options.AssemblyToScan.GetName().Name);

        return services;
    }
}

public class PiperOptions
{
    public Assembly AssemblyToScan { get; set; }
}
