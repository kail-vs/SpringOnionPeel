using SpringOnion.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringOnion.Data.Repositories;

public interface IMessageRepository
{
    Task<Message> AddLocalOutgoingAsync(string conversationId, string senderUserId, string base64Cipher, string? contentType, CancellationToken ct = default);
    Task<List<Message>> GetRecentAsync(string conversationId, int take = 50, CancellationToken ct = default);
}
