using System.IO.Abstractions;
using System.Management.Automation;
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
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, Guid.NewGuid().ToString());

        var powershell = PowerShell.Create();

        var result = powershell.AddCommand("Get-FileHash")
            .AddParameter("Path", tempFile)
            .AddParameter("Algorithm", "MD5")
            .Invoke();

        var expected = result[0].Members["Hash"].Value.ToString();

        var file = new FileSystem()
            .FileInfo.New(tempFile);

        // Act
        var actual = file.CalculateMD5Hash();

        // Assert
        actual.Should().Be(expected);
    }
}