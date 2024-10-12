using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PsFolderDiff.FileHashLookup.Models;
using PsFolderDiff.FileHashLookup.Services;
using PsFolderDiff.FileHashLookup.Services.Interfaces;
using PsFolderDiff.FileHashLookup.Utils;

namespace PsFolderDiff.FileHashLookup.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFileHashLookup(this IServiceCollection services)
    {
        services.AddOptions();
        services.AddSingleton<IFileSystem, FileSystem>();
        services.AddSingleton<IEventAggregator, EventAggregator>();
        services.AddSingleton<IFileHashCalculationService, FileHashCalculationService>();
        services.AddSingleton(typeof(IProgress<>), typeof(Progress<>));
        services.AddSingleton(typeof(IPeriodicalProgressReporter<>), typeof(PeriodicalProgressReporter<>));

        services.AddTransient<IProgress<ProgressEventArgs>>(
            sp => new Progress<ProgressEventArgs>(message =>
            {
                var eventAggregator = sp.GetRequiredService<IEventAggregator>();
                eventAggregator.Publish(message);
            }));

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