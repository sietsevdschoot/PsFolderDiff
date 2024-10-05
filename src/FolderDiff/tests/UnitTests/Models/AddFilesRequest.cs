using System.IO.Abstractions;

namespace PsFolderDiff.FileHashLookup.Services;

public class AddFilesRequest
{
    public IFileInfo[] Files { get; set; }
}