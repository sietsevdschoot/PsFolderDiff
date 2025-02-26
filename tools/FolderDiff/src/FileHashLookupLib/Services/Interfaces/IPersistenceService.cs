using PsFolderDiff.FileHashLookupLib.Configuration;
using PsFolderDiff.FileHashLookupLib.Domain;

namespace PsFolderDiff.FileHashLookupLib.Services.Interfaces;

public interface IPersistenceService
{
    void Save(FileHashLookup fileHashLookup, string? path);

    StorageModel LoadFromFile(string path, FileHashLookupSettings settings);
}