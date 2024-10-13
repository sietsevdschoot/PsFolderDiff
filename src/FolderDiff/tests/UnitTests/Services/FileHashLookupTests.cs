﻿using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using PsFolderDiff.FileHashLookupLib.Services;
using PsFolderDiff.FileHashLookupLib.Services.Interfaces;
using PsFolderDiff.FileHashLookupLib.UnitTests.Extensions;
using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace PsFolderDiff.FileHashLookupLib.UnitTests.Services;

public class FileHashLookupTests
{
    [Fact]
    public async Task AddFolder_Adds_Folder_And_Collects_Files_Recursively()
    {
        // Arrange
        var fixture = new FileHashLookupTestFixture();
        fixture.WithNewFile(@"Folder1\1.txt");
        fixture.WithNewFile(@"Folder1\2.txt");
        fixture.WithNewFile(@"Folder1\Sub1\3.txt");
        fixture.WithNewFile(@"Folder1\Sub1\Sub2\4.txt");
        fixture.WithNewFile(@"Folder2\5.txt");

        // Act
        var includeFolder = @"Folder1\";
        await fixture.Sut.AddFolder(includeFolder);

        // Assert
        fixture.AssertContainsFileNames([1, 2, 3, 4]);
        fixture.AssertContainsIncludePath(includeFolder);
    }

    [Fact]
    public async Task AddIncludePattern_Adds_Folder_And_Collects_Files_Recursively()
    {
        // Arrange
        var fixture = new FileHashLookupTestFixture();
        fixture.WithNewFile(@"Folder1\1.txt");
        fixture.WithNewFile(@"Folder1\2.txt");
        fixture.WithNewFile(@"Folder1\Sub1\3.txt");
        fixture.WithNewFile(@"Folder1\Sub1\Sub2\4.txt");
        fixture.WithNewFile(@"Folder2\5.txt");

        // Act
        var includePattern = @"Folder1\**\Sub1\**\*";
        await fixture.Sut.AddIncludePattern(includePattern);

        // Assert
        fixture.AssertContainsFileNames([3, 4]);
        fixture.AssertContainsIncludePattern(includePattern);
    }

    [Fact]
    public async Task AddExcludePattern_ExcludesPatternFromAlreadyCollectedFiles()
    {
        // Arrange
        var fixture = new FileHashLookupTestFixture();
        fixture.WithNewFile(@"Folder1\1.txt");
        fixture.WithNewFile(@"Folder1\2.txt");
        fixture.WithNewFile(@"Folder1\Sub1\3.txt");
        fixture.WithNewFile(@"Folder1\Sub1\Sub2\4.txt");
        fixture.WithNewFile(@"Folder2\5.txt");

        var excludePattern = @"\**\Sub1\**\*";

        // Act
        await fixture.Sut.AddFolder(@"Folder1\");
        await fixture.Sut.AddExcludePattern(excludePattern);

        // Assert
        fixture.AssertContainsFileNames([1, 2]);
        fixture.AssertContainsExcludesPattern(excludePattern);
    }

    [Fact]
    public async Task AddExcludePattern_Can_exclude_files_from_different_drives()
    {
        // Arrange
        var fixture = new FileHashLookupTestFixture();

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { @"c:\Temp\Folder1\1.txt", new MockFileData(Guid.NewGuid().ToString()) },
            { @"c:\Temp\Folder1\2.txt", new MockFileData(Guid.NewGuid().ToString()) },
            { @"d:\Temp\Folder2\3.txt", new MockFileData(Guid.NewGuid().ToString()) },
            { @"d:\Temp\Folder2\4.txt", new MockFileData(Guid.NewGuid().ToString()) },
        });

        // Continue here.
        fixture.CreateFileHashLookupWithProviderMockFileSystem()

        var fileCollector = new FileCollector(fileSystem);

        // Act
        fileCollector.AddIncludeFolder(@"c:\Temp\Folder1");
        fileCollector.AddIncludeFolder(@"d:\Temp\Folder2");

        // Assert
        var actualFileNames = fileCollector.GetFiles().Select(x => Convert.ToInt32(fileSystem.Path.GetFileNameWithoutExtension(x.FullName)));

