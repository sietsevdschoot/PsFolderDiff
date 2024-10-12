using System.Collections.ObjectModel;
using PsFolderDiff.FileHashLookupLib.Domain;

namespace PsFolderDiff.FileHashLookupLib.Services.Interfaces;

public interface IHasReadonlyLookups
{
    IReadOnlyDictionary<string, BasicFileInfo> File { get; }

    ReadOnlyDictionary<string, ReadOnlyCollection<BasicFileInfo>> Hash { get; }
}