using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using PsFolderDiff.FileHashLookupLib.Models;

namespace PsFolderDiff.FileHashLookupLib.Configuration;

public class FileHashLookupSettings
{
    public static FileHashLookupSettings Default => new FileHashLookupSettings
    {
        ReportProgressDelay = TimeSpan.FromMilliseconds(500),
        ReportProgress = new Progress<ProgressEventArgs>(progress =>
        {
            var progressMessage = string.Format(
                "{0,4}{1}{2}{3}",
                progress.PercentComplete.HasValue ? $"{progress.PercentComplete}% " : null,
                $"{progress.Activity} - {progress.CurrentOperation}",
                !string.IsNullOrEmpty(progress.Status) ? $" | {progress.Status}" : null,
                progress.SecondsRemaining is > 0 ? $" ({progress.SecondsRemaining} remaining)" : null);

            Console.WriteLine(progressMessage);
        }),
        FileSystem = new FileSystem(),
    };

    public IFileSystem FileSystem { get; set; } = default!;

    public TimeSpan ReportProgressDelay { get; set; }

    public List<Action<IServiceCollection, IServiceProvider>> ConfigureServices { get; set; } = new();

    public IProgress<ProgressEventArgs> ReportProgress { get; set; } = new Progress<ProgressEventArgs>(_ =>
    {
        Console.WriteLine($"Configure {nameof(FileHashLookupSettings)}.{nameof(FileHashLookupSettings.ReportProgress)} to display progress.");
    });
}