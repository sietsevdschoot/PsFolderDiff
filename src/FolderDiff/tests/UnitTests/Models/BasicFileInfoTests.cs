using System.IO.Abstractions;
using FluentAssertions;
using PsFolderDiff.FileHashLookup.Models;
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

    private class BasicFileInfoFixture : FileHashTestFixture
    {

    }
}