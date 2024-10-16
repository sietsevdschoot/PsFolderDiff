using System.IO.Abstractions;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using PsFolderDiff.FileHashLookupLib.Utils;

namespace PsFolderDiff.FileHashLookupLib.Services;

public class PersistanceService
{
    private readonly IFileSystem _fileSystem;

    public PersistanceService(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public void Save(string? path, FileHashLookup fileHashLookup)
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

            path = _fileSystem.Path.Combine(_fileSystem.Directory.GetCurrentDirectory(), filename);
        }

        _fileSystem.File.WriteAllText(path, json, Encoding.UTF8);
    }

    public FileHashLookup LoadFileHashLookup(string filename)
    {
        throw new NotImplementedException();
    }
}