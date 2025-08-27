using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using SpringOnion.Contracts;

namespace SpringOnion.Services.Realtime;

public class SignalRRelayClient
{
    private readonly AuthenticationService _auth;
    private readonly string _hubUrl;

    private HubConnection? _connection;

    public event Func<RelayIncomingMessage, Task>? OnIncomingMessage;

    public event Func<RelayAck, Task>? OnAck;

    public bool IsConnected => _connection?.State == HubConnectionState.Connected;

    public SignalRRelayClient(AuthenticationService auth, string hubUrl)
    {
        _auth = auth;
        _hubUrl = hubUrl ?? throw new ArgumentNullException(nameof(hubUrl));
    }

    public async Task StartAsync(CancellationToken ct = default)
    {
        if (_connection is not null && IsConnected)
            return;

        var builder = new HubConnectionBuilder()
            .WithUrl(_hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(_auth.Token);
            })
            .WithAutomaticReconnect();

        _connection = builder.Build();

        _connection.On<RelayIncomingMessage>("ReceiveMessage", async msg =>
        {
            if (OnIncomingMessage != null)
                await OnIncomingMessage.Invoke(msg);
        });

        _connection.On<RelayAck>("Ack", async ack =>
        {
            if (OnAck != null)
                await OnAck.Invoke(ack);
        });

        await _connection.StartAsync(ct);
    }

    public async Task StopAsync(CancellationToken ct = default)
    {
        if (_connection is not null)
        {
            await _connection.StopAsync(ct);
            await _connection.DisposeAsync();
            _connection = null;
        }
    }

    /// <summary>
    /// Send an already-encrypted message to relay
    /// </summary>
    public async Task SendAsync(RelaySendRequest req, CancellationToken ct = default)
    {
        if (_connection is null || _connection.State != HubConnectionState.Connected)
            throw new InvalidOperationException("SignalR not connected");

        await _connection.InvokeAsync("SendMessage", req, ct);
    }
}
