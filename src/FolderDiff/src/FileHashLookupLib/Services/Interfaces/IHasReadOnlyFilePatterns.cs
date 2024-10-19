namespace PsFolderDiff.FileHashLookupLib.Services.Interfaces;

public interface IHasReadOnlyFilePatterns
{
    IReadOnlyCollection<string> IncludePatterns { get; }

    IReadOnlyCollection<string> ExcludePatterns { get; }
}