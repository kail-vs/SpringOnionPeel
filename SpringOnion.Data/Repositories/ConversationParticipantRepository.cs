using Microsoft.EntityFrameworkCore;
using SpringOnion.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringOnion.Data.Repositories
{
    public class ConversationParticipantRepository : IConversationParticipantRepository
    {
        private readonly AppDbContext _db;

        public ConversationParticipantRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<ConversationParticipant>> GetByConversationAsync(string conversationId, CancellationToken ct = default)
        {
            return await _db.ConversationParticipants
                .Where(cp => cp.ConversationId == conversationId)
                .ToListAsync(ct);
        }

        public async Task AddAsync(ConversationParticipant participant, CancellationToken ct = default)
        {
            await _db.ConversationParticipants.AddAsync(participant, ct);
            await _db.SaveChangesAsync(ct);
        }

        public async Task RemoveAsync(string conversationId, string userId, CancellationToken ct = default)
        {
            var cp = await _db.ConversationParticipants
                .FirstOrDefaultAsync(x => x.ConversationId == conversationId && x.UserId == userId, ct);

            if (cp != null)
            {
                _db.ConversationParticipants.Remove(cp);
                await _db.SaveChangesAsync(ct);
            }
        }
    }
}
