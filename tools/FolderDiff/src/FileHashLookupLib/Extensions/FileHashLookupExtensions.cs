using System.Text;
using PsFolderDiff.FileHashLookupLib.Services;

namespace PsFolderDiff.FileHashLookupLib.Extensions;

public static class FileHashLookupExtensions
{
    public static string GetFileHashLookupDescription(this FileHashLookup fileHashLookup)
    {
        var msg = new StringBuilder($"\n{nameof(FileHashLookup)} is empty.\n");

        if (fileHashLookup.File.Keys.Any())
        {
            var totalFileSize = fileHashLookup.File.Values.Sum(x => x.Length);

            msg = new StringBuilder(
                $"\n{nameof(FileHashLookup)} contains {fileHashLookup.File.Keys.Count()} files ({totalFileSize.FormatFileSize()}).\n");
        }

        msg.AppendLine(fileHashLookup.IncludePatterns.Any()
            ? $"\nIncluded patterns: \n\n{string.Join("\n", fileHashLookup.IncludePatterns.Select(x => $"  > {x}"))}"
            : $"\nIncluded patterns: <none>");

        msg.AppendLine(fileHashLookup.ExcludePatterns.Any()
            ? $"\nExcluded patterns: \n\n{string.Join("\n", fileHashLookup.ExcludePatterns.Select(x => $"  > {x}"))}"
            : $"\nExcluded patterns: <none>");

        if (!string.IsNullOrEmpty(fileHashLookup.SavedAsFile))
        {
            msg.AppendLine($"\nLast saved: {fileHashLookup.SavedAsFile}");
        }

        msg.AppendLine($"\nLast updated: {fileHashLookup.LastUpdated.ToString("dd-MM-yyyy HH:mm:ss")}\n");

        return msg.ToString();
    }
}