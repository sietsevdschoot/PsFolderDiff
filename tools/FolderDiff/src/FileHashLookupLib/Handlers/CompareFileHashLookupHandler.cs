using MediatR;
using PsFolderDiff.FileHashLookupLib.Domain;
using PsFolderDiff.FileHashLookupLib.Requests;
using PsFolderDiff.FileHashLookupLib.Services.Interfaces;
using PsFolderDiff.FileHashLookupLib.Utils.Interfaces;

namespace PsFolderDiff.FileHashLookupLib.Handlers;

public class CompareFileHashLookupHandler : IRequestHandler<CompareFileHashLookupRequest, CompareFileHashLookupResult>
{
    private readonly IFileHashLookupState _fileHashLookupState;
    private readonly IPeriodicalProgressReporter<ProgressEventArgs> _progress;

    public CompareFileHashLookupHandler(
        IFileHashLookupState fileHashLookupState,
        IPeriodicalProgressReporter<ProgressEventArgs> progress)
    {
        _fileHashLookupState = fileHashLookupState;
        _progress = progress;
    }

    public async Task<CompareFileHashLookupResult> Handle(CompareFileHashLookupRequest request, CancellationToken cancellationToken)
    {
        var differencesLookup = Services.FileHashLookup.Create();
        var matchesLookup = Services.FileHashLookup.Create();

        _progress.Report(() => new ProgressEventArgs(
            activity: "Compare FileHashLookup",
            currentOperation: "Collecting files in other"));

        var otherFiles = request.FileHashLookup.GetFiles();

        for (var i = 0; i < otherFiles.Count; i++)
        {
            var currentFile = otherFiles[i];

            _progress.Report(
                progress => new ProgressEventArgs(
                    activity: "Compare FileHashLookup",
                    currentOperation: "Comparing files",
                    currentItem: currentFile.FullName,
                    currentProgress: progress,
                    total: otherFiles.Count),
                currentProgress: i);

            if (_fileHashLookupState.Contains(currentFile) == FileContainsState.Match)
            {
                await matchesLookup.AddFileAsync(currentFile, cancellationToken);
            }
            else
            {
                await differencesLookup.AddFileAsync(currentFile, cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        return new CompareFileHashLookupResult
        {
            MatchesInOther = matchesLookup,
            DifferencesInOther = differencesLookup,
        };
    }
}