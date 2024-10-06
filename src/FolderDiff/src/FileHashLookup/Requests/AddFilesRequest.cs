using System.IO.Abstractions;
using MediatR;
using PsFolderDiff.FileHashLookup.Domain;

namespace PsFolderDiff.FileHashLookup.Requests;

public class AddFilesRequest : IRequest
{
    public IFileInfo[] Files { get; set; } = Array.Empty<IFileInfo>();
    public BasicFileInfo[] BasicFiles { get; set; } = Array.Empty<BasicFileInfo>();
}