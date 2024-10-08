using System.IO.Abstractions;
using PsFolderDiff.FileHashLookup.Domain;
using PsFolderDiff.FileHashLookup.Extensions;

namespace PsFolderDiff.FileHashLookup.UnitTests.Utils;

public static class HashingUtil
{
    public static BasicFileInfo CreateBasicFileInfo(IFileInfo file)
    {
        return new BasicFileInfo(file, file.CalculateMD5Hash());
    }
}