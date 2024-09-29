using System.IO.Abstractions;
using Microsoft.Extensions.FileSystemGlobbing;
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

    public FileCollector AddIncludeFolder(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

        return AddIncludePattern($@"{path.Trim('\\')}\**\");
    }

    public FileCollector AddIncludePattern(string includePattern)
    {
        EnsurePatternIsValid(includePattern);

        _includePatterns.Add(ParsePattern(includePattern));

        return this;
    }

    public FileCollector AddExcludePattern(string excludePattern)
    {
        ArgumentException.ThrowIfNullOrEmpty(excludePattern, nameof(excludePattern));

        _excludePatterns.Add(excludePattern);

        return this;
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

    public List<IFileInfo> GetFiles()
    {
        var collectedFiles = new List<IFileInfo>();

        foreach (var includePattern in _includePatterns)
        {
            var matcher = new Matcher(StringComparison.OrdinalIgnoreCase).AddInclude(includePattern.RelativePattern);

            foreach (var excludePattern in _excludePatterns)
            {
                matcher.AddExclude(excludePattern);
            }

            var result = matcher.Execute(_fileSystem, includePattern.Directory);

            if (result.HasMatches)
            {
                foreach (var filePatternMatch in result.Files)
                {
                    var foundFile = _fileSystem.FileInfo.New(
                        _fileSystem.Path.Combine(includePattern.Directory, filePatternMatch.Path));

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