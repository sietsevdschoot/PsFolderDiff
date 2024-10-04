using MediatR;

namespace PsFolderDiff.FileHashLookup.Requests;

public class AddIncludePatternRequest : IRequest
{
    public string IncludePattern { get; set; } = default!;
}