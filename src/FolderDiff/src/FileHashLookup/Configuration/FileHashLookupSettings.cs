using Microsoft.Extensions.DependencyInjection;

namespace PsFolderDiff.FileHashLookup.Configuration;

public class FileHashLookupSettings
{
    public static FileHashLookupSettings Default => new FileHashLookupSettings
    {
        ReportPollingDelay = TimeSpan.FromMilliseconds(500),
    };

    public TimeSpan ReportPollingDelay { get; set; }

    public Action<IServiceCollection, IServiceProvider>? ConfigureServices { get; set; }
}