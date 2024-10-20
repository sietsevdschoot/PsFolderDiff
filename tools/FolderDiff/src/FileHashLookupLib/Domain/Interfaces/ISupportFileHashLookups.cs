namespace PsFolderDiff.FileHashLookupLib.Domain.Interfaces;

public interface ISupportFileHashLookups
{
    Dictionary<string, BasicFileInfo> File { get; set; }

    Dictionary<string, List<BasicFileInfo>> Hash { get; set; }
}