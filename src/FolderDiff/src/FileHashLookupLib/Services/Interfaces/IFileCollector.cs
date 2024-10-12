using System.IO.Abstractions;

namespace PsFolderDiff.FileHashLookupLib.Services.Interfaces;

public interface IFileCollector
{
    List<IFileInfo> AddIncludeFolder(string path);

    List<IFileInfo> AddIncludePattern(string includePattern);

    void AddFileHashLookup(FileHashLookup other);

    IFileCollector AddExcludePattern(string excludePattern);

    List<IFileInfo> GetFiles();
}