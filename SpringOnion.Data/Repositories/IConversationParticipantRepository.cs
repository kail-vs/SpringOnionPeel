using SpringOnion.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringOnion.Data.Repositories
{
    public interface IConversationParticipantRepository
    {
        Task<List<ConversationParticipant>> GetByConversationAsync(string conversationId, CancellationToken ct = default);
        Task AddAsync(ConversationParticipant participant, CancellationToken ct = default);
        Task RemoveAsync(string conversationId, string userId, CancellationToken ct = default);
    }
}
