using System.IO.Abstractions;
using MediatR;

namespace PsFolderDiff.FileHashLookup.Requests;

public class AddFilesRequest : IRequest
{
    public IFileInfo[] Files { get; set; }
}