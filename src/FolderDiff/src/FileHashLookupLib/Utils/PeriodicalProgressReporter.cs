using System.Diagnostics;
using Microsoft.Extensions.Options;
using PsFolderDiff.FileHashLookupLib.Configuration;

namespace PsFolderDiff.FileHashLookupLib.Utils;

public class PeriodicalProgressReporter<T> : IPeriodicalProgressReporter<T>
{
    private readonly IProgress<T> _progress;
    private readonly FileHashLookupSettings _settings;
    private readonly Stopwatch _stopwatch;

    public PeriodicalProgressReporter(
        IProgress<T> progress,
        IOptions<FileHashLookupSettings> settings)
    {
        _progress = progress;
        _settings = settings.Value;

        _stopwatch = Stopwatch.StartNew();
    }

    public void Report(Func<T> getValue)
    {
        if (_stopwatch.Elapsed > _settings.ReportPollingDelay)
        {
            var value = getValue();

            _progress.Report(value);

            _stopwatch.Restart();
        }
    }

    public void Report(Func<long, T> getValue, long currentProgress)
    {
        if (_stopwatch.Elapsed > _settings.ReportPollingDelay)
        {
            Report(() => getValue(currentProgress));
        }
    }
}