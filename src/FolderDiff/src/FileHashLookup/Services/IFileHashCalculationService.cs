using System.IO.Abstractions;

namespace PsFolderDiff.FileHashLookup.Services;

public interface IFileHashCalculationService
{
    IEnumerable<(IFileInfo File, string Hash)> CalculateHash(List<IFileInfo> files);
}