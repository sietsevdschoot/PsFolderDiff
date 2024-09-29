using FluentAssertions;
using PsFolderDiff.FileHashLookup.Services;
using Xunit;

namespace PsFolderDiff.FileHashLookup.UnitTests.Services;

public class FileCollectorTests
{
    [Fact]
    public void AddIncludeFolder_Adding_NonExisting_Folder_Throws()
    {
        // Arrange 
        var fixture = new FileCollectorTestFixture();

        // Act
        var act = () => fixture.AddIncludeFolder("NonExistingFolder");

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
        fixture.WithNewFile(@"Folder1\Sub\3.txt");
        fixture.WithNewFile(@"Folder1\Sub\Sub\4.txt");
        fixture.WithNewFile(@"Folder2\5.txt");

        // Act
        fixture.AddIncludeFolder(@"Folder1\");

        // Assert
        fixture.AssertContainsFileNames([ 1, 2, 3, 4]);
    }

    [Fact] 
    public void AddIncludePattern_Can_use_file_glob()
    {
        // Arrange 
        var fixture = new FileCollectorTestFixture();
        fixture.WithNewFile(@"Folder1\1.txt");
        fixture.WithNewFile(@"Folder1\2.exe");
        fixture.WithNewFile(@"Folder1\Sub\3.exe");
        fixture.WithNewFile(@"Folder1\Sub\Sub\4.txt");

        // Act
        fixture.AddIncludePattern(fixture.WorkingDirectory, @"Folder1\**\*.txt");

        // Assert
        fixture.AssertContainsFileNames([1, 4]);
    }

    [Fact]
    public void AddExcludePattern_ExcludesFolderInCollectedFileResults()
    {
        // Arrange 
        var fixture = new FileCollectorTestFixture();
        fixture.WithNewFile(@"Folder1\1.txt");
        fixture.WithNewFile(@"Folder1\2.txt");
        fixture.WithNewFile(@"Folder1\Sub\3.txt");
        fixture.WithNewFile(@"Folder1\Sub\Sub\4.txt");
        fixture.WithNewFile(@"Folder1\5.txt");
        fixture.WithNewFile(@"Folder2\6.txt");

        // Act
        fixture.AddIncludeFolder(@"\Folder1\");
        fixture.AddExcludePattern(@"\**\Sub\**\*");

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
        fixture.WithNewFile(@"Folder1\Sub\3.doc");
        fixture.WithNewFile(@"Folder1\Sub\Sub\4.doc");
        fixture.WithNewFile(@"Folder1\5.txt");

        // Act
        fixture.AddIncludeFolder(@"Folder1\");
        fixture.AddExcludePattern(@"\**\*.doc");

        // Assert
        fixture.AssertContainsFileNames([1, 5]);
    }

    private class FileCollectorTestFixture : FileHashTestFixture
    {
        private readonly FileCollector _sut;

        public FileCollectorTestFixture()
        {
            _sut = new FileCollector(base.FileSystem);
        }

        public new string WorkingDirectory => base.WorkingDirectory.FullName;

        public FileCollectorTestFixture AddIncludeFolder(string path)
        {
            var fullName = base.FileSystem.Path.Combine(WorkingDirectory, path);

            _sut.AddIncludeFolder(fullName);

            return this;
        }

        public FileCollectorTestFixture AddIncludePattern(string workingDirectory, string includePattern)
        {
            var path = base.FileSystem.Path.Combine(workingDirectory, includePattern);

            _sut.AddIncludePattern(path);

            return this;
        }

        public FileCollectorTestFixture AddExcludePattern(string pattern)
        {
            _sut.AddExcludePattern(pattern);

            return this;
        }

        public void AssertContainsFileNames(params int[] expected)
        {
            var files = _sut.GetFiles();

            var actual = files
                .Select(x => Convert.ToInt32(Path.GetFileNameWithoutExtension(x.FullName)))
                .OrderBy(x => x)
                .ToList();

            actual.Should().BeEquivalentTo(expected);
        }
    }
}