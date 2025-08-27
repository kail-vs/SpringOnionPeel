using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SpringOnion.Contracts;

namespace SpringOnion.Services;

public class SignalRClientService : IAsyncDisposable
{
    private readonly HubConnection _connection;
    private readonly ILogger<SignalRClientService> _logger;

    public event Func<RelayIncomingMessage, Task>? OnMessageReceived;
    public event Func<RelayAck, Task>? OnAckReceived;

    public SignalRClientService(IConfiguration config, ILogger<SignalRClientService> logger)
    {
        _logger = logger;

        var hubUrl = config["SignalRHubUrl"]
                     ?? throw new InvalidOperationException("SignalRHubUrl missing in config");

        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = async () =>
                {
                    return await SecureStorage.GetAsync("access_token");
                };
            })
            .WithAutomaticReconnect()
            .Build();

        RegisterHandlers();
    }

    private void RegisterHandlers()
    {
        _connection.On<RelayIncomingMessage>("ReceiveMessage", async msg =>
        {
            if (OnMessageReceived is not null)
                await OnMessageReceived.Invoke(msg);
        });

        _connection.On<RelayAck>("Ack", async ack =>
        {
            if (OnAckReceived is not null)
                await OnAckReceived.Invoke(ack);
        });
    }

    public async Task StartAsync()
    {
        if (_connection.State == HubConnectionState.Disconnected)
        {
            await _connection.StartAsync();
            _logger.LogInformation("SignalR connection started: {State}", _connection.State);
        }
    }

    public async Task StopAsync()
    {
        if (_connection.State == HubConnectionState.Connected)
            await _connection.StopAsync();
    }

    public async Task SendMessageAsync(RelaySendRequest req)
    {
        await _connection.InvokeAsync("SendMessage", req);
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
