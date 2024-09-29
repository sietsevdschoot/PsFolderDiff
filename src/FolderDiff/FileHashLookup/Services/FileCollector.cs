using System.IO.Abstractions;
using Microsoft.Extensions.FileSystemGlobbing;
using PsFolderDiff.FileHashLookup.Models;
using Vipentti.IO.Abstractions.FileSystemGlobbing;

namespace PsFolderDiff.FileHashLookup.Services;

public class FileCollector
{
    private readonly IFileSystem _fileSystem;

    private readonly List<(string Directory, string RelativePattern)> _includePatterns;
    private readonly List<string> _excludePatterns;

    public FileCollector(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;

        _includePatterns = new List<(string Directory, string RelativePattern)>();
        _excludePatterns = new List<string>();
    }

    public List<IFileInfo> AddIncludeFolder(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

        return AddIncludePattern($@"{path.Trim('\\')}\**\");
    }

    public List<IFileInfo> AddIncludePattern(string includePattern)
    {
        EnsurePatternIsValid(includePattern);

        var parsedIncludePattern = ParsePattern(includePattern);

        _includePatterns.Add(parsedIncludePattern);

        return GetFilesInternal(parsedIncludePattern);
    }

    public FileCollector AddExcludePattern(string excludePattern)
    {
        ArgumentException.ThrowIfNullOrEmpty(excludePattern, nameof(excludePattern));

        _excludePatterns.Add(excludePattern);

        return this;
    }

    public List<IFileInfo> GetFiles()
    {
        return GetFilesInternal();
    }

    private void EnsurePatternIsValid(string pattern)
    {
        ArgumentException.ThrowIfNullOrEmpty(pattern, nameof(pattern));

        var parsedPattern = ParsePattern(pattern);

        if (!_fileSystem.Directory.Exists(parsedPattern.Directory))
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
                matcher.AddExclude(excludePattern);
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

    

    private (string Directory, string RelativePattern) ParsePattern(string pattern)
    {
        var directory = pattern.Split("*", StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;

        var relativePattern = pattern.Replace(directory, null);

        return (Directory: directory, RelativePattern: relativePattern);
    }
}