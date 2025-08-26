using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SpringOnion.Services.Security;

/// <summary>
/// AES-GCM 256bit: output = nonce(12) + ciphertext + tag(16)
/// </summary>
public class CryptoService
{
    public byte[] EncryptAesGcm(byte[] plaintext, byte[] key)
    {
        if (key is null || key.Length != 32)
            throw new ArgumentException("Key must be 32 bytes (256-bit).", nameof(key));

        var nonce = new byte[12];
        RandomNumberGenerator.Fill(nonce);

        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[16];

        using var aes = new AesGcm(key);
        aes.Encrypt(nonce, plaintext, ciphertext, tag);

        var output = new byte[12 + ciphertext.Length + 16];
        Buffer.BlockCopy(nonce, 0, output, 0, 12);
        Buffer.BlockCopy(ciphertext, 0, output, 12, ciphertext.Length);
        Buffer.BlockCopy(tag, 0, output, 12 + ciphertext.Length, 16);
        return output;
    }

    public byte[] DecryptAesGcm(byte[] sealedBytes, byte[] key)
    {
        if (key is null || key.Length != 32)
            throw new ArgumentException("Key must be 32 bytes (256-bit).", nameof(key));
        if (sealedBytes.Length < 12 + 16)
            throw new ArgumentException("Invalid ciphertext.", nameof(sealedBytes));

        var nonce = new byte[12];
        Buffer.BlockCopy(sealedBytes, 0, nonce, 0, 12);

        var tag = new byte[16];
        Buffer.BlockCopy(sealedBytes, sealedBytes.Length - 16, tag, 0, 16);

        var ciphertextLen = sealedBytes.Length - 12 - 16;
        var ciphertext = new byte[ciphertextLen];
        Buffer.BlockCopy(sealedBytes, 12, ciphertext, 0, ciphertextLen);

        var plaintext = new byte[ciphertextLen];
        using var aes = new AesGcm(key);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);
        return plaintext;
    }
}