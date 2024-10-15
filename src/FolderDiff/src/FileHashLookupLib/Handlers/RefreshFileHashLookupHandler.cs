using System.IO.Abstractions;
using MediatR;
using PsFolderDiff.FileHashLookupLib.Domain;
using PsFolderDiff.FileHashLookupLib.Extensions;
using PsFolderDiff.FileHashLookupLib.Models;
using PsFolderDiff.FileHashLookupLib.Requests;
using PsFolderDiff.FileHashLookupLib.Services.Interfaces;
using PsFolderDiff.FileHashLookupLib.Utils;

namespace PsFolderDiff.FileHashLookupLib.Handlers;

public class RefreshFileHashLookupHandler : IRequestHandler<RefreshRequest>
{
    private readonly IFileCollector _fileCollector;
    private readonly IFileHashLookupState _fileHashLookupState;
    private readonly IFileSystem _fileSystem;
    private readonly IPeriodicalProgressReporter<ProgressEventArgs> _progress;
    private IFileHashCalculationService _fileHashCalculationService;

    public RefreshFileHashLookupHandler(
        IFileCollector fileCollector,
        IFileHashLookupState fileHashLookupState,
        IFileSystem fileSystem,
        IFileHashCalculationService fileHashCalculationService,
        IPeriodicalProgressReporter<ProgressEventArgs> progress)
    {
        _fileHashLookupState = fileHashLookupState;
        _fileCollector = fileCollector;
        _fileSystem = fileSystem;
        _fileHashCalculationService = fileHashCalculationService;
        _progress = progress;
    }

    public Task Handle(RefreshRequest request, CancellationToken cancellationToken)
    {
        //// check exist on files in Hash
        //// Collect files from globs
        //// Check if new or updated
        //// calculate hash
        //// new -> add
        //// updated -> remove, add

        _progress.Report(() => new ProgressEventArgs(
            activity: "Refresh FileHashLookup",
            currentOperation: "Collecting files..."));

        var allCollectedFiles = _fileCollector.GetFiles();
        var filesInFileHashLookup = _fileHashLookupState.GetFiles().Select(file => file.AsFileInfo(_fileSystem)).ToList();

        _progress.Report(() => new ProgressEventArgs(
            activity: "Refresh FileHashLookup",
            currentOperation: "Detecting changes..."));

        var matchedFiles = allCollectedFiles.Select(file => new
        {
            File = file,
            FileStatus = _fileHashLookupState.Contains(file),
        })
        .ToList();

        var addedFiles = matchedFiles.Where(x => x.FileStatus == FileContainsState.NoMatch).Select(x => x.File).ToList();
        var modifiedFiles = matchedFiles.Where(x => x.FileStatus == FileContainsState.Modified).Select(x => x.File).ToList();
        var removedFiles = filesInFileHashLookup.Where(x => !x.Exists).ToList();

        var newAndUpdatedFiles = _fileHashCalculationService.CalculateHash(addedFiles.Concat(modifiedFiles).ToList())
            .Select(x => new BasicFileInfo(x.File, x.Hash))
            .ToList();

        _progress.Report(() => new ProgressEventArgs(
            activity: "Refresh FileHashLookup",
            currentOperation: "Updating FileHashLookup..."));

        newAndUpdatedFiles.ForEach(_fileHashLookupState.Add);
        removedFiles.ForEach(_fileHashLookupState.Remove);

        return Task.CompletedTask;
    }
}