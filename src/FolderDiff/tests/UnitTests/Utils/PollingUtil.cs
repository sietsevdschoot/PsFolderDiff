using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace PsFolderDiff.FileHashLookup.UnitTests.Utils;

public class PollingUtil
{
    private readonly ILogger<PollingUtil> _logger;

    public PollingUtil(ILogger<PollingUtil> logger)
    {
        _logger = logger;
    }

    public async Task PollForExpectedResultInternalAsync(
        Func<Task<bool>> checkExpectation,
        Func<Task<string>> retrieveMessage,
        TimeSpan timeout,
        TimeSpan interval)
    {
        _logger.LogInformation("Start polling for expected result...");

        var stopwatch = Stopwatch.StartNew();

        do
        {
            try
            {
                var hasMetExpectation = await checkExpectation.Invoke();

                if (hasMetExpectation)
                {
                    _logger.LogInformation($"Found expected result in {stopwatch.Elapsed:hh\\:mm\\:ss}.");
                    return;
                }
                else
                {
                    await Task.Delay(interval);
                }
            }
            catch (Exception ex) when (ex is not TimeoutException)
            {
                _logger.LogError(ex, "An unexpected error occured while polling.");
            }
        }
        while (stopwatch.Elapsed < timeout);

        var message = await retrieveMessage.Invoke();

        _logger.LogInformation("Finish polling for expected result...");

        throw new TimeoutException(
            $"{message}\n(Polled for {timeout.TotalSeconds} second(s) with {interval.TotalMilliseconds} ms intervals.)");
    }

}