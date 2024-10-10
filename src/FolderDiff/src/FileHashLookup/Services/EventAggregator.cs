using PsFolderDiff.FileHashLookup.Models;
using PsFolderDiff.FileHashLookup.Services.Interfaces;

namespace PsFolderDiff.FileHashLookup.Services;

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