using System.IO.Abstractions;
using MediatR;
using PsFolderDiff.FileHashLookup.Domain;
using PsFolderDiff.FileHashLookup.Requests;
using PsFolderDiff.FileHashLookup.Services.Interfaces;

namespace PsFolderDiff.FileHashLookup.Handlers;

public class AddIncludePatternHandler : IRequestHandler<AddIncludePatternRequest>
{
    private readonly IFileCollector _fileCollector;
    private readonly IFileHashCalculationService _fileHashCalculationService;
    private readonly IFileHashLookupState _fileHashLookupState;

    public AddIncludePatternHandler(
        IFileCollector fileCollector,
        IFileHashCalculationService fileHashCalculationService,
        IFileHashLookupState fileHashLookupState)
    {
        _fileCollector = fileCollector;
        _fileHashCalculationService = fileHashCalculationService;
        _fileHashLookupState = fileHashLookupState;
    }

    public Task Handle(AddIncludePatternRequest request, CancellationToken cancellationToken)
    {
        var collectedFiles = !string.IsNullOrEmpty(request.IncludePath)
            ? _fileCollector.AddIncludeFolder(request.IncludePath)
            : _fileCollector.AddIncludePattern(request.IncludePattern);

        var filesWithHash = _fileHashCalculationService.CalculateHash(collectedFiles).ToList();

        for (var i = 0; i < filesWithHash.Count; i++)
        {
            (IFileInfo File, string Hash) entry = filesWithHash[i];

            _fileHashLookupState.Add(new BasicFileInfo(entry.File, entry.Hash));
        }

        return Task.CompletedTask;
    }
}