namespace PsFolderDiff.FileHashLookupLib.Extensions;

public static class FiileSizeExtensions
{
    private static readonly string[] Suffixes = ["bytes", "KB", "MB", "GB", "TB", "PB"];

    public static string FormatFileSize(this long size)
    {
        int i = 0;

        while (size / 1024 >= 1)
        {
            size /= 1024;
            i++;
        }

        return string.Format($"{Math.Round(Convert.ToDecimal(size), i > 1 ? 2 : 0)} {Suffixes[i]}");
    }
}