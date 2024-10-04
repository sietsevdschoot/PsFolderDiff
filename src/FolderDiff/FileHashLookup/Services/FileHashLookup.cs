using MediatR;
using Microsoft.Extensions.DependencyInjection;
using PsFolderDiff.FileHashLookup.Extensions;
using PsFolderDiff.FileHashLookup.Models;
using PsFolderDiff.FileHashLookup.Requests;

namespace PsFolderDiff.FileHashLookup.Services;

public class FileHashLookup
{
    private readonly FileHashLookupState _fileHashLookupState;
    private readonly IMediator _mediator;

    public static FileHashLookup Create()
    {
        var sp = new ServiceCollection()
            .AddFileHashLookup()
            .BuildServiceProvider();

        var fileHashLookup = sp.GetRequiredService<FileHashLookup>();

        return fileHashLookup;
    }

    public FileHashLookup(
        FileHashLookupState fileHashLookupState,
        IMediator mediator)
    {
        _mediator = mediator;
        _fileHashLookupState = fileHashLookupState;
    }

    public async Task AddFolderAsync(string path, CancellationToken cancellationToken = default)
    {
        await _mediator.Send(new AddIncludePatternRequest
        {
            IncludePattern = path
        }, 
        cancellationToken);
    }

    public async Task AddExcludePattern(string excludePattern, CancellationToken cancellationToken = default)
    {
        await _mediator.Send(new AddExcludePatternRequest
        {
            ExcludePattern = excludePattern
        },
        cancellationToken);
    }

    public List<BasicFileInfo> GetFiles()
    {
        return _fileHashLookupState.File.Values.ToList();
    }
}