using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using PsFolderDiff.FileHashLookup.Domain;
using PsFolderDiff.FileHashLookup.Extensions;
using PsFolderDiff.FileHashLookup.Services;
using PsFolderDiff.FileHashLookup.UnitTests.Extensions;
using Xunit;

namespace PsFolderDiff.FileHashLookup.UnitTests.Services;

public class FileHashLookupStateTests
{
    [Fact]
    public void Creates_2_way_HashTable()
    {
        // Arrange
        var fixture = new IFileHashLookupStateTestsFixture();

        // Act
        fixture.WithAddedFiles();

        // Assert
        fixture.AssertFileAndHashLookupsArePopulated();
    }

    [Fact]
    public void Creates_a_lookup_of_all_files()
    {
        // Arrange
        var fixture = new IFileHashLookupStateTestsFixture();

        // Act
        fixture.WithAddedFiles(nrOfFiles: 3);

        // Assert
        fixture.AssertFileAndHashLookupsContainFiles(nrOfFiles: 3);
    }

    [Fact]
    public void Creates_a_hash_and_file_lookup()
    {
        // Arrange
        var fixture = new IFileHashLookupStateTestsFixture();

        // Act
        fixture.WithAddedFiles();

        // Assert
        var file = Random.Shared.GetItems(fixture.AllFiles, 1)[0];

        fixture.Sut.File[file.FullName].Should().Be(file);
        fixture.Sut.Hash[file.Hash].Should().BeEquivalentTo([file]);
    }

    [Fact]
    public void Creates_a_List_for_files_which_share_the_same_hash()
    {
        // Arrange
        var fixture = new IFileHashLookupStateTestsFixture();

        // Act
        fixture.WithAddedFiles(nrOfFiles: 4, fileContents: "Identical Hash");

        // Assert
        var hash = fixture.AllFiles.OrderByDescending(x => x.CreationTime).First().Hash;
        fixture.Sut.Hash[hash].Should().HaveCount(4);
        fixture.Sut.File.Should().HaveCount(4);
    }

    [Fact]
    public void Will_not_contain_duplicates_files()
    {
        // Arrange
        var fixture = new IFileHashLookupStateTestsFixture()
            .WithAddedFiles();

        // Act
        var file = fixture.AllFiles.First();
        fixture.Sut.Add(file);

        // Assert
        fixture.Sut.File.Should().HaveCount(fixture.AllFiles.Length);
        fixture.Sut.Hash.Should().HaveCount(fixture.AllFiles.Length);
    }

    [Fact]
    public void Will_remove_entry_and_List_if_no_items_left()
    {
        // Arrange
        var fixture = new IFileHashLookupStateTestsFixture()
            .WithAddedFiles();

        // Act
        fixture.WithAllFilesRemoved();

        // Assert
        fixture.AssertHashLookupsAreEmpty();
    }

    [Fact]
    public void Will_remove_entry_from_List_if_file_with_same_hash_exists()
    {
        // Arrange
        var fixture = new IFileHashLookupStateTestsFixture()
            .WithAddedFiles(Enumerable.Range(1, 4), "Identical Hash");

        var file = fixture.AllFiles.First();

        // Act
        fixture.Sut.Remove(file);

        // Assert
        fixture.Sut.File.ContainsKey(file.FullName).Should().BeFalse();
        fixture.Sut.Hash[file.Hash].Should().HaveCount(3);
    }

    [Fact]
    public void Add_When_adding_modified_file_will_remove_original_file_before_adding_modified_file()
    {
        // Arrange
        var fixture = new IFileHashLookupStateTestsFixture();
        var file = fixture.WithNewBasicFile(fileIdentifier: 1);
        var originalHash = file.Hash;

        fixture.Sut.Add(file);

        var updatedFile = fixture.UpdateFile(file);

        // Act
        fixture.Sut.Add(updatedFile);

        // Assert
        fixture.Sut.File[file.FullName].Hash.Should().Be(updatedFile.Hash);

        fixture.Sut.Hash[updatedFile.Hash].Should().HaveCount(1);
        fixture.Sut.Hash.ContainsKey(originalHash).Should().BeFalse();
    }

    [Fact]
    public void Contains_when_file_is_not_present_in_FileHashLookup_returns_NoMatch()
    {
        // Arrange
        var fixture = new IFileHashLookupStateTestsFixture()
            .WithAddedFiles(1);

        var file2 = fixture.WithNewBasicFile(2);

        // Act && Assert
        fixture.Sut.Contains(file2).Should().Be(FileContainsState.NoMatch);
    }

