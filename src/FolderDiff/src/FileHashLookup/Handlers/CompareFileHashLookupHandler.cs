using MediatR;
using PsFolderDiff.FileHashLookup.Domain;
using PsFolderDiff.FileHashLookup.Requests;
using PsFolderDiff.FileHashLookup.Services.Interfaces;

namespace PsFolderDiff.FileHashLookup.Handlers;

public class CompareFileHashLookupHandler : IRequestHandler<CompareFileHashLookupRequest, CompareFileHashLookupResult>
{
    private readonly IFileHashLookupState _fileHashLookupState;

    public CompareFileHashLookupHandler(IFileHashLookupState fileHashLookupState)
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

            if (_fileHashLookupState.Contains(currentFile) == FileContainsState.Match)
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