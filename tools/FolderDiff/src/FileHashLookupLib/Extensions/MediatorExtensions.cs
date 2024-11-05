using MediatR;
using Microsoft.Extensions.Logging;
using PsFolderDiff.FileHashLookupLib.Services;

namespace PsFolderDiff.FileHashLookupLib.Extensions;

public static class MediatorExtensions
{
    public static async Task SendAsyncWithCancellation<TRequest>(
        this IMediator mediator,
        CancellationTokenSource cts,
        ILogger<FileHashLookup> logger,
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
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occured");

            throw;
        }
    }

    public static async Task<TResponse> SendAsyncWithCancellation<TResponse>(
        this IMediator mediator,
        CancellationTokenSource cts,
        ILogger<FileHashLookup> logger,
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
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occured");

            throw;
        }
    }
}