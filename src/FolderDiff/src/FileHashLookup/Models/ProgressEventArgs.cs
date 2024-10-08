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
        string? status,
        string? currentItem,
        int currentProgress,
        int total) : this(activity, currentOperation)
    {
        if (string.IsNullOrEmpty(currentOperation) && string.IsNullOrEmpty(currentItem))
        {
            throw new InvalidOperationException($"Need either '{nameof(currentOperation)}' or '{nameof(currentItem)}'");
        }

        if (total == 0)
        {
            throw new ArgumentException("Total can't be 0");
        }

        CurrentOperation = currentOperation ?? $"({currentProgress} / {total}) {currentItem}";
        PercentComplete = (double)currentProgress / total * 100;
    }

    public ProgressEventArgs(
        string activity,
        string status,
        string currentOperation,
        string currentItem,
        int currentProgress,
        int total,
        TimeSpan currentDuration)
        :this(activity, status, currentOperation, currentItem, currentProgress, total)
    {
        SecondsRemaining =  Convert.ToInt32(currentDuration.TotalSeconds / Math.Max(currentProgress, 1) * (total - currentProgress));
    }

    public string Activity { get; private set; }

    public string Status { get; private set; }

    public string? CurrentOperation { get; private set; }

    public double? PercentComplete { get; private set; }

    public int? SecondsRemaining { get; private set; }
}