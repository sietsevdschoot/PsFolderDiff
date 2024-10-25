#nullable enable

using System;
using System.Threading;
using PsFolderDiff.FileHashLookupLib.Domain;

namespace PsFolderDiff.FileHashLookupLib.UnitTests;

public class InlineSampleService1
{
    public void Execute(Action<ProgressEventArgs>? progress)
    {
        var worker = new MyWorker(progress);
        worker.DoWork();
    }

    private class MyWorker
    {
        private readonly IProgress<ProgressEventArgs> _progress;

        public MyWorker(Action<ProgressEventArgs>? progress)
        {
            var defaultAction = new Action<ProgressEventArgs>(progressEventArgs => System.Console.WriteLine($"Reporting: {progressEventArgs.CurrentOperation}"));
            
            _progress = new Progress<ProgressEventArgs>(progress ?? defaultAction);
        }

        public void DoWork()
        {
            _progress.Report(new ProgressEventArgs("Working", "Doing work"));
        }
    }
}