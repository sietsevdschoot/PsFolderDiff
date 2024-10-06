using System.Collections.ObjectModel;
using System.IO.Abstractions;

namespace PsFolderDiff.FileHashLookup.Domain;

public class FileHashLookupState
{
    private readonly Dictionary<string, BasicFileInfo> _fileLookup = new();
    private readonly Dictionary<string, List<BasicFileInfo>> _hashLookup = new();

    public IReadOnlyDictionary<string, BasicFileInfo> File =>
        new ReadOnlyDictionary<string, BasicFileInfo>(_fileLookup);

    public IReadOnlyDictionary<string, IReadOnlyCollection<BasicFileInfo>> Hash =>
        new ReadOnlyDictionary<string, IReadOnlyCollection<BasicFileInfo>>(
            _hashLookup.ToDictionary(
                k => k.Key,
                v => (IReadOnlyCollection<BasicFileInfo>)new ReadOnlyCollection<BasicFileInfo>(v.Value)));

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

    public void AddFileHashLookup(Services.FileHashLookup other)
    {
        var allOtherFiles = other.GetFiles();

        for (var i = 0; i < allOtherFiles.Count; i++)
        {
            var file = allOtherFiles[i];
            Add(file);
        }
    }

    public void Remove(IFileInfo file)
    {
        if (_fileLookup.TryGetValue(file.FullName, out var basicFileInfo))
        {
            Remove(basicFileInfo);
        }
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
}