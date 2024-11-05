using System.Diagnostics;
using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PsFolderDiff.FileHashLookupLib.Configuration;
using PsFolderDiff.FileHashLookupLib.Domain;
using PsFolderDiff.FileHashLookupLib.Extensions;
using PsFolderDiff.FileHashLookupLib.Services.Interfaces;

namespace PsFolderDiff.FileHashLookupLib.Services;

public class FileHashCalculationService : IFileHashCalculationService
{
    private readonly IProgress<ProgressEventArgs> _progress;
    private readonly IOptions<FileHashLookupSettings> _settings;
    private readonly ILogger<FileHashCalculationService> _logger;

    public FileHashCalculationService(
        IProgress<ProgressEventArgs> progress,
        IOptions<FileHashLookupSettings> settings,
        ILogger<FileHashCalculationService> logger)
    {
        _settings = settings;
        _progress = progress;
        _logger = logger;
    }

    public IEnumerable<(IFileInfo File, string Hash)> CalculateHash(List<IFileInfo> files, CancellationToken cancellationToken)
    {
        _progress.Report(new ProgressEventArgs(
            activity: "Calculate file hashes.",
            currentOperation: "Collecting information before starting hashing."));

        var totalSize = files.Sum(x => x.Length);
        var currentProcessedSize = 0L;

        var updateStatusStopwatch = Stopwatch.StartNew();
        var durationStopwatch = Stopwatch.StartNew();

        for (var i = 0; i < files.Count; i++)
        {
            var file = files[i];

            if (updateStatusStopwatch.Elapsed > _settings.Value.ReportProgressDelay)
            {
                _progress.Report(new ProgressEventArgs(
                    activity: "Calculate file hashes.",
                    currentOperation: "Calculating Hash.",
                    status: $"({i + 1} / {files.Count}) {file.FullName}",
                    currentItem: file.FullName,
                    currentProgress: currentProcessedSize,
                    total: totalSize,
                    currentDuration: durationStopwatch.Elapsed));

                updateStatusStopwatch.Restart();
            }

            var hash = string.Empty;

            try
            {
                hash = file.CalculateMD5Hash();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while read: {file.FullName}");
            }

            if (!string.IsNullOrEmpty(hash))
            {
                yield return (File: file, Hash: hash);
            }

            currentProcessedSize += file.Length;

            cancellationToken.ThrowIfCancellationRequested();
        }
    }
}