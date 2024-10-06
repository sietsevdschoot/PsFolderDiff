using System.IO.Abstractions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using PsFolderDiff.FileHashLookup.Domain;
using PsFolderDiff.FileHashLookup.Extensions;
using PsFolderDiff.FileHashLookup.Requests;

namespace PsFolderDiff.FileHashLookup.Services;

public class FileHashLookup
{
    private readonly FileHashLookupState _fileHashLookupState;
    private readonly IMediator _mediator;
    private readonly FileCollector _fileCollector;

    public FileHashLookup(
        FileHashLookupState fileHashLookupState,
        FileCollector fileCollector,
        IMediator mediator)
    {
        _fileCollector = fileCollector;
        _mediator = mediator;
        _fileHashLookupState = fileHashLookupState;
    }

    public IReadOnlyDictionary<string, BasicFileInfo> File => _fileHashLookupState.File;

    public IReadOnlyDictionary<string, IReadOnlyCollection<BasicFileInfo>> Hash => _fileHashLookupState.Hash;

    public IReadOnlyCollection<(string Directory, string RelativePattern)> IncludePatterns =>
        _fileCollector.IncludePatterns;

    public IReadOnlyCollection<string> ExcludePatterns => _fileCollector.ExcludePatterns;

    public static FileHashLookup Create()
    {
        var sp = new ServiceCollection()
            .AddFileHashLookup()
            .BuildServiceProvider();

        var fileHashLookup = sp.GetRequiredService<FileHashLookup>();

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
        return _fileHashLookupState.File.Values.ToList();
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
                BasicFiles = [file]
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