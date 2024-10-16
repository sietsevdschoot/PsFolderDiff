﻿using System.Collections.ObjectModel;
using System.IO.Abstractions;
using System.Threading;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PsFolderDiff.FileHashLookupLib.Configuration;
using PsFolderDiff.FileHashLookupLib.Domain;
using PsFolderDiff.FileHashLookupLib.Extensions;
using PsFolderDiff.FileHashLookupLib.Requests;
using PsFolderDiff.FileHashLookupLib.Services.Interfaces;

namespace PsFolderDiff.FileHashLookupLib.Services;

public class FileHashLookup
{
    private readonly IMediator _mediator;
    private readonly IHasReadOnlyFilePatterns _filePatterns;
    private readonly IHasReadonlyLookups _fileHashLookups;
    private readonly IFileHashLookupState _fileHashLookupState;

    public FileHashLookup(
        IHasReadOnlyFilePatterns filePatterns,
        IHasReadonlyLookups fileHashLookups,
        IFileHashLookupState fileHashLookupState,
        IMediator mediator)
    {
        _fileHashLookups = fileHashLookups;
        _filePatterns = filePatterns;
        _fileHashLookupState = fileHashLookupState;
        _mediator = mediator;
    }

    public IReadOnlyDictionary<string, BasicFileInfo> File => _fileHashLookups.File;

    public ReadOnlyDictionary<string, ReadOnlyCollection<BasicFileInfo>> Hash => _fileHashLookups.Hash;

    public IReadOnlyCollection<(string Directory, string RelativePattern)> IncludePatterns => _filePatterns.IncludePatterns;

    public IReadOnlyCollection<(string Directory, string RelativePattern)> ExcludePatterns => _filePatterns.ExcludePatterns;

    public static FileHashLookup Create() => Create(FileHashLookupSettings.Default);

    public static FileHashLookup Create(FileHashLookupSettings settings)
    {
        var provider = Create(new ServiceCollection(), settings);

        return provider.FileHashLookup;
    }

    public async Task Include(string includeFolderOrPattern, CancellationToken cancellationToken = default)
    {
        await _mediator.Send(
            new AddIncludePatternRequest
            {
                IncludePattern = includeFolderOrPattern,
            },
            cancellationToken);
    }

    public async Task Exclude(string excludeFolderOrPattern, CancellationToken cancellationToken = default)
    {
        await _mediator.Send(
            new AddExcludePatternRequest
            {
                ExcludePattern = excludeFolderOrPattern,
            },
            cancellationToken);
    }

    public List<BasicFileInfo> GetFiles()
    {
        return _fileHashLookupState.GetFiles();
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

    public async Task Refresh(CancellationToken cancellationToken = default)
    {
        await _mediator.Send(
            new RefreshRequest(),
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

    internal static (FileHashLookup FileHashLookup, IServiceProvider ServiceProvider) Create(IServiceCollection services, FileHashLookupSettings settings)
    {
        services
            .AddSingleton(Options.Create(settings))
            .AddFileHashLookup();

        settings.ConfigureServices?.Invoke(services, services.BuildServiceProvider());

        var sp = services.BuildServiceProvider();

        sp.GetRequiredService<IEventAggregator>()
            .Subscribe(settings.ReportProgress);

        return (
            FileHashLookup: sp.GetRequiredService<FileHashLookup>(),
            ServiceProvider: sp);
    }

    public async Task Save(CancellationToken cancellationToken = default)
    {
        ////$absolutePath = [IO.DirectoryInfo](GetAbsolutePath $path)

        ////    $replaceableChars = ([IO.Path]::GetInvalidFileNameChars() + ' ' | Foreach - Object { [Regex]::Escape($_) }) -join "|"
        ////    $fileName = "$($absolutePath.FullName -replace $replaceableChars, "_").xml"

        ////    $this.SavedAsFile = (GetAbsolutePath $fileName)



    }
}