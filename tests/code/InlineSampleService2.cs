#nullable enable

using System;


namespace PsFolderDiff.FileHashLookupLib.UnitTests;

public class InlineSampleService2
{
    public void Execute(Action<string>? progress)
    {
        var defaultAction = new Action<string>(str => Console.WriteLine($"Reporting: {str}"));

        var worker = new MyWorker((progress ?? defaultAction));
        worker.DoWork();
    }

    private class MyWorker
    {
        private readonly IProgress<string> _progress;

        public MyWorker(Action<string> progress)
        {
            _progress = new Progress<string>((progress));
        }

        public void DoWork()
        {
            _progress.Report("Doing work");
        }
    }
}