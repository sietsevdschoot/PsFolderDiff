using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using PsFolderDiff.FileHashLookupLib.Domain;
using PsFolderDiff.FileHashLookupLib.Utils;
using PsFolderDiff.FileHashLookupLib.Utils.Interfaces;

namespace PsFolderDiff.FileHashLookupLib.Configuration;

public class FileHashLookupSettings
{
    public static FileHashLookupSettings Default => new FileHashLookupSettings
    {
        FileSystem = new FileSystem(),
        ReportProgressDelay = TimeSpan.FromMilliseconds(500),
        ReportProgress = new ConsoleProgressAction<ProgressEventArgs>(progress =>
        {
            var progressMessage = string.Format(
                "{0,4}{1}{2}{3}",
                progress.PercentComplete.HasValue ? $"{progress.PercentComplete}% " : null,
                $"{progress.Activity} - {progress.CurrentOperation}",
                !string.IsNullOrEmpty(progress.Status) ? $" | {progress.Status}" : null,
                progress.SecondsRemaining is > 0 ? $" ({progress.SecondsRemaining} remaining)" : null);

            Console.WriteLine(progressMessage);
        }),
    };

    public IFileSystem FileSystem { get; set; } = default!;

    public TimeSpan ReportProgressDelay { get; set; }

    public List<Action<IServiceCollection, IServiceProvider>> ConfigureServices { get; set; } = new();

    public IProgressAction<ProgressEventArgs> ReportProgress { get; set; } = new ConsoleProgressAction<ProgressEventArgs>(_ =>
    {
        Console.WriteLine($"Configure {nameof(FileHashLookupSettings)}.{nameof(ReportProgress)} to display progress.");
    });
}