using System.IO.Abstractions;

namespace PsFolderDiff.FileHashLookup.Extensions;

public static class FileInfoExtensions
{
    public static byte[] ReadAllBytes(this IFileInfo file)
    {
        using var stream = file.OpenRead();
        using var ms = new MemoryStream();
        
        stream.CopyTo(ms);

        return ms.ToArray();
    }
}