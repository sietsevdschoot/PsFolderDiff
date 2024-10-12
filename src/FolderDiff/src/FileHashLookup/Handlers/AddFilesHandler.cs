using MediatR;
using PsFolderDiff.FileHashLookup.Domain;
using PsFolderDiff.FileHashLookup.Models;
using PsFolderDiff.FileHashLookup.Requests;
using PsFolderDiff.FileHashLookup.Services.Interfaces;
using PsFolderDiff.FileHashLookup.Utils;

namespace PsFolderDiff.FileHashLookup.Handlers;

public class AddFilesHandler : IRequestHandler<AddFilesRequest>
{
    private readonly IFileHashCalculationService _fileHashCalculationService;
    private readonly IFileHashLookupState _fileHashLookupState;
    private readonly IPeriodicalProgressReporter<ProgressEventArgs> _progress;

    public AddFilesHandler(
        IFileHashLookupState fileHashLookupState,
        IFileHashCalculationService fileHashCalculationService,
        IPeriodicalProgressReporter<ProgressEventArgs> progress)
    {
        _fileHashLookupState = fileHashLookupState;
        _fileHashCalculationService = fileHashCalculationService;
        _progress = progress;
    }

    public Task Handle(AddFilesRequest request, CancellationToken cancellationToken)
    {
        _progress.Report(() => new ProgressEventArgs(
            activity: "Adding files to FileHashTable",
            currentOperation: "Collecting files"));

        List<BasicFileInfo> filesToAdd;

        if (request.BasicFiles.Any())
        {
            filesToAdd = request.BasicFiles.ToList();
        }
        else
        {
            filesToAdd = _fileHashCalculationService
                .CalculateHash(request.Files.ToList())
                .Select(x => new BasicFileInfo(x.File, x.Hash))
                .ToList();
        }

        for (var i = 0; i < filesToAdd.Count; i++)
        {
            var basicFileInfo = filesToAdd[i];

            _progress.Report(
                progress => new ProgressEventArgs(
                    activity: "Adding files to FileHashTable",
                    currentOperation: "Adding files",
                    currentItem: basicFileInfo.FullName,
                    currentProgress: progress,
                    total: filesToAdd.Count),
                currentProgress: i);

            _fileHashLookupState.Add(basicFileInfo);
        }

        return Task.CompletedTask;
    }
}