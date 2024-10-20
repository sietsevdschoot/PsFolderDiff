namespace PsFolderDiff.FileHashLookupLib.Services.Interfaces;

public interface IHasReadonlySaveInformation
{
    string SavedAsFile { get; }

    DateTime LastUpdated { get; }
}