using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using PsFolderDiff.FileHashLookup.Extensions;
using PsFolderDiff.FileHashLookup.Services.Interfaces;
using Xunit;

namespace PsFolderDiff.FileHashLookup.UnitTests.Extensions;

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
}