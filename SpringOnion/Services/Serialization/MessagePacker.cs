using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpringOnion.Contracts;
using SpringOnion.Services.Security;

namespace SpringOnion.Services.Serialization;

public class MessagePacker
{
    private readonly CryptoService _crypto;

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

        var msgpack = MessagePackSerializer.Serialize(wire);
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
        return MessagePackSerializer.Deserialize<WireMessage>(msgpack);
    }
}
