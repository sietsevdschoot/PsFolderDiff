using FluentAssertions;
using Xunit;

namespace PsFolderDiff.FileHashLookup.UnitTests.Services;

public class FileHashLookupTests
{

    [Fact]
    public async Task AddIncludeFolder_Adds_Folder_And_Collects_Files_Recursively()
    {
        // Arrange 
        var fixture = new FileHashLookupTestFixture();
        fixture.WithNewFile(@"Folder1\1.txt");
        fixture.WithNewFile(@"Folder1\2.txt");
        fixture.WithNewFile(@"Folder1\Sub\3.txt");
        fixture.WithNewFile(@"Folder1\Sub\Sub\4.txt");
        fixture.WithNewFile(@"Folder2\5.txt");

        // Act
        await fixture.AddFolder(@"Folder1\");

        // Assert
        fixture.AssertContainsFileNames([1, 2, 3, 4]);
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

        public FileHashLookupTestFixture()
        {
            _sut = FileHashLookup.Services.FileHashLookup.Create();
        }

        public async Task AddFolder(string path)
        {
            await _sut.AddFolder(path);
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