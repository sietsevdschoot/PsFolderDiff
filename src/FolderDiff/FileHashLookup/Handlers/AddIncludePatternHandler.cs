using System.IO.Abstractions;
using MediatR;
using PsFolderDiff.FileHashLookup.Models;
using PsFolderDiff.FileHashLookup.Requests;
using PsFolderDiff.FileHashLookup.Services;

namespace PsFolderDiff.FileHashLookup.Handlers;

public class AddIncludePatternHandler : IRequestHandler<AddIncludePatternRequest>
{
    private readonly FileCollector _fileCollector;
    private readonly IFileHashCalculationService _fileHashCalculationService;
    private readonly FileHashLookupState _fileHashLookupState;

    public AddIncludePatternHandler(
        FileCollector fileCollector,
        IFileHashCalculationService fileHashCalculationService,
        FileHashLookupState fileHashLookupState)
    {
        _fileCollector = fileCollector;
        _fileHashCalculationService = fileHashCalculationService;
        _fileHashLookupState = fileHashLookupState;
    }

    public Task Handle(AddIncludePatternRequest request, CancellationToken cancellationToken)
    {
        var collectedFiles = _fileCollector.AddIncludeFolder(request.IncludePattern);

        var filesWithHash = _fileHashCalculationService.CalculateHash(collectedFiles);

        foreach ((IFileInfo File, string Hash) entry in filesWithHash)
        {
            _fileHashLookupState.Add(new BasicFileInfo(entry.File, entry.Hash));
        }

        return Task.CompletedTask;
    }
}