using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using MediatR;
using Microsoft.Extensions.FileSystemGlobbing;
using PsFolderDiff.FileHashLookupLib.Domain;
using PsFolderDiff.FileHashLookupLib.Requests;
using PsFolderDiff.FileHashLookupLib.Services.Interfaces;
using PsFolderDiff.FileHashLookupLib.Utils;
using Vipentti.IO.Abstractions.FileSystemGlobbing;

namespace PsFolderDiff.FileHashLookupLib.Handlers;

public class ExcludePatternHandler : IRequestHandler<ExcludePatternRequest>
{
    private readonly IFileCollector _fileCollector;
    private readonly IFileHashLookupState _fileHashLookupState;
    private readonly IPeriodicalProgressReporter<ProgressEventArgs> _progress;
    private readonly IFileSystem _fileSystem;

    public ExcludePatternHandler(
        IFileCollector fileCollector,
        IFileHashLookupState fileHashLookupState,
        IFileSystem fileSystem,
        IPeriodicalProgressReporter<ProgressEventArgs> progress)
    {
        _fileCollector = fileCollector;
        _fileHashLookupState = fileHashLookupState;
        _fileSystem = fileSystem;
        _progress = progress;
    }

    public Task Handle(ExcludePatternRequest request, CancellationToken cancellationToken)
    {
        _progress.Report(() => new ProgressEventArgs(
            activity: "Excluding files or patterns",
            currentOperation: "Collecting files to remove"));

        var files = CollectFilesToExclude(FilePattern.Create(_fileSystem, request.ExcludePattern));

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
            }
        }

        _fileCollector.ExcludePattern(request.ExcludePattern);

        return Task.CompletedTask;
    }

    private List<IFileInfo> CollectFilesToExclude(FilePattern excludePattern)
    {
        var inMemoryFileSystem = new MockFileSystem();

        var allFiles = _fileCollector.GetFiles();
        allFiles.ForEach(file => inMemoryFileSystem.AddFile(file, new MockFileData(string.Empty)));

        var matcher = new Matcher(StringComparison.OrdinalIgnoreCase)
            .AddInclude(excludePattern.RelativePattern);

        List<IFileInfo> matchedFiles;

        if (!string.IsNullOrEmpty(excludePattern.Directory))
        {
            matchedFiles = (
                from file in matcher.Execute(inMemoryFileSystem, excludePattern.Directory).Files
                select CreateFileInfo(inMemoryFileSystem, excludePattern.Directory, file))
            .ToList();
        }
        else
        {
            matchedFiles = (
                from drive in inMemoryFileSystem.AllDrives
                from file in matcher.Execute(inMemoryFileSystem, drive).Files
                select CreateFileInfo(inMemoryFileSystem, drive, file))
            .ToList();
        }

        return matchedFiles;
    }

    private IFileInfo CreateFileInfo(IFileSystem fileSystem, string directory, FilePatternMatch file)
    {
        var fullName = fileSystem.Path.Combine(directory, file.Path);
        return fileSystem.FileInfo.New(fullName);
    }
}