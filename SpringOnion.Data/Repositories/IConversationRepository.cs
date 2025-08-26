using SpringOnion.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SpringOnion.Data.Repositories;

public interface IConversationRepository
{
    Task<Conversation> EnsureDirectAsync(string userA, string userB, CancellationToken ct = default);
    Task<Conversation?> GetByIdAsync(string conversationId, CancellationToken ct = default);
    Task<List<Conversation>> GetForUserAsync(string userId, CancellationToken ct = default);
}