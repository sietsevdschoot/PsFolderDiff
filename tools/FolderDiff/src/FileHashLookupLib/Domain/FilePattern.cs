using System.IO.Abstractions;

namespace PsFolderDiff.FileHashLookupLib.Domain;

public class FilePattern : IEquatable<FilePattern>
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

            directory = fileSystem.Path.IsPathRooted(directory)
                ? fileSystem.Path.GetFullPath(directory)
                : fileSystem.Path.GetFullPath(directory, fileSystem.Directory.GetCurrentDirectory());
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

    #region Equality member implementation

    public override string ToString()
    {
        return Value;
    }

    public bool Equals(FilePattern? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Directory == other.Directory && RelativePattern == other.RelativePattern;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((FilePattern)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Directory, RelativePattern);
    }

    #endregion
}