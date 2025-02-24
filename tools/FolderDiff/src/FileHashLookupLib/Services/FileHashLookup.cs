using System.Collections.ObjectModel;
using System.IO.Abstractions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PsFolderDiff.FileHashLookupLib.Configuration;
using PsFolderDiff.FileHashLookupLib.Domain;
using PsFolderDiff.FileHashLookupLib.Extensions;
using PsFolderDiff.FileHashLookupLib.Requests;
using PsFolderDiff.FileHashLookupLib.Services.Interfaces;
using PsFolderDiff.FileHashLookupLib.Utils;
using PsFolderDiff.FileHashLookupLib.Utils.Interfaces;

namespace PsFolderDiff.FileHashLookupLib.Services;

public class FileHashLookup
{
    private readonly IMediator _mediator;
    private readonly IHasReadOnlyFilePatterns _filePatterns;
    private readonly IHasReadonlyLookups _fileHashLookups;
    private readonly IFileHashLookupState _fileHashLookupState;
    private readonly IPersistenceService _persistenceService;
    private readonly IHasReadonlySaveInformation _readonlySaveInformation;
    private readonly CancellationTokenSource _cts;
    private readonly ILogger<FileHashLookup> _logger;

    public FileHashLookup(
        IHasReadOnlyFilePatterns filePatterns,
        IHasReadonlyLookups fileHashLookups,
        IFileHashLookupState fileHashLookupState,
        IPersistenceService persistenceService,
        IHasLastUpdateInformation lastUpdateInformation,
        IHasReadonlySaveInformation readonlySaveInformation,
        IMediator mediator,
        ILogger<FileHashLookup> logger,
        CancellationTokenSource cts)
    {
        _logger = logger;
        _cts = cts;
        _fileHashLookups = fileHashLookups;
        _filePatterns = filePatterns;
        _fileHashLookupState = fileHashLookupState;
        _readonlySaveInformation = readonlySaveInformation;
        _persistenceService = persistenceService;
        _mediator = mediator;

        lastUpdateInformation.LastUpdated = DateTime.Now;
    }

    public IReadOnlyDictionary<string, BasicFileInfo> File => _fileHashLookups.File;

    public ReadOnlyDictionary<string, ReadOnlyCollection<BasicFileInfo>> Hash => _fileHashLookups.Hash;

    public IReadOnlyCollection<string> IncludePatterns => _filePatterns.IncludePatterns.Select(x => x.Value).ToList();

    public IReadOnlyCollection<string> ExcludePatterns => _filePatterns.ExcludePatterns.Select(x => x.Value).ToList();

    public string SavedAsFile => _readonlySaveInformation.SavedAsFile;

    public DateTime LastUpdated => _readonlySaveInformation.LastUpdated;

    public static FileHashLookup Create() => Create(FileHashLookupSettings.Default);

    public static FileHashLookup Create(FileHashLookupSettings settings)
    {
        var services = new ServiceCollection()
            .AddFileHashLookup(settings);

        return Create(services, settings);
    }

    public static FileHashLookup Load(string path)
    {
        return Load(path, FileHashLookupSettings.Default);
    }

    public static FileHashLookup Load(string path, FileHashLookupSettings settings)
    {
        settings.ConfigureServices.Add((services, sp) =>
        {
            var persistenceService = sp.GetRequiredService<IPersistenceService>();
            var progress = sp.GetRequiredService<IPeriodicalProgressReporter<ProgressEventArgs>>();

            progress.Report(() => new ProgressEventArgs(
                activity: "Loading FileHashLookup.",
                currentOperation: $"Loaded FileHashLookup from {path}."));

            var storageModel = persistenceService.LoadFromFile(path, settings);

            services.AddSingleton<StorageModel>(storageModel);
        });

        var services = new ServiceCollection()
            .AddFileHashLookup(settings);

        return Create(services, settings);
    }

    public void Save(string? path = null)
    {
        _persistenceService.Save(this, path ?? SavedAsFile);
    }

