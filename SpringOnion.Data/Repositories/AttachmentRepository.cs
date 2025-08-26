using Microsoft.EntityFrameworkCore;
using SpringOnion.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringOnion.Data.Repositories
{
    public class AttachmentRepository : IAttachmentRepository
    {
        private readonly AppDbContext _db;

        public AttachmentRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Attachment?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            return await _db.Attachments.FirstOrDefaultAsync(a => a.AttachmentId == id, ct);
        }

        public async Task<List<Attachment>> GetByMessageAsync(string messageId, CancellationToken ct = default)
        {
            return await _db.Attachments
                .Where(a => a.MessageId == messageId)
                .ToListAsync(ct);
        }

        public async Task AddAsync(Attachment attachment, CancellationToken ct = default)
        {
            await _db.Attachments.AddAsync(attachment, ct);
            await _db.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(string id, CancellationToken ct = default)
        {
            var attach = await _db.Attachments.FindAsync(new object?[] { id }, ct);
            if (attach != null)
            {
                _db.Attachments.Remove(attach);
                await _db.SaveChangesAsync(ct);
            }
        }
    }
}
