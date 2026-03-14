using System.Security.Cryptography;
using System.Text;
using AiAgent.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AiAgent.Infrastructure.Security;

public sealed class AesGcmEncryptionService : ISecretEncryptionService
{
    private readonly byte[]? _masterKey;

    public AesGcmEncryptionService(IConfiguration configuration)
    {
        var keyBase64 = configuration["Encryption:MasterKey"];
        if (!string.IsNullOrWhiteSpace(keyBase64))
            _masterKey = Convert.FromBase64String(keyBase64);
    }

    public string Encrypt(string plaintext)
    {
        if (_masterKey is null) return plaintext;

        var nonce = new byte[AesGcm.NonceByteSizes.MaxSize]; // 12 bytes
        var tag = new byte[AesGcm.TagByteSizes.MaxSize];     // 16 bytes
        RandomNumberGenerator.Fill(nonce);

        var plainBytes = Encoding.UTF8.GetBytes(plaintext);
        var cipherBytes = new byte[plainBytes.Length];

        using var aes = new AesGcm(_masterKey, AesGcm.TagByteSizes.MaxSize);
        aes.Encrypt(nonce, plainBytes, cipherBytes, tag);

        // Layout: nonce(12) | tag(16) | ciphertext
        var combined = new byte[nonce.Length + tag.Length + cipherBytes.Length];
        Buffer.BlockCopy(nonce, 0, combined, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, combined, nonce.Length, tag.Length);
        Buffer.BlockCopy(cipherBytes, 0, combined, nonce.Length + tag.Length, cipherBytes.Length);

        return Convert.ToBase64String(combined);
    }

    public string Decrypt(string ciphertext)
    {
        if (_masterKey is null) return ciphertext;

        var combined = Convert.FromBase64String(ciphertext);
        const int nonceSize = 12;
        const int tagSize = 16;

        var nonce = combined[..nonceSize];
        var tag = combined[nonceSize..(nonceSize + tagSize)];
        var cipher = combined[(nonceSize + tagSize)..];
        var plainBytes = new byte[cipher.Length];

        using var aes = new AesGcm(_masterKey, tagSize);
        aes.Decrypt(nonce, cipher, tag, plainBytes);

        return Encoding.UTF8.GetString(plainBytes);
    }
}
