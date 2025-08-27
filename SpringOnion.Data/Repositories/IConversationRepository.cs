using SpringOnion.Data.Entities;

namespace SpringOnion.Data.Repositories
{
    public interface IConversationRepository
    {
        Task<Conversation> EnsureDirectAsync(string userA, string userB, CancellationToken ct = default);
        Task<Conversation?> GetByIdAsync(string conversationId, CancellationToken ct = default);
        Task<List<Conversation>> GetForUserAsync(string userId, CancellationToken ct = default);
    }
}
