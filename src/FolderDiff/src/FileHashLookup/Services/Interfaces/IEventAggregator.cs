using PsFolderDiff.FileHashLookup.Models;

namespace PsFolderDiff.FileHashLookup.Services.Interfaces;

public interface IEventAggregator
{
    void Subscribe(IProgress<ProgressEventArgs> progress);
    void Publish(ProgressEventArgs progressEvent);
}