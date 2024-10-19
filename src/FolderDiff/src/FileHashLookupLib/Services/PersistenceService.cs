using System.IO.Abstractions;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PsFolderDiff.FileHashLookupLib.Configuration;
using PsFolderDiff.FileHashLookupLib.Domain;
using PsFolderDiff.FileHashLookupLib.Models;
using PsFolderDiff.FileHashLookupLib.Services.Interfaces;
using PsFolderDiff.FileHashLookupLib.Utils;

namespace PsFolderDiff.FileHashLookupLib.Services;

public class PersistenceService : IPersistenceService
{
    private readonly StorageModel _storageModel;
    private readonly IFileSystem _fileSystem;
    private readonly IPeriodicalProgressReporter<ProgressEventArgs> _progress;

    public PersistenceService(
        StorageModel storageModel,
        IFileSystem fileSystem,
        IPeriodicalProgressReporter<ProgressEventArgs> progress)
    {
        _storageModel = storageModel;
        _fileSystem = fileSystem;
        _progress = progress;
    }

    public string SavedAsFile => _storageModel.SavedAsFile;

    public static FileHashLookup LoadFileHashLookup(string path, FileHashLookupSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));
        if (!settings.FileSystem.File.Exists(path))
        {
            throw new ArgumentException($"Can't find '{path}'", nameof(path));
        }

        var json = settings.FileSystem.File.ReadAllText(path);
        var storageModel = JsonConvert.DeserializeObject<StorageModel>(json, new JsonSerializerSettings
        {
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
        });

        if (storageModel == null)
        {
            throw new InvalidOperationException($"Unable to deserialize '{path}'");
        }

        settings.ConfigureServices.Add((services, _) => services.AddSingleton(storageModel));

        return FileHashLookup.Create(settings);
    }

    public void Save(FileHashLookup fileHashLookup, string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            var directory = fileHashLookup.IncludePatterns.FirstOrDefault(x => !string.IsNullOrEmpty(x.Directory)).Directory;

            var filename = !string.IsNullOrEmpty(directory)
                ? PathUtils.CreateFilenameFromPath(_fileSystem.DirectoryInfo.New(directory))
                : _fileSystem.Path.GetTempFileName();

            _storageModel.SavedAsFile = _fileSystem.Path.Combine(_fileSystem.Directory.GetCurrentDirectory(), filename);
        }
        else
        {
            _storageModel.SavedAsFile = _fileSystem.Path.IsPathRooted(path)
                ? _fileSystem.Path.GetFullPath(path)
                : _fileSystem.Path.Combine(_fileSystem.Directory.GetCurrentDirectory(), path);
        }

        _progress.Report(() => new ProgressEventArgs(
            activity: "Saving FileHashLookup.",
            currentOperation: "Serializing FileHashLookup."));

        var json = JsonConvert.SerializeObject(_storageModel, new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
        });

        _fileSystem.File.WriteAllText(_storageModel.SavedAsFile, json, Encoding.UTF8);

        _progress.Report(() => new ProgressEventArgs(
            activity: "Saving FileHashLookup.",
            currentOperation: "Finished saving FileHashLookup."));
    }
}