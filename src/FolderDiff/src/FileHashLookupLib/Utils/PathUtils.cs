﻿using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;

namespace PsFolderDiff.FileHashLookupLib.Utils;

public static class PathUtils
{
    public static (string Directory, string RelativePattern) ParseFileGlobbingPattern(string pattern)
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

        return (Directory: directory, RelativePattern: relativePattern);
    }

    public static string CreateFilenameFromPath(IDirectoryInfo directory)
    {
        var replaceableChars = string.Join(
            "|",
            Path.GetInvalidFileNameChars().Concat([' ']).Select(x => Regex.Escape(x.ToString())));

        var filename = $"{Regex.Replace(directory.FullName, replaceableChars, "_")}.json";

        return filename;
    }
}