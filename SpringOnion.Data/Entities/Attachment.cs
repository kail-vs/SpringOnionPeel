using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringOnion.Data.Entities
{
    public class Attachment
    {
        public string AttachmentId { get; set; } = null!; 
        public string MessageId { get; set; } = null!;
        public string LocalPath { get; set; } = null!; 
        public string? ContentType { get; set; }
        public long? SizeBytes { get; set; }
        public string? Hash { get; set; }   
        public string? MetadataJson { get; set; }
        public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

        public Message Message { get; set; } = null!;
    }
}
