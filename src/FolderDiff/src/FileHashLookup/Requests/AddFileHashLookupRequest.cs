using MediatR;

namespace PsFolderDiff.FileHashLookup.Requests;

public class AddFileHashLookupRequest : IRequest
{
    public Services.FileHashLookup FileHashLookup { get; set; } = default!;
}