using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using PsFolderDiff.FileHashLookupLib.Domain;
using PsFolderDiff.FileHashLookupLib.Utils;
using PsFolderDiff.FileHashLookupLib.Utils.Interfaces;

namespace PsFolderDiff.FileHashLookupLib.Configuration;

public class FileHashLookupSettings
{
    static FileHashLookupSettings()
    {
        var settings = new FileHashLookupSettings
        {
            FileSystem = new FileSystem(),
            ReportProgressDelay = TimeSpan.FromMilliseconds(500),
            BuildProgressMessage = progress => string.Format(
                "{0,4}{1}{2}{3}",
                progress.PercentComplete.HasValue ? $"{progress.PercentComplete}% " : null,
                $"{progress.Activity} - {progress.CurrentOperation}",
                !string.IsNullOrEmpty(progress.Status) ? $" | {progress.Status}" : null,
                progress.SecondsRemaining > 0 ? $" ({progress.SecondsRemaining} remaining)" : null),
        };

        settings.ReportProgress = new ConsoleProgressAction<ProgressEventArgs>(progress =>
        {
            var progressMessage = settings.BuildProgressMessage(progress);

            Console.WriteLine(progressMessage);
        });

        Default = settings;
    }

    public static FileHashLookupSettings Default { get; }

    public IFileSystem FileSystem { get; set; } = default!;

    public TimeSpan ReportProgressDelay { get; set; }

    public List<Action<IServiceCollection, IServiceProvider>> ConfigureServices { get; set; } = new();

    public CancellationTokenSource CancellationTokenSource { get; set; } = new();

    public Func<ProgressEventArgs, string> BuildProgressMessage { get; set; } = _ => "Configure build message first.";

    public IProgressAction<ProgressEventArgs> ReportProgress { get; set; } = new ConsoleProgressAction<ProgressEventArgs>(_ =>
    {
        Console.WriteLine($"Configure {nameof(FileHashLookupSettings)}.{nameof(BuildProgressMessage)} to display progress.");
    });
}