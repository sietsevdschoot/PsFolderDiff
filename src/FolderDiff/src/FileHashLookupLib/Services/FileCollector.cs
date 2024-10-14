using System.IO.Abstractions;
using Microsoft.Extensions.FileSystemGlobbing;
using PsFolderDiff.FileHashLookupLib.Extensions;
using PsFolderDiff.FileHashLookupLib.Services.Interfaces;
using PsFolderDiff.FileHashLookupLib.Utils;
using Vipentti.IO.Abstractions.FileSystemGlobbing;

namespace PsFolderDiff.FileHashLookupLib.Services;

public class FileCollector : IHasReadOnlyFilePatterns, IFileCollector
{
    private readonly IFileSystem _fileSystem;

    private readonly List<(string Directory, string RelativePattern)> _includePatterns;
    private readonly List<(string Directory, string RelativePattern)> _excludePatterns;

    public FileCollector(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;

        _includePatterns = new List<(string Directory, string RelativePattern)>();
        _excludePatterns = new List<(string Directory, string RelativePattern)>();
    }

    public IReadOnlyCollection<(string Directory, string RelativePattern)> IncludePatterns => _includePatterns.AsReadOnly();

    public IReadOnlyCollection<(string Directory, string RelativePattern)> ExcludePatterns => _excludePatterns.AsReadOnly();

    public List<IFileInfo> AddIncludeFolder(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

        return AddIncludePattern(path);
    }

    public List<IFileInfo> AddIncludePattern(string includePattern)
    {
        EnsurePatternIsValid(includePattern);

        var parsedIncludePattern = PathUtils.ParseFileGlobbingPattern(includePattern);

        _includePatterns.Add(parsedIncludePattern);

        return GetFilesInternal(parsedIncludePattern);
    }

    public void AddFileHashLookup(FileHashLookup other)
    {
        _includePatterns.InsertNewItems(other.IncludePatterns.ToList());
        _excludePatterns.InsertNewItems(other.ExcludePatterns.ToList());
     }

    public IFileCollector AddExcludePattern(string excludePattern)
    {
        EnsurePatternIsValid(excludePattern);

        var parsedExcludePattern = PathUtils.ParseFileGlobbingPattern(excludePattern);

        _excludePatterns.Add(parsedExcludePattern);

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

    private List<IFileInfo> GetFilesInternal((string Directory, string RelativePattern) includePattern = default)
    {
        var collectedFiles = new List<IFileInfo>();

        var patternsToRetrieve = !includePattern.Equals(default)
            ? [includePattern]
            : _includePatterns;

        foreach (var pattern in patternsToRetrieve)
        {
            var matcher = new Matcher(StringComparison.OrdinalIgnoreCase)
                .AddInclude(pattern.RelativePattern);

            foreach (var excludePattern in _excludePatterns)
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