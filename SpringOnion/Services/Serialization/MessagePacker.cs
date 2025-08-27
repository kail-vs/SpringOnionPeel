using MessagePack;
using MessagePack.Resolvers;
using SpringOnion.Contracts;
using SpringOnion.Services.Security;
using System;

namespace SpringOnion.Services.Serialization;

public class MessagePacker
{
    private readonly CryptoService _crypto;

    private static readonly MessagePackSerializerOptions _options =
        MessagePackSerializerOptions.Standard.WithResolver(ContractlessStandardResolver.Instance);

    public MessagePacker(CryptoService crypto)
    {
        _crypto = crypto;
    }

    /// <summary>
    /// PlainMessage -> MessagePack -> gzip -> AES-GCM -> base64
    /// </summary>
    public string PackToBase64(PlainMessage plain, byte[] key)
    {
        var wire = new WireMessage
        {
            SchemaVersion = 1,
            Text = plain.Text,
            ContentType = plain.ContentType,
            Attachments = plain.Attachments,
            Meta = plain.Meta
        };

        var msgpack = MessagePackSerializer.Serialize(wire, _options);
        var compressed = CompressionService.GzipCompress(msgpack);
        var sealedBytes = _crypto.EncryptAesGcm(compressed, key);
        return Convert.ToBase64String(sealedBytes);
    }

    /// <summary>
    /// base64 -> AES-GCM -> gunzip -> MessagePack -> WireMessage
    /// </summary>
    public WireMessage UnpackFromBase64(string base64Cipher, byte[] key)
    {
        var sealedBytes = Convert.FromBase64String(base64Cipher);
        var compressed = _crypto.DecryptAesGcm(sealedBytes, key);
        var msgpack = CompressionService.GzipDecompress(compressed);
        return MessagePackSerializer.Deserialize<WireMessage>(msgpack, _options);
    }
}
