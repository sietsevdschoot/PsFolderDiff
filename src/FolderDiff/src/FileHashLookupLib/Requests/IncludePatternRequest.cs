using MediatR;

namespace PsFolderDiff.FileHashLookupLib.Requests;

public class IncludePatternRequest : IRequest
{
    public string IncludePattern { get; set; } = default!;
}