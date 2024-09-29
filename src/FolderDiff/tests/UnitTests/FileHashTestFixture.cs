using System.IO.Abstractions;
using PsFolderDiff.FileHashLookup.Models;
using PsFolderDiff.FileHashLookup.UnitTests.Utils;

namespace PsFolderDiff.FileHashLookup.UnitTests;

public abstract class FileHashTestFixture
{
    protected readonly FileSystem FileSystem;
    private readonly string _workingDirectory;
    private int _i;

    protected FileHashTestFixture()
    {
        FileSystem = new FileSystem();

        _workingDirectory = FileSystem.Path
            .Combine(FileSystem.Path.GetTempPath(), "FolderDiff", $"{DateTime.Now:yyyy-MM-dd}-{Guid.NewGuid()}");

        FileSystem.Directory.CreateDirectory(_workingDirectory);
        FileSystem.Directory.SetCurrentDirectory(_workingDirectory);

        _i = 1;
    }

    public IDirectoryInfo WorkingDirectory => FileSystem.DirectoryInfo.New(_workingDirectory);

    public BasicFileInfo[] AllFiles => WorkingDirectory.GetFiles("*.*", SearchOption.AllDirectories).Select(HashingUtil.CreateBasicFileInfo).ToArray();

    public IFileInfo WithNewFile(int? fileIdentifier = null, string? contents = null)
    {
        return WithNewFile($"{fileIdentifier ?? _i++}.txt", contents);
    }

    public IFileInfo WithNewFile(string relativePath, string? contents = null)
    {
        var fullName = FileSystem.Path.Combine(_workingDirectory, relativePath);

        if (!FileSystem.Path.HasExtension(fullName))
        {
            throw new ArgumentException($"Expected relative path to a file. Found: '{relativePath}'", nameof(relativePath));
        }

        var file = FileSystem.FileInfo.New(fullName);
        var fileContent = (contents ?? Guid.NewGuid().ToString());

        if (!file.Directory!.Exists)
        {
            FileSystem.Directory.CreateDirectory(file.Directory.FullName);
        } 

        FileSystem.File.WriteAllText(file.FullName, fileContent);

        return file;
    }

    public BasicFileInfo WithNewBasicFile(int? fileIdentifier = null, string? contents = null)
    {
        return HashingUtil.CreateBasicFileInfo(WithNewFile(fileIdentifier, contents));
    }

    public IFileInfo UpdateFile(IFileInfo file)
    {
        return UpdateFile(file.FullName);
    }

    public BasicFileInfo UpdateFile(BasicFileInfo file)
    {
        return HashingUtil.CreateBasicFileInfo(UpdateFile(file.FullName));
    }
    
    public IFileInfo UpdateFile(string fullName)
    {
        FileSystem.File.AppendAllText(fullName, "Updated");

        return FileSystem.FileInfo.New(fullName);
    }
}