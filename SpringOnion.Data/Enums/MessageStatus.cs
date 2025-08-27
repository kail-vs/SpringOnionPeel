using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringOnion.Data.Enums
{
    public enum MessageStatus
    {
        LocalOnly = 0,    // created locally, not yet sent to relay
        Sending = 1,      // currently being sent
        Sent = 2,         // sent to relay (server ack)
        Delivered = 3,    // receiver delivered ack
        Failed = 99
    }
}
