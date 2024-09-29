using Microsoft.Extensions.DependencyInjection;
using PsFolderDiff.FileHashLookup.Extensions;
using PsFolderDiff.FileHashLookup.Models;

namespace PsFolderDiff.FileHashLookup.Services;

public class FileHashLookup
{
    private FileHashLookupState _state;

    public static FileHashLookup Create()
    {
        var sp = new ServiceCollection()
            .AddFileHashLookup()
            .BuildServiceProvider();

        var fileHashLookup = sp.GetRequiredService<FileHashLookup>();

        return fileHashLookup;
    }

    private FileHashLookup(FileHashLookupState state)
    {
        _state = state;
    }


}