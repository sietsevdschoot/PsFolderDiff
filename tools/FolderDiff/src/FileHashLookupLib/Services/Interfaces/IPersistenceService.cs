using PsFolderDiff.FileHashLookupLib.Configuration;

namespace PsFolderDiff.FileHashLookupLib.Services.Interfaces;

public interface IPersistenceService
{
    void Save(FileHashLookup fileHashLookup, string? path);

    FileHashLookup LoadFileHashLookup(string path, FileHashLookupSettings settings);
}