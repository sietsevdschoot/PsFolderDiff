using Microsoft.Extensions.DependencyInjection;
using PsFolderDiff.FileHashLookup.Models;

namespace PsFolderDiff.FileHashLookup.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFileHashLookup(this IServiceCollection services)
    {
        services.AddSingleton<Services.FileHashLookup>();
        services.AddSingleton<FileHashLookupState>();

        return services;
    }
}