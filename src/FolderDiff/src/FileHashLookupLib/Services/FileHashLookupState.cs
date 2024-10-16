using System.Collections.ObjectModel;
using System.IO.Abstractions;
using PsFolderDiff.FileHashLookupLib.Domain;
using PsFolderDiff.FileHashLookupLib.Models;
using PsFolderDiff.FileHashLookupLib.Services.Interfaces;
using PsFolderDiff.FileHashLookupLib.Utils;

namespace PsFolderDiff.FileHashLookupLib.Services;

public class FileHashLookupState : IHasReadonlyLookups, IFileHashLookupState
{
    private readonly Dictionary<string, BasicFileInfo> _fileLookup = new(StringComparer.InvariantCultureIgnoreCase);
    private readonly Dictionary<string, List<BasicFileInfo>> _hashLookup = new(StringComparer.InvariantCultureIgnoreCase);
    private readonly IPeriodicalProgressReporter<ProgressEventArgs> _progress;

    public FileHashLookupState(IPeriodicalProgressReporter<ProgressEventArgs> progress)
    {
        _progress = progress;
    }

    public IReadOnlyDictionary<string, BasicFileInfo> File => _fileLookup.AsReadOnly();

    public ReadOnlyDictionary<string, ReadOnlyCollection<BasicFileInfo>> Hash => _hashLookup.AsReadOnly()
        .ToDictionary(k => k.Key, v => v.Value.AsReadOnly()).AsReadOnly();

    public void Add(BasicFileInfo file)
    {
        ArgumentNullException.ThrowIfNull(file, nameof(file));

        if (_fileLookup.TryGetValue(file.FullName, out var existingFile))
        {
            Remove(existingFile);
        }

        _fileLookup[file.FullName] = file;

        var items = _hashLookup.TryGetValue(file.Hash, out var existingItems)
            ? existingItems.Concat([file]).ToList()
            : [file];

        _hashLookup[file.Hash] = items;
    }

    public void Remove(BasicFileInfo file)
    {
        if (_fileLookup.ContainsKey(file.FullName))
        {
            _fileLookup.Remove(file.FullName);

            var entriesWithSameHash = _hashLookup[file.Hash];

            entriesWithSameHash.Remove(file);

            if (!entriesWithSameHash.Any())
            {
                _hashLookup.Remove(file.Hash);
            }
        }
    }

    public void Remove(IFileInfo file)
    {
        if (_fileLookup.TryGetValue(file.FullName, out var basicFileInfo))
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
        if (_fileLookup.TryGetValue(file.FullName, out var foundFile))
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
        if (_fileLookup.TryGetValue(file.FullName, out var foundFile))
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
        return File.Values.ToList();
    }
}