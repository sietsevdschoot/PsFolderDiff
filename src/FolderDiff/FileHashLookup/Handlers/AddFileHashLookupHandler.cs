using MediatR;
using PsFolderDiff.FileHashLookup.Models;
using PsFolderDiff.FileHashLookup.Requests;
using PsFolderDiff.FileHashLookup.Services;

namespace PsFolderDiff.FileHashLookup.Handlers;

public class AddFileHashLookupHandler : IRequestHandler<AddFileHashLookupRequest>
{
    private FileHashLookupState _fileHashLookupState;
    private FileCollector _fileCollector;

    public AddFileHashLookupHandler(
        FileCollector fileCollector,
        FileHashLookupState fileHashLookupState)
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