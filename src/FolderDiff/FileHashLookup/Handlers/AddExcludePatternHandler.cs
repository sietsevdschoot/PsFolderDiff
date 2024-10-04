using System.IO.Abstractions.TestingHelpers;
using MediatR;
using Microsoft.Extensions.FileSystemGlobbing;
using PsFolderDiff.FileHashLookup.Models;
using PsFolderDiff.FileHashLookup.Requests;
using PsFolderDiff.FileHashLookup.Services;
using Vipentti.IO.Abstractions.FileSystemGlobbing;


namespace PsFolderDiff.FileHashLookup.Handlers;

public class AddExcludePatternHandler : IRequestHandler<AddExcludePatternRequest>
{
    private readonly FileCollector _fileCollector;
    private readonly FileHashLookupState _fileHashLookupState;

    public AddExcludePatternHandler(
        FileCollector fileCollector,
        FileHashLookupState fileHashLookupState)
    {
        _fileCollector = fileCollector;
        _fileHashLookupState = fileHashLookupState;
    }

    public Task Handle(AddExcludePatternRequest request, CancellationToken cancellationToken)
    {
        var inMemoryFileSystem = new MockFileSystem();

        var allFiles = _fileCollector.GetFiles();
        allFiles.ForEach(file => inMemoryFileSystem.AddFile(file, new MockFileData(string.Empty)));

        var matcher = new Matcher(StringComparison.OrdinalIgnoreCase)
            .AddInclude(request.ExcludePattern);

        var rootDirectory = inMemoryFileSystem.DriveInfo.GetDrives().First().RootDirectory;
        var result = matcher.Execute(inMemoryFileSystem, rootDirectory.FullName);

        if (result.HasMatches)
        {
            var files = result.Files.Select(x =>
            {
                var fullName = inMemoryFileSystem.Path.Combine(rootDirectory.FullName, x.Path);
                return inMemoryFileSystem.FileInfo.New(fullName);
            })
            .ToList();

            foreach (var file in files)
            {
                _fileHashLookupState.Remove(file);
            }
        }

        _fileCollector.AddExcludePattern(request.ExcludePattern);

        return Task.CompletedTask;
    }
}