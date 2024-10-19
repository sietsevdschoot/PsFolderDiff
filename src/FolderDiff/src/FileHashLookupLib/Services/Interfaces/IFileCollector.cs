using System.IO.Abstractions;

namespace PsFolderDiff.FileHashLookupLib.Services.Interfaces;

public interface IFileCollector
{
    void AddFileHashLookup(FileHashLookup other);

    List<IFileInfo> IncludePattern(string includePattern);

    void ExcludePattern(string excludePattern);

    List<IFileInfo> GetFiles();
}