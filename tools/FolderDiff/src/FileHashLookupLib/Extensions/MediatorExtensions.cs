using MediatR;

namespace PsFolderDiff.FileHashLookupLib.Extensions;

public static class MediatorExtensions
{
    public static async Task SendAsyncWithCancellation<TRequest>(
        this IMediator mediator,
        CancellationTokenSource cts,
        TRequest message,
        CancellationToken cancellationToken)
        where TRequest : IRequest
    {
        try
        {
            await mediator.Send(message, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            if (!cts.TryReset())
            {
                Console.WriteLine("Unable to reset cancellationToken.");
            }
        }
    }

    public static async Task<TResponse> SendAsyncWithCancellation<TResponse>(
        this IMediator mediator,
        CancellationTokenSource cts,
        IRequest<TResponse> message,
        CancellationToken cancellationToken)
        where TResponse : class
    {
        try
        {
            return await mediator.Send(message, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            if (!cts.TryReset())
            {
                Console.WriteLine("Unable to reset cancellationToken.");
            }

            throw;
        }
    }
}