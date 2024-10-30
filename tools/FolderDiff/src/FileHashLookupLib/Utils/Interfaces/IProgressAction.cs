namespace PsFolderDiff.FileHashLookupLib.Utils.Interfaces;

public interface IProgressAction<in TProgress>
    where TProgress : class
{
    void Action(TProgress progress);
}