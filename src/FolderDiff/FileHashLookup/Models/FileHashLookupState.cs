using System.Collections.ObjectModel;
using System.IO.Abstractions;

namespace PsFolderDiff.FileHashLookup.Models;

public class FileHashLookupState
{
    private readonly Dictionary<string, BasicFileInfo> _fileLookup = new();
    private readonly Dictionary<string, List<BasicFileInfo>> _hashLookup = new();

    public IReadOnlyDictionary<string, BasicFileInfo> File => _fileLookup;

    public IReadOnlyDictionary<string, IReadOnlyCollection<BasicFileInfo>> Hash => _hashLookup
        .ToDictionary(k => k.Key, v => (IReadOnlyCollection<BasicFileInfo>)v.Value)
        .AsReadOnly();

    public void Add(IFileInfo file, string hash)
    {
        ArgumentNullException.ThrowIfNull(file, nameof(file));
        ArgumentNullException.ThrowIfNull(hash, nameof(hash));

        Add(new BasicFileInfo(file, hash));
    }

    public void Add(BasicFileInfo file)
    {
        ArgumentNullException.ThrowIfNull(file, nameof(file));

        _fileLookup[file.FullName] = file;

        var items = _hashLookup.TryGetValue(file.Hash, out var existingItems)
            ? existingItems.Concat([file]).ToList()
            : [file];


        _hashLookup[file.Hash] = items;
    }
}