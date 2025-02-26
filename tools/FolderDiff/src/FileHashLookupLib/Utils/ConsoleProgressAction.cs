using Microsoft.Extensions.Logging;
using PsFolderDiff.FileHashLookupLib.Services;
using PsFolderDiff.FileHashLookupLib.Utils.Interfaces;

namespace PsFolderDiff.FileHashLookupLib.Utils;

public class ConsoleProgressAction<TProgress> : IProgressAction<TProgress>
    where TProgress : class
{
    private readonly ILogger<FileHashLookup>? _logger;
    private Action<TProgress>? _reportAction;
    private Action<TProgress, ILogger>? _reportActionWithLogger;

    public ConsoleProgressAction()
    {
    }

    public ConsoleProgressAction(ILogger<FileHashLookup> logger)
    {
        _logger = logger;
    }

    public ConsoleProgressAction<TProgress> SetReportProgress(Action<TProgress> action)
    {
        _reportAction = action;

        return this;
    }

    public ConsoleProgressAction<TProgress> SetReportProgress(Action<TProgress, ILogger> action)
    {
        _reportActionWithLogger = action;

        return this;
    }

    public void Action(TProgress progress)
    {
        if (_logger != null && _reportActionWithLogger != null)
        {
            _reportActionWithLogger(progress, _logger);
            return;
        }

        if (_reportAction == null)
        {
            throw new ArgumentNullException(nameof(_reportAction));
        }

        _reportAction(progress);
    }
}