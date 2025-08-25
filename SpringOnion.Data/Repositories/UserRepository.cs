using Microsoft.EntityFrameworkCore;
using SpringOnion.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringOnion.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;

        public UserRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<UserProfile>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.UserProfiles
                .AsNoTracking()
                .OrderBy(u => u.DisplayName ?? u.UserId)
                .ToListAsync(ct);
        }

        public async Task<UserProfile?> GetByIdAsync(string userId, CancellationToken ct = default)
        {
            return await _db.UserProfiles.FindAsync(new object?[] { userId }, ct);
        }

        public async Task UpsertUsersAsync(IEnumerable<UserProfile> users, CancellationToken ct = default)
        {
            var incoming = users.ToList();
            var ids = incoming.Select(u => u.UserId).ToList();

            var existing = await _db.UserProfiles
                .Where(u => ids.Contains(u.UserId))
                .ToDictionaryAsync(u => u.UserId, ct);

            foreach (var u in incoming)
            {
                if (existing.TryGetValue(u.UserId, out var ex))
                {
                    ex.DisplayName = u.DisplayName;
                    ex.AvatarPath = u.AvatarPath;
                    ex.UpdatedAtUtc = u.UpdatedAtUtc;
                    _db.UserProfiles.Update(ex);
                }
                else
                {
                    await _db.UserProfiles.AddAsync(u, ct);
                }
            }

            await _db.SaveChangesAsync(ct);
        }
    }
}

