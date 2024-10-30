using System.Collections.ObjectModel;
using System.IO.Abstractions;
using PsFolderDiff.FileHashLookupLib.Domain;
using PsFolderDiff.FileHashLookupLib.Domain.Interfaces;
using PsFolderDiff.FileHashLookupLib.Services.Interfaces;
using PsFolderDiff.FileHashLookupLib.Utils.Interfaces;

namespace PsFolderDiff.FileHashLookupLib.Services;

public class FileHashLookupState : IFileHashLookupState, IHasReadonlyLookups
{
    private readonly IPeriodicalProgressReporter<ProgressEventArgs> _progress;
    private readonly ISupportFileHashLookups _storageModel;

    public FileHashLookupState(
        ISupportFileHashLookups storageModel,
        IPeriodicalProgressReporter<ProgressEventArgs> progress)
    {
        _storageModel = storageModel;
        _progress = progress;
    }

    public IReadOnlyDictionary<string, BasicFileInfo> File => _storageModel.File.AsReadOnly();

    public ReadOnlyDictionary<string, ReadOnlyCollection<BasicFileInfo>> Hash => _storageModel.Hash.AsReadOnly()
        .ToDictionary(k => k.Key, v => v.Value.AsReadOnly()).AsReadOnly();

    public void Add(BasicFileInfo file)
    {
        ArgumentNullException.ThrowIfNull(file, nameof(file));

        if (_storageModel.File.TryGetValue(file.FullName, out var existingFile))
        {
            Remove(existingFile);
        }

        _storageModel.File[file.FullName] = file;

        var items = _storageModel.Hash.TryGetValue(file.Hash, out var existingItems)
            ? existingItems.Concat([file]).ToList()
            : [file];

        _storageModel.Hash[file.Hash] = items;
    }

    public void Remove(BasicFileInfo file)
    {
        if (_storageModel.File.ContainsKey(file.FullName))
        {
            _storageModel.File.Remove(file.FullName);

            var entriesWithSameHash = _storageModel.Hash[file.Hash];

            entriesWithSameHash.Remove(file);

            if (!entriesWithSameHash.Any())
            {
                _storageModel.Hash.Remove(file.Hash);
            }
        }
    }

    public void Remove(IFileInfo file)
    {
        if (_storageModel.File.TryGetValue(file.FullName, out var basicFileInfo))
        {
            Remove(basicFileInfo);
        }
    }

    public void AddFileHashLookup(FileHashLookup other)
    {
        _progress.Report(() => new ProgressEventArgs(
            activity: "Adding FileHashLookup",
            currentOperation: "Collecting files"));

        var allOtherFiles = other.GetFiles();

        for (var i = 0; i < allOtherFiles.Count; i++)
        {
            var file = allOtherFiles[i];

            _progress.Report(
                progress => new ProgressEventArgs(
                    activity: "Adding FileHashLookup",
                    currentOperation: "Adding files.",
                    currentItem: file.FullName,
                    currentProgress: progress,
                    total: allOtherFiles.Count),
                currentProgress: i);

            Add(file);
        }
    }

    public FileContainsState Contains(BasicFileInfo file)
    {
        if (_storageModel.File.TryGetValue(file.FullName, out var foundFile))
        {
            return foundFile.CreationTime == file.CreationTime
                   && foundFile.Length == file.Length
                   && foundFile.Hash == file.Hash
                ? FileContainsState.Match
                : FileContainsState.Modified;
        }
        else
        {
            return FileContainsState.NoMatch;
        }
    }

    public FileContainsState Contains(IFileInfo file)
    {
        if (_storageModel.File.TryGetValue(file.FullName, out var foundFile))
        {
            return foundFile.CreationTime == file.CreationTime
                   && foundFile.Length == file.Length
                ? FileContainsState.Match
                : FileContainsState.Modified;
        }
        else
        {
            return FileContainsState.NoMatch;
        }
    }

    public List<BasicFileInfo> GetFiles()
    {
        return _storageModel.File.Values.ToList();
    }
}