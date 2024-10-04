using MediatR;

namespace PsFolderDiff.FileHashLookup.Requests;

public class AddExcludePatternRequest : IRequest
{
    public string ExcludePattern { get; set; } = default!;
}