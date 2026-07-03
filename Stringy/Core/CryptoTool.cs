using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;

namespace Stringy.Core;

/// <summary>
/// AES-256-CBC encryption with PBKDF2 (HMAC-SHA1, 10,000 iterations) key derivation.
/// Wire format: Base64( salt[16] | iv[16] | ciphertext ).
/// Uses BouncyCastle so it runs inside the browser/WASM sandbox, and stays byte-for-byte
/// interoperable with the original CryptoJS implementation.
/// </summary>
public static class CryptoTool
{
    private const int SaltSize = 16;   // 128 bits
    private const int IvSize = 16;     // 128 bits
    private const int KeyBits = 256;   // AES-256
    private const int Iterations = 10_000;

    public static string Encrypt(string text, string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var iv = RandomNumberGenerator.GetBytes(IvSize);
        var cipherBytes = RunCipher(true, Encoding.UTF8.GetBytes(text), DeriveKey(password, salt), iv);

        var combined = new byte[salt.Length + iv.Length + cipherBytes.Length];
        Buffer.BlockCopy(salt, 0, combined, 0, salt.Length);
        Buffer.BlockCopy(iv, 0, combined, salt.Length, iv.Length);
        Buffer.BlockCopy(cipherBytes, 0, combined, salt.Length + iv.Length, cipherBytes.Length);
        return Convert.ToBase64String(combined);
    }

    public static string Decrypt(string ciphertext, string password)
    {
        var combined = Convert.FromBase64String(ciphertext.Trim());
        if (combined.Length < SaltSize + IvSize)
            throw new FormatException("Ciphertext is too short.");

        var salt = combined[..SaltSize];
        var iv = combined[SaltSize..(SaltSize + IvSize)];
        var cipher = combined[(SaltSize + IvSize)..];

        var plain = RunCipher(false, cipher, DeriveKey(password, salt), iv);
        return Encoding.UTF8.GetString(plain);
    }

    private static byte[] DeriveKey(string password, byte[] salt)
    {
        var generator = new Pkcs5S2ParametersGenerator(new Sha1Digest());
        generator.Init(Encoding.UTF8.GetBytes(password), salt, Iterations);
        return ((KeyParameter)generator.GenerateDerivedMacParameters(KeyBits)).GetKey();
    }

    private static byte[] RunCipher(bool encrypt, byte[] input, byte[] key, byte[] iv)
    {
        var cipher = new PaddedBufferedBlockCipher(new CbcBlockCipher(new AesEngine()), new Pkcs7Padding());
        cipher.Init(encrypt, new ParametersWithIV(new KeyParameter(key), iv));

        var output = new byte[cipher.GetOutputSize(input.Length)];
        int written = cipher.ProcessBytes(input, 0, input.Length, output, 0);
        written += cipher.DoFinal(output, written);

        if (written == output.Length) return output;
        var trimmed = new byte[written];
        Buffer.BlockCopy(output, 0, trimmed, 0, written);
        return trimmed;
    }
}
