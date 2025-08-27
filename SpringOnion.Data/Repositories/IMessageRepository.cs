using SpringOnion.Data.Entities;

namespace SpringOnion.Data.Repositories
{
    public interface IMessageRepository
    {
        Task<Message> AddMessageAsync(
            string conversationId,
            string senderUserId,
            string cipherText,
            string contentType,
            CancellationToken ct = default);

        Task<List<Message>> GetMessagesForConversationAsync(
            string conversationId,
            int limit = 50,
            CancellationToken ct = default);

        Task<List<Message>> GetOutgoingPendingAsync(
            int limit = 50,
            CancellationToken ct = default);

        Task MarkMessageAsSentAsync(
            string messageId,
            string? remoteId = null,
            CancellationToken ct = default);

        Task MarkMessageAsDeliveredAsync(
            string messageId,
            CancellationToken ct = default);

        Task MarkMessageAsFailedAsync(
            string messageId,
            CancellationToken ct = default);
    }
}
