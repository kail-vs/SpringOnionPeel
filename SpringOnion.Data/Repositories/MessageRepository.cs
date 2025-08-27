using Microsoft.EntityFrameworkCore;
using SpringOnion.Data.Entities;
using SpringOnion.Data.Enums;

namespace SpringOnion.Data.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly AppDbContext _db;

        public MessageRepository(AppDbContext db) => _db = db;

        public async Task<Message> AddMessageAsync(
            string conversationId,
            string senderUserId,
            string cipherText,
            string contentType,
            CancellationToken ct = default)
        {
            var sortId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var msg = new Message
            {
                MessageId = Guid.NewGuid().ToString(),
                ConversationId = conversationId,
                SenderUserId = senderUserId,
                SortId = sortId,
                SentAtUtc = DateTimeOffset.UtcNow,
                CipherText = cipherText,
                ContentType = contentType,
                Status = MessageStatus.LocalOnly
            };

            _db.Messages.Add(msg);
            await _db.SaveChangesAsync(ct);
            return msg;
        }

        public async Task<List<Message>> GetMessagesForConversationAsync(
            string conversationId,
            int limit = 50,
            CancellationToken ct = default)
        {
            return await _db.Messages
                .Where(m => m.ConversationId == conversationId && !m.IsDeleted)
                .OrderByDescending(m => m.SortId)
                .Take(limit)
                .ToListAsync(ct);
        }

        public async Task<List<Message>> GetOutgoingPendingAsync(int limit = 50, CancellationToken ct = default)
        {
            return await _db.Messages
                .Where(m => m.Status == MessageStatus.LocalOnly || m.Status == MessageStatus.Failed)
                .OrderBy(m => m.SortId)
                .Take(limit)
                .ToListAsync(ct);
        }

        public async Task MarkMessageAsSentAsync(string messageId, string? remoteId = null, CancellationToken ct = default)
        {
            var msg = await _db.Messages.FirstOrDefaultAsync(m => m.MessageId == messageId, ct);
            if (msg != null)
            {
                msg.Status = MessageStatus.Sent;
                msg.RemoteId = remoteId ?? msg.RemoteId;
                await _db.SaveChangesAsync(ct);
            }
        }

        public async Task MarkMessageAsDeliveredAsync(string messageId, CancellationToken ct = default)
        {
            var msg = await _db.Messages.FirstOrDefaultAsync(m => m.MessageId == messageId, ct);
            if (msg != null)
            {
                msg.Status = MessageStatus.Delivered;
                msg.ReceivedAtUtc = DateTimeOffset.UtcNow;
                await _db.SaveChangesAsync(ct);
            }
        }

        public async Task MarkMessageAsFailedAsync(string messageId, CancellationToken ct = default)
        {
            var msg = await _db.Messages.FirstOrDefaultAsync(m => m.MessageId == messageId, ct);
            if (msg != null)
            {
                msg.Status = MessageStatus.Failed;
                await _db.SaveChangesAsync(ct);
            }
        }
    }
}
