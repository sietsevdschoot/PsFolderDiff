namespace PsFolderDiff.FileHashLookupLib.Services;

public interface IPersistenceService
{
    string? SavedAsFile { get; }

    void Save(FileHashLookup fileHashLookup, string? path);
}