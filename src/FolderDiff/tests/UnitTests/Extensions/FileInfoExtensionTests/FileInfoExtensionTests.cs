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
        var fixture = new FileInfoExtensionFixture();

        fixture.WithNewRandomFile();
        fixture.WithPowershellCalculatedMd5Hash();

        // Act && Assert
        fixture.AssertCalculateMd5HashMatchesPowershellHash();
    }

    private class FileInfoExtensionFixture
    {
        private string? _tempFile;
        private string? _expected;

        public void WithNewRandomFile()
        {
            _tempFile = Path.GetTempFileName();
            File.WriteAllText(_tempFile, Guid.NewGuid().ToString());
        }

        public void WithPowershellCalculatedMd5Hash()
        {
            var powershell = PowerShell.Create();

            var result = powershell.AddCommand("Get-FileHash")
                .AddParameter("Path", _tempFile)
                .AddParameter("Algorithm", "MD5")
                .Invoke();

            _expected = result[0].Members["Hash"].Value.ToString();
        }

        public void AssertCalculateMd5HashMatchesPowershellHash()
        {
            var file = new FileSystem()
                .FileInfo.New(_tempFile ?? string.Empty);

            // Act
            var actual = file.CalculateMD5Hash();

            // Assert
            actual.Should().Be(_expected);
        }
    }
}