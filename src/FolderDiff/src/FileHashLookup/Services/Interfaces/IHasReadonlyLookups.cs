using System.Collections.ObjectModel;
using PsFolderDiff.FileHashLookup.Domain;

namespace PsFolderDiff.FileHashLookup.Services.Interfaces;

public interface IHasReadonlyLookups
{
    IReadOnlyDictionary<string, BasicFileInfo> File { get; }

    ReadOnlyDictionary<string, ReadOnlyCollection<BasicFileInfo>> Hash { get; }
}