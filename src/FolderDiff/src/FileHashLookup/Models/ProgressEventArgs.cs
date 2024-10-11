namespace PsFolderDiff.FileHashLookup.Models;

////	Write-Progress
////        -Activity "Adding or updating files" `
////		-Status "Analyzing differences..." `
////		-CurrentOperation "($i of $($files.Count)) $($currentFile.FullName)" `
////		-PercentComplete (($i / $files.Count) * 100) `
////		-SecondsRemaining ([int](([double]$durationTimer.Elapsed.TotalSeconds / [Math]::Max($i, 1)) * ($files.Count - $i)))
////
////
public class ProgressEventArgs : EventArgs
{
    public ProgressEventArgs(
        string activity,
        string currentOperation)
    {
        Activity = activity;
        CurrentOperation = currentOperation;
    }

    public ProgressEventArgs(
        string activity,
        string currentOperation,
        long currentProgress,
        long total,
        string? status = null,
        string? currentItem = null)
            : this(activity, currentOperation)
    {
        if (string.IsNullOrEmpty(currentOperation) && string.IsNullOrEmpty(currentItem))
        {
            throw new InvalidOperationException($"Need either '{nameof(currentOperation)}' or '{nameof(currentItem)}'");
        }

        if (total == 0)
        {
            throw new ArgumentException("Total can't be 0");
        }

        Status = status ?? $"({currentProgress} / {total}) {currentItem}";
        PercentComplete = Math.Round((double)currentProgress / total * 100, 2);
    }

    public ProgressEventArgs(
        string activity,
        string currentOperation,
        long currentProgress,
        long total,
        TimeSpan currentDuration,
        string? status = null,
        string? currentItem = null)
        : this(activity, currentOperation, currentProgress, total, status, currentItem)
    {
        SecondsRemaining = Convert.ToInt32(currentDuration.TotalSeconds / Math.Max(currentProgress, 1) * (total - currentProgress));
    }

    public string Activity { get; private set; }

    public string CurrentOperation { get; private set; }

    public string? Status { get; private set; }

    public double? PercentComplete { get; private set; }

    public int? SecondsRemaining { get; private set; }
}