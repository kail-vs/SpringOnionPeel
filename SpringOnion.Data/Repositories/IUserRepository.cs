using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpringOnion.Data.Entities;

namespace SpringOnion.Data.Repositories
{
    public interface IUserRepository
    {
        Task UpsertUsersAsync(IEnumerable<UserProfile> users, CancellationToken ct = default);
        Task<List<UserProfile>> GetAllAsync(CancellationToken ct = default);
        Task<UserProfile?> GetByIdAsync(string userId, CancellationToken ct = default);
    }
}

