using PsFolderDiff.FileHashLookupLib.Models;
using PsFolderDiff.FileHashLookupLib.Services.Interfaces;

namespace PsFolderDiff.FileHashLookupLib.Services;

public class EventAggregator : IEventAggregator
{
    private readonly List<IProgress<ProgressEventArgs>> _subscribers = new();

    public void Subscribe(IProgress<ProgressEventArgs> progress)
    {
        _subscribers.Add(progress);
    }

    public void Publish(ProgressEventArgs progressEvent)
    {
        foreach (var subscriber in _subscribers)
        {
            subscriber.Report(progressEvent);
        }
    }
}