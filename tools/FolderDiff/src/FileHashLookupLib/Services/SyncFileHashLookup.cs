using System.Collections.ObjectModel;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using PsFolderDiff.FileHashLookupLib.Configuration;
using PsFolderDiff.FileHashLookupLib.Domain;

namespace PsFolderDiff.FileHashLookupLib.Services;

public class SyncFileHashLookup
{
    private readonly FileHashLookup _fileHashLookup;
    private readonly CancellationTokenSource _cts;

    private SyncFileHashLookup(
        FileHashLookup fileHashLookup,
        CancellationTokenSource cts)
    {
        _fileHashLookup = fileHashLookup;
        _cts = cts;
    }

    public IReadOnlyDictionary<string, BasicFileInfo> File => _fileHashLookup.File;

    public ReadOnlyDictionary<string, ReadOnlyCollection<BasicFileInfo>> Hash => _fileHashLookup.Hash;

    public IReadOnlyCollection<string> IncludePatterns => _fileHashLookup.IncludePatterns;

    public IReadOnlyCollection<string> ExcludePatterns => _fileHashLookup.ExcludePatterns;

    public string SavedAsFile => _fileHashLookup.SavedAsFile;

    public DateTime LastUpdated => _fileHashLookup.LastUpdated;

    public FileHashLookup FileHashLookup => _fileHashLookup;

    public static SyncFileHashLookup Create() => Create(FileHashLookupSettings.Default);

    public static SyncFileHashLookup Create(FileHashLookupSettings settings)
    {
        var provider = FileHashLookup.Create(new ServiceCollection(), settings);

        return new SyncFileHashLookup(provider.FileHashLookup, settings.CancellationTokenSource);
    }

    public static SyncFileHashLookup Load(string path)
    {
        return Load(path, FileHashLookupSettings.Default);
    }

    public static SyncFileHashLookup Load(string path, FileHashLookupSettings settings)
    {
        return new SyncFileHashLookup(PersistenceService.LoadFileHashLookup(path, settings), settings.CancellationTokenSource);
    }

    public void Save(string? path = null)
    {
        _fileHashLookup.Save(path ?? _fileHashLookup.SavedAsFile);
    }

    public void Include(string includeFolderOrPattern)
    {
        _fileHashLookup.IncludeAsync(includeFolderOrPattern, _cts.Token).GetAwaiter().GetResult();
    }

    public void Exclude(string excludeFolderOrPattern)
    {
        _fileHashLookup.ExcludeAsync(excludeFolderOrPattern, _cts.Token).GetAwaiter().GetResult();
    }

    public List<BasicFileInfo> GetFiles()
    {
        return _fileHashLookup.GetFiles();
    }

    public void AddFile(IFileInfo file)
    {
        _fileHashLookup.AddFileAsync(file).GetAwaiter().GetResult();
    }

    public void AddFile(BasicFileInfo file)
    {
        _fileHashLookup.AddFileAsync(file).GetAwaiter().GetResult();
    }

    public void AddFiles(IFileInfo[] files)
    {
        _fileHashLookup.AddFilesAsync(files).GetAwaiter().GetResult();
    }

    public void AddFileHashLookup(SyncFileHashLookup other)
    {
        _fileHashLookup.AddFileHashLookupAsync(other.FileHashLookup, _cts.Token).GetAwaiter().GetResult();
    }

    public void Refresh()
    {
        _fileHashLookup.RefreshAsync(_cts.Token).GetAwaiter().GetResult();
    }

    public SyncFileHashLookup GetDifferencesInOther(SyncFileHashLookup other)
    {
        var differencesInOther = _fileHashLookup.GetDifferencesInOtherAsync(other.FileHashLookup, _cts.Token).GetAwaiter().GetResult();

        return new SyncFileHashLookup(differencesInOther, _cts);
    }

    public SyncFileHashLookup GetMatchesInOther(SyncFileHashLookup other)
    {
        var getMatchesInOther = _fileHashLookup.GetMatchesInOtherAsync(other.FileHashLookup, _cts.Token).GetAwaiter().GetResult();

        return new SyncFileHashLookup(getMatchesInOther, _cts);
    }

    public override string ToString() => _fileHashLookup.ToString();
}