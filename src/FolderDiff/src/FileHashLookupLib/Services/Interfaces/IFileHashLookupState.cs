using System.IO.Abstractions;
using PsFolderDiff.FileHashLookupLib.Domain;

namespace PsFolderDiff.FileHashLookupLib.Services.Interfaces;

public interface IFileHashLookupState
{
    void Add(BasicFileInfo file);

    void AddFileHashLookup(FileHashLookup other);

    FileContainsState Contains(BasicFileInfo file);

    FileContainsState Contains(IFileInfo file);

    void Remove(IFileInfo file);

    void Remove(BasicFileInfo file);

    List<BasicFileInfo> GetFiles();
}
