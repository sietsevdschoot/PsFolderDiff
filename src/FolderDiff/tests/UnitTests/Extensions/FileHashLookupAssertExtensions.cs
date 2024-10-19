using FluentAssertions;
using PsFolderDiff.FileHashLookupLib.Domain;
using PsFolderDiff.FileHashLookupLib.Services;

namespace PsFolderDiff.FileHashLookupLib.UnitTests.Extensions;

public static class FileHashLookupAssertExtensions
{
    public static void AssertContainsFileNames(this FileHashLookup fileHashLookup, params int[] expected)
    {
        var files = fileHashLookup.GetFiles();

        var actual = files
            .Select(x => Convert.ToInt32(Path.GetFileNameWithoutExtension(x.FullName)))
            .OrderBy(x => x)
            .ToList();

        actual.Should().BeEquivalentTo(expected);
    }

    public static void AssertContainsIncludePath(this FileHashLookup fileHashLookup, FilePattern includeFolderPattern)
    {
        fileHashLookup.IncludePatterns.Should().Contain(includeFolderPattern.Value);
    }

    public static void AssertIncludePatternsAreEmpty(this FileHashLookup fileHashLookup)
    {
        fileHashLookup.IncludePatterns.Should().BeEmpty();
    }

    public static void AssertExcludePatternsAreEmpty(this FileHashLookup fileHashLookup)
    {
        fileHashLookup.ExcludePatterns.Should().BeEmpty();
    }
}