    [Fact]
    public void Contains_when_file_is_present_in_FileHashLookup_returns_Match()
    {
        // Arrange
        var fixture = new IFileHashLookupStateTestsFixture()
            .WithAddedFiles(1);

        var file1 = fixture.GetBasicFileInfo(1);

        // Act && Assert
        fixture.Sut.Contains(file1).Should().Be(FileContainsState.Match);
    }

    [Fact]
    public void Contains_when_file_is_present_in_FileHashLookup_and_changed_after_returns_Modified()
    {
        // Arrange
        var fixture = new IFileHashLookupStateTestsFixture()
            .WithAddedFiles(1);

        var file1 = fixture.GetBasicFileInfo(1);
        fixture.UpdateFile(file1);
        file1 = fixture.GetBasicFileInfo(1);

        // Act && Assert
        fixture.Sut.Contains(file1).Should().Be(FileContainsState.Modified);
    }

    [Fact]
    public void Contains_returns_true_if_file_matches()
    {
        // Arrange
        var fixture = new IFileHashLookupStateTestsFixture()
            .WithAddedFiles(Enumerable.Range(1, 3));

        var file1 = fixture.GetFileInfo("1.txt");

        // Act && Assert
        fixture.Sut.Contains(file1).Should().BeTrue();
    }

    [Fact]
    public void Will_look_at_the_file_hash_to_determine_file_equality_or_difference()
    {
        // Arrange

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
    public void Exposes_the_paths_which_were_used()
    {
        // Arrange

        // Act

        // Assert
    }

    [Fact]
    public void Can_exclude_file_patterns()
    {
        // Arrange

        // Act

        // Assert
    }

    [Fact]
    public void Can_exclude_folders()
    {
        // Arrange

        // Act

        // Assert
    }

    [Fact]
    public void Can_include_file_patterns()
    {
        // Arrange

        // Act

        // Assert
    }

    [Fact]
    public void Will_remove_already_added_files_which_match_excludeFilePatterns()
    {
        // Arrange

        // Act

        // Assert
    }

    [Fact]
    public void Will_remove_already_added_files_which_match_excluded_folders_()
    {
        // Arrange

        // Act

        // Assert
    }

    [Fact]
    public void GetFiles_returns_all_files()
    {
        // Arrange

        // Act

        // Assert
    }

    [Fact]
    public void GetFilesByHash_returns_all_files_matching_the_hash()
    {
        // Arrange

        // Act

        // Assert
    }

    [Fact]
    public void Can_display_contents_by_using_ToString()
    {
        // Arrange

        // Act

        // Assert
    }

    [Fact]
    public void Can_save_with_correct_filename_when_relative_path_for_folder_is_used()
    {
        // Arrange

        // Act

        // Assert
    }

    [Fact]
    public void Contains_returns_if_fileHashTable_contains_file()
    {
        // Arrange

        // Act

        // Assert
    }

    [Fact]
    public void Can_add_other_FileHashLookup()
    {
        // Arrange

        // Act

        // Assert
    }

    private class IFileHashLookupStateTestsFixture : FileHashTestFixture
    {
        public IFileHashLookupStateTestsFixture()
        {
            var provider = CreateFileHashLookupWithProvider();
            Sut = provider.ServiceProvider.GetRequiredService<FileHashLookupState>();
        }

        public FileHashLookupState Sut { get; }

        public IFileHashLookupStateTestsFixture WithAddedFiles(int nrOfFiles = 10, string? fileContents = null)
        {
            return WithAddedFiles(Enumerable.Range(1, nrOfFiles), fileContents);
        }

        public IFileHashLookupStateTestsFixture WithAddedFiles(IEnumerable<int> range, string? fileContents = null)
        {
            foreach (var i in range)
            {
                var file = WithNewBasicFile(i, fileContents);

                Sut.Add(file);
            }

            return this;
        }

        public void AssertFileAndHashLookupsArePopulated()
        {
            Sut.File.Should().NotBeEmpty();
            Sut.Hash.Should().NotBeEmpty();
        }

        public void AssertFileAndHashLookupsContainFiles(int nrOfFiles)
        {
            Sut.File.Should().HaveCount(nrOfFiles);
            Sut.Hash.Should().HaveCount(nrOfFiles);
        }

        public FileHashTestFixture WithAllFilesRemoved()
        {
            AllFiles.ForEach(Sut.Remove);

            return this;
        }

        public void AssertHashLookupsAreEmpty()
        {
            Sut.File.Should().BeEmpty();
            Sut.Hash.Should().BeEmpty();
        }
    }
}
