using PsFolderDiff.FileHashLookupLib.Domain;

namespace PsFolderDiff.FileHashLookupLib.Services.Interfaces;

public interface IHasReadOnlyFilePatterns
{
    IReadOnlyCollection<FilePattern> IncludePatterns { get; }

    IReadOnlyCollection<FilePattern> ExcludePatterns { get; }
}