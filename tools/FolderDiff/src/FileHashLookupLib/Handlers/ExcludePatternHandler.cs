using MediatR;
using PsFolderDiff.FileHashLookupLib.Domain;
using PsFolderDiff.FileHashLookupLib.Requests;
using PsFolderDiff.FileHashLookupLib.Services.Interfaces;
using PsFolderDiff.FileHashLookupLib.Utils.Interfaces;

namespace PsFolderDiff.FileHashLookupLib.Handlers;

public class ExcludePatternHandler : IRequestHandler<ExcludePatternRequest>
{
    private readonly IFileCollector _fileCollector;
    private readonly IFileHashLookupState _fileHashLookupState;
    private readonly IPeriodicalProgressReporter<ProgressEventArgs> _progress;

    public ExcludePatternHandler(
        IFileCollector fileCollector,
        IFileHashLookupState fileHashLookupState,
        IPeriodicalProgressReporter<ProgressEventArgs> progress)
    {
        _fileCollector = fileCollector;
        _fileHashLookupState = fileHashLookupState;
        _progress = progress;
    }

    public Task Handle(ExcludePatternRequest request, CancellationToken cancellationToken)
    {
        _progress.Report(() => new ProgressEventArgs(
            activity: "Excluding files or patterns",
            currentOperation: "Collecting files to remove"));

        var files = _fileCollector.ExcludePattern(request.ExcludePattern);

        if (files.Any())
        {
            for (var i = 0; i < files.Count; i++)
            {
                var file = files[i];

                _progress.Report(
                    progress => new ProgressEventArgs(
                        activity: "Excluding files or patterns",
                        currentOperation: "Excluding files.",
                        currentItem: file.FullName,
                        currentProgress: progress,
                        total: files.Count),
                    currentProgress: i);

                _fileHashLookupState.Remove(file);

                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        return Task.CompletedTask;
    }
}