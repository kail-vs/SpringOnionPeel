using System.IO;
using System.IO.Compression;

namespace SpringOnion.Services.Security;

public static class CompressionService
{
    public static byte[] GzipCompress(byte[] input)
    {
        using var ms = new MemoryStream();
        using (var gz = new GZipStream(ms, CompressionLevel.SmallestSize, leaveOpen: true))
        {
            gz.Write(input, 0, input.Length);
        }
        return ms.ToArray();
    }

    public static byte[] GzipDecompress(byte[] compressed)
    {
        using var input = new MemoryStream(compressed);
        using var gz = new GZipStream(input, CompressionMode.Decompress);
        using var outMs = new MemoryStream();
        gz.CopyTo(outMs);
        return outMs.ToArray();
    }
}
