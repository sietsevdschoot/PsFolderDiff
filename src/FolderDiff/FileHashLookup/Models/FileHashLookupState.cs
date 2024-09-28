﻿using System.Collections.ObjectModel;
using System.IO.Abstractions;

namespace PsFolderDiff.FileHashLookup.Models;

public class FileHashLookupState
{
    private readonly Dictionary<string, BasicFileInfo> _fileLookup = new();
    private readonly Dictionary<string, List<BasicFileInfo>> _hashLookup = new();

    public IReadOnlyDictionary<string, BasicFileInfo> File => new ReadOnlyDictionary<string, BasicFileInfo>(_fileLookup);

    public IReadOnlyDictionary<string, IReadOnlyCollection<BasicFileInfo>> Hash => new ReadOnlyDictionary<string, IReadOnlyCollection<BasicFileInfo>>(
        _hashLookup.ToDictionary(k => k.Key, v => (IReadOnlyCollection<BasicFileInfo>)v.Value));


    public void Add(BasicFileInfo file)
    {
        ArgumentNullException.ThrowIfNull(file, nameof(file));

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
}