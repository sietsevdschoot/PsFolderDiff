using System.IO.Abstractions;
using FluentAssertions;
using PsFolderDiff.FileHashLookup.Extensions;
using Xunit;

namespace PsFolderDiff.FileHashLookup.UnitTests.Extensions.FileInfoExtensionTests;

public class FileInfoExtensionTests
{
    [Fact]
    public void CalculateMD5Hash_CalculatesIdenticalHash_as_Powershell_GetFileHash_Cmdlet()
    {
        // Arrange
        var fileSystem = new FileSystem();
        var file = fileSystem.FileInfo.New(
            "C:\\Users\\Sietse\\AppData\\Local\\Temp\\FolderDiff\\2024-10-01-65c6180e-4c76-478e-bcf5-830385794e56\\1.txt");

        // Act
        file.Exists.Should().BeTrue();
        file.CalculateMD5Hash().Should().Be("477665D7FA50198BF79D363D006C3788", $"Hash '{file.CalculateMD5Hash()}' is not expected format");

        // Assert
    }
}