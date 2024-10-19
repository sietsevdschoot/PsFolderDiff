using PsFolderDiff.FileHashLookupLib.Domain;

namespace PsFolderDiff.FileHashLookupLib.Services.Interfaces;

public interface IEventAggregator
{
    void Subscribe(IProgress<ProgressEventArgs> progress);

    void Publish(ProgressEventArgs progressEvent);
}