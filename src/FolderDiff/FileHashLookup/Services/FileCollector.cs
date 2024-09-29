using System.IO.Abstractions;
using Microsoft.Extensions.FileSystemGlobbing;
using Vipentti.IO.Abstractions.FileSystemGlobbing;

namespace PsFolderDiff.FileHashLookup.Services;

public class FileCollector
{
    private readonly IFileSystem _fileSystem;
    private readonly List<string> _includePatterns;

    public FileCollector(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;

        _includePatterns = new List<string>();
    }

    public FileCollector AddIncludeFolder(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

        return AddIncludePattern($@"{path.Trim('\\')}\**\");
    }

    public FileCollector AddIncludePattern(string includePattern)
    {
        ArgumentException.ThrowIfNullOrEmpty(includePattern, nameof(includePattern));

        var pattern = ParsePattern(includePattern);

        if (!_fileSystem.Directory.Exists(pattern.Directory))
        {
            throw new ArgumentException($"Folder '{pattern.Directory}' does not exist");
        }

        _includePatterns.Add(includePattern);

        return this;
    }

    public List<IFileInfo> GetFiles()
    {
        var collectedFiles = new List<IFileInfo>();

        foreach (var includePattern in _includePatterns)
        {
            var pattern = ParsePattern(includePattern);
            
            var matcher = new Matcher(StringComparison.OrdinalIgnoreCase)
                .AddInclude(pattern.RelativePattern);

            var result = matcher.Execute(_fileSystem, pattern.Directory);

            if (result.HasMatches)
            {
                foreach (var filePatternMatch in result.Files)
                {
                    var foundFile = _fileSystem.FileInfo.New(_fileSystem.Path.Combine(pattern.Directory, filePatternMatch.Path));

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