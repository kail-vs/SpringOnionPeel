using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using SpringOnion.Data;
using SpringOnion.Data.Entities;
using SpringOnion.Data.Repositories;
using SpringOnion.Services;
using SpringOnion.Contracts;

namespace SpringOnion.ViewModels;

public class DashboardViewModel : BaseViewModel
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly AuthenticationService _auth;
    private readonly MessageSyncService _sync;

    public ObservableCollection<Conversation> Conversations { get; } = new();

    public ICommand RefreshConversationsCommand { get; }
    public ICommand CreateTestConversationCommand { get; }

    private string _statusMessage;
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public DashboardViewModel(
        IDbContextFactory<AppDbContext> dbFactory,
        AuthenticationService auth,
        MessageSyncService sync)
    {
        _dbFactory = dbFactory;
        _auth = auth;
        _sync = sync;

        RefreshConversationsCommand = new Command(async () => await LoadConversationsAsync());
        CreateTestConversationCommand = new Command(async () => await CreateTestConversationAsync());
    }

    /// <summary>
    /// Called once when the Dashboard page loads.
    /// Starts sync service (which connects SignalR under the hood).
    /// </summary>
    public async Task InitAsync()
    {
        try
        {
            await _sync.StartAsync();
            StatusMessage = "Sync service started.";
            await LoadConversationsAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error starting sync: {ex.Message}";
        }
    }

    private async Task LoadConversationsAsync()
    {
        Conversations.Clear();

        await using var db = await _dbFactory.CreateDbContextAsync();
        var repo = new ConversationRepository(db);

        var myUserId = _auth.UserId;
        if (string.IsNullOrEmpty(myUserId))
        {
            StatusMessage = "User not authenticated.";
            return;
        }

        var convos = await repo.GetForUserAsync(myUserId);

        foreach (var c in convos)
            Conversations.Add(c);

        StatusMessage = $"Loaded {convos.Count} conversations.";
    }

    private async Task CreateTestConversationAsync()
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var repo = new ConversationRepository(db);

            var myUserId = _auth.UserId;
            if (string.IsNullOrEmpty(myUserId))
            {
                StatusMessage = "Current user not found.";
                return;
            }

            var otherUser = await db.UserProfiles
                .Where(u => u.UserId != myUserId)
                .FirstOrDefaultAsync();

            if (otherUser is null)
            {
                StatusMessage = "No other users available.";
                return;
            }

            var convo = await repo.EnsureDirectAsync(myUserId, otherUser.UserId);

            var msg = new Message
            {
                MessageId = Guid.NewGuid().ToString("N"),
                ConversationId = convo.ConversationId,
                SenderUserId = myUserId,
                SortId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                SentAtUtc = DateTime.UtcNow,
                CipherText = "Hello from Dashboard test!",
                ContentType = "text/plain"
            };

            db.Messages.Add(msg);
            await db.SaveChangesAsync();

            StatusMessage = $"Test message prepared for {otherUser.DisplayName ?? otherUser.UserId}";

            await _sync.SendMessageAsync(msg);

            await LoadConversationsAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }
}
