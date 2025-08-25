using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringOnion.Data.Entities
{
    public class ConversationParticipant
    {
        public string ConversationId { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public DateTimeOffset JoinedAtUtc { get; set; } = DateTimeOffset.UtcNow;
        public Conversation Conversation { get; set; } = null!;
    }
}
