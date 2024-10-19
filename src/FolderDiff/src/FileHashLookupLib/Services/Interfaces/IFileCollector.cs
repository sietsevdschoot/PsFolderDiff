using System.IO.Abstractions;

namespace PsFolderDiff.FileHashLookupLib.Services.Interfaces;

public interface IFileCollector
{
    List<IFileInfo> IncludePattern(string includePattern);

    void AddFileHashLookup(FileHashLookup other);

    IFileCollector ExcludePattern(string excludePattern);

    List<IFileInfo> GetFiles();
}