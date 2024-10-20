using Microsoft.Extensions.DependencyInjection;
using PsFolderDiff.FileHashLookupLib.Configuration;
using PsFolderDiff.FileHashLookupLib.Services;

namespace PsFolderDiff.FileHashLookupLib.UnitTests.Extensions;

public static class FileHashTestFixtureCreationExtensions
{
    public static FileHashLookup CreateFileHashLookup<TFixture>(this TFixture fixture)
        where TFixture : FileHashTestFixture
    {
        var provider = CreateFileHashLookupWithProvider(fixture, fixture.FileHashLookupSettings);

        return provider.FileHashLookup;
    }

    public static (FileHashLookup FileHashLookup, IServiceProvider ServiceProvider) CreateFileHashLookupWithProvider<TFixture>(
        this TFixture fixture, Action<FileHashLookupSettings>? configureSettings = null)
        where TFixture : FileHashTestFixture
    {
        var settings = FileHashLookupSettings.Default;
        configureSettings?.Invoke(settings);

        return fixture.CreateFileHashLookupWithProvider(settings);
    }

    public static (FileHashLookup FileHashLookup, IServiceProvider ServiceProvider) CreateFileHashLookupWithProvider<TFixture>(
        this TFixture fixture, FileHashLookupSettings settings)
        where TFixture : FileHashTestFixture
    {
        var services = new ServiceCollection();

        return FileHashLookup.Create(services, settings);
    }
}