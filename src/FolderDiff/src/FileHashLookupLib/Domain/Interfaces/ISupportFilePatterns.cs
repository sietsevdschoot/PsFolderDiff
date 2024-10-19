namespace PsFolderDiff.FileHashLookupLib.Domain.Interfaces;

public interface ISupportFilePatterns
{
    List<(string Directory, string RelativePattern)> IncludePatterns { get; set; }

    List<(string Directory, string RelativePattern)> ExcludePatterns { get; set; }
}