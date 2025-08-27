using Microsoft.EntityFrameworkCore;
using SpringOnion.Data.Entities;
using System.Security.Cryptography;
using System.Text;

namespace SpringOnion.Data.Repositories
{
    public class ConversationRepository : IConversationRepository
    {
        private readonly AppDbContext _db;

        public ConversationRepository(AppDbContext db) => _db = db;

        public async Task<Conversation?> GetByIdAsync(string conversationId, CancellationToken ct = default)
            => await _db.Conversations
                        .Include(c => c.Participants)
                        .FirstOrDefaultAsync(c => c.ConversationId == conversationId, ct);

        public async Task<Conversation> EnsureDirectAsync(string userA, string userB, CancellationToken ct = default)
        {
            var cid = BuildDeterministicDirectId(userA, userB);
            var convo = await _db.Conversations
                                 .Include(c => c.Participants)
                                 .FirstOrDefaultAsync(c => c.ConversationId == cid, ct);

            if (convo is not null) return convo;

            convo = new Conversation
            {
                ConversationId = cid,
                Type = "Direct",
                CreatedAtUtc = DateTime.UtcNow,
                LastActivityUtc = DateTime.UtcNow
            };

            var p1 = new ConversationParticipant { ConversationId = cid, UserId = userA, JoinedAtUtc = DateTime.UtcNow };
            var p2 = new ConversationParticipant { ConversationId = cid, UserId = userB, JoinedAtUtc = DateTime.UtcNow };

            _db.Conversations.Add(convo);
            _db.ConversationParticipants.AddRange(p1, p2);

            await _db.SaveChangesAsync(ct);
            return convo;
        }

        public async Task<List<Conversation>> GetForUserAsync(string userId, CancellationToken ct = default)
            => await _db.Conversations
                        .Include(c => c.Participants)
                        .Where(c => c.Participants.Any(p => p.UserId == userId))
                        .OrderByDescending(c => c.LastActivityUtc)
                        .ToListAsync(ct);

        private static string BuildDeterministicDirectId(string a, string b)
        {
            var x = string.CompareOrdinal(a, b) <= 0 ? $"{a}|{b}" : $"{b}|{a}";
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes("direct:" + x));
            var hex = Convert.ToHexString(hash);
            return "D_" + hex[..30];
        }
    }
}
