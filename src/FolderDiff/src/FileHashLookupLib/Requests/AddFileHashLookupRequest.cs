using MediatR;

namespace PsFolderDiff.FileHashLookupLib.Requests;

public class AddFileHashLookupRequest : IRequest
{
    public Services.FileHashLookup FileHashLookup { get; set; } = default!;
}