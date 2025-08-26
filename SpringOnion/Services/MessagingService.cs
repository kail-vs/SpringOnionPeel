using SpringOnion.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpringOnion.Contracts;
using SpringOnion.Services.Security;
using SpringOnion.Services.Serialization;

namespace SpringOnion.Services;

public class MessagingService
{
    private readonly AuthenticationService _auth;
    private readonly IConversationRepository _convos;
    private readonly IMessageRepository _messages;
    private readonly MessagePacker _packer;

    private const string AppSecret = "SPRINGONION_DEV_SECRET_CHANGE_ME";

    public MessagingService(
        AuthenticationService auth,
        IConversationRepository convos,
        IMessageRepository messages,
        MessagePacker packer)
    {
        _auth = auth;
        _convos = convos;
        _messages = messages;
        _packer = packer;
    }

    public async Task<(bool ok, string message)> SendTextDirectAsync(string toUserId, string text, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_auth.UserId))
            return (false, "Not authenticated.");

        var me = _auth.UserId!;
        var convo = await _convos.EnsureDirectAsync(me, toUserId, ct);

        var plain = new PlainMessage
        {
            Text = text,
            ContentType = "text/plain"
        };

        var key = KeyDerivation.DeriveConversationKey(AppSecret, me, toUserId);
        var base64Cipher = _packer.PackToBase64(plain, key);

        await _messages.AddLocalOutgoingAsync(convo.ConversationId, me, base64Cipher, plain.ContentType, ct);

        return (true, "Message queued locally.");
    }

    public async Task<(bool ok, string message)> SendAttachmentDirectAsync(string toUserId, string filePath, string? contentType = null, CancellationToken ct = default)
    {
        if (!File.Exists(filePath))
            return (false, "File not found.");
        var len = new FileInfo(filePath).Length;
        if (len > 10 * 1024 * 1024)
            return (false, "File too large (max 10 MB).");

        var data = await File.ReadAllBytesAsync(filePath, ct);
        var attachment = new WireAttachment
        {
            FileName = Path.GetFileName(filePath),
            ContentType = contentType,
            Data = data
        };

        var plain = new PlainMessage
        {
            ContentType = "application/octet-stream",
            Attachments = new List<WireAttachment> { attachment },
            Meta = new Dictionary<string, string> { ["kind"] = "attachment-only" }
        };

        var me = _auth.UserId!;
        var convo = await _convos.EnsureDirectAsync(me, toUserId, ct);
        var key = KeyDerivation.DeriveConversationKey(AppSecret, me, toUserId);
        var base64Cipher = _packer.PackToBase64(plain, key);

        await _messages.AddLocalOutgoingAsync(convo.ConversationId, me, base64Cipher, plain.ContentType, ct);

        return (true, "Attachment queued locally.");
    }
}
