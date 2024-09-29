namespace PsFolderDiff.FileHashLookup.Requests;

public class AddIncludePatternRequest : MediatR.IRequest
{
    public string IncludePattern { get; set; } = default!;
}