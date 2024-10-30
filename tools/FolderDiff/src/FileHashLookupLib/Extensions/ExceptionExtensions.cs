using ExceptionLayoutFormatter;

namespace PsFolderDiff.FileHashLookupLib.Extensions;

public static class ExceptionExtensions
{
    private static readonly IExceptionFormatter? ExceptionFormatter;

    static ExceptionExtensions()
    {
        ExceptionFormatter = ExceptionLayoutFormatter.ExceptionFormatter.Create();
    }

    public static string FormatException(this Exception ex)
    {
        return ExceptionFormatter!.FormatException(ex);
    }
}