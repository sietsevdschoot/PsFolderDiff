namespace PsFolderDiff.FileHashLookupLib.Services.Interfaces;

public interface IHasReadOnlyFilePatterns
{
    IReadOnlyCollection<(string Directory, string RelativePattern)> IncludePatterns { get; }

    IReadOnlyCollection<string> ExcludePatterns { get; }
}