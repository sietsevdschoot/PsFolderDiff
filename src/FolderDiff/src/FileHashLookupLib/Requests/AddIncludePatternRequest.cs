using MediatR;

namespace PsFolderDiff.FileHashLookupLib.Requests;

public class AddIncludePatternRequest : IRequest
{
    public string IncludePattern { get; set; } = default!;
}