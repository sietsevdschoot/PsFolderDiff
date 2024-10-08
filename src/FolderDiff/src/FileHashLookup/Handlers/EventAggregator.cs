using PsFolderDiff.FileHashLookup.Models;

namespace PsFolderDiff.FileHashLookup.Handlers;

public class EventAggregator
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