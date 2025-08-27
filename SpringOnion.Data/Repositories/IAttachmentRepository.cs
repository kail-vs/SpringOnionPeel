using SpringOnion.Data.Entities;

namespace SpringOnion.Data.Repositories
{
    public interface IAttachmentRepository
    {
        Task<Attachment?> GetByIdAsync(string id, CancellationToken ct = default);
        Task<List<Attachment>> GetByMessageAsync(string messageId, CancellationToken ct = default);
        Task AddAsync(Attachment attachment, CancellationToken ct = default);
        Task DeleteAsync(string id, CancellationToken ct = default);
    }
}
