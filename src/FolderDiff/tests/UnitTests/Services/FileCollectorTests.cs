﻿using System.IO.Abstractions;
using FluentAssertions;
using PsFolderDiff.FileHashLookup.Services;
using PsFolderDiff.FileHashLookup.Services.Interfaces;
using Xunit;

namespace PsFolderDiff.FileHashLookup.UnitTests.Services;

public class FileCollectorTests
{
    [Fact]
    public void AddIncludeFolder_Adding_NonExisting_Folder_Throws()
    {
        // Arrange
        var fixture = new IFileCollectorTestFixture();

        // Act
        var act = () => fixture.AddIncludeFolder("NonExistingFolder");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddIncludeFolder_Adds_Folder_And_Collects_Files_Recursively()
    {
        // Arrange
        var fixture = new IFileCollectorTestFixture();
        fixture.WithNewFile(@"Folder1\1.txt");
        fixture.WithNewFile(@"Folder1\2.txt");
        fixture.WithNewFile(@"Folder1\Sub1\3.txt");
        fixture.WithNewFile(@"Folder1\Sub1\Sub2\4.txt");
        fixture.WithNewFile(@"Folder2\5.txt");

        // Act
        fixture.AddIncludeFolder(@"Folder1\");

        // Assert
        fixture.AssertContainsFileNames([1, 2, 3, 4]);
    }

    [Fact]
    public void AddIncludeFolder_Returns_Collected_Files_For_This_Include_Pattern()
    {
        // Arrange
        var fixture = new IFileCollectorTestFixture();
        fixture.WithNewFile(@"Folder1\1.txt");
        fixture.WithNewFile(@"Folder1\2.txt");
        fixture.WithNewFile(@"Folder2\3.txt");
        fixture.WithNewFile(@"Folder2\4.txt");

        // Act
        fixture.AddIncludeFolder(@"Folder1\");
        var actual = fixture.AddIncludeFolder(@"Folder2\");

        // Assert
        fixture.AssertContainsFileNames(actual, [3, 4]);
    }

    [Fact]
    public void AddIncludePattern_Can_use_file_glob()
    {
        // Arrange
        var fixture = new IFileCollectorTestFixture();
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
        var fixture = new IFileCollectorTestFixture();
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
    public void AddExcludePattern_ExcludesFolderInCollectedFileResults()
    {
        // Arrange
        var fixture = new IFileCollectorTestFixture();
        fixture.WithNewFile(@"Folder1\1.txt");
        fixture.WithNewFile(@"Folder1\2.txt");
        fixture.WithNewFile(@"Folder1\Sub1\3.txt");
        fixture.WithNewFile(@"Folder1\Sub1\Sub2\4.txt");
        fixture.WithNewFile(@"Folder1\5.txt");
        fixture.WithNewFile(@"Folder2\6.txt");

        // Act
        fixture.AddIncludeFolder(@"\Folder1\");
        fixture.AddExcludePattern(@"\**\Sub1\**\*");

        // Assert
        fixture.AssertContainsFileNames([1, 2, 5]);
    }

    [Fact]
    public void AddExcludePattern_CanExcludesPatternInCollectedFilesResults()
    {
        // Arrange
        var fixture = new IFileCollectorTestFixture();
        fixture.WithNewFile(@"Folder1\1.txt");
        fixture.WithNewFile(@"Folder1\2.doc");
        fixture.WithNewFile(@"Folder1\Sub1\3.doc");
        fixture.WithNewFile(@"Folder1\Sub1\Sub2\4.doc");
        fixture.WithNewFile(@"Folder1\5.txt");

        // Act
        fixture.AddIncludeFolder(@"Folder1\");
        fixture.AddExcludePattern(@"\**\*.doc");

        // Assert
        fixture.AssertContainsFileNames([1, 5]);
    }

    [Fact]
    public void GetFiles_Returns_All_Collected_Files()
    {
        // Arrange
        var fixture = new IFileCollectorTestFixture();
        fixture.WithNewFile(@"Folder1\1.txt");
        fixture.WithNewFile(@"Folder1\Sub1\2.txt");
        fixture.WithNewFile(@"Folder2\Sub1\3.txt");
        fixture.WithNewFile(@"Folder2\4.txt");

        fixture.AddIncludeFolder(@"Folder1\");
        fixture.AddIncludeFolder(@"Folder2\");
        fixture.AddExcludePattern(@"**\Sub1\**\*");

        // Act
        var actual = fixture.GetFiles();

        // Assert
        fixture.AssertContainsFileNames(actual, [1, 4]);
    }

    private class IFileCollectorTestFixture : FileHashTestFixture
    {
        private readonly IFileCollector _sut;

        public IFileCollectorTestFixture()
        {
            _sut = new FileCollector(FileSystem);
        }

        public new string WorkingDirectory => base.WorkingDirectory.FullName;

        public IFileCollector Sut => _sut;

        public List<IFileInfo> AddIncludeFolder(string path)
        {
            var fullName = FileSystem.Path.Combine(WorkingDirectory, path);

            return _sut.AddIncludeFolder(fullName);
        }

        public List<IFileInfo> AddIncludePattern(string workingDirectory, string includePattern)
        {
            var path = FileSystem.Path.Combine(workingDirectory, includePattern);

            return _sut.AddIncludePattern(path);
        }

        public IFileCollectorTestFixture AddExcludePattern(string pattern)
        {
            _sut.AddExcludePattern(pattern);

            return this;
        }

        public List<IFileInfo> GetFiles()
        {
            return _sut.GetFiles();
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
            AssertContainsFileNames(_sut.GetFiles(), expected);
        }
    }
}