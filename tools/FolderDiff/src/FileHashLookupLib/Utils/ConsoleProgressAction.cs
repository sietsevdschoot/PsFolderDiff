using PsFolderDiff.FileHashLookupLib.Utils.Interfaces;

namespace PsFolderDiff.FileHashLookupLib.Utils;

public class ConsoleProgressAction<TProgress> : IProgressAction<TProgress>
    where TProgress : class
{
    private readonly Action<TProgress> _action;

    public ConsoleProgressAction(Action<TProgress> action)
    {
        _action = action;
    }

    public void Action(TProgress progress)
    {
        _action(progress);
    }
}