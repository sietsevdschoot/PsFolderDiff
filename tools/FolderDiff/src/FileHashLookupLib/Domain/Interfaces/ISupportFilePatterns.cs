namespace PsFolderDiff.FileHashLookupLib.Domain.Interfaces;

public interface ISupportFilePatterns
{
    List<FilePattern> IncludePatterns { get; set; }

    List<FilePattern> ExcludePatterns { get; set; }
}