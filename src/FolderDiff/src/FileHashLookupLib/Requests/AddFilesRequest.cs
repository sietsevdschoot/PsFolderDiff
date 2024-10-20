using System.IO.Abstractions;
using MediatR;
using PsFolderDiff.FileHashLookupLib.Domain;

namespace PsFolderDiff.FileHashLookupLib.Requests;

public class AddFilesRequest : IRequest
{
    public IFileInfo[] Files { get; set; } = Array.Empty<IFileInfo>();

    public BasicFileInfo[] BasicFiles { get; set; } = Array.Empty<BasicFileInfo>();
}