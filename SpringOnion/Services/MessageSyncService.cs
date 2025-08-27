using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SpringOnion.Contracts;
using SpringOnion.Data.Repositories;
using SpringOnion.Services.Realtime;
using SpringOnion.Services.Security;
using SpringOnion.Services.Serialization;

namespace SpringOnion.Services;

public class MessageSyncService
{
    private readonly SignalRRelayClient _relay;
    private readonly IMessageRepository _messages;
    private readonly IConversationRepository _convos;
    private readonly AuthenticationService _auth;
    private readonly MessagePacker _packer;
    private readonly string _appSecret;

    public MessageSyncService(
        SignalRRelayClient relay,
        IMessageRepository messages,
        IConversationRepository convos,
        AuthenticationService auth,
        MessagePacker packer,
        string appSecret)
    {
        _relay = relay;
        _messages = messages;
        _convos = convos;
        _auth = auth;
        _packer = packer;
        _appSecret = appSecret;

        _relay.OnIncomingMessage += HandleIncomingAsync;
        _relay.OnAck += HandleAckAsync;
    }

    public async Task StartAsync(CancellationToken ct = default)
    {
        await _relay.StartAsync(ct);
        _ = FlushOutboxOnceAsync(ct);
    }

    public async Task StopAsync(CancellationToken ct = default)
        => await _relay.StopAsync(ct);

    public async Task FlushOutboxOnceAsync(CancellationToken ct = default)
    {
        var pending = await _messages.GetOutgoingPendingAsync(50, ct);
        if (pending.Count == 0) return;

        foreach (var m in pending)
        {
            try
            {
                var me = _auth.UserId!;
                var convo = await _convos.GetByIdAsync(m.ConversationId, ct);
                if (convo == null) { await _messages.MarkMessageAsFailedAsync(m.MessageId, ct); continue; }
                var toUser = convo.Participants
                                  .Select(p => p.UserId)
                                  .FirstOrDefault(u => !string.Equals(u, me, StringComparison.Ordinal));
                if (string.IsNullOrWhiteSpace(toUser))
                {
                    await _messages.MarkMessageAsFailedAsync(m.MessageId, ct);
                    continue;
                }

                await _messages.MarkMessageAsSentAsync(m.MessageId, null, ct);

                var req = new RelaySendRequest
                {
                    ConversationId = m.ConversationId,
                    ToUserId = toUser,
                    SenderUserId = m.SenderUserId,
                    CipherTextBase64 = m.CipherText!,
                    ContentType = m.ContentType,
                    LocalMessageId = m.MessageId
                };

                await _relay.SendAsync(req, ct);
            }
            catch
            {
                await _messages.MarkMessageAsFailedAsync(m.MessageId, ct);
            }
        }
    }

    private async Task HandleAckAsync(RelayAck ack)
    {
        try
        {
            await _messages.MarkMessageAsSentAsync(ack.LocalMessageId, ack.RemoteMessageId);
        }
        catch
        {
            // swallow/log
        }
    }

    private async Task HandleIncomingAsync(RelayIncomingMessage incoming)
    {
        try
        {
            var me = _auth.UserId!;
            var convo = await _convos.GetByIdAsync(incoming.ConversationId);
            if (convo == null)
            {
                convo = await _convos.EnsureDirectAsync(me, incoming.SenderUserId);
            }

            await _messages.AddMessageAsync(
                conversationId: incoming.ConversationId,
                senderUserId: incoming.SenderUserId,
                cipherText: incoming.CipherTextBase64,
                contentType: incoming.ContentType ?? "application/octet-stream"
            );
        }
        catch
        {
            // swallow/log
        }
    }
    public async Task SendMessageAsync(string conversationId, string plainText, string contentType = "text/plain", CancellationToken ct = default)
    {
        var myUserId = _auth.UserId ?? throw new InvalidOperationException("Not authenticated.");

        var msg = await _messages.AddMessageAsync(
            conversationId: conversationId,
            senderUserId: myUserId,
            cipherText: plainText,
            contentType: contentType,
            ct: ct);

        await FlushOutboxOnceAsync(ct);
    }

}
