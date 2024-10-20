using System.IO.Abstractions;
using System.Text.RegularExpressions;

namespace PsFolderDiff.FileHashLookupLib.Utils;

public static class PathUtils
{
    public static string CreateFilenameFromPath(IDirectoryInfo directory)
    {
        var replaceableChars = string.Join(
            "|",
            Path.GetInvalidFileNameChars().Concat([' ']).Select(x => Regex.Escape(x.ToString())));

        var filename = $"{Regex.Replace(directory.FullName, replaceableChars, "_")}.json";

        return filename;
    }
}