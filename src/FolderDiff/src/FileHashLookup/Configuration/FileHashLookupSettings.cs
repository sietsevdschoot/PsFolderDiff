﻿using Microsoft.Extensions.DependencyInjection;
using PsFolderDiff.FileHashLookup.Models;

namespace PsFolderDiff.FileHashLookup.Configuration;

public class FileHashLookupSettings
{
    public static FileHashLookupSettings Default => new FileHashLookupSettings
    {
        ReportPollingDelay = TimeSpan.Zero,
        ////ReportPollingDelay = TimeSpan.FromMilliseconds(500),
        ReportProgress = new Progress<ProgressEventArgs>(progress =>
        {
            var progressMessage = string.Format(
                "{0}{1}{2}{3}",
                progress.PercentComplete.HasValue ? $"{progress.PercentComplete}% " : null,
                $"{progress.Activity} - {progress.CurrentOperation}",
                !string.IsNullOrEmpty(progress.Status) ? $" | {progress.Status}" : null,
                progress.SecondsRemaining.HasValue ? $" ({progress.SecondsRemaining} remaining)" : null);

            Console.WriteLine(progressMessage);
        }),
    };

    public TimeSpan ReportPollingDelay { get; set; }

    public Action<IServiceCollection, IServiceProvider>? ConfigureServices { get; set; }

    public IProgress<ProgressEventArgs> ReportProgress { get; set; } = new Progress<ProgressEventArgs>(_ => { /* empty */ });
}