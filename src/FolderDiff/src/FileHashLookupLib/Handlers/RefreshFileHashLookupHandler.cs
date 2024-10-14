using MediatR;
using PsFolderDiff.FileHashLookupLib.Requests;
using PsFolderDiff.FileHashLookupLib.Services.Interfaces;

namespace PsFolderDiff.FileHashLookupLib.Handlers;

public class RefreshFileHashLookupHandler : IRequestHandler<RefreshRequest>
{
    private readonly IFileCollector _fileCollector;
    private readonly IFileHashLookupState _fileHashLookupState;

    public RefreshFileHashLookupHandler(
        IFileCollector fileCollector,
        IFileHashLookupState fileHashLookupState)
    {
        _fileHashLookupState = fileHashLookupState;
        _fileCollector = fileCollector;
    }

    public Task Handle(RefreshRequest request, CancellationToken cancellationToken)
    {
        //// check exist on files in Hash
        //// Collect files from globs
        //// Check if new or updated
        //// calculate hash
        //// new -> add
        //// updated -> remove, add

        var allCollectedFiles = _fileCollector.GetFiles();

        var matches = allCollectedFiles.Select(file => _fileHashLookupState.Contains(file));

        throw new NotImplementedException("Continue here");
    }
}