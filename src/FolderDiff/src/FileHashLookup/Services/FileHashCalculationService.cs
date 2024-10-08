using System.Diagnostics;
using System.IO.Abstractions;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using PsFolderDiff.FileHashLookup.Configuration;
using PsFolderDiff.FileHashLookup.Extensions;
using PsFolderDiff.FileHashLookup.Models;
using PsFolderDiff.FileHashLookup.Services.Interfaces;

namespace PsFolderDiff.FileHashLookup.Services;

public class FileHashCalculationService : IFileHashCalculationService
{
    private readonly IProgress<ProgressEventArgs> _progress;
    private IOptions<FileHashLookupSettings> _settings;

    public FileHashCalculationService(
        IProgress<ProgressEventArgs> progress,
        IOptions<FileHashLookupSettings> settings)
    {
        _settings = settings;
        _progress = progress;
    }

    public IEnumerable<(IFileInfo File, string Hash)> CalculateHash(List<IFileInfo> files)
    {
        _progress.Report(new ProgressEventArgs
        {
            Activity = "Adding or updating files.",
            CurrentOperation = "Collecting information before starting hashing.",
        });

        var totalSize = files.Sum(x => x.Length);
        var currentProcessedSize = 0d;

        var updateStatusStopwatch = Stopwatch.StartNew();
        var durationStopwatch = Stopwatch.StartNew();

        for (var i = 0; i < files.Count; i++)
        {
            var file = files[i];

            if (updateStatusStopwatch.Elapsed > _settings.Value.ReportPollingDelay)
            {
                _progress.Report(new ProgressEventArgs
                {
                    Activity = "Adding or updating files.",
                    CurrentOperation = "Calculating Hash",
                    Status = $"({i} of {files.Count}) {file.FullName}",
                    CurrentProgress = i,
                    Total = files.Count,
                    Elapsed = durationStopwatch.Elapsed,
                });

                updateStatusStopwatch.Restart();
            }

            yield return (File: file, Hash: file.CalculateMD5Hash());

            currentProcessedSize += file.Length;
        }
    }
}