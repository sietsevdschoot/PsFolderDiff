namespace PsFolderDiff.FileHashLookupLib.Services.Interfaces;

public interface IPersistenceService : IHasReadonlySavePath
{
    void Save(FileHashLookup fileHashLookup, string? path);
}