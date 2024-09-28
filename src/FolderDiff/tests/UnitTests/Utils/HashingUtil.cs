using System.IO.Abstractions;
using System.Security.Cryptography;
using PsFolderDiff.FileHashLookup.Extensions;
using PsFolderDiff.FileHashLookup.Models;

namespace PsFolderDiff.FileHashLookup.UnitTests.Utils;

public static class HashingUtil
{
    public static BasicFileInfo CreateBasicFileInfo(IFileInfo file)
    {
        return new BasicFileInfo(file, file.CalculateMD5Hash());
    }
}