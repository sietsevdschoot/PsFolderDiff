using System.IO.Abstractions;
using System.Security.Cryptography;
using PsFolderDiff.FileHashLookup.Extensions;

namespace PsFolderDiff.FileHashLookup.Services;

public class FileHashCalculationService : IFileHashCalculationService
{
    public IEnumerable<(IFileInfo File, string Hash)> CalculateHash(List<IFileInfo> files)
    {
        using var md5 = MD5.Create();

        foreach (var file in files)
        {
            var hash = Convert.ToBase64String(md5.ComputeHash(file.ReadAllBytes()));

            yield return (File: file, Hash: hash);
        }
    }
}