using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto.Digests;

namespace Stringy.Core;

/// <summary>Hex-encoded cryptographic hashes of UTF-8 input. Output matches js-sha256/js-sha512/js-md5.</summary>
public static class Hasher
{
    public record Option(string Value, string Label);

    public static readonly IReadOnlyList<Option> Options =
    [
        new("sha256", "SHA-256"),
        new("sha512", "SHA-512"),
        new("md5", "MD5"),
    ];

    public static string Compute(string input, string algorithm)
    {
        var bytes = Encoding.UTF8.GetBytes(input ?? string.Empty);
        var hash = algorithm switch
        {
            "sha256" => SHA256.HashData(bytes),
            "sha512" => SHA512.HashData(bytes),
            "md5" => Md5(bytes),
            _ => throw new ArgumentException($"Unsupported hash algorithm: {algorithm}"),
        };
        return Convert.ToHexStringLower(hash);
    }

    // MD5 via BouncyCastle: the framework's MD5 is unavailable in the browser/WASM sandbox.
    private static byte[] Md5(byte[] bytes)
    {
        var digest = new MD5Digest();
        digest.BlockUpdate(bytes, 0, bytes.Length);
        var result = new byte[digest.GetDigestSize()];
        digest.DoFinal(result, 0);
        return result;
    }
}
