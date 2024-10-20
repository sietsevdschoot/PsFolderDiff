namespace PsFolderDiff.FileHashLookupLib.Services.Interfaces;

public interface IPersistenceService
{
    void Save(FileHashLookup fileHashLookup, string? path);
}