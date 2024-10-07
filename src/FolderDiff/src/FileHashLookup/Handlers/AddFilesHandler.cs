using MediatR;
using PsFolderDiff.FileHashLookup.Domain;
using PsFolderDiff.FileHashLookup.Requests;
using PsFolderDiff.FileHashLookup.Services;
using PsFolderDiff.FileHashLookup.Services.Interfaces;

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
        var filesToAdd = request.BasicFiles.Any()
            ? request.BasicFiles.ToList()
            : _fileHashCalculationService.CalculateHash(request.Files.ToList())
                .Select(x => new BasicFileInfo(x.File, x.Hash))
                .ToList();

        foreach (var basicFileInfo in filesToAdd)
        {
            _fileHashLookupState.Add(basicFileInfo);
        }

        return Task.CompletedTask;
    }
}