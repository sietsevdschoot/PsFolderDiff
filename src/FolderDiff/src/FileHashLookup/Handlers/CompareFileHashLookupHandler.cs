using System.IO.Abstractions;
using MediatR;
using PsFolderDiff.FileHashLookup.Domain;
using PsFolderDiff.FileHashLookup.Requests;
using PsFolderDiff.FileHashLookup.Services;

namespace PsFolderDiff.FileHashLookup.Handlers;

public class CompareFileHashLookupHandler : IRequestHandler<CompareFileHashLookupRequest, CompareFileHashLookupResult>
{
    private readonly FileCollector _fileCollector;

    public CompareFileHashLookupHandler(FileCollector fileCollector)
    {
        _fileCollector = fileCollector;
    }

    public Task<CompareFileHashLookupResult> Handle(CompareFileHashLookupRequest request, CancellationToken cancellationToken)
    {
        var differencesLookup = Services.FileHashLookup.Create();
        var matchesLookup = Services.FileHashLookup.Create();

        var otherFiles = request.FileHashLookup.GetFiles();

        for (var i = 0; i < otherFiles.Count; i++)
        {
            var currentFile = otherFiles[i];

            if (_fileCollector.Contains(currentFile))
            {
                matchesLookup.AddFile(currentFile);
            }
            else
            {
                differencesLookup.AddFile(currentFile);

            }
        }

        return  new Task<CompareFileHashLookupResult>();
    }
}