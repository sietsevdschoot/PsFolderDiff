using System.IO.Abstractions;

namespace PsFolderDiff.FileHashLookupLib.Domain;

public class FilePattern
{
    public string Directory { get; init; } = default!;

    public string RelativePattern { get; init; } = default!;

    public string Value => $"{Directory}{RelativePattern}";

    public static FilePattern Create(IFileSystem fileSystem, string pattern)
    {
        ArgumentException.ThrowIfNullOrEmpty(pattern, nameof(pattern));

        var directory = pattern.Split("*", StringSplitOptions.TrimEntries).FirstOrDefault() ?? string.Empty;

        var relativePattern = !string.IsNullOrEmpty(directory)
            ? pattern.Replace(directory, null).Trim()
            : pattern;

        if (!string.IsNullOrWhiteSpace(directory))
        {
            directory = $@"{directory.Trim('\\')}\";
        }

        if (!string.IsNullOrEmpty(relativePattern))
        {
            relativePattern = !string.IsNullOrEmpty(directory)
                ? relativePattern
                : $@"**\{relativePattern}";
        }
        else
        {
            relativePattern = @"**\*";
        }

        if (!string.IsNullOrEmpty(directory) && !fileSystem.Directory.Exists(directory))
        {
            throw new ArgumentException($"Folder '{directory}' does not exist");
        }

        return new FilePattern
        {
            Directory = directory,
            RelativePattern = relativePattern,
        };
    }

    public override string ToString()
    {
        return Value;
    }
}