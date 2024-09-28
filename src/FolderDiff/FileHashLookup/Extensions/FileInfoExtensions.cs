using System.IO.Abstractions;
using System.Security.Cryptography;

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

    // ReSharper disable once InconsistentNaming
    public static string CalculateMD5Hash(this IFileInfo file)
    {
        using var md5 = MD5.Create();

        var hash = Convert.ToBase64String(md5.ComputeHash(file.ReadAllBytes()));

        return hash;
    }
}