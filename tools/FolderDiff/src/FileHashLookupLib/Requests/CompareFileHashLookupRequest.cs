using MediatR;

namespace PsFolderDiff.FileHashLookupLib.Requests;

public class CompareFileHashLookupRequest : IRequest<CompareFileHashLookupResult>
{
    public Services.FileHashLookup FileHashLookup { get; set; } = default!;
}