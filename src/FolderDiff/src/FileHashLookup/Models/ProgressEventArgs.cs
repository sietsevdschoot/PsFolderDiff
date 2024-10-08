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
    public string Activity { get; init; } = default!;

    public string Status { get; init; } = default!;

    public string? CurrentOperation { get; set; } = default!;

    public int? CurrentProgress { get; set; }

    public int? Total { get; set; }

    public TimeSpan? Elapsed { get; set; }

    public double? PercentComplete => Total.HasValue
        ? (CurrentProgress ?? 0 / Total).GetValueOrDefault() * 100
        : null;

    public int? SecondsRemaining => Convert.ToInt32(Elapsed.GetValueOrDefault().TotalSeconds / Math.Max(CurrentProgress.GetValueOrDefault(), 1) * Total);
}