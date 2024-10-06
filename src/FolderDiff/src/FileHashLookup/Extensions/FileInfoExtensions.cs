﻿using System.IO.Abstractions;
using System.Security.Cryptography;

namespace PsFolderDiff.FileHashLookup.Extensions;

public static class FileInfoExtensions
{
    #pragma warning disable SCS0006
    // SCS0006: Weak hashing algorithm: MD5 is used for simple file identification.
    // ReSharper disable once InconsistentNaming
    private static readonly MD5 MD5 = MD5.Create();

    public static byte[] ReadAllBytes(this IFileInfo file)
    {
        using var stream = file.OpenRead();
        using var ms = new MemoryStream();

        stream.CopyTo(ms);

        return ms.ToArray();
    }

    /// <summary>
    /// TODO: Measure buffer optimum for hashing (large) files.
    /// https://github.com/dotnet/BenchmarkDotNet.
    /// https://stackoverflow.com/a/1177744.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static string CalculateMD5Hash(this IFileInfo file)
    {
        // Not sure if BufferedStream should be wrapped in using block
        using var fileStream = file.OpenRead();
        using var bufferedStream = new BufferedStream(fileStream, 1200000);

        return Convert.ToHexString(MD5.ComputeHash(bufferedStream));
    }
}