using System.IO.Abstractions;
using System.Security.Cryptography;
using PsFolderDiff.FileHashLookup.Extensions;

namespace PsFolderDiff.FileHashLookup.Services;

public class FileHashCalculationService : IFileHashCalculationService
{
    public IEnumerable<(IFileInfo File, string Hash)> CalculateHash(List<IFileInfo> files)
    {
        foreach (var file in files)
        {
            yield return (File: file, Hash: file.CalculateMD5Hash());
        }
    }
}