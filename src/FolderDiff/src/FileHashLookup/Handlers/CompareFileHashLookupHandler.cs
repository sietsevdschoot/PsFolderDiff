using MediatR;
using PsFolderDiff.FileHashLookup.Domain;
using PsFolderDiff.FileHashLookup.Requests;

namespace PsFolderDiff.FileHashLookup.Handlers;

public class CompareFileHashLookupHandler : IRequestHandler<CompareFileHashLookupRequest, CompareFileHashLookupResult>
{
    private FileHashLookupState _fileHashLookupState;

    public CompareFileHashLookupHandler(FileHashLookupState fileHashLookupState)
    {
        _fileHashLookupState = fileHashLookupState;
    }

    public async Task<CompareFileHashLookupResult> Handle(CompareFileHashLookupRequest request, CancellationToken cancellationToken)
    {
        var differencesLookup = Services.FileHashLookup.Create();
        var matchesLookup = Services.FileHashLookup.Create();

        var otherFiles = request.FileHashLookup.GetFiles();

        for (var i = 0; i < otherFiles.Count; i++)
        {
            var currentFile = otherFiles[i];

            if (_fileHashLookupState.Contains(currentFile))
            {
                await matchesLookup.AddFile(currentFile, cancellationToken);
            }
            else
            {
                await differencesLookup.AddFile(currentFile, cancellationToken);
            }
        }

        return new CompareFileHashLookupResult
        {
            MatchesInOther = matchesLookup,
            DifferencesInOther = differencesLookup,
        };
    }
}