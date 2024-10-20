using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using PsFolderDiff.FileHashLookupLib.Extensions;
using PsFolderDiff.FileHashLookupLib.Services;
using PsFolderDiff.FileHashLookupLib.Services.Interfaces;
using Xunit;

namespace PsFolderDiff.FileHashLookupLib.UnitTests.Extensions;

public class ServiceCollectionExtensionTests
{
    [Fact]
    public void Register_MultipleInterfacesResolveToSameInstance()
    {
        // Arrange
        var sp = new ServiceCollection()
            .AddFileHashLookup()
            .BuildServiceProvider();

        // Act
        var fileCollector = sp.GetRequiredService<IFileCollector>();
        var readOnlyFilePattern = sp.GetRequiredService<IHasReadOnlyFilePatterns>();

        // Assert
        fileCollector.Should().BeSameAs(readOnlyFilePattern);
    }

    [Fact]
    public void Register_CanResolve_FileHashLookup()
    {
        var sp = new ServiceCollection()
            .AddFileHashLookup()
            .BuildServiceProvider();

        // Act
        var fileHashLookup = sp.GetRequiredService<FileHashLookup>();

        // Assert
        fileHashLookup.Should().NotBeNull();
    }
}