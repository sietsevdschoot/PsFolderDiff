using System.IO.Abstractions;
using Microsoft.Extensions.FileSystemGlobbing;
using PsFolderDiff.FileHashLookupLib.Domain.Interfaces;
using PsFolderDiff.FileHashLookupLib.Extensions;
using PsFolderDiff.FileHashLookupLib.Services.Interfaces;
using PsFolderDiff.FileHashLookupLib.Utils;
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

    public IReadOnlyCollection<string> IncludePatterns => _storageModel.IncludePatterns.AsReadOnly();

    public IReadOnlyCollection<string> ExcludePatterns => _storageModel.ExcludePatterns.AsReadOnly();

    public void AddFileHashLookup(FileHashLookup other)
    {
        _storageModel.IncludePatterns.InsertNewItems(other.IncludePatterns.ToList());
        _storageModel.ExcludePatterns.InsertNewItems(other.ExcludePatterns.ToList());
    }

    public List<IFileInfo> IncludePattern(string includePattern)
    {
        EnsurePatternIsValid(includePattern);

        var parsedIncludePattern = PathUtils.ParseFileGlobbingPatternAsString(includePattern);

        _storageModel.IncludePatterns.Add(parsedIncludePattern);

        return GetFilesInternal(parsedIncludePattern);
    }

    public IFileCollector ExcludePattern(string excludePattern)
    {
        EnsurePatternIsValid(excludePattern);

        var parsedExcludePattern = PathUtils.ParseFileGlobbingPatternAsString(excludePattern);

        _storageModel.ExcludePatterns.Add(parsedExcludePattern);

        return this;
    }

    public List<IFileInfo> GetFiles()
    {
        return GetFilesInternal();
    }

    private void EnsurePatternIsValid(string pattern)
    {
        ArgumentException.ThrowIfNullOrEmpty(pattern, nameof(pattern));

        var parsedPattern = PathUtils.ParseFileGlobbingPattern(pattern);

        if (!string.IsNullOrEmpty(parsedPattern.Directory) && !_fileSystem.Directory.Exists(parsedPattern.Directory))
        {
            throw new ArgumentException($"Folder '{parsedPattern.Directory}' does not exist");
        }
    }

    private List<IFileInfo> GetFilesInternal(string? includePattern = null)
    {
        var collectedFiles = new List<IFileInfo>();

        var patternsToRetrieve = !string.IsNullOrEmpty(includePattern)
            ? [includePattern]
            : _storageModel.IncludePatterns;

        var parsedIncludePatterns = patternsToRetrieve.Select(PathUtils.ParseFileGlobbingPattern).ToList();
        var parsedExcludePatterns = _storageModel.ExcludePatterns.Select(PathUtils.ParseFileGlobbingPattern).ToList();

        foreach (var pattern in parsedIncludePatterns)
        {
            var matcher = new Matcher(StringComparison.OrdinalIgnoreCase)
                .AddInclude(pattern.RelativePattern);

            foreach (var excludePattern in parsedExcludePatterns)
            {
                matcher.AddExclude(excludePattern.RelativePattern);
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
}