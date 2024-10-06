namespace PsFolderDiff.FileHashLookup.Requests;

public class CompareFileHashLookupResult
{
    public Services.FileHashLookup DifferencesInOther { get; init; } = default!;

    public Services.FileHashLookup MatchesInOther { get; init; } = default!;
}