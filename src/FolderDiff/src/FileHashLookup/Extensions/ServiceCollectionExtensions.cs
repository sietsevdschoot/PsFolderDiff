using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PsFolderDiff.FileHashLookup.Services;
using PsFolderDiff.FileHashLookup.Services.Interfaces;

namespace PsFolderDiff.FileHashLookup.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFileHashLookup(this IServiceCollection services)
    {
        services.AddSingleton<IFileSystem, FileSystem>();
        services.AddSingleton<IFileHashCalculationService, FileHashCalculationService>();

        services.AddSingleton<FileCollector>();
        services.AddSingleton<IFileCollector, IFileCollector>(sp => sp.GetRequiredService<FileCollector>());
        services.AddSingleton<IHasReadOnlyFilePatterns, FileCollector>(sp => sp.GetRequiredService<FileCollector>());

        services.AddSingleton<FileHashLookupState>();
        services.AddSingleton<IFileHashLookupState, IFileHashLookupState>(sp => sp.GetRequiredService<FileHashLookupState>());
        services.AddSingleton<IHasReadonlyLookups, FileHashLookupState>(sp => sp.GetRequiredService<FileHashLookupState>());

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