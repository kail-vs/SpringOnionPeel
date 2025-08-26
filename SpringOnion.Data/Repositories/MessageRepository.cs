using Microsoft.EntityFrameworkCore;
using SpringOnion.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringOnion.Data.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly AppDbContext _db;

    public MessageRepository(AppDbContext db) => _db = db;

    public async Task<Message> AddLocalOutgoingAsync(string conversationId, string senderUserId, string base64Cipher, string? contentType, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var msg = new Message
        {
            MessageId = Guid.NewGuid().ToString("N"), // fits 64
            ConversationId = conversationId,
            SenderUserId = senderUserId,
            SortId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            SentAtUtc = now,
            CipherText = base64Cipher,
            ContentType = contentType ?? "application/x-msgpack+gzip+aesgcm",
            IsDeleted = false
        };

        _db.Messages.Add(msg);

        var convo = await _db.Conversations.FirstOrDefaultAsync(c => c.ConversationId == conversationId, ct);
        if (convo != null)
            convo.LastActivityUtc = now;

        await _db.SaveChangesAsync(ct);
        return msg;
    }

    public async Task<List<Message>> GetRecentAsync(string conversationId, int take = 50, CancellationToken ct = default)
        => await _db.Messages
                    .Where(m => m.ConversationId == conversationId)
                    .OrderByDescending(m => m.SortId)
                    .Take(take)
                    .AsNoTracking()
                    .ToListAsync(ct);
}