using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PsFolderDiff.FileHashLookupLib.Domain;

namespace PsFolderDiff.FileHashLookupLib.Configuration;

public class FileHashLookupSettings
{
    public static FileHashLookupSettings Default
    {
        get
        {
            var defaultSettings = new FileHashLookupSettings
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

            defaultSettings.ReportProgress = (progress, logger) =>
            {
                var progressMessage = defaultSettings.BuildProgressMessage(progress);

                logger.LogInformation(progressMessage);
            };

            Console.CancelKeyPress += (_, args) =>
            {
                if (args.SpecialKey == ConsoleSpecialKey.ControlC)
                {
                    defaultSettings.CancellationTokenSource.Cancel();
                }
            };

            return defaultSettings;
        }
    }

    public IFileSystem FileSystem { get; set; } = default!;

    public TimeSpan ReportProgressDelay { get; set; }

    public List<Action<IServiceCollection, IServiceProvider>> ConfigureServices { get; set; } = new();

    public CancellationTokenSource CancellationTokenSource { get; set; } = new();

    public Func<ProgressEventArgs, string> BuildProgressMessage { get; set; } = _ => "Configure build message first.";

    public Action<ProgressEventArgs, ILogger> ReportProgress { get; set; } = (_, _) =>
    {
        Console.WriteLine($"Configure {nameof(FileHashLookupSettings)}.{nameof(BuildProgressMessage)} to display progress.");
    };
}