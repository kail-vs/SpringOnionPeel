using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringOnion.Contracts;

[MessagePackObject]
public class WireAttachment
{
    [Key(0)] public string FileName { get; set; } = string.Empty;
    [Key(1)] public string? ContentType { get; set; }
    [Key(2)] public byte[] Data { get; set; } = Array.Empty<byte>();
}
