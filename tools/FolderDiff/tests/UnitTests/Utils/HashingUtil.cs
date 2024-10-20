using System.IO.Abstractions;
using PsFolderDiff.FileHashLookupLib.Domain;
using PsFolderDiff.FileHashLookupLib.Extensions;

namespace PsFolderDiff.FileHashLookupLib.UnitTests.Utils;

public static class HashingUtil
{
    public static BasicFileInfo CreateBasicFileInfo(IFileInfo file)
    {
        return new BasicFileInfo(file, file.CalculateMD5Hash());
    }
}