    public async Task IncludeAsync(string includeFolderOrPattern, CancellationToken cancellationToken = default)
    {
        await _mediator.SendAsyncWithCancellation(
            _cts,
            _logger,
            new IncludePatternRequest
            {
                IncludePattern = includeFolderOrPattern,
            },
            cancellationToken);
    }

    public async Task ExcludeAsync(string excludeFolderOrPattern, CancellationToken cancellationToken = default)
    {
        await _mediator.SendAsyncWithCancellation(
            _cts,
            _logger,
            new ExcludePatternRequest
            {
                ExcludePattern = excludeFolderOrPattern,
            },
            cancellationToken);
    }

    public List<BasicFileInfo> GetFiles()
    {
        return _fileHashLookupState.GetFiles();
    }

    public async Task AddFileAsync(IFileInfo file, CancellationToken cancellationToken = default)
    {
        await _mediator.SendAsyncWithCancellation(
            _cts,
            _logger,
            new AddFilesRequest
            {
                Files = [file],
            },
            cancellationToken);
    }

    public async Task AddFileAsync(BasicFileInfo file, CancellationToken cancellationToken = default)
    {
        await _mediator.SendAsyncWithCancellation(
            _cts,
            _logger,
            new AddFilesRequest
            {
                BasicFiles = [file],
            },
            cancellationToken);
    }

    public async Task AddFilesAsync(IFileInfo[] files, CancellationToken cancellationToken = default)
    {
        await _mediator.SendAsyncWithCancellation(
            _cts,
            _logger,
            new AddFilesRequest
            {
                Files = files,
            },
            cancellationToken);
    }

    public async Task AddFileHashLookupAsync(FileHashLookup other, CancellationToken cancellationToken = default)
    {
        await _mediator.SendAsyncWithCancellation(
            _cts,
            _logger,
            new AddFileHashLookupRequest
            {
                FileHashLookup = other,
            },
            cancellationToken);
    }

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        await _mediator.SendAsyncWithCancellation(
            _cts,
            _logger,
            new RefreshRequest(),
            cancellationToken);
    }

    public async Task<FileHashLookup> GetDifferencesInOtherAsync(FileHashLookup other, CancellationToken cancellationToken = default)
    {
        var compareResult = await _mediator.SendAsyncWithCancellation(
            _cts,
            _logger,
            new CompareFileHashLookupRequest
            {
                FileHashLookup = other,
            },
            cancellationToken);

        return compareResult.DifferencesInOther;
    }

    public async Task<FileHashLookup> GetMatchesInOtherAsync(FileHashLookup other, CancellationToken cancellationToken = default)
    {
        var compareResult = await _mediator.SendAsyncWithCancellation(
            _cts,
            _logger,
            new CompareFileHashLookupRequest
            {
                FileHashLookup = other,
            },
            cancellationToken);

        return compareResult.MatchesInOther;
    }

    public override string ToString() => this.GetFileHashLookupDescription();

    internal static FileHashLookup Create(IServiceCollection services, FileHashLookupSettings settings)
    {
        Console.CancelKeyPress += (_, args) =>
        {
            if (args.SpecialKey == ConsoleSpecialKey.ControlC)
            {
                settings.CancellationTokenSource.Cancel();
            }
        };

        var sp = services.BuildServiceProvider();

        var consoleProgressAction = sp.GetRequiredService<ConsoleProgressAction<ProgressEventArgs>>()
            .SetReportProgress((progress, logger) =>
            {
                var progressMessage = settings.BuildProgressMessage(progress);

                Console.WriteLine(progressMessage);
                logger.LogInformation(progressMessage);
            });

        sp.GetRequiredService<IEventAggregator>()
            .Subscribe(new Progress<ProgressEventArgs>(consoleProgressAction.Action));

        var fileHashLookup = sp.GetRequiredService<FileHashLookup>();

        return fileHashLookup;
    }
}