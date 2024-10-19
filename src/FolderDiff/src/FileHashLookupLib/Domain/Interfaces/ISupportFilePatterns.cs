namespace PsFolderDiff.FileHashLookupLib.Domain.Interfaces;

public interface ISupportFilePatterns
{
    List<string> IncludePatterns { get; set; }

    List<string> ExcludePatterns { get; set; }
}