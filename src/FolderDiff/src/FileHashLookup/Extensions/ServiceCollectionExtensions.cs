using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PsFolderDiff.FileHashLookup.Domain;
using PsFolderDiff.FileHashLookup.Services;

namespace PsFolderDiff.FileHashLookup.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFileHashLookup(this IServiceCollection services)
    {
        services.AddSingleton<IFileSystem, FileSystem>();
        services.AddSingleton<IFileHashCalculationService, FileHashCalculationService>();
        services.AddSingleton<FileHashLookupState>();
        services.AddSingleton<FileCollector>();
        services.AddSingleton<FileHashLookupState>();
        services.AddSingleton<Services.FileHashLookup>();

        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Information);
            builder.AddConsole();
        });

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Services.FileHashLookup).Assembly);
        });

        return services;
    }
}