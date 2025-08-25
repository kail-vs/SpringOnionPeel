using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringOnion.Data.Entities
{
    public class UserProfile
    {
        public string UserId { get; set; } = null!;
        public string? DisplayName { get; set; }
        public string? AvatarPath { get; set; }
        public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    }
}
