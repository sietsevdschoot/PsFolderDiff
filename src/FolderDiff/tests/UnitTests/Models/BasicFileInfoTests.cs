using FluentAssertions;
using PsFolderDiff.FileHashLookup.UnitTests.Utils;
using Xunit;

namespace PsFolderDiff.FileHashLookup.UnitTests.Models;

public class BasicFileInfoTests
{
    [Fact]
    public void Compare_same_file_as_FileInfo_and_BasicFileInfo_equals()
    {
        // Arrange
        var fixture = new BasicFileInfoFixture();
        var file = fixture.WithNewFile();

        // Act
        var basicFile = HashingUtil.CreateBasicFileInfo(file);
        
        // Assert
        basicFile.Equals(file).Should().BeTrue();
    }

    [Fact]
    public void Compare_same_file_as_BasicFileInfo_equals()
    {
        // Arrange
        var fixture = new BasicFileInfoFixture();
        var file = fixture.WithNewFile();

        // Act
        var basicFile1 = HashingUtil.CreateBasicFileInfo(file);
        var basicFile2 = HashingUtil.CreateBasicFileInfo(file);
        
        // Assert
        basicFile1.Equals(basicFile2).Should().BeTrue();
    }

    [Fact]
    public void Later_modified_file_is_greater_than_original_file_due_LastWriteTime()
    {
        // Arrange
        var fixture = new BasicFileInfoFixture();
        var file = fixture.WithNewFile();

        var basicFile1 = HashingUtil.CreateBasicFileInfo(file);

        var updatedFile = fixture.UpdateFile(file);

        // Act
        var updatedBasicFile = HashingUtil.CreateBasicFileInfo(updatedFile);

        // Assert
        basicFile1.CompareTo(updatedBasicFile).Should().Be(-1);
    }

    ////[Fact]
    ////public void Exists_If_File_exists_on_fileSystem_returns_true()
    ////{
    ////    // Arrange
    ////    var fixture = new BasicFileInfoFixture();

    ////    var file = fixture.WithNewBasicFile();

    ////    // Act
    ////    var exists = file.Exists;

    ////    // Assert
    ////    exists.Should().BeTrue();
    ////}

    ////[Fact]
    ////public void Exists_If_File_does_not_exist_on_fileSystem_returns_false()
    ////{
    ////    // Arrange
    ////    var fixture = new BasicFileInfoFixture();

    ////    var file = fixture.WithNewBasicFile();
    ////    fixture.RemoveFile(file.FullName);
        
    ////    // Act
    ////    var exists = file.Exists;

    ////    // Assert
    ////    exists.Should().BeFalse();
    ////}

    private class BasicFileInfoFixture : FileHashTestFixture
    {
        public BasicFileInfoFixture RemoveFile(string fullName)
        {
            FileSystem.FileInfo.New(fullName).Delete();

            return this;
        }

    }
}