using System.IO.Abstractions;

namespace PsFolderDiff.FileHashLookupLib.Services.Interfaces;

public interface IFileHashCalculationService
{
    IEnumerable<(IFileInfo File, string Hash)> CalculateHash(List<IFileInfo> files);
}