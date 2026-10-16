using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace NovaLive.Infrastructure.Livestreams.Agora.DynamicKey;

public static partial class DynamicKeyUtil
{
    [GeneratedRegex("^[0-9a-fA-F]{32}$")]
    private static partial Regex Hex32Regex();

    public static byte[] EncodeHmacSha256(byte[] keyBytes, byte[] textBytes)
    {
        using var hmac = new HMACSHA256(keyBytes);
        return hmac.ComputeHash(textBytes);
    }

    public static byte[] EncodeHmacSha256(string key, byte[] message)
    {
        return EncodeHmacSha256(Encoding.UTF8.GetBytes(key), message);
    }

    public static bool IsHex32(string? value)
    {
        if (string.IsNullOrEmpty(value) || value.Length != 32)
        {
            return false;
        }

        return Hex32Regex().IsMatch(value);
    }

    public static byte[] Compress(byte[] data)
    {
        using var outputStream = new MemoryStream();
        using (var zlibStream = new ZLibStream(outputStream, CompressionLevel.Optimal, leaveOpen: true))
        {
            zlibStream.Write(data, 0, data.Length);
        }
        return outputStream.ToArray();
    }

    public static byte[] Decompress(byte[] data)
    {
        using var inputStream = new MemoryStream(data);
        using var zlibStream = new ZLibStream(inputStream, CompressionMode.Decompress);
        using var outputStream = new MemoryStream();
        zlibStream.CopyTo(outputStream);
        return outputStream.ToArray();
    }
}
