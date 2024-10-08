namespace PsFolderDiff.FileHashLookup.Configuration;

public class FileHashLookupSettings
{
    public static FileHashLookupSettings Default => new FileHashLookupSettings
    {
        ReportPollingDelay = TimeSpan.FromMilliseconds(500),
    };

    public TimeSpan ReportPollingDelay { get; set; }
}