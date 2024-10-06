using System.IO.Abstractions;
using PsFolderDiff.FileHashLookup.Extensions;

namespace PsFolderDiff.FileHashLookup.Domain;

public sealed record BasicFileInfo
: IComparable<IFileInfo>,
  IComparable<BasicFileInfo>,
  IComparable<FileInfo>,
  IEquatable<FileInfo>,
  IEquatable<IFileInfo>
{
    public BasicFileInfo(IFileInfo file, string hash)
    {
        ArgumentNullException.ThrowIfNull(file, nameof(file));
        ArgumentException.ThrowIfNullOrEmpty(hash, nameof(hash));

        FullName = file.FullName;
        CreationTime = file.CreationTime;
        LastWriteTime = file.LastWriteTime;
        Length = file.Length;
        Hash = hash;
    }

    public string FullName { get; init; }

    public DateTime CreationTime { get; init; }

    public DateTime LastWriteTime { get; init; }

    public long Length { get; init; }

    public string Hash { get; init; }

    public static explicit operator FileInfo(BasicFileInfo file) => new FileInfo(file.FullName);

    public static explicit operator BasicFileInfo(FileInfoBase file) => new BasicFileInfo(file, file.CalculateMD5Hash());

    public int CompareTo(BasicFileInfo? other)
    {
        if (ReferenceEquals(null, other))
        {
            return 1;
        }

        var diff = (CreationTime - other.CreationTime + (LastWriteTime - other.LastWriteTime)).TotalMilliseconds;

        var compare = diff == 0 ? 0 : diff > 1 ? 1 : -1;

        return compare;
    }

    public int CompareTo(IFileInfo? other)
    {
        if (ReferenceEquals(null, other))
        {
            return 1;
        }

        var diff = (CreationTime - other.CreationTime + (LastWriteTime - other.LastWriteTime)).TotalMilliseconds;

        var compare = diff == 0 ? 0 : diff > 1 ? 1 : -1;

        return compare;
    }

    public int CompareTo(FileInfo? other)
    {
        if (ReferenceEquals(null, other))
        {
            return 1;
        }

        var diff = (CreationTime - other.CreationTime + (LastWriteTime - other.LastWriteTime)).TotalMilliseconds;

        var compare = diff == 0 ? 0 : diff > 1 ? 1 : -1;

        return compare;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(FullName, CreationTime, LastWriteTime, Length, Hash);
    }

    public bool Equals(BasicFileInfo? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        var isEqual = string.Equals(FullName, other.FullName, StringComparison.Ordinal)
          && CreationTime == other.CreationTime
          && LastWriteTime == other.LastWriteTime
          && Length == other.Length;

        return isEqual;
    }

    public bool Equals(FileInfo? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        var isEqual = string.Equals(FullName, other.FullName, StringComparison.Ordinal)
          && CreationTime == other.CreationTime
          && LastWriteTime == other.LastWriteTime
          && Length == other.Length;

        return isEqual;
    }

    public bool Equals(IFileInfo? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        var isEqual = string.Equals(FullName, other.FullName, StringComparison.Ordinal)
          && CreationTime == other.CreationTime
          && LastWriteTime == other.LastWriteTime
          && Length == other.Length;

        return isEqual;
    }

    public override string ToString() => FullName;
}