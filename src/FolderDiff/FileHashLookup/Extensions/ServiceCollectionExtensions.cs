using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using PsFolderDiff.FileHashLookup.Models;
using PsFolderDiff.FileHashLookup.Services;

namespace PsFolderDiff.FileHashLookup.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFileHashLookup(this IServiceCollection services)
    {
        services.AddSingleton<IFileSystem, FileSystem>();
        services.AddSingleton<FileHashLookupState>();
        services.AddSingleton<FileCollector>();
        services.AddSingleton<IFileHashCalculationService, FileHashCalculationService>();
        services.AddSingleton<FileHashLookupState>();
        services.AddSingleton<Services.FileHashLookup>();

        services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssembly(typeof(Services.FileHashLookup).Assembly);
        });

        return services;
    }
}