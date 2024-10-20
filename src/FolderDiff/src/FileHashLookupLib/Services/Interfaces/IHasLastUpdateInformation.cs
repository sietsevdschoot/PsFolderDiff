namespace PsFolderDiff.FileHashLookupLib.Services.Interfaces;

public interface IHasLastUpdateInformation
{
    DateTime LastUpdated { get; set; }
}