        actualFileNames.Should().BeEquivalentTo([1, 2, 3, 4]);
    }


    [Fact]
    public async Task AddFiles_AddsFilesAndCalculatesHash()
    {
        // Arrange
        var fixture = new FileHashLookupTestFixture();
        fixture.WithNewFile(@"Folder1\1.txt");
        fixture.WithNewFile(@"Folder1\2.txt");
        fixture.WithNewFile(@"Folder1\Sub1\3.txt");
        fixture.WithNewFile(@"Folder1\Sub1\Sub2\4.txt");
        fixture.WithNewFile(@"Folder2\5.txt");

        var files = fixture.AllFiles
            .Select(fixture.AsFileInfo)
            .OrderBy(x => x.Name)
            .ToArray();

        // Act
        await fixture.Sut.AddFiles(files);

        // Assert
        fixture.AssertContainsFileNames([1, 2, 3, 4, 5]);
        fixture.AssertIncludePatternsAreEmpty();
    }

    [Fact]
    public async Task AddFile_AddsFileAndCalculatesHash()
    {
        // Arrange
        var fixture = new FileHashLookupTestFixture();
        fixture.WithNewFile(@"Folder1\1.txt");

        var file = fixture.AllFiles
            .Select(fixture.AsFileInfo)
            .OrderBy(x => x.Name)
            .Single();

        // Act
        await fixture.Sut.AddFile(file);

        // Assert
        fixture.AssertContainsFileNames([1]);
        fixture.AssertIncludePatternsAreEmpty();
    }

    [Fact]
    public async Task AddFileHashLookup_CopiesFileAndPathPatternsFromOther()
    {
        // Arrange
        var fixture = new FileHashLookupTestFixture();
        fixture.WithNewFile(@"Folder1\1.txt");
        fixture.WithNewFile(@"Folder1\2.txt");
        fixture.WithNewFile(@"Folder2\3.txt");
        fixture.WithNewFile(@"Folder2\4.txt");
        fixture.WithNewFile(@"Folder3\5.txt");

        var fileHashLookup1 = fixture.CreateFileHashLookup();
        await fileHashLookup1.AddFolder("Folder1");

        var fileHashLookup2 = fixture.CreateFileHashLookup();
        await fileHashLookup2.AddFolder("Folder2");

        // Act
        var fileHashLookup = fixture.CreateFileHashLookup();
        await fileHashLookup.AddFileHashLookup(fileHashLookup1);
        await fileHashLookup.AddFileHashLookup(fileHashLookup2);

        // Assert
        fileHashLookup.AssertContainsFileNames([1, 2, 3, 4]);
        fileHashLookup.IncludePatterns.Select(x => x.Directory).Should().BeEquivalentTo(new[]
        {
            "Folder1\\",
            "Folder2\\",
        });
    }

    [Fact]
    public void Will_look_at_the_file_hash_to_determine_file_equality_or_difference()
    {
        // Arrange
        var fixture = new FileHashLookupTestFixture();

        fixture.WithNewFile("Folder1\\File1.txt");

        // Act

        // Assert

        ////    1..3 | ForEach - Object { New - Item - ItemType File Testdrive:\MyFolder\$_.txt - Value "My Test Value" - Force  }
        ////    $myHash = GetFileHashTable "$TestDrive\MyFolder"
        ////    4..5 | ForEach - Object { New - Item - ItemType File Testdrive:\MyFolder2\$_.txt - Value "My Test Value" - Force  }
        ////    $newHash = GetFileHashTable "$TestDrive\MyFolder2"
        ////    $actual = $myHash.GetMatchesInOther($newHash)
        ////    $actual.GetFiles() | ForEach - Object { [int]($_.Name - replace $_.Extension) } | Should - Be @(4, 5)
    }

    [Fact]
    public async Task GetMatchesInOther_Returns_file_only_found_in_other()
    {
        // Arrange
        var fixture = new FileHashLookupTestFixture();
        fixture.WithNewFile(@"Folder1\1.txt");
        fixture.WithNewFile(@"Folder1\2.txt");

        var fileHashLookup1 = fixture.CreateFileHashLookup();
        await fileHashLookup1.AddFolder("Folder1");

        // Update
        fixture.WithNewFile(@"Folder1\3.txt");
        fixture.WithNewFile(@"Folder1\4.txt");
        var fileHashLookup2 = fixture.CreateFileHashLookup();
        await fileHashLookup2.AddFolder("Folder1");

        // Act
        var diffInOther = await fileHashLookup1.GetMatchesInOther(fileHashLookup2);

        // Assert
        diffInOther.AssertContainsFileNames([1, 2]);
        diffInOther.AssertIncludePatternsAreEmpty();
        diffInOther.AssertExcludePatternsAreEmpty();
    }

    [Fact]
    public async Task GetDifferencesInOther_Returns_file_only_found_in_other()
    {
        // Arrange
        var fixture = new FileHashLookupTestFixture();
        fixture.WithNewFile(@"Folder1\1.txt");
        fixture.WithNewFile(@"Folder1\2.txt");

        var fileHashLookup1 = fixture.CreateFileHashLookup();
        await fileHashLookup1.AddFolder("Folder1");

        // Update
        fixture.WithNewFile(@"Folder1\3.txt");
        fixture.WithNewFile(@"Folder1\4.txt");
        var fileHashLookup2 = fixture.CreateFileHashLookup();
        await fileHashLookup2.AddFolder("Folder1");

        // Act
        var diffInOther = await fileHashLookup1.GetDifferencesInOther(fileHashLookup2);

        // Assert
        diffInOther.AssertContainsFileNames([3, 4]);
        diffInOther.AssertIncludePatternsAreEmpty();
        diffInOther.AssertExcludePatternsAreEmpty();
    }

    [Fact]
    public void GetDifferencesInOther_Doesnt_copy_include_and_exclude_paths()
    {
        // Arrange

        // Act

        // Assert
    }

    [Fact]
    public void Creates_a_file_containing_the_HashTable()
    {
        // Arrange

        // Act

        // Assert
    }

    [Fact]

    public void Refresh_Removes_folders_and_files_from_paths_which_do_no_longer_exists()
    {
        // Arrange

        // Act

        // Assert
    }

    [Fact]
    public void Refresh_updated_the_LastUpdated_date()
    {
        // Arrange

        // Act

        // Assert
    }

    [Fact]
    public void Refresh_updates_the_hash_of_changed_files()
    {
        // Arrange

        // Act

        // Assert
    }

    [Fact]
    public void Returns_unique_items_from_other_object_it_is_compared_to()
    {
        // Arrange

        // Act

        // Assert
    }

    [Fact]
    public void Save_without_filename_will_pick_tempfile_name()
    {
        // Arrange

        // Act

        // Assert
    }

    [Fact]
    public void Sets_the_LastUpdated_time_when_FileHashTable_on_instantiation()
    {
        // Arrange

        // Act

        // Assert
    }

    [Fact]
    public void Uses_the_folder_path_to_generate_the_filename()
    {
        // Arrange

        // Act

        // Assert
    }

    [Fact]
    public void When_Save_is_called_without_filename_uses_last_known_filename()
    {
        // Arrange

        // Act

        // Assert
    }

    [Fact]
    public void Can_load_new_instance_from_filename()
    {
        // Arrange

        // Act

        // Assert
    }

    [Fact]
    public void Can_refresh_itself_By_adding_new_files_and_removing_no_longer_existing_files()
    {
        // Arrange

        // Act

        // Assert
    }

    [Fact]
    public void Returns_matching_items_from_other_object_it_is_compared_to()
    {
        // Arrange

        // Act

        // Assert
    }

    [Fact]
    public void After_load_can_use_all_properties()
    {
        // Arrange

        // Act

        // Assert
    }

    [Fact]
    public void Can_add_relative_folders()
    {
        // Arrange

        // Act

        // Assert
    }

    private class FileHashLookupTestFixture : FileHashTestFixture
    {
        private readonly IHasReadOnlyFilePatterns _fileCollector;

        public FileHashLookupTestFixture()
        {
            var provider = this.CreateFileHashLookupWithProvider(settings =>
            {
                settings.ReportPollingDelay = TimeSpan.Zero;
                settings.ConfigureServices = (services, _) =>
                {
                    services.AddSingleton(FileSystem);
                };
            });

            Sut = provider.FileHashLookup;
            _fileCollector = provider.ServiceProvider.GetRequiredService<IHasReadOnlyFilePatterns>();
        }

        public FileHashLookup Sut { get; }

        public void AssertContainsFileNames(params int[] expected)
        {
            Sut.AssertContainsFileNames(expected);
        }

        public void AssertContainsExcludesPattern(string excludePattern)
        {
            _fileCollector.ExcludePatterns.Contains(excludePattern).Should().BeTrue();
        }

        public void AssertContainsIncludePath(string includeFolder)
        {
            Sut.AssertContainsIncludePath(includeFolder);
        }

        public void AssertContainsIncludePattern(string includePattern)
        {
            var parsedPattern = FileCollector.ParseFileGlobbingPattern(includePattern);

            _fileCollector.IncludePatterns.Should().Contain(parsedPattern);
        }

        public void AssertIncludePatternsAreEmpty()
        {
            Sut.IncludePatterns.Should().BeEmpty();
        }
    }
}