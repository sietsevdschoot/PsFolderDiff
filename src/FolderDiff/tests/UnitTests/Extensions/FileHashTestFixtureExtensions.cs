using System.IO.Abstractions;
using PsFolderDiff.FileHashLookup.Domain;

namespace PsFolderDiff.FileHashLookup.UnitTests.Extensions;

public static class FileHashTestFixtureExtensions
{
    public static IFileInfo GetFileInfo(this FileHashTestFixture fixture, string filename)
    {
        var file = fixture.AllFiles.SingleOrDefault(x =>
            string.Equals(fixture.FileSystem.Path.GetFileName(x.FullName), filename, StringComparison.InvariantCultureIgnoreCase));

        if (file == null)
        {
            throw new FileNotFoundException($"Not found", filename);
        }

        return fixture.AsFileInfo(file);
    }

    public static IFileInfo GetFileInfo(this FileHashTestFixture fixture, int identifier)
    {
        var file = fixture.AllFiles.Select(fixture.AsFileInfo).SingleOrDefault(x =>
            string.Equals(fixture.FileSystem.Path.GetFileNameWithoutExtension(x.Name), identifier.ToString(), StringComparison.InvariantCultureIgnoreCase));

        if (file == null)
        {
            throw new FileNotFoundException($"Not found", $"{identifier}.*");
        }

        return file;
    }

    public static BasicFileInfo GetBasicFileInfo(this FileHashTestFixture fixture, string name)
    {
        var file = fixture.AllFiles.SingleOrDefault(x =>
            string.Equals(fixture.FileSystem.Path.GetFileName(x.FullName), name, StringComparison.InvariantCultureIgnoreCase));

        if (file == null)
        {
            throw new FileNotFoundException($"Not found", name);
        }

        return file;
    }

    public static BasicFileInfo GetBasicFileInfo(this FileHashTestFixture fixture, int identifier)
    {
        var file = fixture.AllFiles.SingleOrDefault(x =>
            string.Equals(fixture.FileSystem.Path.GetFileNameWithoutExtension(x.FullName), identifier.ToString(), StringComparison.InvariantCultureIgnoreCase));

        if (file == null)
        {
            throw new FileNotFoundException($"Not found", $"{identifier}.*");
        }

        return file;
    }
}