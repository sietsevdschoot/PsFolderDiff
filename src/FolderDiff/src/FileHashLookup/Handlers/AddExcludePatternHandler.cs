using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using MediatR;
using Microsoft.Extensions.FileSystemGlobbing;
using PsFolderDiff.FileHashLookup.Models;
using PsFolderDiff.FileHashLookup.Requests;
using PsFolderDiff.FileHashLookup.Services.Interfaces;
using PsFolderDiff.FileHashLookup.Utils;
using Vipentti.IO.Abstractions.FileSystemGlobbing;

namespace PsFolderDiff.FileHashLookup.Handlers;

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
        var inMemoryFileSystem = new MockFileSystem();

        var allFiles = _fileCollector.GetFiles();
        allFiles.ForEach(file => inMemoryFileSystem.AddFile(file, new MockFileData(string.Empty)));

        var matcher = new Matcher(StringComparison.OrdinalIgnoreCase)
            .AddInclude(excludePattern);

        // TODO: Can we add files from different drives?
        var rootDirectory = inMemoryFileSystem.DriveInfo.GetDrives().First().RootDirectory;
        var result = matcher.Execute(inMemoryFileSystem, rootDirectory.FullName);

        return result.HasMatches
            ? result.Files.Select(file => CreateFileInfo(inMemoryFileSystem, rootDirectory, file)).ToList()
            : new List<IFileInfo>();
    }

    private IFileInfo CreateFileInfo(IFileSystem fileSystem, IDirectoryInfo directory, FilePatternMatch file)
    {
        var fullName = fileSystem.Path.Combine(directory.FullName, file.Path);
        return fileSystem.FileInfo.New(fullName);
    }
}