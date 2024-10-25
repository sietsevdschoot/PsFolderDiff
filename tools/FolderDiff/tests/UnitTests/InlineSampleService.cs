namespace PsFolderDiff.FileHashLookupLib.UnitTests;

public class InlineSampleService2
{
    public void Execute(Action<string>? progress)
    {
        var worker = new MyWorker(progress);
        worker.DoWork();
    }

    private class MyWorker
    {
        private readonly IProgress<string> _progress;

        public MyWorker(Action<string>? progress)
        {
            var defaultAction = new Action<string>(str => Console.WriteLine($"Reporting: {str}"));

            _progress = new Progress<string>((progress ?? defaultAction));
        }

        public void DoWork()
        {
            _progress.Report("Doing work");
        }
    }
}