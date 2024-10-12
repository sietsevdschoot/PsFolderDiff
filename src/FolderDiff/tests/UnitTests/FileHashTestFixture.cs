using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Microsoft.Extensions.DependencyInjection;
using PsFolderDiff.FileHashLookup.Configuration;
using PsFolderDiff.FileHashLookup.Domain;
using PsFolderDiff.FileHashLookup.UnitTests.Utils;

namespace PsFolderDiff.FileHashLookup.UnitTests;

public abstract class FileHashTestFixture
{
    #pragma warning disable SA1401 // Field is used by other private fixtures.

    private readonly string _workingDirectory;
    private int _i = 1;

    protected FileHashTestFixture()
    {
        FileSystem = new MockFileSystem();

        _workingDirectory = FileSystem.Path
            .Combine(FileSystem.Path.GetTempPath(), "FolderDiff", $"{DateTime.Now:yyyy-MM-dd}-{Guid.NewGuid()}");

        FileSystem.Directory.CreateDirectory(_workingDirectory);
        FileSystem.Directory.SetCurrentDirectory(_workingDirectory);
    }

    public IFileSystem FileSystem { get; }

    public IDirectoryInfo WorkingDirectory => FileSystem.DirectoryInfo.New(_workingDirectory);

    public BasicFileInfo[] AllFiles => WorkingDirectory.GetFiles("*.*", SearchOption.AllDirectories).Select(HashingUtil.CreateBasicFileInfo).ToArray();

    public int GetNextIdentifier()
    {
        return _i++;
    }
}