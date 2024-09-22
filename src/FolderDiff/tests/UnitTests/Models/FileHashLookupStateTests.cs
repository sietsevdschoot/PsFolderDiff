using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using PsFolderDiff.FileHashLookup.Models;
using Xunit;

namespace PsFolderDiff.FileHashLookup.UnitTests.Models;

public class FileHashLookupStateTests
{
    [Fact]
    public void Creates_2_way_HashTable()
    {
        // Arrange
        var fixture = new FileHashLookupStateTestsFixture();

        // Act
        fixture.WithFilesAddedToState();

        // Assert
        fixture.AssertFileAndHashLookupsArePopulated();
    }
    
    [Fact]
    public void Creates_a_lookup_of_all_files()
    {
        // Arrange
        var fixture = new FileHashLookupStateTestsFixture();

        // Act
        fixture.WithFilesAddedToState(nrOfFiles: 3);

        // Assert
        fixture.AssertFileAndHashLookupsContainFiles(nrOfFiles: 3);
    }

    [Fact]
    public void Creates_a_hash_and_file_lookup()
    {
        // Arrange
        var fixture = new FileHashLookupStateTestsFixture(); 
        
        // Act
        fixture.WithFilesAddedToState();

        // Assert
        var file = Random.Shared.GetItems(fixture.AllFiles, 1)[0];

        fixture.Sut.File[file.FullName].Should().Be(file);
        fixture.Sut.Hash[file.Hash].Should().BeEquivalentTo([file]);
    }



    [Fact]
    public void Creates_a_List_for_files_which_share_the_same_hash()
    {
        // Arrange
        

        // Act

        // Assert
    }

    [Fact]
    public void Creates_a_file_containing_the_HashTable_()
    {
        // Arrange
        

        // Act

        // Assert
    }

    [Fact]
    public void ExcludeFilePattern_Adding_invalid_pattern_throws()
    {
        // Arrange
        

        // Act

        // Assert
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
    public void can_add_other_FileHashLookup()
    {
        // Arrange
        

        // Act

        // Assert
    }

    [Fact]
    public void will_look_at_the_file_hash_to_determine_file_equality_or_difference()
    {
        // Arrange
        

        // Act

        // Assert
    }

    [Fact]
    public void will_not_contain_duplicates_files()
    {
        // Arrange
        

        // Act

        // Assert
    }

    [Fact]
    public void will_remove_entry_and_List_if_no_items_left()
    {
        // Arrange
        

        // Act

        // Assert
    }

    [Fact]
    public void will_remove_entry_from_List_if_file_with_same_hash_exists()
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

    private class FileHashLookupStateTestsFixture : FileHashTestFixture
    {
        public FileHashLookupState Sut { get; } = new();

        public void WithFilesAddedToState(int? nrOfFiles = null)
        {
            for (var i = 0; i < (nrOfFiles ?? 10); i++)
            {
                var file = WithNewBasicFile();

                Sut.Add(file);
            }
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
    }
}
