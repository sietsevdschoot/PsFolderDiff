namespace PsFolderDiff.FileHashLookupLib.Domain.Interfaces;

public interface ISupportSaveInformation
{
    string SavedAsFile { get; set; }

    DateTime LastUpdated { get; set; }
}