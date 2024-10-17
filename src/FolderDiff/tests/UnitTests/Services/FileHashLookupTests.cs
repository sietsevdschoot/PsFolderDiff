using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using PsFolderDiff.FileHashLookupLib.Services;
using PsFolderDiff.FileHashLookupLib.Services.Interfaces;
using PsFolderDiff.FileHashLookupLib.UnitTests.Extensions;
using PsFolderDiff.FileHashLookupLib.Utils;
using Xunit;

namespace PsFolderDiff.FileHashLookupLib.UnitTests.Services;

public class FileHashLookupTests
{
    [Fact]
    public async Task IncludeFolder_Adds_Folder_And_Collects_Files_Recursively()
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
        await fixture.Sut.Include(includeFolder);

        // Assert
        fixture.AssertContainsFileNames([1, 2, 3, 4]);
        fixture.AssertContainsIncludePath(includeFolder);
    }

    [Fact]
    public async Task IncludePattern_Adds_Folder_And_Collects_Files_Recursively()
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
        await fixture.Sut.Include(includePattern);

        // Assert
        fixture.AssertContainsFileNames([3, 4]);
        fixture.AssertContainsIncludePattern(includePattern);
    }

    [Fact]
    public async Task ExcludePattern_ExcludesPatternFromAlreadyCollectedFiles()
    {
        // Arrange
        var fixture = new FileHashLookupTestFixture();
        fixture.WithNewFile(@"Folder1\1.txt");
        fixture.WithNewFile(@"Folder1\2.txt");
        fixture.WithNewFile(@"Folder1\Sub1\3.txt");
        fixture.WithNewFile(@"Folder1\Sub1\Sub2\4.txt");
        fixture.WithNewFile(@"Folder2\5.txt");

        var excludePattern = @$"**\Sub1\**\*";

        // Act
        await fixture.Sut.Include(@"Folder1");
        await fixture.Sut.Exclude(excludePattern);

        // Assert
        fixture.AssertContainsFileNames([1, 2]);
        fixture.AssertContainsExcludesPattern(excludePattern);
    }

    [Fact]
    public async Task ExcludePattern_Can_exclude_files_from_different_drives()
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

        var provider = fixture.CreateFileHashLookupWithProvider(settings =>
        {
            settings.ConfigureServices = (services, _) =>
            {
                services.AddSingleton<IFileSystem>(fileSystem);
            };
        });

        var fileHashlookup = provider.FileHashLookup;

        // Act
        await fileHashlookup.Include(@"c:\Temp\Folder1\");
        await fileHashlookup.Include(@"d:\Temp\Folder2\");

        await fileHashlookup.Exclude(@"d:\Temp\");

        // Assert
        var actualFileNames = fileHashlookup.GetFiles().Select(x => Convert.ToInt32(fileSystem.Path.GetFileNameWithoutExtension(x.FullName)));

        actualFileNames.Should().BeEquivalentTo([1, 2]);
    }

    [Fact]
    public async Task ExcludePattern_Can_exclude_pattern_over_different_drives()
    {
        // Arrange
        var fixture = new FileHashLookupTestFixture();

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { @"c:\Temp\Folder1\1.txt", new MockFileData(Guid.NewGuid().ToString()) },
            { @"c:\Temp\Folder1\2.doc", new MockFileData(Guid.NewGuid().ToString()) },
            { @"d:\Temp\Folder2\3.txt", new MockFileData(Guid.NewGuid().ToString()) },
            { @"d:\Temp\Folder2\4.doc", new MockFileData(Guid.NewGuid().ToString()) },
        });

        var provider = fixture.CreateFileHashLookupWithProvider(settings =>
        {
            settings.ConfigureServices = (services, _) =>
            {
                services.AddSingleton<IFileSystem>(fileSystem);
            };
        });

        var fileHashlookup = provider.FileHashLookup;

        // Act
        await fileHashlookup.Include(@"c:\Temp\Folder1\");
        await fileHashlookup.Include(@"d:\Temp\Folder2\");

        await fileHashlookup.Exclude("*.doc");

        // Assert
        var actualFileNames = fileHashlookup.GetFiles().Select(x => Convert.ToInt32(fileSystem.Path.GetFileNameWithoutExtension(x.FullName)));

        actualFileNames.Should().BeEquivalentTo([1, 3]);
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
        await fileHashLookup1.Include("Folder1");

        var fileHashLookup2 = fixture.CreateFileHashLookup();
        await fileHashLookup2.Include("Folder2");

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
    public async Task Will_look_at_the_file_hash_to_determine_file_equality_or_difference()
    {
        // Arrange
        var fixture = new FileHashLookupTestFixture();

        string content = "Identical file content";

        fixture.WithNewFile(@"Folder1\1.txt", content);
        fixture.WithNewFile(@"Folder1\2.txt", content);
        fixture.WithNewFile(@"Folder1\3.txt", content);

        var fileHashLookup1 = fixture.CreateFileHashLookup();
        await fileHashLookup1.Include(@"Folder1");

        fixture.WithNewFile(@"Folder2\4.txt", content);
        fixture.WithNewFile(@"Folder2\5.txt", content);

        var fileHashLookup2 = fixture.CreateFileHashLookup();
        await fileHashLookup2.Include(@"Folder2");

        // Act
        var actual = await fileHashLookup1.GetDifferencesInOther(fileHashLookup2);

        // Assert
        actual.GetFiles()
            .Select(x => Convert.ToInt32(fixture.FileSystem.Path.GetFileNameWithoutExtension(x.FullName)))
            .Should().BeEquivalentTo([4, 5]);
    }

    [Fact]
    public async Task GetMatchesInOther_Returns_file_only_found_in_other()
    {
        // Arrange
        var fixture = new FileHashLookupTestFixture();
        fixture.WithNewFile(@"Folder1\1.txt");
        fixture.WithNewFile(@"Folder1\2.txt");

        var fileHashLookup1 = fixture.CreateFileHashLookup();
        await fileHashLookup1.Include("Folder1");

        // Update
        fixture.WithNewFile(@"Folder1\3.txt");
        fixture.WithNewFile(@"Folder1\4.txt");
        var fileHashLookup2 = fixture.CreateFileHashLookup();
        await fileHashLookup2.Include("Folder1");

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
        await fileHashLookup1.Include("Folder1");

        // Update
        fixture.WithNewFile(@"Folder1\3.txt");
        fixture.WithNewFile(@"Folder1\4.txt");
        var fileHashLookup2 = fixture.CreateFileHashLookup();
        await fileHashLookup2.Include("Folder1");

        // Act
        var diffInOther = await fileHashLookup1.GetDifferencesInOther(fileHashLookup2);

        // Assert
        diffInOther.AssertContainsFileNames([3, 4]);
        diffInOther.AssertIncludePatternsAreEmpty();
        diffInOther.AssertExcludePatternsAreEmpty();
    }

    [Fact]
    public async Task Refresh_Adds_New_Files()
    {
        // Arrange
        var fixture = new FileHashLookupTestFixture();
        fixture.WithNewFile(@"Folder1\1.txt");
        await fixture.Sut.Include("Folder1");

        // Act
        fixture.WithNewFile(@"Folder1\2.txt");
        await fixture.Sut.Refresh();

        // Assert
        fixture.AssertContainsFileNames([1, 2]);
    }

    [Fact]
    public async Task Refresh_Removes_No_Longer_Existing_Files()
    {
        // Arrange
        var fixture = new FileHashLookupTestFixture();
        fixture.WithNewFile(@"Folder1\1.txt");
        fixture.WithNewFile(@"Folder1\2.txt");
        fixture.WithNewFile(@"Folder1\3.txt");
        await fixture.Sut.Include("Folder1");

        // Act
        fixture.DeleteFile(@"Folder1\2.txt");
        await fixture.Sut.Refresh();

        // Assert
        fixture.AssertContainsFileNames([1, 3]);
    }

    [Fact]
    public async Task Refresh_Updated_Modified_Files()
    {
        // Arrange
        var fixture = new FileHashLookupTestFixture();
        var file1 = fixture.WithNewFile(@"Folder1\1.txt");
        await fixture.Sut.Include("Folder1");

        // Act
        fixture.UpdateFile(file1);
        await fixture.Sut.Refresh();

        // Assert
        var expected = fixture.GetBasicFileInfo(file1.FullName);
        var actual = fixture.Sut.File[file1.FullName];

        actual.Should().BeEquivalentTo(expected);
        fixture.Sut.File.Should().HaveCount(1);
    }

    [Fact]
    public void Creates_a_file_containing_the_HashTable()
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
        var fixture = new FileHashLookupTestFixture();

        // Act
        fixture.Sut.Save();

        // Assert
        fixture.AllFiles.Should().HaveCount(1);
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
                settings.ReportProgressDelay = TimeSpan.Zero;
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
            var parsedPattern = PathUtils.ParseFileGlobbingPattern(excludePattern);

            _fileCollector.ExcludePatterns.Contains(parsedPattern).Should().BeTrue();
        }

        public void AssertContainsIncludePath(string includeFolder)
        {
            Sut.AssertContainsIncludePath(includeFolder);
        }

        public void AssertContainsIncludePattern(string includePattern)
        {
            var parsedPattern = PathUtils.ParseFileGlobbingPattern(includePattern);

            _fileCollector.IncludePatterns.Should().Contain(parsedPattern);
        }

        public void AssertIncludePatternsAreEmpty()
        {
            Sut.IncludePatterns.Should().BeEmpty();
        }
    }
}