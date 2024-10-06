using FluentAssertions;
using PsFolderDiff.FileHashLookup.Services;

namespace PsFolderDiff.FileHashLookup.UnitTests.Extensions;

public static class FileHashLookupAssertExtensions
{
    public static void AssertContainsFileNames(this FileHashLookup.Services.FileHashLookup fileHashLookup, params int[] expected)
    {
        var files = fileHashLookup.GetFiles();

        var actual = files
            .Select(x => Convert.ToInt32(Path.GetFileNameWithoutExtension(x.FullName)))
            .OrderBy(x => x)
            .ToList();

        actual.Should().BeEquivalentTo(expected);
    }

    public static void AssertContainsIncludePath(this FileHashLookup.Services.FileHashLookup fileHashLookup, string includeFolder)
    {
        var expected = $@"{includeFolder.Trim(Path.DirectorySeparatorChar)}\**\";

        var parsedPattern = FileCollector.ParseFileGlobbingPattern(expected);

        fileHashLookup.IncludePatterns.Should().Contain(parsedPattern);
    }
}