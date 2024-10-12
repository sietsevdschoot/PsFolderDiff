using PsFolderDiff.FileHashLookupLib.Models;

namespace PsFolderDiff.FileHashLookupLib.Services.Interfaces;

public interface IEventAggregator
{
    void Subscribe(IProgress<ProgressEventArgs> progress);

    void Publish(ProgressEventArgs progressEvent);
}