using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SpringOnion.Contracts;

namespace SpringOnion.Contracts;

public class PlainMessage
{
    public string? Text { get; set; }
    public string? ContentType { get; set; }
    public List<WireAttachment>? Attachments { get; set; }
    public Dictionary<string, string>? Meta { get; set; }
}
