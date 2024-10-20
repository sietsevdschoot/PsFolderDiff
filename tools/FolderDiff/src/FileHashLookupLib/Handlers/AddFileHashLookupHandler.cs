using MediatR;
using PsFolderDiff.FileHashLookupLib.Requests;
using PsFolderDiff.FileHashLookupLib.Services.Interfaces;

namespace PsFolderDiff.FileHashLookupLib.Handlers;

public class AddFileHashLookupHandler : IRequestHandler<AddFileHashLookupRequest>
{
    private readonly IFileHashLookupState _fileHashLookupState;
    private readonly IFileCollector _fileCollector;

    public AddFileHashLookupHandler(
        IFileCollector fileCollector,
        IFileHashLookupState fileHashLookupState)
    {
        _fileCollector = fileCollector;
        _fileHashLookupState = fileHashLookupState;
    }

    public Task Handle(AddFileHashLookupRequest request, CancellationToken cancellationToken)
    {
        var other = request.FileHashLookup;

        _fileCollector.AddFileHashLookup(other);
        _fileHashLookupState.AddFileHashLookup(other);

        return Task.CompletedTask;
    }
}