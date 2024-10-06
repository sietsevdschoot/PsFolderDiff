using System.IO.Abstractions;

namespace PsFolderDiff.FileHashLookup.UnitTests.Extensions;

public static class FileHashTestFixtureExtensions
{
    public static IFileInfo GetFileInfo(this FileHashTestFixture fixture, string name)
    {
        var file = fixture.AllFiles.Select(fixture.AsFileInfo).SingleOrDefault(x =>
            string.Equals(x.Name, name, StringComparison.InvariantCultureIgnoreCase));

        ArgumentNullException.ThrowIfNull(file, nameof(name));

        return file;
    }
}