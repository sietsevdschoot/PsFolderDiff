using System.Collections.ObjectModel;
using System.IO.Abstractions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PsFolderDiff.FileHashLookup.Configuration;
using PsFolderDiff.FileHashLookup.Domain;
using PsFolderDiff.FileHashLookup.Extensions;
using PsFolderDiff.FileHashLookup.Requests;
using PsFolderDiff.FileHashLookup.Services.Interfaces;

namespace PsFolderDiff.FileHashLookup.Services;

public class FileHashLookup
{
    private readonly IMediator _mediator;
    private readonly IHasReadOnlyFilePatterns _filePatterns;
    private readonly IHasReadonlyLookups _fileHashLookups;
    private readonly IEventAggregator _eventAggregator;

    public FileHashLookup(
        IHasReadOnlyFilePatterns filePatterns,
        IHasReadonlyLookups fileHashLookups,
        IEventAggregator eventAggregator,
        IMediator mediator)
    {
        _fileHashLookups = fileHashLookups;
        _filePatterns = filePatterns;
        _eventAggregator = eventAggregator;
        _mediator = mediator;
    }

    public IReadOnlyDictionary<string, BasicFileInfo> File => _fileHashLookups.File;

    public ReadOnlyDictionary<string, ReadOnlyCollection<BasicFileInfo>> Hash => _fileHashLookups.Hash;

    public IReadOnlyCollection<(string Directory, string RelativePattern)> IncludePatterns => _filePatterns.IncludePatterns;

    public IReadOnlyCollection<string> ExcludePatterns => _filePatterns.ExcludePatterns;

    public static FileHashLookup Create() => Create(FileHashLookupSettings.Default);

    public static FileHashLookup Create(FileHashLookupSettings settings)
    {
        var services = new ServiceCollection()
            .AddSingleton(Options.Create(settings))
            .AddFileHashLookup();

        if (settings.ConfigureServices != null)
        {
            settings.ConfigureServices(services, services.BuildServiceProvider());
        }

        var sp = services.BuildServiceProvider();

        var fileHashLookup = sp.GetRequiredService<FileHashLookup>();
        var eventAggregator = sp.GetRequiredService<IEventAggregator>();

        eventAggregator.Subscribe(settings.ReportProgress);

        return fileHashLookup;
    }

    public async Task AddFolder(string path, CancellationToken cancellationToken = default)
    {
        await _mediator.Send(
            new AddIncludePatternRequest
            {
                IncludePath = path,
            },
            cancellationToken);
    }

    public async Task AddIncludePattern(string includePattern, CancellationToken cancellationToken = default)
    {
        await _mediator.Send(
            new AddIncludePatternRequest
            {
                IncludePattern = includePattern,
            },
            cancellationToken);
    }

    public async Task AddExcludePattern(string excludePattern, CancellationToken cancellationToken = default)
    {
        await _mediator.Send(
            new AddExcludePatternRequest
            {
                ExcludePattern = excludePattern,
            },
            cancellationToken);
    }

    public List<BasicFileInfo> GetFiles()
    {
        return _fileHashLookups.File.Values.ToList();
    }

    public async Task AddFile(IFileInfo file, CancellationToken cancellationToken = default)
    {
        await _mediator.Send(
            new AddFilesRequest
            {
                Files = [file],
            },
            cancellationToken);
    }

    public async Task AddFile(BasicFileInfo file, CancellationToken cancellationToken = default)
    {
        await _mediator.Send(
            new AddFilesRequest
            {
                BasicFiles = [file],
            },
            cancellationToken);
    }

    public async Task AddFiles(IFileInfo[] files, CancellationToken cancellationToken = default)
    {
        await _mediator.Send(
            new AddFilesRequest
            {
                Files = files,
            },
            cancellationToken);
    }

    public async Task AddFileHashLookup(FileHashLookup other, CancellationToken cancellationToken = default)
    {
        await _mediator.Send(
            new AddFileHashLookupRequest
            {
                FileHashLookup = other,
            },
            cancellationToken);
    }

    public async Task<FileHashLookup> GetDifferencesInOther(FileHashLookup other, CancellationToken cancellationToken = default)
    {
        var compareResult = await _mediator.Send(
            new CompareFileHashLookupRequest
            {
                FileHashLookup = other,
            },
            cancellationToken);

        return compareResult.DifferencesInOther;
    }

    public async Task<FileHashLookup> GetMatchesInOther(FileHashLookup other, CancellationToken cancellationToken = default)
    {
        var compareResult = await _mediator.Send(
            new CompareFileHashLookupRequest
            {
                FileHashLookup = other,
            },
            cancellationToken);

        return compareResult.MatchesInOther;
    }
}