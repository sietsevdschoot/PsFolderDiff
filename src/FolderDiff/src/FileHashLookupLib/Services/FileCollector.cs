using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Microsoft.Extensions.FileSystemGlobbing;
using PsFolderDiff.FileHashLookupLib.Domain;
using PsFolderDiff.FileHashLookupLib.Domain.Interfaces;
using PsFolderDiff.FileHashLookupLib.Extensions;
using PsFolderDiff.FileHashLookupLib.Services.Interfaces;
using Vipentti.IO.Abstractions.FileSystemGlobbing;

namespace PsFolderDiff.FileHashLookupLib.Services;

public class FileCollector : IHasReadOnlyFilePatterns, IFileCollector
{
    private readonly IFileSystem _fileSystem;
    private readonly ISupportFilePatterns _storageModel;

    public FileCollector(
        ISupportFilePatterns storageModel,
        IFileSystem fileSystem)
    {
        _storageModel = storageModel;
        _fileSystem = fileSystem;
    }

    public IReadOnlyCollection<FilePattern> IncludePatterns => _storageModel.IncludePatterns.AsReadOnly();

    public IReadOnlyCollection<FilePattern> ExcludePatterns => _storageModel.ExcludePatterns.AsReadOnly();

    public void AddFileHashLookup(FileHashLookup other)
    {
        _storageModel.IncludePatterns.InsertNewItems(other.IncludePatterns.Select(x => FilePattern.Create(_fileSystem, x)).ToList());
        _storageModel.ExcludePatterns.InsertNewItems(other.ExcludePatterns.Select(x => FilePattern.Create(_fileSystem, x)).ToList());
    }

    public List<IFileInfo> IncludePattern(string includePattern)
    {
        var parsedIncludePattern = FilePattern.Create(_fileSystem, includePattern);

        _storageModel.IncludePatterns.Add(parsedIncludePattern);

        var filesToInclude = GetFilesInternal(parsedIncludePattern);

        return filesToInclude;
    }

    public List<IFileInfo> ExcludePattern(string excludePattern)
    {
        var parsedExcludePattern = FilePattern.Create(_fileSystem, excludePattern);

        var getFilesToExclude = GetFilesToExclude(parsedExcludePattern);

        _storageModel.ExcludePatterns.Add(parsedExcludePattern);

        return getFilesToExclude;
    }

    public List<IFileInfo> GetFiles()
    {
        return GetFilesInternal();
    }

    private List<IFileInfo> GetFilesInternal(FilePattern? includePattern = null)
    {
        var collectedFiles = new List<IFileInfo>();

        var patternsToRetrieve = includePattern != null
            ? [includePattern]
            : _storageModel.IncludePatterns;

        foreach (var pattern in patternsToRetrieve)
        {
            var matcher = new Matcher(StringComparison.OrdinalIgnoreCase)
                .AddInclude(pattern.RelativePattern);

            foreach (var excludePattern in ExcludePatterns)
            {
                if (string.IsNullOrEmpty(excludePattern.Directory) || excludePattern.Directory.StartsWith(pattern.Directory))
                {
                    matcher.AddExclude(excludePattern.RelativePattern);
                }
            }

            var result = matcher.Execute(_fileSystem, pattern.Directory);

            if (result.HasMatches)
            {
                foreach (var filePatternMatch in result.Files)
                {
                    var fullName = _fileSystem.Path.Combine(pattern.Directory, filePatternMatch.Path);
                    var foundFile = _fileSystem.FileInfo.New(fullName);

                    collectedFiles.Add(foundFile);
                }
            }
        }

        return collectedFiles;
    }

    private List<IFileInfo> GetFilesToExclude(FilePattern excludePattern)
    {
        var inMemoryFileSystem = new MockFileSystem();

        var allFiles = GetFilesInternal();
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