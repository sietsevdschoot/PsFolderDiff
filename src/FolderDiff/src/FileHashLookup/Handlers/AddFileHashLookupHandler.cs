using MediatR;
using PsFolderDiff.FileHashLookup.Requests;
using PsFolderDiff.FileHashLookup.Services;
using PsFolderDiff.FileHashLookup.Services.Interfaces;

namespace PsFolderDiff.FileHashLookup.Handlers;

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