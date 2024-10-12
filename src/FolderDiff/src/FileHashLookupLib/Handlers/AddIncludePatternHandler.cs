using MediatR;
using PsFolderDiff.FileHashLookupLib.Domain;
using PsFolderDiff.FileHashLookupLib.Models;
using PsFolderDiff.FileHashLookupLib.Requests;
using PsFolderDiff.FileHashLookupLib.Services.Interfaces;
using PsFolderDiff.FileHashLookupLib.Utils;

namespace PsFolderDiff.FileHashLookupLib.Handlers;

public class AddIncludePatternHandler : IRequestHandler<AddIncludePatternRequest>
{
    private readonly IFileCollector _fileCollector;
    private readonly IFileHashCalculationService _fileHashCalculationService;
    private readonly IFileHashLookupState _fileHashLookupState;
    private readonly IPeriodicalProgressReporter<ProgressEventArgs> _progress;

    public AddIncludePatternHandler(
        IFileCollector fileCollector,
        IFileHashCalculationService fileHashCalculationService,
        IFileHashLookupState fileHashLookupState,
        IPeriodicalProgressReporter<ProgressEventArgs> progress)
    {
        _fileCollector = fileCollector;
        _fileHashCalculationService = fileHashCalculationService;
        _fileHashLookupState = fileHashLookupState;
        _progress = progress;
    }

    public Task Handle(AddIncludePatternRequest request, CancellationToken cancellationToken)
    {
        _progress.Report(() => new ProgressEventArgs(
            activity: "Including folders or patterns",
            currentOperation: "Collecting files to include"));

        var collectedFiles = !string.IsNullOrEmpty(request.IncludePath)
            ? _fileCollector.AddIncludeFolder(request.IncludePath)
            : _fileCollector.AddIncludePattern(request.IncludePattern);

        var filesWithHash = _fileHashCalculationService.CalculateHash(collectedFiles).ToList();

        for (var i = 0; i < filesWithHash.Count; i++)
        {
            var entry = filesWithHash[i];

            _progress.Report(
                progress => new ProgressEventArgs(
                    activity: "Excluding files or patterns",
                    currentOperation: "Excluding files.",
                    currentItem: entry.File.FullName,
                    currentProgress: progress,
                    total: filesWithHash.Count),
                currentProgress: i);

            _fileHashLookupState.Add(new BasicFileInfo(entry.File, entry.Hash));
        }

        return Task.CompletedTask;
    }
}