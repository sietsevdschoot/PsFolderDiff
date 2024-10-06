using MediatR;

namespace PsFolderDiff.FileHashLookup.Requests;

public class CompareFileHashLookupRequest : IRequest<CompareFileHashLookupResult>
{
    public Services.FileHashLookup FileHashLookup { get; set; } = default!;
}