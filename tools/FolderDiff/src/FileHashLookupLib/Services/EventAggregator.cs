using PsFolderDiff.FileHashLookupLib.Domain;
using PsFolderDiff.FileHashLookupLib.Services.Interfaces;

namespace PsFolderDiff.FileHashLookupLib.Services;

public class EventAggregator : IEventAggregator
{
    private readonly List<IProgress<ProgressEventArgs>> _subscribers = new();
    ////private readonly SynchronizationContext _synchronizationContext;

    public EventAggregator()
    {
        ////_synchronizationContext = SynchronizationContext.Current ?? new SynchronizationContext();
    }

    public void Subscribe(IProgress<ProgressEventArgs> progress)
    {
        _subscribers.Add(progress);
    }

    public void Publish(ProgressEventArgs progressEvent)
    {
        ArgumentNullException.ThrowIfNull(progressEvent, nameof(progressEvent));

        foreach (var subscriber in _subscribers)
        {
            ////_synchronizationContext.Post(state => subscriber.Report((ProgressEventArgs)state!), progressEvent);
            subscriber.Report(progressEvent);
        }
    }
}