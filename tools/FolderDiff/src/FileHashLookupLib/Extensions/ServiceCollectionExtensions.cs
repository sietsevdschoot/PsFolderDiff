using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PsFolderDiff.FileHashLookupLib.Domain;
using PsFolderDiff.FileHashLookupLib.Domain.Interfaces;
using PsFolderDiff.FileHashLookupLib.Services;
using PsFolderDiff.FileHashLookupLib.Services.Interfaces;
using PsFolderDiff.FileHashLookupLib.Utils;
using PsFolderDiff.FileHashLookupLib.Utils.Interfaces;

namespace PsFolderDiff.FileHashLookupLib.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFileHashLookup(this IServiceCollection services)
    {
        services
            .AddOptions()
            .AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddConsole();
            })
            .AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(FileHashLookup).Assembly);
            });

        services
            .AddSingleton<FileHashLookup>()
            .AddSingleton<IFileSystem, FileSystem>()
            .AddSingleton<IEventAggregator, EventAggregator>()
            .AddSingleton<IFileHashCalculationService, FileHashCalculationService>()
            .AddSingleton<IPersistenceService, PersistenceService>()
            .AddSingleton(typeof(IProgress<>), typeof(Progress<>))
            .AddSingleton(typeof(IPeriodicalProgressReporter<>), typeof(PeriodicalProgressReporter<>));

        services
            .AddSingleton<PersistenceService>()
            .AddSingleton<IPersistenceService>(sp => sp.GetRequiredService<PersistenceService>())
            .AddSingleton<IHasReadonlySaveInformation>(sp => sp.GetRequiredService<PersistenceService>())
            .AddSingleton<IHasLastUpdateInformation>(sp => sp.GetRequiredService<PersistenceService>());

        services
            .AddSingleton<StorageModel>()
            .AddSingleton<ISupportFileHashLookups>(sp => sp.GetRequiredService<StorageModel>())
            .AddSingleton<ISupportFilePatterns>(sp => sp.GetRequiredService<StorageModel>())
            .AddSingleton<ISupportSaveInformation>(sp => sp.GetRequiredService<StorageModel>());

        services
            .AddSingleton<FileCollector>()
            .AddSingleton<IFileCollector>(sp => sp.GetRequiredService<FileCollector>())
            .AddSingleton<IHasReadOnlyFilePatterns>(sp => sp.GetRequiredService<FileCollector>());

        services
            .AddSingleton<FileHashLookupState>()
            .AddSingleton<IFileHashLookupState>(sp => sp.GetRequiredService<FileHashLookupState>())
            .AddSingleton<IHasReadonlyLookups>(sp => sp.GetRequiredService<FileHashLookupState>());

        services.AddTransient<IProgress<ProgressEventArgs>>(
            sp => new Progress<ProgressEventArgs>(message =>
            {
                var eventAggregator = sp.GetRequiredService<IEventAggregator>();
                eventAggregator.Publish(message);
            }));

        return services;
    }
}