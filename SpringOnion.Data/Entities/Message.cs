using SpringOnion.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SpringOnion.Data.Entities
{
    public class Message
    {
        public string MessageId { get; set; } = null!;
        public string ConversationId { get; set; } = null!;
        public string SenderUserId { get; set; } = null!;
        public long SortId { get; set; } 
        public DateTimeOffset SentAtUtc { get; set; }
        public DateTimeOffset? ReceivedAtUtc { get; set; }
        public DateTimeOffset? ReadAtUtc { get; set; }
        public bool IsDeleted { get; set; }

        public string? CipherText { get; set; }  
        public string? ContentType { get; set; } 
        public string? MetadataJson { get; set; } 

        public Conversation Conversation { get; set; } = null!;
        public MessageStatus Status { get; set; } = MessageStatus.LocalOnly;
        public string? RemoteId { get; set; }
        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    }
}
