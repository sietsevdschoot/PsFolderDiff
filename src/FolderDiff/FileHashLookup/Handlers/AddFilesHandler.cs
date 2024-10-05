using MediatR;
using PsFolderDiff.FileHashLookup.Models;
using PsFolderDiff.FileHashLookup.Requests;
using PsFolderDiff.FileHashLookup.Services;
using System.IO.Abstractions;

namespace PsFolderDiff.FileHashLookup.Handlers;

public class AddFilesHandler : IRequestHandler<AddFilesRequest>
{
    private readonly IFileHashCalculationService _fileHashCalculationService;
    private readonly FileHashLookupState _fileHashLookupState;

    public AddFilesHandler(
        FileHashLookupState fileHashLookupState,
        IFileHashCalculationService fileHashCalculationService)
    {
        _fileHashLookupState = fileHashLookupState;
        _fileHashCalculationService = fileHashCalculationService;
    }
    public Task Handle(AddFilesRequest request, CancellationToken cancellationToken)
    {
        var filesWithHash = _fileHashCalculationService.CalculateHash(request.Files.ToList());

        foreach ((IFileInfo File, string Hash) entry in filesWithHash)
        {
            _fileHashLookupState.Add(new BasicFileInfo(entry.File, entry.Hash));
        }

        return Task.CompletedTask;
    }
}