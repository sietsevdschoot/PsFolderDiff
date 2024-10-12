using MediatR;

namespace PsFolderDiff.FileHashLookupLib.Requests;

public class AddExcludePatternRequest : IRequest
{
    public string ExcludePattern { get; set; } = default!;
}