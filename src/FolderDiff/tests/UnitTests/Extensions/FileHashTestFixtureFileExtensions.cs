using System.IO.Abstractions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PsFolderDiff.FileHashLookupLib.Domain;
using PsFolderDiff.FileHashLookupLib.UnitTests.Utils;

namespace PsFolderDiff.FileHashLookupLib.UnitTests.Extensions;

public static class FileHashTestFixtureFileExtensions
{
    public static IFileInfo GetFileInfo<TFixture>(this TFixture fixture, string filename)
        where TFixture : FileHashTestFixture
    {
        var file = fixture.AllFiles.SingleOrDefault(x =>
            string.Equals(fixture.FileSystem.Path.GetFileName(x.FullName), filename, StringComparison.InvariantCultureIgnoreCase));

        if (file == null)
        {
            throw new FileNotFoundException($"Not found", filename);
        }

        return fixture.AsFileInfo(file);
    }

    public static IFileInfo GetFileInfo<TFixture>(this TFixture fixture, int identifier)
        where TFixture : FileHashTestFixture
    {
        var file = fixture.AllFiles.Select(fixture.AsFileInfo).SingleOrDefault(x =>
            string.Equals(fixture.FileSystem.Path.GetFileNameWithoutExtension(x.Name), identifier.ToString(), StringComparison.InvariantCultureIgnoreCase));

        if (file == null)
        {
            throw new FileNotFoundException($"Not found", $"{identifier}.*");
        }

        return file;
    }

    public static BasicFileInfo GetBasicFileInfo<TFixture>(this TFixture fixture, string name)
        where TFixture : FileHashTestFixture
    {
        var file = fixture.AllFiles.SingleOrDefault(x =>
            string.Equals(fixture.FileSystem.Path.GetFileName(x.FullName), name, StringComparison.InvariantCultureIgnoreCase));

        if (file == null)
        {
            throw new FileNotFoundException($"Not found", name);
        }

        return file;
    }

    public static BasicFileInfo GetBasicFileInfo<TFixture>(this TFixture fixture, int identifier)
        where TFixture : FileHashTestFixture
    {
        var file = fixture.AllFiles.SingleOrDefault(x =>
            string.Equals(fixture.FileSystem.Path.GetFileNameWithoutExtension(x.FullName), identifier.ToString(), StringComparison.InvariantCultureIgnoreCase));

        if (file == null)
        {
            throw new FileNotFoundException($"Not found", $"{identifier}.*");
        }

        return file;
    }

    public static TFixture WithAddedFiles<TFixture>(this TFixture fixture, int nrOfFiles = 10, string? fileContents = null)
        where TFixture : FileHashTestFixture
    {
        return fixture.WithAddedFiles(Enumerable.Range(1, nrOfFiles).ToList(), fileContents);
    }

    public static TFixture WithAddedFiles<TFixture>(this TFixture fixture, List<int> range, string? fileContents = null)
        where TFixture : FileHashTestFixture
    {
        List<BasicFileInfo> files = range.Select(x => fixture.WithNewBasicFile(x, fileContents)).ToList();

        files.ForEach(file => fixture.AddFile(file.FullName, fileContents));

        return fixture;
    }

    public static IFileInfo AsFileInfo<TFixture>(this TFixture fixture, BasicFileInfo file)
        where TFixture : FileHashTestFixture
    {
        return fixture.FileSystem.FileInfo.New(file.FullName);
    }

    public static BasicFileInfo AsBasicFileInfo<TFixture>(this TFixture fixture, IFileInfo file)
        where TFixture : FileHashTestFixture
    {
        return HashingUtil.CreateBasicFileInfo(file);
    }

    public static IFileInfo WithNewFile<TFixture>(this TFixture fixture, int? fileIdentifier = null, string? contents = null)
        where TFixture : FileHashTestFixture
    {
        return fixture.WithNewFile($"{fileIdentifier ?? fixture.GetNextIdentifier()}.txt", contents);
    }

    public static void DeleteFile<TFixture>(this TFixture fixture, string relativePath)
        where TFixture : FileHashTestFixture
    {
        var fullName = fixture.FileSystem.Path.Combine(fixture.WorkingDirectory.FullName, relativePath);

        fixture.FileSystem.File.Delete(fullName);
    }

    public static IFileInfo WithNewFile<TFixture>(this TFixture fixture, string relativePath, string? contents = null)
        where TFixture : FileHashTestFixture
    {
        var fullName = fixture.FileSystem.Path.Combine(fixture.WorkingDirectory.FullName, relativePath);

        if (!fixture.FileSystem.Path.HasExtension(fullName))
        {
            throw new ArgumentException($"Expected relative path to a file. Found: '{relativePath}'", nameof(relativePath));
        }

        var directoryName = fixture.FileSystem.Path.GetDirectoryName(fullName)!;

        var fileContent = contents ?? Guid.NewGuid().ToString();

        if (!fixture.FileSystem.Directory.Exists(directoryName))
        {
            fixture.FileSystem.Directory.CreateDirectory(directoryName);
        }

        return fixture.AddFile(fullName, fileContent);
    }

    public static BasicFileInfo WithNewBasicFile<TFixture>(this TFixture fixture, int? fileIdentifier = null, string? contents = null)
        where TFixture : FileHashTestFixture
    {
        return HashingUtil.CreateBasicFileInfo(fixture.WithNewFile(fileIdentifier, contents));
    }

    public static IFileInfo UpdateFile<TFixture>(this TFixture fixture, IFileInfo file)
        where TFixture : FileHashTestFixture
    {
        return fixture.UpdateFile(file.FullName);
    }

    public static BasicFileInfo UpdateFile<TFixture>(this TFixture fixture, BasicFileInfo file)
        where TFixture : FileHashTestFixture
    {
        return HashingUtil.CreateBasicFileInfo(fixture.UpdateFile(file.FullName));
    }

    public static IFileInfo UpdateFile<TFixture>(this TFixture fixture, string fullName)
        where TFixture : FileHashTestFixture
    {
        fixture.FileSystem.File.AppendAllText(fullName, "-Updated");

        return fixture.FileSystem.FileInfo.New(fullName);
    }

    private static IFileInfo AddFile<TFixture>(this TFixture fixture, string fullName, string? fileContents = null)
        where TFixture : FileHashTestFixture
    {
        fileContents ??= Guid.NewGuid().ToString();

        fixture.FileSystem.File.WriteAllText(fullName, fileContents);
        var createdFile = fixture.FileSystem.FileInfo.New(fullName);

        return createdFile;
    }
}