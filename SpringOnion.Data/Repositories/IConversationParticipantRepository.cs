using SpringOnion.Data.Entities;

namespace SpringOnion.Data.Repositories
{
    public interface IConversationParticipantRepository
    {
        Task<List<ConversationParticipant>> GetByConversationAsync(string conversationId, CancellationToken ct = default);
        Task AddAsync(ConversationParticipant participant, CancellationToken ct = default);
        Task RemoveAsync(string conversationId, string userId, CancellationToken ct = default);
    }
}
