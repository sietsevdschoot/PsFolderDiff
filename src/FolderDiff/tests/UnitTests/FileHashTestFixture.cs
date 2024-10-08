using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PsFolderDiff.FileHashLookup.Configuration;
using PsFolderDiff.FileHashLookup.Domain;
using PsFolderDiff.FileHashLookup.UnitTests.Utils;
using FileSystem = System.IO.Abstractions.FileSystem;

namespace PsFolderDiff.FileHashLookup.UnitTests;

public abstract class FileHashTestFixture
{
    #pragma warning disable SA1401 // Field is used by other private fixtures.
    protected readonly MockFileSystem FileSystem;

    private static readonly PollingUtil PollingUtil;
    private readonly string _workingDirectory;
    private int _i;

    static FileHashTestFixture()
    {
        var sp = new ServiceCollection()
            .AddLogging(builder => builder
                .AddConsole()
                .SetMinimumLevel(LogLevel.Warning))
            .AddSingleton<PollingUtil>()
            .BuildServiceProvider();

        PollingUtil = sp.GetRequiredService<PollingUtil>();
    }

    protected FileHashTestFixture()
    {
        FileSystem = new MockFileSystem();

        _workingDirectory = FileSystem.Path
            .Combine(FileSystem.Path.GetTempPath(), "FolderDiff", $"{DateTime.Now:yyyy-MM-dd}-{Guid.NewGuid()}");

        FileSystem.Directory.CreateDirectory(_workingDirectory);
        FileSystem.Directory.SetCurrentDirectory(_workingDirectory);

        _i = 1;
    }

    public IDirectoryInfo WorkingDirectory => FileSystem.DirectoryInfo.New(_workingDirectory);

    public BasicFileInfo[] AllFiles => WorkingDirectory.GetFiles("*.*", SearchOption.AllDirectories).Select(HashingUtil.CreateBasicFileInfo).ToArray();

    public IFileInfo AsFileInfo(BasicFileInfo file)
    {
        return FileSystem.FileInfo.New(file.FullName);
    }

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

        var directoryName = FileSystem.Path.GetDirectoryName(fullName)!;

        var fileContent = contents ?? Guid.NewGuid().ToString();

        if (!FileSystem.Directory.Exists(directoryName))
        {
            FileSystem.Directory.CreateDirectory(directoryName);
        }

        var file = new MockFileInfo(FileSystem, fullName);
        FileSystem.AddFile(file, new MockFileData(fileContent));

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

    public FileHashLookup.Services.FileHashLookup CreateFileHashLookup()
    {
        return FileHashLookup.Services.FileHashLookup.Create(new FileHashLookupSettings
        {
            ReportPollingDelay = TimeSpan.FromMilliseconds(500),
            ConfigureServices = (services, sp) =>
            {
                services.AddSingleton<IFileSystem>(FileSystem);
            },
        });
    }
}