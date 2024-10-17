using System.IO.Abstractions;
using System.Text;
using Newtonsoft.Json;
using PsFolderDiff.FileHashLookupLib.Utils;

namespace PsFolderDiff.FileHashLookupLib.Services;

public class PersistenceService : IPersistenceService
{
    private readonly IFileSystem _fileSystem;

    public PersistenceService(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public string? SavedAsFile { get; private set; }

    public static FileHashLookup LoadFileHashLookup(string filename)
    {
        throw new NotImplementedException();
    }

    public void Save(FileHashLookup fileHashLookup, string? path)
    {
        var json = JsonConvert.SerializeObject(fileHashLookup, new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
        });

        if (string.IsNullOrEmpty(path))
        {
            var directory = fileHashLookup.IncludePatterns.FirstOrDefault(x => !string.IsNullOrEmpty(x.Directory)).Directory;

            var filename = !string.IsNullOrEmpty(directory)
                ? PathUtils.CreateFilenameFromPath(_fileSystem.DirectoryInfo.New(directory))
                : _fileSystem.Path.GetTempFileName();

            SavedAsFile = _fileSystem.Path.Combine(_fileSystem.Directory.GetCurrentDirectory(), filename);
        }
        else
        {
            SavedAsFile = _fileSystem.Path.IsPathRooted(path)
                ? _fileSystem.Path.GetFullPath(path)
                : _fileSystem.Path.Combine(_fileSystem.Directory.GetCurrentDirectory(), path);
        }

        _fileSystem.File.WriteAllText(SavedAsFile, json, Encoding.UTF8);
    }
}