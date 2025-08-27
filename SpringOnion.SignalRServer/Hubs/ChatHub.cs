using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SpringOnion.Contracts;


namespace SpringOnion.SignalRServer.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private static readonly Dictionary<string, HashSet<string>> _connections = new();

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst("sub")?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            lock (_connections)
            {
                if (!_connections.TryGetValue(userId, out var set))
                {
                    set = new HashSet<string>();
                    _connections[userId] = set;
                }
                set.Add(Context.ConnectionId);
            }

            // Optionally: add to a SignalR group named by userId:
            await Groups.AddToGroupAsync(Context.ConnectionId, $"u:{userId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst("sub")?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            lock (_connections)
            {
                if (_connections.TryGetValue(userId, out var set))
                {
                    set.Remove(Context.ConnectionId);
                    if (set.Count == 0) _connections.Remove(userId);
                }
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"u:{userId}");
        }

        await base.OnDisconnectedAsync(exception);
    }

    // Client -> Server: send one encrypted message
    public async Task SendMessage(RelaySendRequest req)
    {
        // Optionally persist/assign remote ID here
        var remoteId = Guid.NewGuid().ToString("N");

        // Ack back to sender immediately
        await Clients.Caller.SendAsync("Ack", new RelayAck
        {
            LocalMessageId = req.LocalMessageId,
            RemoteMessageId = remoteId
        });

        // Forward to recipient (by group)
        await Clients.Group($"u:{req.ToUserId}").SendAsync("ReceiveMessage", new RelayIncomingMessage
        {
            ConversationId = req.ConversationId,
            SenderUserId = req.SenderUserId,
            CipherTextBase64 = req.CipherTextBase64,
            ContentType = req.ContentType,
            RemoteMessageId = remoteId,
            LocalMessageIdFromSender = req.LocalMessageId
        });
    }
}
