namespace PsFolderDiff.FileHashLookup.Utils;

public interface IPeriodicalProgressReporter<T>
{
    void Report(Func<T> getValue);

    void Report(Func<long, T> getValue, long currentProgress);
}