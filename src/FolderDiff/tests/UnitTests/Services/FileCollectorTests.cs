using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using PsFolderDiff.FileHashLookupLib.Services;
using PsFolderDiff.FileHashLookupLib.UnitTests.Extensions;
using Xunit;

namespace PsFolderDiff.FileHashLookupLib.UnitTests.Services;

public class FileCollectorTests
{
    [Fact]
    public void AddIncludePattern_Adding_NonExisting_Folder_Throws()
    {
        // Arrange
        var fixture = new FileCollectorTestFixture();

        // Act
        var act = () => fixture.AddIncludePattern("NonExistingFolder");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddIncludeFolder_Adds_Folder_And_Collects_Files_Recursively()
    {
        // Arrange
        var fixture = new FileCollectorTestFixture();
        fixture.WithNewFile(@"Folder1\1.txt");
        fixture.WithNewFile(@"Folder1\2.txt");
        fixture.WithNewFile(@"Folder1\Sub1\3.txt");
        fixture.WithNewFile(@"Folder1\Sub1\Sub2\4.txt");
        fixture.WithNewFile(@"Folder2\5.txt");

        // Act
        fixture.AddIncludePattern(@"Folder1\");

        // Assert
        fixture.AssertContainsFileNames([1, 2, 3, 4]);
    }

    [Fact]
    public void AddIncludePattern_Returns_Collected_Files_For_This_Include_Pattern()
    {
        // Arrange
        var fixture = new FileCollectorTestFixture();
        fixture.WithNewFile(@"Folder1\1.txt");
        fixture.WithNewFile(@"Folder1\2.txt");
        fixture.WithNewFile(@"Folder2\3.txt");
        fixture.WithNewFile(@"Folder2\4.txt");

        // Act
        fixture.AddIncludePattern(@"Folder1\");
        var actual = fixture.AddIncludePattern(@"Folder2\");

        // Assert
        fixture.AssertContainsFileNames(actual, [3, 4]);
    }

    [Fact]
    public void AddIncludePattern_Can_use_file_glob()
    {
        // Arrange
        var fixture = new FileCollectorTestFixture();
        fixture.WithNewFile(@"Folder1\1.txt");
        fixture.WithNewFile(@"Folder1\2.exe");
        fixture.WithNewFile(@"Folder1\Sub1\3.exe");
        fixture.WithNewFile(@"Folder1\Sub1\Sub2\4.txt");

        // Act
        fixture.AddIncludePattern(fixture.WorkingDirectory, @"Folder1\**\*.txt");

        // Assert
        fixture.AssertContainsFileNames([1, 4]);
    }

    [Fact]
    public void AddIncludePattern_Can_Include_SubFolder()
    {
        // Arrange
        var fixture = new FileCollectorTestFixture();
        fixture.WithNewFile(@"Folder1\1.txt");
        fixture.WithNewFile(@"Folder1\2.exe");
        fixture.WithNewFile(@"Folder1\Sub1\3.exe");
        fixture.WithNewFile(@"Folder1\Sub1\Sub2\4.txt");

        // Act
        fixture.AddIncludePattern(fixture.WorkingDirectory, @"**\Sub1\**\*");

        // Assert
        fixture.AssertContainsFileNames([3, 4]);
    }

    [Fact]
    public void AddIncludePattern_Can_Include_RelativeFolder()
    {
        // Arrange
        var fixture = new FileCollectorTestFixture()
            .WithFileSystem(new MockFileSystem(
                new Dictionary<string, MockFileData>
                {
                    { @"c:\Temp\Folder1\1.txt", new MockFileData(string.Empty) },
                    { @"c:\Temp\Folder2\2.txt", new MockFileData(string.Empty) },
                    { @"c:\Temp\Folder2\3.txt", new MockFileData(string.Empty) },
                },
                currentDirectory: @"c:\Temp\Folder1\"));

        // Act
        fixture.Sut.IncludePattern(@"..\Folder2\");

        // Assert
        fixture.AssertContainsFileNames([2, 3]);
        fixture.Sut.IncludePatterns.Select(x => x.Value).Should().Contain(@"c:\Temp\Folder2\**\*");
    }

    [Fact]
    public void AddExcludePattern_ExcludesFolderInCollectedFileResults()
    {
        // Arrange
        var fixture = new FileCollectorTestFixture();
        fixture.WithNewFile(@"Folder1\1.txt");
        fixture.WithNewFile(@"Folder1\2.txt");
        fixture.WithNewFile(@"Folder1\Sub1\3.txt");
        fixture.WithNewFile(@"Folder1\Sub1\Sub2\4.txt");
        fixture.WithNewFile(@"Folder1\5.txt");
        fixture.WithNewFile(@"Folder2\6.txt");

        // Act
        fixture.AddIncludePattern(@"\Folder1\");
        fixture.ExcludePattern(@"**\Sub1\**\*");

        // Assert
        fixture.AssertContainsFileNames([1, 2, 5]);
    }

    [Fact]
    public void AddExcludePattern_CanExcludesPatternInCollectedFilesResults()
    {
        // Arrange
        var fixture = new FileCollectorTestFixture();
        fixture.WithNewFile(@"Folder1\1.txt");
        fixture.WithNewFile(@"Folder1\2.doc");
        fixture.WithNewFile(@"Folder1\Sub1\3.doc");
        fixture.WithNewFile(@"Folder1\Sub1\Sub2\4.doc");
        fixture.WithNewFile(@"Folder1\5.txt");

        // Act
        fixture.AddIncludePattern(@"Folder1\");
        fixture.ExcludePattern(@"**\*.doc");

        // Assert
        fixture.AssertContainsFileNames([1, 5]);
    }

    [Fact]
    public void AddExcludePattern_Can_Exclude_RelativeFolder()
    {
        // Arrange
        var fixture = new FileCollectorTestFixture()
            .WithFileSystem(new MockFileSystem(
                new Dictionary<string, MockFileData>
                {
                    { @"c:\Temp\Folder1\1.txt", new MockFileData(string.Empty) },
                    { @"c:\Temp\Folder2\2.txt", new MockFileData(string.Empty) },
                    { @"c:\Temp\Folder2\3.txt", new MockFileData(string.Empty) },
                },
                currentDirectory: @"c:\Temp\Folder2\"));

        // Act
        fixture.Sut.IncludePattern(@"c:\Temp\Folder1\");
        fixture.Sut.IncludePattern(@"c:\Temp\Folder2\");
        fixture.Sut.ExcludePattern(@"..\Folder1\");

        // Assert
        fixture.AssertContainsFileNames([2, 3]);
        fixture.Sut.ExcludePatterns.Select(x => x.Value).Should().Contain(@"c:\Temp\Folder1\**\*");
    }

    [Fact]
    public void ExcludePattern_Can_exclude_pattern_over_multiple_drives()
    {
        // Arrange
        var fixture = new FileCollectorTestFixture()
            .WithFileSystem(new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\Temp\Folder1\1.txt", new MockFileData(string.Empty) },
                { @"c:\Temp\Folder1\2.doc", new MockFileData(string.Empty) },
                { @"d:\Temp\Folder2\3.doc", new MockFileData(string.Empty) },
                { @"d:\Temp\Folder2\4.txt", new MockFileData(string.Empty) },
            }));

        // Act
        fixture.Sut.IncludePattern(@"c:\Temp\Folder1");
        fixture.Sut.IncludePattern(@"d:\Temp\Folder2");
        fixture.Sut.ExcludePattern(@"*.doc");

        // Assert
        fixture.AssertContainsFileNames([1, 4]);
    }

    [Fact]
    public void GetFiles_Returns_All_Collected_Files()
    {
        // Arrange
        var fixture = new FileCollectorTestFixture();
        fixture.WithNewFile(@"Folder1\1.txt");
        fixture.WithNewFile(@"Folder1\Sub1\2.txt");
        fixture.WithNewFile(@"Folder2\Sub1\3.txt");
        fixture.WithNewFile(@"Folder2\4.txt");

        fixture.AddIncludePattern(@"Folder1\");
        fixture.AddIncludePattern(@"Folder2\");
        fixture.ExcludePattern(@"**\Sub1\**\*");

        // Act
        var actual = fixture.GetFiles();

        // Assert
        fixture.AssertContainsFileNames(actual, [1, 4]);
    }

    [Fact]
    public void IncludePattern_Can_collect_files_from_different_drives()
    {
        // Arrange
        var fixture = new FileCollectorTestFixture()
            .WithFileSystem(new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\Temp\Folder1\1.txt", new MockFileData(string.Empty) },
                { @"c:\Temp\Folder1\2.txt", new MockFileData(string.Empty) },
                { @"d:\Temp\Folder2\3.txt", new MockFileData(string.Empty) },
                { @"d:\Temp\Folder2\4.txt", new MockFileData(string.Empty) },
            }));

        // Act
        fixture.Sut.IncludePattern(@"c:\Temp\Folder1");
        fixture.Sut.IncludePattern(@"d:\Temp\Folder2");

        // Assert
        fixture.AssertContainsFileNames([1, 2, 3, 4]);
    }

    private class FileCollectorTestFixture : FileHashTestFixture
    {
        private readonly Lazy<FileCollector> _sut;

        public FileCollectorTestFixture()
        {
            _sut = new Lazy<FileCollector>(() => ServiceProvider.GetRequiredService<FileCollector>());
        }

        public FileCollector Sut => _sut.Value;

        public new string WorkingDirectory => base.WorkingDirectory.FullName;

        public List<IFileInfo> AddIncludePattern(string path)
        {
            return AddIncludePattern(WorkingDirectory, path);
        }

        public List<IFileInfo> AddIncludePattern(string workingDirectory, string includePattern)
        {
            var path = FileSystem.Path.Combine(workingDirectory, includePattern);

            return Sut.IncludePattern(path);
        }

        public void ExcludePattern(string excludePattern)
        {
            Sut.ExcludePattern(excludePattern);
        }

        public List<IFileInfo> GetFiles()
        {
            return Sut.GetFiles();
        }

        public void AssertContainsFileNames(List<IFileInfo> files, int[] expected)
        {
            var actual = files
                .Select(x => Convert.ToInt32(Path.GetFileNameWithoutExtension(x.FullName)))
                .OrderBy(x => x)
                .ToList();

            actual.Should().BeEquivalentTo(expected);
        }

        public void AssertContainsFileNames(params int[] expected)
        {
            AssertContainsFileNames(Sut.GetFiles(), expected);
        }
    }
}