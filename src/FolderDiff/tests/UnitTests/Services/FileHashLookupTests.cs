using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.IO.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using PsFolderDiff.FileHashLookup.Extensions;
using PsFolderDiff.FileHashLookup.Services;
using PsFolderDiff.FileHashLookup.UnitTests.Extensions;
using Xunit;

namespace PsFolderDiff.FileHashLookup.UnitTests.Services;

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
        await fixture.AddFolder(includeFolder);

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
        await fixture.AddIncludePattern(includePattern);

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
        await fixture.AddFolder(@"Folder1\");
        await fixture.AddExcludePattern(excludePattern);

        // Assert
        fixture.AssertContainsFileNames([1, 2]);
        fixture.AssertContainsExcludesPattern(excludePattern);
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
            .ToList();

        // Act
        await fixture.AddFiles(files);

        // Assert
        fixture.AssertContainsFileNames([1, 2, 3, 4, 5]);
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

        var fileHashLookup1 = FileHashLookup.Services.FileHashLookup.Create();
        await fileHashLookup1.AddFolder("Folder1");

        var fileHashLookup2 = FileHashLookup.Services.FileHashLookup.Create();
        await fileHashLookup2.AddFolder("Folder2");

        // Act
        var fileHashLookup = FileHashLookup.Services.FileHashLookup.Create();
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
    public void can_load_new_instance_from_filename()
    {
        // Arrange


        // Act

        // Assert
    }

    [Fact]
    public void can_refresh_itself_By_adding_new_files_and_removing_no_longer_existing_files()
    {
        // Arrange


        // Act

        // Assert
    }

    [Fact]
    public void returns_matching_items_from_other_object_it_is_compared_to()
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
        private readonly FileHashLookup.Services.FileHashLookup _sut;
        private readonly FileCollector _fileCollector;

        public FileHashLookupTestFixture()
        {
            var sp = new ServiceCollection()
                .AddFileHashLookup()
                .BuildServiceProvider();

            _fileCollector = sp.GetRequiredService<FileCollector>();
            _sut = sp.GetRequiredService<FileHashLookup.Services.FileHashLookup>();
        }

        public async Task AddFolder(string path)
        {
            await _sut.AddFolder(path);
        }

        public async Task AddIncludePattern(string includePattern)
        {
            await _sut.AddIncludePattern(includePattern);
        }

        public async Task AddExcludePattern(string excludePattern)
        {
            await _sut.AddExcludePattern(excludePattern);
        }

        public void AssertContainsFileNames(params int[] expected)
        {
            _sut.AssertContainsFileNames(expected);
        }

        public void AssertContainsExcludesPattern(string excludePattern)
        {
            _fileCollector.ExcludePatterns.Contains(excludePattern).Should().BeTrue();
        }

        public void AssertContainsIncludePath(string includeFolder)
        {
            _sut.AssertContainsIncludePath(includeFolder);
        }

        public async Task AddFiles(List<IFileInfo> files)
        {
            await _sut.AddFiles(files.ToArray());
        }

        public void AssertContainsIncludePattern(string includePattern)
        {
            var parsedPattern = FileCollector.ParseFileGlobbingPattern(includePattern);

            _fileCollector.IncludePatterns.Should().Contain(parsedPattern);
        }

        public void AssertIncludePatternsAreEmpty()
        {
            _fileCollector.IncludePatterns.Should().BeEmpty();
        }
    }
}