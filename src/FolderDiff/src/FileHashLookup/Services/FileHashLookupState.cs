﻿using System.Collections.ObjectModel;
using System.IO.Abstractions;
using PsFolderDiff.FileHashLookup.Domain;
using PsFolderDiff.FileHashLookup.Extensions;
using PsFolderDiff.FileHashLookup.Models;
using PsFolderDiff.FileHashLookup.Services.Interfaces;
using PsFolderDiff.FileHashLookup.Utils;

namespace PsFolderDiff.FileHashLookup.Services;

public class FileHashLookupState : IHasReadonlyLookups, IFileHashLookupState
{
    private readonly Dictionary<string, BasicFileInfo> _fileLookup = new();
    private readonly Dictionary<string, List<BasicFileInfo>> _hashLookup = new();
    private IPeriodicalProgressReporter<ProgressEventArgs> _progress;

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
            return foundFile.CreationTime == file.CreationTime && foundFile.Hash == file.Hash
                ? FileContainsState.Match
                : FileContainsState.Modified;
        }
        else
        {
            return FileContainsState.NoMatch;
        }
    }

    public bool Contains(IFileInfo file)
    {
        var basicFileInfo = new BasicFileInfo(file, file.CalculateMD5Hash());

        var fileState = Contains(basicFileInfo);

        return fileState == FileContainsState.Match || fileState == FileContainsState.Modified;
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