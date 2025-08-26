using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using SpringOnion.Data;
using SpringOnion.Data.Entities;
using SpringOnion.Data.Repositories;
using SpringOnion.Services;

namespace SpringOnion.ViewModels;

public class DashboardViewModel : BaseViewModel
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly AuthenticationService _auth;

    public ObservableCollection<Conversation> Conversations { get; } = new();

    public ICommand RefreshConversationsCommand { get; }
    public ICommand CreateTestConversationCommand { get; }

    private string _statusMessage;
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public DashboardViewModel(IDbContextFactory<AppDbContext> dbFactory, AuthenticationService auth)
    {
        _dbFactory = dbFactory;
        _auth = auth;

        RefreshConversationsCommand = new Command(async () => await LoadConversationsAsync());
        CreateTestConversationCommand = new Command(async () => await CreateTestConversationAsync());
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

            // Just pick the first available other user
            var otherUser = await db.UserProfiles
                .Where(u => u.UserId != myUserId)
                .FirstOrDefaultAsync();

            if (otherUser is null)
            {
                StatusMessage = "No other users available.";
                return;
            }

            // Ensure a direct conversation
            var convo = await repo.EnsureDirectAsync(myUserId, otherUser.UserId);

            // Add a dummy test message
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

            StatusMessage = $"Test message sent to {otherUser.DisplayName ?? otherUser.UserId}";

            await LoadConversationsAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }
}
