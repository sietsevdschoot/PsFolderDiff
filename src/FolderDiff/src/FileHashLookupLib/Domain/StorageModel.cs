using PsFolderDiff.FileHashLookupLib.Domain.Interfaces;

namespace PsFolderDiff.FileHashLookupLib.Domain;

public class StorageModel
    : ISupportFileHashLookups,
      ISupportFilePatterns,
      ISupportSavePath
{
    public Dictionary<string, BasicFileInfo> File { get; set; } = new(StringComparer.InvariantCultureIgnoreCase);

    public Dictionary<string, List<BasicFileInfo>> Hash { get; set; } = new(StringComparer.InvariantCultureIgnoreCase);

    public List<FilePattern> IncludePatterns { get; set; } = new();

    public List<FilePattern> ExcludePatterns { get; set; } = new();

    public string SavedAsFile { get; set; } = string.Empty;
}