using Microsoft.Extensions.DependencyInjection;
using PsFolderDiff.FileHashLookupLib.Configuration;
using PsFolderDiff.FileHashLookupLib.Services;

namespace PsFolderDiff.FileHashLookupLib.UnitTests.Extensions;

public static class FileHashTestFixtureCreationExtensions
{
    public static FileHashLookup CreateFileHashLookup<TFixture>(this TFixture fixture)
        where TFixture : FileHashTestFixture
    {
        var provider = CreateFileHashLookupWithProviderMockFileSystem(fixture);

        return provider.FileHashLookup;
    }

    public static (FileHashLookup FileHashLookup, IServiceProvider ServiceProvider) CreateFileHashLookupWithProviderMockFileSystem<TFixture>(this TFixture fixture)
        where TFixture : FileHashTestFixture
    {
        return CreateFileHashLookupWithProvider(fixture, settings =>
        {
            settings.ReportProgressDelay = TimeSpan.Zero;
            settings.ConfigureServices.Add((services, _) =>
            {
                services.AddSingleton(fixture.FileSystem);
            });
        });
    }

    public static (FileHashLookup FileHashLookup, IServiceProvider ServiceProvider) CreateFileHashLookupWithProvider<TFixture>(
        this TFixture fixture, FileHashLookupSettings settings)
        where TFixture : FileHashTestFixture
    {
        var services = new ServiceCollection();

        return FileHashLookup.Create(services, settings);
    }

    public static (FileHashLookup FileHashLookup, IServiceProvider ServiceProvider) CreateFileHashLookupWithProvider<TFixture>(
        this TFixture fixture, Action<FileHashLookupSettings>? configureSettings = null)
        where TFixture : FileHashTestFixture
    {
        var settings = FileHashLookupSettings.Default;
        configureSettings?.Invoke(settings);

        return fixture.CreateFileHashLookupWithProvider(settings);
    }
}