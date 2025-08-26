using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringOnion.Contracts;

[MessagePackObject]
public class WireMessage
{
    [Key(0)] public int SchemaVersion { get; set; } = 1;

    [Key(1)] public string? Text { get; set; }

    [Key(2)] public List<WireAttachment>? Attachments { get; set; }

    [Key(3)] public string? ContentType { get; set; }
    [Key(4)] public Dictionary<string, string>? Meta { get; set; }
}
