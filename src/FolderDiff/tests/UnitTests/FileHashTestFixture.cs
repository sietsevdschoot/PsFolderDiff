using System.IO.Abstractions;
using PsFolderDiff.FileHashLookup.Models;
using PsFolderDiff.FileHashLookup.UnitTests.Utils;

namespace PsFolderDiff.FileHashLookup.UnitTests;

public abstract class FileHashTestFixture
{
    private readonly FileSystem _fileSystem;
    private readonly string _workingDirectory;
    private int _i;

    protected FileHashTestFixture()
    {
        _fileSystem = new FileSystem();

        _workingDirectory = _fileSystem.Path
            .Combine(_fileSystem.Path.GetTempPath(), "FolderDiff", $"{DateTime.Now:yyyy-MM-dd}-{Guid.NewGuid()}");

        _fileSystem.Directory.CreateDirectory(_workingDirectory);
        _fileSystem.Directory.SetCurrentDirectory(_workingDirectory);

        _i = 1;
    }

    public IDirectoryInfo WorkingDirectory => _fileSystem.DirectoryInfo.New(_workingDirectory);

    public BasicFileInfo[] AllFiles => WorkingDirectory.GetFiles("*.*", SearchOption.AllDirectories).Select(HashingUtil.CreateBasicFileInfo).ToArray();

    public IFileInfo WithNewFile(int? fileIdentifier = null, string? contents = null)
    {
        var fileName = _fileSystem.Path.Combine(_workingDirectory, $"{fileIdentifier ?? _i++}.txt");

        var file = _fileSystem.FileInfo.New(fileName);
        var fileContent = (contents ?? Guid.NewGuid().ToString());

        _fileSystem.File.WriteAllText(file.FullName, fileContent);

        return file;
    }

    public BasicFileInfo WithNewBasicFile(int? fileIdentifier = null, string? contents = null)
    {
        return HashingUtil.CreateBasicFileInfo(WithNewFile(fileIdentifier, contents));
    }

    public IFileInfo UpdateFile(IFileInfo file)
    {
        _fileSystem.File.AppendAllText(file.FullName, "Updated");

        return _fileSystem.FileInfo.New(file.FullName);
    }
}