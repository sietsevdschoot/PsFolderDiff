using System.IO.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog;
using NLog.Extensions.Logging;
using PsFolderDiff.FileHashLookupLib.Configuration;
using PsFolderDiff.FileHashLookupLib.Domain;
using PsFolderDiff.FileHashLookupLib.Domain.Interfaces;
using PsFolderDiff.FileHashLookupLib.Services;
using PsFolderDiff.FileHashLookupLib.Services.Interfaces;
using PsFolderDiff.FileHashLookupLib.Utils;
using PsFolderDiff.FileHashLookupLib.Utils.Interfaces;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace PsFolderDiff.FileHashLookupLib.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFileHashLookup(
        this IServiceCollection services,
        FileHashLookupSettings settings)
    {
        // https://github.com/NLog/NLog.Extensions.Logging/wiki/NLog-configuration-with-appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetFileProvider(new EmbeddedFileProvider(typeof(FileHashLookup).Assembly))
            .AddJsonFile("appsettings.json")
            .Build();

        services
            .AddSingleton<IConfiguration>(configuration)
            .AddLogging(configuration)
            .RegisterFileHashLookupServices(settings);

        foreach (var configure in settings.ConfigureServices)
        {
            var serviceProvider = services.BuildServiceProvider();
            configure(services, serviceProvider);
        }

        return services;
    }

    public static IServiceCollection RegisterFileHashLookupServices(
        this IServiceCollection services,
        FileHashLookupSettings settings)
    {
        services
            .AddOptions()
            .AddSingleton(Options.Create(settings))
            .AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(FileHashLookup).Assembly))
            .AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddConsole();
            });
        services
            .AddSingleton<FileHashLookup>()
            .AddSingleton<IFileSystem>(settings.FileSystem)
            .AddSingleton(settings.CancellationTokenSource)
            .AddSingleton<IEventAggregator, EventAggregator>()
            .AddSingleton<IFileHashCalculationService, FileHashCalculationService>()
            .AddSingleton<IPersistenceService, PersistenceService>()
            .AddSingleton(typeof(ConsoleProgressAction<>))
            .AddSingleton(typeof(IProgress<>), typeof(Progress<>))
            .AddSingleton(typeof(IProgressAction<>), typeof(ConsoleProgressAction<>))
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

    public static IServiceCollection AddLogging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddLogging(builder => builder.AddNLog(configuration));

        var nlogSection = configuration.GetSection("nlog");

        if (LogManager.Configuration == null)
        {
            LogManager.Configuration = new NLogLoggingConfiguration(nlogSection);
        }

        return services;
    }
}