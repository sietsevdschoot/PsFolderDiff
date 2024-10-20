using MediatR;

namespace PsFolderDiff.FileHashLookupLib.Requests;

public class ExcludePatternRequest : IRequest
{
    public string ExcludePattern { get; set; } = default!;
}