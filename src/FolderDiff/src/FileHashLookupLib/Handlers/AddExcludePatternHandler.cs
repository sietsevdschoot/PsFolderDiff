using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using MediatR;
using Microsoft.Extensions.FileSystemGlobbing;
using PsFolderDiff.FileHashLookupLib.Models;
using PsFolderDiff.FileHashLookupLib.Requests;
using PsFolderDiff.FileHashLookupLib.Services.Interfaces;
using PsFolderDiff.FileHashLookupLib.Utils;
using Vipentti.IO.Abstractions.FileSystemGlobbing;

namespace PsFolderDiff.FileHashLookupLib.Handlers;

public class AddExcludePatternHandler : IRequestHandler<AddExcludePatternRequest>
{
    private readonly IFileCollector _fileCollector;
    private readonly IFileHashLookupState _fileHashLookupState;
    private readonly IPeriodicalProgressReporter<ProgressEventArgs> _progress;

    public AddExcludePatternHandler(
        IFileCollector fileCollector,
        IFileHashLookupState fileHashLookupState,
        IPeriodicalProgressReporter<ProgressEventArgs> progress)
    {
        _fileCollector = fileCollector;
        _fileHashLookupState = fileHashLookupState;
        _progress = progress;
    }

    public Task Handle(AddExcludePatternRequest request, CancellationToken cancellationToken)
    {
        _progress.Report(() => new ProgressEventArgs(
            activity: "Excluding files or patterns",
            currentOperation: "Collecting files to remove"));

        var files = CollectFilesToExclude(request.ExcludePattern);

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

        _fileCollector.AddExcludePattern(request.ExcludePattern);

        return Task.CompletedTask;
    }

    private List<IFileInfo> CollectFilesToExclude(string excludePattern)
    {
        var parsedPattern = PathUtils.ParseFileGlobbingPattern(excludePattern);

        var inMemoryFileSystem = new MockFileSystem();

        var allFiles = _fileCollector.GetFiles();
        allFiles.ForEach(file => inMemoryFileSystem.AddFile(file, new MockFileData(string.Empty)));

        var matcher = new Matcher(StringComparison.OrdinalIgnoreCase)
            .AddInclude(parsedPattern.RelativePattern);

        List<IFileInfo> matchedFiles;

        if (!string.IsNullOrEmpty(parsedPattern.Directory))
        {
            matchedFiles = (
                from file in matcher.Execute(inMemoryFileSystem, parsedPattern.Directory).Files
                select CreateFileInfo(inMemoryFileSystem, parsedPattern.Directory, file))
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